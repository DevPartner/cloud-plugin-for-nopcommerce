using System;

namespace DevPartner.Nop.Plugin.CloudStorage.Domain
{
    public class FileData
    {
        public int? Width { get; set; }
        public int? Height { get; set; }

        public long Length { get; set; }

        public DateTime LastWriteTime { get; set; }
    }
}
