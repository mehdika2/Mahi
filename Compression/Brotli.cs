using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahi.Compression
{
    public class Brotli
    {
        public static byte[] CompressBrotli(byte[] data)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var brotliStream = new BrotliStream(outputStream, CompressionMode.Compress))
                {
                    brotliStream.Write(data, 0, data.Length);
                }
                return outputStream.ToArray();
            }
        }

        public static byte[] DecompressBrotli(byte[] compressedData)
        {
            using (var inputStream = new MemoryStream(compressedData))
            using (var brotliStream = new BrotliStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                brotliStream.CopyTo(outputStream);
                return outputStream.ToArray();
            }
        }
    }
}
