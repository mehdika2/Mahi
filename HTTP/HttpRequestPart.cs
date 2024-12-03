namespace Mahi
{
    public class HttpRequestPart
    {
        public HeaderCollection Headers { get; set; }
        public byte[] Content { get; set; }

        public HttpRequestPart()
        {
            Headers = new HeaderCollection();
        }

        public HttpRequestPart(HeaderCollection headers, byte[] content)
        {
            Headers = headers;
            Content = content;
        }
    }
}
