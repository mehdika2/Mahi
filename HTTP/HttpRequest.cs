using System.Text;
using System.Net.Sockets;

namespace Mahi
{
    public class HttpRequest
    {
        public string Method { get; internal set; }
        public string Url { get; set; }
        public string HttpVersion { get; set; }
        public HeaderCollection Headers { get; set; } = new HeaderCollection();
        public byte[] Content { get; set; }
        public HttpRequestPartCollection FormDatas { get; set; }

        #region Dependent Properties
        public bool IsMultipartRequest
        {
            get
            {
                return Headers.GetValue("Content-Type")?.StartsWith("multipart/form-data") ?? false;
            }
        }
        #endregion

        public override string ToString()
        {
            return Method + " " + Url;
        }
    }
}
