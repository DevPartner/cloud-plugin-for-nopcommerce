using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace DevPartner.Nop.Plugin.CloudStorage.Models
{
    public class MediaModel : BaseNopModel
    {
        [NopResourceDisplayName("DevPartner.CloudStorage.ConfigureModel.PictureStoreType")]
        public string PictureStoreType { get; set; }
    }
}
