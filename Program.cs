using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Mahi;

class Program
{
    const string ip = "0.0.0.0";
    const int port = 1010;

    static void Main()
    {
        //var server = new HttpServer(IPAddress.Parse(ip), port, "cert.pfx", "$R%T4r5t");
        var server = new HttpServer(IPAddress.Parse(ip), port);
        server.Start();

        Console.WriteLine("Server started");

        while (true)
        {
            var context = server.GetContext();
            var request = context.Request;
            var response = context.Response;

            Console.WriteLine(request.Url);

            var stream = context.Response.ResponseStream;
            var bytes = Encoding.UTF8.GetBytes("Hello");
            stream.Write(bytes, 0, bytes.Length);
            context.Close();
        }

        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:10100/");
        listener.Start();
        while (true)
        {
            var context = listener.GetContext();
            var request = context.Request;
            var response = context.Response;

            Console.WriteLine(request.Url?.AbsoluteUri);

            // Check the Accept-Encoding header
            string acceptEncoding = request.Headers["Accept-Encoding"];
            byte[] responseData = Encoding.UTF8.GetBytes("Hello, World!");

            // Determine if we should compress the response
            if (!string.IsNullOrEmpty(acceptEncoding))
            {
                if (acceptEncoding.Contains("gzip"))
                {
                    response.Headers.Add("Content-Encoding", "gzip");
                    response.ContentType = "text/plain";

                    using (MemoryStream compressedStream = new MemoryStream())
                    using (GZipStream gzipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                    {
                        gzipStream.Write(responseData, 0, responseData.Length);
                        response.ContentLength64 = compressedStream.Length;
                        compressedStream.Position = 0;
                        compressedStream.CopyTo(response.OutputStream);
                    }
                }
                else if (acceptEncoding.Contains("deflate"))
                {
                    response.Headers.Add("Content-Encoding", "deflate");
                    response.ContentType = "text/plain";

                    using (MemoryStream compressedStream = new MemoryStream())
                    using (DeflateStream deflateStream = new DeflateStream(compressedStream, CompressionMode.Compress))
                    {
                        deflateStream.Write(responseData, 0, responseData.Length);
                        response.ContentLength64 = compressedStream.Length;
                        compressedStream.Position = 0;
                        compressedStream.CopyTo(response.OutputStream);
                    }
                }
                else
                {
                    // If no compression is accepted, send the uncompressed response
                    response.ContentLength64 = responseData.Length;
                    response.OutputStream.Write(responseData, 0, responseData.Length);
                }
            }
            else
            {
                // If no Accept-Encoding header, send the uncompressed response
                response.ContentLength64 = responseData.Length;
                response.OutputStream.Write(responseData, 0, responseData.Length);
            }

            // Close the response
            response.OutputStream.Close();
        }
    }
}
