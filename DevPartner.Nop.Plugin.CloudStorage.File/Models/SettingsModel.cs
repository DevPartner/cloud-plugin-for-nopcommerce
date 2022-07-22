using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace DevPartner.Nop.Plugin.CloudStorage.File.Models
{
    public record SettingsModel : BaseNopModel
    {
        [NopResourceDisplayName("DevPartner.CloudStorage.FileProvider.SettingsModel.DirectoryForFiles")]
        public string DirectoryForFiles { get; set; }

        public string ProviderType { get; set; }
    }
}
