using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace DevPartner.Nop.Plugin.CloudStorage.Amazon.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("DevPartner.CloudStorage.AmazonBlobProvider.ConfigureModel.AwsAccessKeyId")]
        public string AwsAccessKeyId { get; set; }

        [NopResourceDisplayName("DevPartner.CloudStorage.AmazonBlobProvider.ConfigureModel.AwsSecretAccessKey")]
        public string AwsSecretAccessKey { get; set; }

        [NopResourceDisplayName("DevPartner.CloudStorage.AmazonBlobProvider.ConfigureModel.RegionEndPointSystemName")]
        public string RegionEndPointSystemName { get; set; }
        [NopResourceDisplayName("DevPartner.CloudStorage.AmazonBlobProvider.ConfigureModel.DomainNameForCDN")]
        public string DomainNameForCDN { get; set; }
    }
}
