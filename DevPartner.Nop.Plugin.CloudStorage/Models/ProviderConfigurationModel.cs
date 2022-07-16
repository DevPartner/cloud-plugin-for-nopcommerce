using Nop.Web.Framework.Models;

namespace DevPartner.Nop.Plugin.CloudStorage.Models
{
    public partial record ProviderConfigurationModel : BaseNopModel
    {
        public string ProviderSystemName { get; set; }

        public string ProviderConfigurationControllerName { get; set; }

        public string ProviderConfigurationActionName { get; set; }
    }
}
