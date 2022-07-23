using Nop.Core;
using Nop.Core.Configuration;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevPartner.Nop.Plugin.CloudStorage.Azure
{
    public class AzureBlobProviderPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin
    {
        #region Consts

        public const string ProviderSystemName = "Azure";
        public const string ComponentName = "AzureSettings";

        /// <summary>
        /// The key of the container setting to save 
        /// </summary>
        public const string ContainerSettingsKey = "CloudStorage.Azure.{0}.Container";

        /// <summary>
        /// The key of the endpoint settingto save 
        /// </summary>
        public const string EndPointSettingsKey = "CloudStorage.Azure.{0}.EndPoint";

        #endregion

        #region Fields

        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly AppSettings _appSettings;
        private readonly IPluginService _pluginService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor
        public AzureBlobProviderPlugin(ISettingService settingService,
            IWebHelper webHelper,
            AppSettings appSettings,
            IPluginService pluginService,
            ILocalizationService localizationService)
        {
            _settingService = settingService;
            _webHelper = webHelper;
            _appSettings = appSettings;
            _pluginService = pluginService;
            _localizationService = localizationService;
        }
        #endregion

        #region IMiscPlugin
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/AzureBlobCloudStorage/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override async Task InstallAsync()
        {
            //settings
            var settings = new AzureBlobProviderSettings()
            {
                ConnectionString = _appSettings.Get<AzureBlobConfig>().ConnectionString
            };
            await _settingService.SaveSettingAsync(settings);

            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["DevPartner.CloudStorage.AzureBlobProvider.ConfigureModel.ConnectionString"] = "Connection String",
                ["DevPartner.CloudStorage.AzureBlobProvider.ConfigureModel.ConnectionString.Hint"] = "Specify your connection string for BLOB storage here",
                ["DevPartner.CloudStorage.AzureBlobProvider.SettingsModel.Container"] = "Container",
                ["DevPartner.CloudStorage.AzureBlobProvider.SettingsModel.Container.Hint"] = "Enter your container name",
                ["DevPartner.CloudStorage.AzureBlobProvider.SettingsModel.EndPoint"] = "End point",
                ["DevPartner.CloudStorage.AzureBlobProvider.SettingsModel.EndPoint.Hint"] = "Enter your end point for BLOB storage here",
                ["DevPartner.CloudStorage.AzureBlobProvider.Settings"] = "Azure settings"
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<AzureBlobProviderSettings>();

            await _localizationService.DeleteLocaleResourcesAsync("DevPartner.CloudStorage.AzureBlobProvider");

            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var pluginCloudStorage = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("DevPartner.CloudStorage", LoadPluginsMode.All);
            if (pluginCloudStorage == null || !pluginCloudStorage.Installed)
                return;

            var pluginAzure = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("DevPartner.CloudStorage.Azure", LoadPluginsMode.All);
            if (pluginAzure == null || !pluginAzure.Installed)
                return;

            var devCommerceNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "DevCommerce");
            if (devCommerceNode != null && devCommerceNode.Visible)
            {
                var cloudNode = devCommerceNode.ChildNodes.FirstOrDefault(x => x.SystemName == "DP_Cloud");

                if (cloudNode != null && cloudNode.Visible)
                {
                    cloudNode.ChildNodes.Insert(cloudNode.ChildNodes.Count - 1, new SiteMapNode
                    {
                        SystemName = "DP_CloudSettingsAzure",
                        Visible = true,
                        Title = await _localizationService.GetResourceAsync("DevPartner.CloudStorage.AzureBlobProvider.Settings"),
                        IconClass = "fa fa-dot-circle",
                        Url = GetConfigurationPageUrl()
                    });
                }

            }
        }

        #endregion
    }
}
