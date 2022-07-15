using BasePicture = Nop.Core.Domain.Media.Picture;

namespace DevPartner.Nop.Plugin.CloudStorage.Domain
{
    public partial class PictureExt : BasePicture
    {
        /// <summary>
        /// Gets or sets the PictureId
        /// </summary>
        public virtual string ExternalUrl { get; set; }
    }
}
