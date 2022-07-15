using System.Collections.Generic;
using System.Linq;
using Autofac.Features.Metadata;
using Microsoft.AspNetCore.Http;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;

namespace DevPartner.Nop.Plugin.CloudStorage.Cloud
{
    public class CloudProviderFactory
    {
        private readonly ISettingService _settingService;

        public CloudProviderFactory(ISettingService settingService)
        {
            _settingService = settingService;
        }
        public ICloudStorageProvider Create(string key)
        {
            var setting = _settingService.GetSetting("DevPartnerCloudStorageSetting." + key);
            var provider = Create(key, setting != null ?  setting.Value : CloudStoragePlugin.NULL_CLOUD_PROVIDER_NAME);
            provider = provider as ICloudStorageProvider;
            return provider;
        }

        public ICloudStorageProvider Create(string key, string providerSystemName)
        {
            var provider = EngineContext.Current.Resolve<IEnumerable<Meta<ICloudStorageProviderFactory>>>()
                .FirstOrDefault(a => a.Metadata["SystemName"].Equals(providerSystemName));
            return provider?.Value.Create(key) ?? new NullCloudStorageProvider();
        }

        public void SaveProviderSettings(string key, string providerSystemName, IFormCollection form)
        {
            var provider = EngineContext.Current.Resolve<IEnumerable<Meta<ICloudStorageProviderFactory>>>()
                .FirstOrDefault(a => a.Metadata["SystemName"].Equals(providerSystemName));
            provider?.Value.SaveSettings(key, form);
        }
    }
}