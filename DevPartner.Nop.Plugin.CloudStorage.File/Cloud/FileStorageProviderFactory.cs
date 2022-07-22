using DevPartner.Nop.Plugin.CloudStorage.Attributes;
using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using Microsoft.AspNetCore.Http;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using System;
using System.Threading.Tasks;

namespace DevPartner.Nop.Plugin.CloudStorage.File.Cloud
{
    [SystemName(FileProviderPlugin.PROVIDER_SYSTEM_NAME)]
    [ComponentNameAttribute(FileProviderPlugin.COMPONENT_NAME)]
    public class FileStorageProviderFactory : ICloudStorageProviderFactory
    {
        #region Fields
        private readonly ISettingService _settingService;
        #endregion

        #region Ctor
        public FileStorageProviderFactory(ISettingService settingService)
        {
            _settingService = settingService;
        }
        #endregion

        #region Methods
        public async Task<ICloudStorageProvider> Create(string providerType)
        {
            var fileProvider = EngineContext.Current.Resolve<FileProvider>();
            var directory = await _settingService.GetSettingAsync(String.Format(FileProviderPlugin.DIRECTORY_FOR_FILES_SETTINGS_KEY, providerType));
            if (directory == null)
                throw new Exception("Directory is not specified");

            fileProvider.RunAtAppStartup(directory.Value);
            return fileProvider;
        }

        public async Task SaveSettings(string providerType, IFormCollection form)
        {
            var nameDir = providerType + ".File." + "DirectoryForFiles";

            if (form.ContainsKey(nameDir))
            {
                var dir = form[nameDir].ToString();
                await _settingService.SetSettingAsync(String.Format(FileProviderPlugin.DIRECTORY_FOR_FILES_SETTINGS_KEY, providerType), dir);
            }
        }
        #endregion
    }
}
