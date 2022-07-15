using System.IO;
using System.IO.Compression;
using System.Linq;

namespace DevPartner.Nop.Plugin.CloudStorage.Services
{
    public class ZipService
    {
        #region Fields

        public readonly int MinimalUncompressedFileSize = 200;

        #endregion

        #region Constr

        public ZipService()
        {
            
        }

        #endregion
      
        #region Methods

        public byte[] UnzipData(byte[] bytes)
        {
            byte[] result = null;
            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                {
                    if (archive.Entries.Any())
                    {
                        var entry = archive.Entries.First();
                        using (var stream = new BinaryReader(entry.Open()))
                        {
                            result = stream.ReadBytes((int) entry.Length);
                        }
                    }
                }
            }
            return result;
        }

        public byte[] ZipData(byte[] bytes, string entryName)
        {
            byte[] result = null;
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                    using (var writeStream = new BinaryWriter(entry.Open()))
                    {
                        writeStream.Write(bytes);
                    }
                }

                result = new byte[memoryStream.Length];
                memoryStream.Seek(0, SeekOrigin.Begin);
                memoryStream.Read(result, 0, (int) memoryStream.Length);
            }
            return result;
        }

        #endregion
    }
}
