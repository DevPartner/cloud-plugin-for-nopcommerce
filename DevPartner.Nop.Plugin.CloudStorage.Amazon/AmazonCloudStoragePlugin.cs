using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevPartner.Nop.Plugin.CloudStorage.Amazon
{
    public class AmazonCloudStoragePlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin
    {
        #region Consts

        public const string ProviderSystemName = "Amazon";
        public const string ComponentName = "AmazonSettings";

        /// <summary>
        /// The key of the container setting to save 
        /// </summary>
        public const string BucketNameSettingsKey = "CloudStorage.Amazon.{0}.BucketName";

        /// <summary>
        /// The key of the endpoint settingto save 
        /// </summary>
        public const string EndPointSettingsKey = "CloudStorage.Amazon.{0}.EndPoint";

        #endregion

        #region Fields

        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IPluginService _pluginService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor
        public AmazonCloudStoragePlugin(ISettingService settingService,
            IWebHelper webHelper,
            IPluginService pluginService,
            ILocalizationService localizationService)
        {
            _settingService = settingService;
            _webHelper = webHelper;
            _pluginService = pluginService;
            _localizationService = localizationService;
        }
        #endregion

        #region IMiscPlugin
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/AmazonCloudStorage/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override async Task InstallAsync()
        {
            //settings
            var settings = new AmazonCloudStorageSettings()
            {
                //ConnectionString = _nopConfig.AmazonBlobStorageConnectionString
            };

            await _settingService.SaveSettingAsync(settings);

            await _localizationService.AddLocaleResourceAsync(new Dictionary<string, string>
            {
                ["DevPartner.CloudStorage.AmazonBlobProvider.SettingsModel.BucketName"] = "BucketName",
                ["DevPartner.CloudStorage.AmazonBlobProvider.SettingsModel.BucketName.Hint"] = "Enter your Bucket name",
                ["DevPartner.CloudStorage.AmazonBlobProvider.Settings"] = "Amazon settings",
                ["DevPartner.CloudStorage.AmazonBlobProvider.ConfigureModel.AwsAccessKeyId"] = "AWS AccessKeyId",
                ["DevPartner.CloudStorage.AmazonBlobProvider.ConfigureModel.AwsAccessKeyId.Hint"] = "Enter AccessKeyId obtained from Amazon",
                ["DevPartner.CloudStorage.AmazonBlobProvider.ConfigureModel.AwsSecretAccessKey"] = "AWS SecretAccessKey",
                ["DevPartner.CloudStorage.AmazonBlobProvider.ConfigureModel.AwsSecretAccessKey.Hint"] = "Enter SecretAccessKey obtained from Amazon",
                ["DevPartner.CloudStorage.AmazonBlobProvider.ConfigureModel.RegionEndPointSystemName"] = "AWS Endpoint",
                ["DevPartner.CloudStorage.AmazonBlobProvider.ConfigureModel.RegionEndPointSystemName.Hint"] = "Select AWS Endpoint",
                ["DevPartner.CloudStorage.AmazonBlobProvider.ConfigureModel.DomainNameForCDN"] = "CloudFront CDN Domain Name",
                ["DevPartner.CloudStorage.AmazonBlobProvider.ConfigureModel.DomainNameForCDN.Hint"] = "Full domain name and protocol (http/https), eg.: https://uniquename.cloudfront.net. Leave empty to use S3 only."
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<AmazonCloudStorageSettings>();

            await _localizationService.DeleteLocaleResourcesAsync("DevPartner.CloudStorage.AmazonBlobProvider");

            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var pluginCloudStorage = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("DevPartner.CloudStorage", LoadPluginsMode.All);
            if (pluginCloudStorage == null || !pluginCloudStorage.Installed)
                return;

            var pluginAmazon = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("DevPartner.CloudStorage.Amazon", LoadPluginsMode.All);
            if (pluginAmazon == null || !pluginAmazon.Installed)
                return;

            var devCommerceNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "DevCommerce");
            if (devCommerceNode != null && devCommerceNode.Visible)
            {
                var cloudNode = devCommerceNode.ChildNodes.FirstOrDefault(x => x.SystemName == "DP_Cloud");

                if (cloudNode != null && cloudNode.Visible)
                {
                    cloudNode.ChildNodes.Insert(cloudNode.ChildNodes.Count - 1, new SiteMapNode
                    {
                        SystemName = "DP_CloudSettingsAmazon",
                        Visible = true,
                        Title = await _localizationService.GetResourceAsync("DevPartner.CloudStorage.AmazonBlobProvider.Settings"),
                        IconClass = "fa fa-dot-circle",
                        Url = GetConfigurationPageUrl()
                    });
                }

            }
        }

        #endregion

    }
}
