using DevPartner.Nop.Plugin.CloudStorage.Cloud;

namespace DevPartner.Nop.Plugin.CloudStorage.Extensions
{

    public static class CloudStorageProviderExtensions
    {
        public static bool IsNull(this ICloudStorageProvider provider)
        {
            if (provider == null)
                return true;
            var emptyProvider = provider as NullCloudStorageProvider;
            return emptyProvider != null ? true : false;
        }
    }
}
