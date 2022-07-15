using DevPartner.Nop.Plugin.CloudStorage.Attributes;
using Microsoft.AspNetCore.Http;

namespace DevPartner.Nop.Plugin.CloudStorage.Cloud
{
    [SystemName(CloudStoragePlugin.NULL_CLOUD_PROVIDER_NAME)]
    public class NullCloudStorageProviderFactory : ICloudStorageProviderFactory
    {
        public ICloudStorageProvider Create(string providerType)
        {
            return new NullCloudStorageProvider();
        }

        public void SaveSettings(string key, IFormCollection form)
        {
        }
    }
}
