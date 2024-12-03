using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Mahi
{
    public class HttpContext
    {
        public HttpRequest Request { get; internal set; }
        public HttpResponse Response { get; set; } = new HttpResponse();
        public Socket Client { get; internal set; }

        public void Close()
        {
            byte[] oldBytes = CompressResponse(ReadAllBytes(Response.ResponseStream));

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{Response.HttpVersion} {Response.StatusCode}{(Response.StatusText != string.Empty ? " " + Response.StatusText : "")}");
            foreach (var header in Response.Headers)
                sb.AppendLine($"{header.Name}: {string.Join("; ", header.Values)}");
            sb.AppendLine("Server: Mahi 1.0.0");
            sb.AppendLine("content-type: text/html; charset=UTF-8");
            sb.AppendLine("Content-Length: " + oldBytes.Length);
            sb.AppendLine();
            byte[] newBytes = Encoding.UTF8.GetBytes(sb.ToString());

            //int originalLength = (int)Response.OutputStream.Length;
            //Response.OutputStream.SetLength(newBytes.Length + originalLength);
            //Response.OutputStream.Position = newBytes.Length;
            //Response.OutputStream.Write(Response.OutputStream.GetBuffer(), 0, originalLength);
            //Response.OutputStream.Position = 0;
            //Response.OutputStream.Write(newBytes, 0, newBytes.Length);


            if (Response.SecureStream == null)
            {
                Client.Send(newBytes);
                Client.Send(oldBytes);
            }
            else
            {
                Response.SecureStream.Write(newBytes);
                Response.SecureStream.Write(oldBytes);
                Response.SecureStream.Close();
                Response.NetowrkStream.Close();
            }
            Response.ResponseStream.Close();
            Client.Close();
        }

        private byte[] CompressResponse(byte[] bytes)
        {
            if (Request.Headers.AcceptEncoding == null)
                return bytes;

            string[] acceptEncodings = Request.Headers.AcceptEncoding.Split(',');
            foreach (string acceptEncoding in acceptEncodings)
                switch (acceptEncoding.Trim())
                {
                    case "gzip":
                        Response.Headers.Add("vary", "Accept-Encoding");
                        Response.Headers.Add("content-encoding", acceptEncoding);
                        return Compression.Gzip.CompressGzip(bytes);
                    case "deflate":
                        Response.Headers.Add("vary", "Accept-Encoding");
                        Response.Headers.Add("content-encoding", acceptEncoding);
                        return Compression.Deflate.CompressDeflate(bytes);
                    case "br":
                        Response.Headers.Add("vary", "Accept-Encoding");
                        Response.Headers.Add("content-encoding", acceptEncoding);
                        return Compression.Brotli.CompressBrotli(bytes);
                }
            return bytes;
        }

        private byte[] ReadAllBytes(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            // Ensure the stream is at the beginning
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Buffer for reading
                byte[] buffer = new byte[4096]; // 4 KB buffer
                int bytesRead;

                // Read from the stream until no more bytes are available
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    memoryStream.Write(buffer, 0, bytesRead);
                }

                // Return the byte array
                return memoryStream.ToArray();
            }
        }
    }
}
