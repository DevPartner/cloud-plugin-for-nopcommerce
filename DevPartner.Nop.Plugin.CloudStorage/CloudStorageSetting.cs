using Nop.Core.Configuration;

namespace DevPartner.Nop.Plugin.CloudStorage
{
    public class DevPartnerCloudStorageSetting : ISettings
    {
        public string PictureStoreType { get; set; }
        public string DownloadStoreType { get; set; }
        public bool AlwaysShowMainImage { get; set; }
        public bool CheckIfImageExist { get; set; }
        public bool ArchiveDownloads { get; set; }
        public string ContentStoreType { get; set; }
        public string DefaultImageName { get; set; }
        public string DefaultAvatarImageName { get; set; }
        public bool StoreImageInDb { get; internal set; }
        public string LicenseKey { get; internal set; }
    }
}
