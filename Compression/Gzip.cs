using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahi.Compression
{
    public class Gzip
    {
        public static byte[] CompressGzip(byte[] data)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
                    gzipStream.Write(data, 0, data.Length);
                return outputStream.ToArray();
            }
        }

        public static byte[] DecompressGzip(byte[] compressedData)
        {
            using (var inputStream = new MemoryStream(compressedData))
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                gzipStream.CopyTo(outputStream);
                return outputStream.ToArray();
            }
        }
    }
}
