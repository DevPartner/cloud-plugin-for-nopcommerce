using System.Linq;

namespace DevPartner.Nop.Plugin.CloudStorage.Extensions
{

    public static class StringExtensions
    {
        public static string GetFileExtension(this string filePath)
        {
            return $".{filePath.Split(".".ToCharArray()).Last().ToLower().Trim()}";
        }

        public static bool IsZip(this string extension)
        {
            return extension == ".zip";
        }
        public static string GetMimeFromExtension(this string filePath)
        {
            var fileExt = GetFileExtension(filePath);
            switch (fileExt)
            {
                case ".css":
                    return "text/css";
                case ".js":
                    return "application/javascript";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                default:
                    //return "text/plain";
                    return "application/octet-stream";
            }
        }

    }
}
