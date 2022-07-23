using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace DevPartner.Nop.Plugin.CloudStorage.Azure.Models
{
    public record SettingsModel : BaseNopModel
    {
        [NopResourceDisplayName("DevPartner.CloudStorage.AzureBlobProvider.SettingsModel.Container")]
        public string Container { get; set; }
        [NopResourceDisplayName("DevPartner.CloudStorage.AzureBlobProvider.SettingsModel.EndPoint")]
        public string EndPoint { get; set; }

        public string ProviderType { get; set; }
    }
}
