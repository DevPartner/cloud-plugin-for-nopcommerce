using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace DevPartner.Nop.Plugin.CloudStorage.Models
{
    public class LogFilterModel : BaseNopModel
    {
        [NopResourceDisplayName("DevPartner.CloudStorage.LogFilterModel.ShowPictures")]
        public bool ShowPictures { get; set; }

        [NopResourceDisplayName("DevPartner.CloudStorage.LogFilterModel.ShowDownloads")]
        public bool ShowDownloads { get; set; }

        [NopResourceDisplayName("DevPartner.CloudStorage.LogFilterModel.ShowFiles")]
        public bool ShowFiles { get; set; }


        [NopResourceDisplayName("DevPartner.CloudStorage.LogFilterModel.ShowPending")]
        public bool ShowPending { get; set; }

        [NopResourceDisplayName("DevPartner.CloudStorage.LogFilterModel.ShowProcessing")]
        public bool ShowProcessing { get; set; }

        [NopResourceDisplayName("DevPartner.CloudStorage.LogFilterModel.ShowSucceed")]
        public bool ShowSucceed { get; set; }

        [NopResourceDisplayName("DevPartner.CloudStorage.LogFilterModel.ShowFailed")]
        public bool ShowFailed { get; set; }
    }
}
