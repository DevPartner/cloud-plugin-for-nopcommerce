using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace DevPartner.Nop.Plugin.CloudStorage.Models
{
    public partial record MediaModel : BaseNopModel
    {
        [NopResourceDisplayName("DevPartner.CloudStorage.ConfigureModel.PictureStoreType")]
        public string PictureStoreType { get; set; }
    }
}
