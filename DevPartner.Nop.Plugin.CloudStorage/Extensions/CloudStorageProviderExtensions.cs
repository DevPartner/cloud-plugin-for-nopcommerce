using DevPartner.Nop.Plugin.CloudStorage.Cloud;

namespace DevPartner.Nop.Plugin.CloudStorage.Extensions
{

    public static class CloudStorageProviderExtensions
    {
        public static ICloudStorageProvider IsNotNull(this ICloudStorageProvider provider)
        {
            if (!CloudStoragePlugin.IsActive())
                return null;
            if(!CloudStoragePlugin.IsValidLicense())
                return null;
            var emptyProvider = provider as NullCloudStorageProvider;
            return emptyProvider != null ? null : provider;
        }
    }
}
