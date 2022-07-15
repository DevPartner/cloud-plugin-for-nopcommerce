using System;
using DevPartner.Nop.Plugin.CloudStorage.Attributes;
using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using Microsoft.AspNetCore.Http;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;

namespace DevPartner.Nop.Plugin.CloudStorage.Azure.Cloud
{
    [SystemName(AzureBlobProviderPlugin.ProviderSystemName)]
    public class BlobStorageProviderFactory : ICloudStorageProviderFactory
    {
        #region Fields
        private readonly ISettingService _settingService;
        #endregion

        #region Ctor
        public BlobStorageProviderFactory(ISettingService settingService)
        {
            _settingService = settingService;
        }
        #endregion

        #region Methods
        public ICloudStorageProvider Create(string providerType)
        {
            var blobProvider = EngineContext.Current.Resolve<BlobProvider>();
            var container = _settingService.GetSetting(String.Format(AzureBlobProviderPlugin.ContainerSettingsKey, providerType));
            var endPoint = _settingService.GetSetting(String.Format(AzureBlobProviderPlugin.EndPointSettingsKey, providerType));
            if (container == null)
                throw new Exception("Azure container name for BLOB is not specified");

            if (endPoint == null)
                throw new Exception("Azure end point for BLOB is not specified");

            blobProvider.RunAtAppStartup(container.Value, endPoint.Value);
            return blobProvider;
        }

        public void SaveSettings(string providerType, IFormCollection form)
        {
            var nameContainer = providerType + ".Azure." + "Container";
            var nameEndPoint = providerType + ".Azure." + "EndPoint";

            if (form.ContainsKey(nameContainer))
            {
                var container = form[nameContainer].ToString();
                _settingService.SetSetting(String.Format(AzureBlobProviderPlugin.ContainerSettingsKey, providerType), container);
            }

            if (form.ContainsKey(nameEndPoint))
            {
                var endPoint = form[nameEndPoint].ToString();
                _settingService.SetSetting(String.Format(AzureBlobProviderPlugin.EndPointSettingsKey, providerType), endPoint);
            }
        }
        #endregion
    }
}
