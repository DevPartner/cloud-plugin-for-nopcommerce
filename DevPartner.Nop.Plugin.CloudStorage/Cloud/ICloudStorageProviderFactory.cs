using System.Collections.Generic;
using DevPartner.Nop.Plugin.CloudStorage.Domain;
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
        ICloudStorageProvider Create(string providerType);
        void SaveSettings(string key, IFormCollection form);
    }
}
