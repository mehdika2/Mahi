using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Mahi
{
    public class HttpServer
    {
        private const int BufferSize = 1024; // Read in chunks of 4KB
        private const int MaxRequestSize = 10 * 1024 * 1024; // Limit request size to 10 MB
        private const int ReceiveTimeoutMillis = 5000; // Timeout for receiving data

        public int Backlog { get; set; } = 5;
        public bool IsTlsSecure {  get { return _certificate != null; } }

        Socket _server;
        X509Certificate2 _certificate;
        bool _started = false;
        public HttpServer(IPAddress address, int port)
        {
            _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _server.Bind(new IPEndPoint(address, port));
        }

        public HttpServer(IPAddress address, int port, string certFile, string certPassword)
        {
            _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _server.Bind(new IPEndPoint(address, port));
            _certificate = new X509Certificate2(certFile, certPassword);
        }

        public void Start()
        {
            _server.Listen(Backlog);
            _started = true;
        }

        public void Stop()
        {
            _server.Shutdown(SocketShutdown.Both);
            _started = false;
        }

        public HttpContext GetContext()
        {
            while (true)
            {
                if (!_started)
                    throw new HttpServerException("Server is not started, you must call Start() before getting context.");

                Socket client = _server.Accept();
                //client.ReceiveTimeout = ReceiveTimeoutMillis;

                var context = HandleClient(client);
                if (context == null)
                    continue;
                return context;
            }
        }

        private HttpContext HandleClient(Socket client)
        {
            while (true)
            {
                var totalBytesReceived = new List<byte>();
                if (_certificate == null)
                {
                    byte[] buffer = new byte[BufferSize];
                    int count;
                    while (true)
                    {
                        if (totalBytesReceived.Count == 0)
                            if (!client.Poll(1000, SelectMode.SelectRead))
                                break;
                        count = client.Receive(buffer);

                        // check is https or not
                        if (totalBytesReceived.Count == 0 && count >= 5)
                            // Check if it matches the SSL/TLS handshake format
                            if (buffer[0] == 0x16 && // Handshake record
                                (buffer[1] == 0x03 && // SSL/TLS major version
                                (buffer[2] == 0x01 || buffer[2] == 0x02 || buffer[2] == 0x03 || buffer[2] == 0x04))) // TLS versions
                            {
                                client.Close();
                                return null;
                            }

                        // read bytes
                        if (count > 0)
                        {
                            byte[] validBytes = new byte[count];
                            Array.Copy(buffer, 0, validBytes, 0, count);
                            totalBytesReceived.AddRange(validBytes);
                            if (count > BufferSize) continue;
                        }
                        break;
                    }

                    if (totalBytesReceived.Count == 0)
                    {
                        client.Send(Encoding.UTF8.GetBytes("Accept"));
                        client.Close();
                        continue;
                    }

                    HttpContext context = new HttpContext();
                    context.Request = HttpParser.Parse(totalBytesReceived.ToArray());
                    context.Client = client;

                    return context;
                }
                else
                {
                    var stream = new NetworkStream(client, true);
                    SslStream sslStream = new SslStream(stream, false);
                    try
                    {
                        sslStream.AuthenticateAsServer(_certificate, false, System.Security.Authentication.SslProtocols.Tls12, true);

                        if (!sslStream.IsAuthenticated)
                        {
                            sslStream.Close();
                            stream.Close();
                            client.Close();
                            return null;
                        }

                        var buffer = new byte[BufferSize];

                        while (true)
                        {
                            if (!client.Poll(1000, SelectMode.SelectRead))
                                break;
                            int bytesRead = sslStream.Read(buffer, 0, buffer.Length);
                            if (bytesRead > 0)
                            {
                                totalBytesReceived.AddRange(buffer.AsSpan(0, bytesRead).ToArray());
                                if (totalBytesReceived.Count > MaxRequestSize)
                                {
                                    Console.WriteLine("Request size exceeded. Dropping connection.");
                                    break;
                                }
                            }
                            else break;
                        }
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine($"Client disconnected or timeout occurred: {ex.Message}");
                        sslStream.Close();
                        stream.Close();
                        client.Close();
                        return null;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("SSL error: " + ex.Message);
                        sslStream.Close();
                        stream.Close();
                        client.Close();
                        return null;
                    }

                    if (totalBytesReceived.Count == 0)
                    {
                        byte[] responseBytes = Encoding.ASCII.GetBytes("Accept");
                        sslStream.Write(responseBytes, 0, responseBytes.Length);
                        sslStream.Flush();
                        return null;
                    }

                    HttpContext context = new HttpContext();
                    context.Request = HttpParser.Parse(totalBytesReceived.ToArray());
                    context.Client = client;
                    context.Response.NetowrkStream = stream;
                    context.Response.SecureStream = sslStream;

                    return context;
                }
            }
        }
    }
}
