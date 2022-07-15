using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace DevPartner.Nop.Plugin.CloudStorage.Models
{
    public class MovingItemModel : BaseNopModel
    {
        [NopResourceDisplayName("DevPartner.CloudStorage.MovingItemModel.Id")]
        public int Id { get; set; }

        [NopResourceDisplayName("DevPartner.CloudStorage.MovingItemModel.Item")]
        public string Item { get; set; }

        [NopResourceDisplayName("DevPartner.CloudStorage.MovingItemModel.StoreType")]
        public string StoreType { get; set; }

        [NopResourceDisplayName("DevPartner.CloudStorage.MovingItemModel.Status")]
        public string Status { get; set; }

        [NopResourceDisplayName("DevPartner.CloudStorage.MovingItemModel.CreatedOnUtc")]
        public string CreatedOnUtc { get; set; }

        [NopResourceDisplayName("DevPartner.CloudStorage.MovingItemModel.ChangedOnUtc")]
        public string ChangedOnUtc { get; set; }
    }
}
