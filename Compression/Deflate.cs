using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahi.Compression
{
    public class Deflate
    {
        public static byte[] CompressDeflate(byte[] data)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var deflateStream = new DeflateStream(outputStream, CompressionMode.Compress))
                {
                    deflateStream.Write(data, 0, data.Length);
                }
                return outputStream.ToArray();
            }
        }

        public static byte[] DecompressDeflate(byte[] compressedData)
        {
            using (var inputStream = new MemoryStream(compressedData))
            using (var deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                deflateStream.CopyTo(outputStream);
                return outputStream.ToArray();
            }
        }
    }
}
