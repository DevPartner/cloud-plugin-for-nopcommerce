using DevPartner.Nop.Plugin.CloudStorage.Extensions;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;
using System.Collections.Generic;
using System.Linq;

namespace DevPartner.Nop.Plugin.CloudStorage.File
{
    public class FileProviderPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin
    {
        #region Consts
        public const string ProviderSystemName = "File";
        public const string ComponentName = "FileSettings";
        /// <summary>
        /// The key of the Directory setting to save 
        /// </summary>
        public const string DirectoryForFilesSettingsKey = "CloudStorage.File.{0}.DirectoryForFiles";
        #endregion

        #region Fields

        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly NopConfig _nopConfig;
        private readonly IPluginService _pluginService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor
        public FileProviderPlugin(ISettingService settingService, 
            IWebHelper webHelper, 
            NopConfig nopConfig,
            IPluginService pluginService, 
            ILocalizationService localizationService)
        {
            _settingService = settingService;
            _webHelper = webHelper;
            _nopConfig = nopConfig;
            _pluginService = pluginService;
            _localizationService = localizationService;
        }
        #endregion

        #region IMiscPlugin
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/FileStorage/Configure";
        }

        protected Dictionary<string, string> GetLocalResourceList()
        {
            var result = new Dictionary<string, string>()
            {
                { "DevPartner.CloudStorage.FileProvider.SettingsModel.DirectoryForFiles" , "Directory for files" },
                { "DevPartner.CloudStorage.FileProvider.Settings" , "File settings" },
            };
            return result;
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new FileProviderSettings()
            {
                DirectoryForFiles = ""
            };
            _settingService.SaveSetting(settings);

            GetLocalResourceList().AddOrUpdatePluginLocaleResource();

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<FileProviderSettings>();

            foreach (var localResourse in GetLocalResourceList())
            {
                _localizationService.DeletePluginLocaleResource(localResourse.Key);
            }

            base.Uninstall();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var pluginCloudStorage = _pluginService.GetPluginDescriptorBySystemName<IPlugin>("DevPartner.CloudStorage", LoadPluginsMode.All);
            if (pluginCloudStorage == null || !pluginCloudStorage.Installed)
                return;

            var pluginFile = _pluginService.GetPluginDescriptorBySystemName<IPlugin>("DevPartner.CloudStorage.File", LoadPluginsMode.All);
            if (pluginFile == null || !pluginFile.Installed)
                return;

            var devCommerceNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "DevCommerce");
            if (devCommerceNode != null && devCommerceNode.Visible)
            {
                var cloudNode = devCommerceNode.ChildNodes.FirstOrDefault(x => x.SystemName == "DP_Cloud");
                
                if (cloudNode !=null && cloudNode.Visible)
                {
                    cloudNode.ChildNodes.Add(new SiteMapNode
                    {
                        SystemName = "DP_CloudSettingsFile",
                        Visible = true,
                        Title = _localizationService.GetResource("DevPartner.CloudStorage.FileProvider.Settings"),
                        IconClass = "fa-gears",
                        Url = GetConfigurationPageUrl()
                    });
                }
                
            }
        }

        #endregion

    }
}
