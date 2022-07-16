using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace DevPartner.Nop.Plugin.CloudStorage.Models
{
    public partial record ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("DevPartner.CloudStorage.ConfigureModel.PictureStoreType")]
        public string PictureStoreType { get; set; }
        //[NopResourceDisplayName("DevPartner.CloudStorage.ConfigureModel.ThumbPictureStoreType")]
        //public string ThumbPictureStoreType { get; set; }
        [NopResourceDisplayName("DevPartner.CloudStorage.ConfigureModel.DownloadStoreType")]
        public string DownloadStoreType { get; set; }
        [NopResourceDisplayName("DevPartner.CloudStorage.ConfigureModel.ContentStoreType")]
        public string ContentStoreType { get; set; }
        [NopResourceDisplayName("DevPartner.CloudStorage.ConfigureModel.AlwaysShowMainImage")]
        public bool AlwaysShowMainImage { get; set; }
        [NopResourceDisplayName("DevPartner.CloudStorage.ConfigureModel.CheckIfImageExist")]
        public bool CheckIfImageExist { get; set; }
        [NopResourceDisplayName("DevPartner.CloudStorage.ConfigureModel.ArchiveDownloads")]
        public bool ArchiveDownloads { get; set; }
        [NopResourceDisplayName("DevPartner.CloudStorage.ConfigureModel.StoreImageInDb")]
        public bool StoreImageInDb { get; set; }
        [NopResourceDisplayName("DevPartner.CloudStorage.ConfigureModel.LicenseKey")]
        public string LicenseKey { get; set; }
        public bool LicenseValid { get; internal set; }
    }
}
