using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace DevPartner.Nop.Plugin.CloudStorage.Amazon.Models
{
    public record SettingsModel : BaseNopModel
    {
        [NopResourceDisplayName("DevPartner.CloudStorage.AmazonBlobProvider.SettingsModel.BucketName")]
        public string BucketName { get; set; }
        public string ProviderType { get; set; }
    }
}
