using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace DevPartner.Nop.Plugin.CloudStorage.Azure.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("DevPartner.CloudStorage.AzureBlobProvider.ConfigureModel.ConnectionString")]
        public string ConnectionString { get; set; }
        /*
        [NopResourceDisplayName("DevPartner.CloudStorage.AzureBlobProvider.ConfigureModel.AccountKey")]
        public string AccountKey { get; set; }*/
        /*
        [NopResourceDisplayName("DevPartner.CloudStorage.AzureBlobProvider.ConfigureModel.ContainerForPictures")]
        public string ContainerForPictures { get; set; }

        [NopResourceDisplayName("DevPartner.CloudStorage.AzureBlobProvider.ConfigureModel.ContainerForFiles")]
        public string ContainerForFiles { get; set; }

        [NopResourceDisplayName("DevPartner.CloudStorage.AzureBlobProvider.ConfigureModel.ContainerForContent")]
        public string ContainerForContent { get; set; }

        [NopResourceDisplayName("DevPartner.CloudStorage.AzureBlobProvider.ConfigureModel.CDNUrl")]
        public string CDNUrl { get; set; }

        [NopResourceDisplayName("DevPartner.CloudStorage.AzureBlobProvider.ConfigureModel.CDNAccount")]
        public string CDNAccount { get; set; }

        [NopResourceDisplayName("DevPartner.CloudStorage.AzureBlobProvider.ConfigureModel.UseCDNForPictures")]
        public bool UseCDNForPictures { get; set; }

        [NopResourceDisplayName("DevPartner.CloudStorage.AzureBlobProvider.ConfigureModel.UseCDNForFiles")]
        public bool UseCDNForFiles { get; set; }

        [NopResourceDisplayName("DevPartner.CloudStorage.AzureBlobProvider.ConfigureModel.UseCDNForContent")]
        public bool UseCDNForContent { get; set; }
        [NopResourceDisplayName("DevPartner.CloudStorage.AzureBlobProvider.ConfigureModel.UseCDNForThumbPictures")]
        public bool UseCDNForThumbPictures { get; set; }
        [NopResourceDisplayName("DevPartner.CloudStorage.AzureBlobProvider.ConfigureModel.ContainerForThumbPictures")]
        public string ContainerForThumbPictures { get; set; }*/
    }
}
