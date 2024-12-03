using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Mahi
{
    internal static class HttpParser
    {
        public static HttpRequest Parse(byte[] buffer)
        {
            var requestString = Encoding.UTF8.GetString(buffer);
            var requestLines = requestString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            var request = new HttpRequest();

            // Parse request line
            var requestLineParts = requestLines[0].Split(' ');
            if (requestLineParts.Length >= 3)
            {
                request.Method = requestLineParts[0];
                request.Url = requestLineParts[1];
                request.HttpVersion = requestLineParts[2];
            }

            int i = 1;
            for (; i < requestLines.Length && requestLines[i] != ""; i++)
                request.Headers.Add(Header.Parse(requestLines[i]));

            if (request.Method == "POST" || request.Method == "PUT")
            {
                var bodyStartIndex = requestString.IndexOf("\r\n\r\n") + 4;
                if (request.Headers.GetValue("Content-Type")?.StartsWith("multipart/form-data") ?? false)
                {
                    request.Content = buffer.Skip(bodyStartIndex).ToArray();

                    var boundary = request.Headers.GetValue("Content-Type", 1).Split('=')[1];
                    var boundaryBytes = Encoding.ASCII.GetBytes("--" + boundary);
                    var parts = SplitByBoundary(request.Content, boundaryBytes);

                    request.FormDatas = (HttpRequestPartCollection)parts;
                }
                else
                {
                    request.Content = buffer.Skip(bodyStartIndex).ToArray();
                }
            }

            return request;
        }

        private static List<HttpRequestPart> SplitByBoundary(byte[] body, byte[] boundary)
        {
            var parts = new List<HttpRequestPart>();
            var start = 0;
            while (start < body.Length)
            {
                var end = IndexOf(body, boundary, start);
                if (end == -1)
                {
                    break;
                }

                var content = body.Skip(start).Take(end - start - 2).ToArray();
                if (content.Length > 2)
                {
                    var headerContentSeparator = Encoding.UTF8.GetBytes("\r\n\r\n");
                    int separatorIndex = IndexOf(content, headerContentSeparator, 0);
                    if (separatorIndex == -1)
                    {
                        HeaderCollection headers = new HeaderCollection();
                        string contentString = Encoding.UTF8.GetString(content);
                        string[] contentLines = contentString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                        for (int i = 0; i < contentLines.Length && contentLines[i] != ""; i++)
                            headers.Add(Header.Parse(contentLines[i]));

                        var part = new HttpRequestPart
                        {
                            Content = new byte[0],
                            Headers = headers
                        };
                        parts.Add(part);
                    }
                    else
                    {
                        byte[] contentBytes = new byte[content.Length - separatorIndex - 4];
                        Array.Copy(content, separatorIndex + 4, contentBytes, 0, contentBytes.Length);

                        byte[] headersBytes = new byte[separatorIndex];
                        Array.Copy(content, headersBytes, separatorIndex);
                        HeaderCollection headers = new HeaderCollection();
                        string contentString = Encoding.UTF8.GetString(headersBytes);
                        string[] contentLines = contentString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                        for (int i = 0; i < contentLines.Length && contentLines[i] != ""; i++)
                            headers.Add(Header.Parse(contentLines[i]));

                        var part = new HttpRequestPart
                        {
                            Content = contentBytes,
                            Headers = headers
                        };
                        parts.Add(part);
                    }
                }

                start = end + boundary.Length + 2; // Skip the boundary and the trailing "\r\n"
            }
            return parts;
        }

        static int IndexOf(byte[] source, byte[] pattern, int start)
        {
            for (int i = start; i < source.Length - pattern.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (source[i + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
