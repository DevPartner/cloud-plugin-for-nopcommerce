using DevPartner.Nop.Plugin.CloudStorage.Amazon.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Amazon.Models;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace DevPartner.Nop.Plugin.CloudStorage.Amazon.Controllers
{
    public class AmazonCloudStorageController : BasePluginController
    {
        #region Fileds
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly BucketStorageProviderFactory _blobStorageProviderFactory;
        private readonly INotificationService _notificationService;
        #endregion

        #region Ctor
        public AmazonCloudStorageController(
            ISettingService settingService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            BucketStorageProviderFactory blobStorageProviderFactory,
            INotificationService notificationService)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _blobStorageProviderFactory = blobStorageProviderFactory;
            _notificationService = notificationService;
        }
        #endregion

        #region Config Actions 
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync("DevPartner.DevCommerce.CloudStorage"))
                return AccessDeniedView();

            var amazonCloudStorageSettings = await _settingService.LoadSettingAsync<AmazonCloudStorageSettings>();

            var model = new ConfigurationModel
            {
                AwsAccessKeyId = amazonCloudStorageSettings.AwsAccessKeyId,
                AwsSecretAccessKey = amazonCloudStorageSettings.AwsSecretAccessKey,
                RegionEndPointSystemName = amazonCloudStorageSettings.RegionEndPointSystemName,
                DomainNameForCDN = amazonCloudStorageSettings.DomainNameForCDN
            };

            return View("~/Plugins/DevPartner.CloudStorage.Amazon/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            var amazonCloudStorageSettings = await _settingService.LoadSettingAsync<AmazonCloudStorageSettings>();

            amazonCloudStorageSettings.AwsAccessKeyId = model.AwsAccessKeyId;
            amazonCloudStorageSettings.AwsSecretAccessKey = model.AwsSecretAccessKey;
            amazonCloudStorageSettings.RegionEndPointSystemName = model.RegionEndPointSystemName;
            amazonCloudStorageSettings.DomainNameForCDN = model.DomainNameForCDN;

            await _settingService.SaveSettingAsync(amazonCloudStorageSettings);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> SaveSettings(string container,
            string endPoint,
            string providerType)
        {
            if (!await _permissionService.AuthorizeAsync("DevPartner.DevCommerce.CloudStorage"))
                return AccessDeniedView();

            //save settings
            await _settingService.SetSettingAsync(string.Format(AmazonCloudStoragePlugin.BucketNameSettingsKey, providerType), container);
            await _settingService.SetSettingAsync(string.Format(AmazonCloudStoragePlugin.EndPointSettingsKey, providerType), endPoint);

            //check settings
            try
            {
                await _blobStorageProviderFactory.Create(providerType);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
            return new NullJsonResult();
        }
        #endregion
    }
}
