using DevPartner.Nop.Plugin.CloudStorage.Attributes;
using DevPartner.Nop.Plugin.CloudStorage.Domain;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DevPartner.Nop.Plugin.CloudStorage.Cloud
{
    [SystemName(DPCloudDefaults.NULL_CLOUD_PROVIDER_NAME)]
    public class NullCloudStorageProviderFactory : ICloudStorageProviderFactory
    {

        public async Task<ICloudStorageProvider> Create(string providerType)
        {
            return new NullCloudStorageProvider();
        }

        public async Task SaveSettings(string key, IFormCollection form)
        {
            return;
        }
    }
}
