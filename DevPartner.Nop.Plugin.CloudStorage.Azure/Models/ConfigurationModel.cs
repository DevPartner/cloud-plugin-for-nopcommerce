using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace DevPartner.Nop.Plugin.CloudStorage.Azure.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("DevPartner.CloudStorage.AzureBlobProvider.ConfigureModel.ConnectionString")]
        public string ConnectionString { get; set; }
    }
}
