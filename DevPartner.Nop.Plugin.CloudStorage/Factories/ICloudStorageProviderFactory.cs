using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DevPartner.Nop.Plugin.CloudStorage.Cloud
{
    /// <summary>
    /// Cloud Storage Provider Factory
    /// </summary>
    public interface ICloudStorageProviderFactory
    {
        /// <summary>
        /// Create Provider
        /// </summary>
        /// <returns>return provider or error message</returns>
        Task<ICloudStorageProvider> Create(string providerType);
        Task SaveSettings(string key, IFormCollection form);
    }
}
