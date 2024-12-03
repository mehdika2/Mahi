using System.IO;
using System.Net.Security;
using System.Net.Sockets;

namespace Mahi
{
    public class HttpResponse
    {
        public string HttpVersion { get; set; } = "HTTP/1.1";
        public int StatusCode { get; set; } = 200;
        public string StatusText { get; set; }
        public HeaderCollection Headers { get; set; } = new HeaderCollection();
        public MemoryStream ResponseStream { get; set; } = new MemoryStream();
        public NetworkStream NetowrkStream { get; set; }
        public SslStream SecureStream { get; set; }
    }
}
