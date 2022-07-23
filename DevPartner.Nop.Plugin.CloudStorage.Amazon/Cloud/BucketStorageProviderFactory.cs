using DevPartner.Nop.Plugin.CloudStorage.Attributes;
using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using Microsoft.AspNetCore.Http;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using System;
using System.Threading.Tasks;

namespace DevPartner.Nop.Plugin.CloudStorage.Amazon.Cloud
{
    [SystemName(AmazonCloudStoragePlugin.ProviderSystemName)]
    [ComponentNameAttribute(AmazonCloudStoragePlugin.ComponentName)]
    public class BucketStorageProviderFactory : ICloudStorageProviderFactory
    {
        #region Fields
        private readonly ISettingService _settingService;
        #endregion

        #region Ctor
        public BucketStorageProviderFactory(ISettingService settingService)
        {
            _settingService = settingService;
        }
        #endregion

        #region Methods
        public async Task<ICloudStorageProvider> Create(string providerType)
        {
            var backetProvider = EngineContext.Current.Resolve<BucketProvider>();
            
            var backet = await _settingService.GetSettingAsync(String.Format(AmazonCloudStoragePlugin.BucketNameSettingsKey, providerType));
            
            if (backet == null)
                throw new Exception("Amazon backet name is not specified");

            backetProvider.RunAtAppStartup(backet.Value);
           
            return backetProvider;
        }

        public async Task SaveSettings(string providerType, IFormCollection form)
        {
            var nameBacket = providerType + ".Amazon.Bucket";

            if (form.ContainsKey(nameBacket))
            {
                var backet = form[nameBacket].ToString();
                await _settingService.SetSettingAsync(String.Format(AmazonCloudStoragePlugin.BucketNameSettingsKey, providerType), backet);
            }
        }
        #endregion
    }
}
