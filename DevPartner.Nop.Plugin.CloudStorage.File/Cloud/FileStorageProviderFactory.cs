using System;
using DevPartner.Nop.Plugin.CloudStorage.Attributes;
using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;

namespace DevPartner.Nop.Plugin.CloudStorage.File.Cloud
{
    [SystemName(FileProviderPlugin.ProviderSystemName)]
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
        public ICloudStorageProvider Create(string providerType)
        {
            var fileProvider = EngineContext.Current.Resolve<FileProvider>();
            var directory = _settingService.GetSetting(String.Format(FileProviderPlugin.DirectoryForFilesSettingsKey, providerType));
            if (directory == null)
                throw new Exception("Directory is not specified");

            fileProvider.RunAtAppStartup(directory.Value);
            return fileProvider;
        }
        #endregion
    }
}
