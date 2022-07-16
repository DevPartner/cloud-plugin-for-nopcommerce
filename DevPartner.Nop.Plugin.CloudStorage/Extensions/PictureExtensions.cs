using Nop.Core.Domain.Media;

namespace DevPartner.Nop.Plugin.CloudStorage.Extensions
{

    public static class PictureExtensions
    {
        public static bool IsStoreInProvider(this Picture picture)
        {
            return picture != null /*&& 
                (picture.PictureBinary == null || 
                (picture.PictureBinary != null && picture.PictureBinary.BinaryData == null) || 
                (picture.PictureBinary != null && picture.PictureBinary.BinaryData != null && picture.PictureBinary.BinaryData.Length == 0))*/
                ;
        }
    }
}
