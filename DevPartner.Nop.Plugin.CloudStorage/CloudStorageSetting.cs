using Nop.Core.Configuration;

namespace DevPartner.Nop.Plugin.CloudStorage
{
    public class DevPartnerCloudStorageSetting : ISettings
    {
        public string DownloadStoreType { get; set; }
        public bool AlwaysShowMainImage { get; set; }
        public bool CheckIfImageExist { get; set; }
        public bool ArchiveDownloads { get; set; }
        public string FileStoreType { get; set; }
        public string DefaultImageName { get; set; }
        public string DefaultAvatarImageName { get; set; }
        public bool StoreImageInDb { get; internal set; }
    }
}
