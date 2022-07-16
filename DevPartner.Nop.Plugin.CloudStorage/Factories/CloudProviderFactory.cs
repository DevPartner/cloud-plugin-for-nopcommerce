
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevPartner.Nop.Plugin.CloudStorage.Domain;
using DevPartner.Nop.Plugin.CloudStorage.Extensions;
using Microsoft.AspNetCore.Http;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;

namespace DevPartner.Nop.Plugin.CloudStorage.Cloud
{
    public class CloudProviderFactory
    {
        #region Fields
        private readonly ISettingService _settingService;
        private readonly ITypeFinder _typeFinder;
        #endregion

        #region Ctor
        public CloudProviderFactory(ISettingService settingService, ITypeFinder typeFinder)
        {
            _settingService = settingService;
            _typeFinder = typeFinder;
            InitCloudStorageProviderFactories();
        }
        #endregion

        #region Methods
        public async Task<ICloudStorageProvider> Create(string key)
        {
            var setting = await _settingService.GetSettingAsync("DevPartnerCloudStorageSetting." + key);
            var provider = await Create(key, setting != null ?  setting.Value : DPCloudDefaults.NULL_CLOUD_PROVIDER_NAME);
            provider = provider as ICloudStorageProvider;
            return provider;
        }

        public async Task<ICloudStorageProvider> Create(string key, string providerSystemName)
        {
            if (Singleton<Dictionary<string, ICloudStorageProviderFactory>>.Instance.ContainsKey(providerSystemName))
            {
                var factory = await (Singleton<Dictionary<string, ICloudStorageProviderFactory>>.Instance[providerSystemName]).Create(key);
                return factory ?? new NullCloudStorageProvider();
            }
            return new NullCloudStorageProvider();
        }

        public string GetProviderComponentName(string providerSystemName)
        {
            if (Singleton<Dictionary<string, ICloudStorageProviderFactory>>.Instance.ContainsKey(providerSystemName))
            {
                var provider = Singleton<Dictionary<string, ICloudStorageProviderFactory>>.Instance[providerSystemName];
                return provider.GetType().GetComponentName();
            }
            return "";
        }

        public void SaveProviderSettings(string key, string providerSystemName, IFormCollection form)
        {
            if (Singleton<Dictionary<string, ICloudStorageProviderFactory>>.Instance.ContainsKey(providerSystemName))
            {
                var factory = Singleton<Dictionary<string, ICloudStorageProviderFactory>>.Instance[providerSystemName];
                factory.SaveSettings(key, form);
            }
        }

        public Dictionary<string, ICloudStorageProviderFactory> GetAll()
        {
            return Singleton<Dictionary<string, ICloudStorageProviderFactory>>.Instance;
        }
        #endregion

        #region Utils
        private void InitCloudStorageProviderFactories()
        {
            if (Singleton<Dictionary<string, ICloudStorageProviderFactory>>.Instance == null)
            {
                var dict = new Dictionary<string, ICloudStorageProviderFactory>();
                var factories = _typeFinder.FindClassesOfType(typeof(ICloudStorageProviderFactory), false).ToList();
                foreach (var type in factories)
                {
                    var factory = EngineContext.Current.Resolve(type) as ICloudStorageProviderFactory;
                    var systemName = type.GetSystemName();
                    dict.Add(systemName, factory);
                }
                Singleton<Dictionary<string, ICloudStorageProviderFactory>>.Instance = dict;
            }
        }
        #endregion

    }
}