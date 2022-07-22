using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevPartner.Nop.Plugin.CloudStorage.File
{
    public class FileProviderPlugin : BasePlugin, IMiscPlugin
    {
        #region Consts
        public const string PROVIDER_SYSTEM_NAME = "File";
        public const string COMPONENT_NAME = "FileSettings";
        /// <summary>
        /// The key of the Directory setting to save 
        /// </summary>
        public const string DIRECTORY_FOR_FILES_SETTINGS_KEY = "CloudStorage.File.{0}.DirectoryForFiles";
        #endregion

        #region Fields
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor
        public FileProviderPlugin(ISettingService settingService, 
            ILocalizationService localizationService)
        {
            _settingService = settingService;
            _localizationService = localizationService;
        }
        #endregion

        #region IMiscPlugin
        public override async Task InstallAsync()
        {
            //settings
            var settings = new FileProviderSettings()
            {
                DirectoryForFiles = ""
            };
            await _settingService.SaveSettingAsync(settings);

            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                [ "DevPartner.CloudStorage.FileProvider.SettingsModel.DirectoryForFiles"] = "Directory for files" ,
                [ "DevPartner.CloudStorage.FileProvider.Settings"] = "File settings" ,
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<FileProviderSettings>();

            await _localizationService.DeleteLocaleResourcesAsync("DevPartner.CloudStorage.FileProvider");

            await base.UninstallAsync();
        }
        #endregion
    }
}
