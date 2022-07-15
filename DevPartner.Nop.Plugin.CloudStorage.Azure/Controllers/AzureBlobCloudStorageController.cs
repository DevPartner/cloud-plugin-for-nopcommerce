using DevPartner.Nop.Plugin.CloudStorage.Azure.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Azure.Models;
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

namespace DevPartner.Nop.Plugin.CloudStorage.Azure.Controllers
{
    public class AzureBlobCloudStorageController : BasePluginController
    {
        #region Fileds
        private readonly ISettingService _settingService;
        private readonly AzureBlobProviderSettings _azureBlobProviderSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly BlobStorageProviderFactory _blobStorageProviderFactory;
        private readonly INotificationService _notificationService;
        #endregion

        #region Ctor
        public AzureBlobCloudStorageController(
            ISettingService settingService,
            AzureBlobProviderSettings azureBlobProviderSettings,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            BlobStorageProviderFactory blobStorageProviderFactory,
            INotificationService notificationService)
        {
            _settingService = settingService;
            _azureBlobProviderSettings = azureBlobProviderSettings;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _blobStorageProviderFactory = blobStorageProviderFactory;
            _notificationService = notificationService;
        }
        #endregion

        #region Config Actions 

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize("DevPartner.DevCommerce.CloudStorage"))
                return AccessDeniedView();

            var model = new ConfigurationModel { ConnectionString = _azureBlobProviderSettings.ConnectionString };

            return View("~/Plugins/DevPartner.CloudStorage.Azure/Views/Configure.cshtml", model);
        }


        [HttpPost]
        [AdminAntiForgery]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult SaveSettings(string container,
            string endPoint,
            string providerType)
        {
            if (!_permissionService.Authorize("DevPartner.DevCommerce.CloudStorage"))
                return AccessDeniedView();

            //save settings
            _settingService.SetSetting(String.Format(AzureBlobProviderPlugin.ContainerSettingsKey, providerType), container);
            _settingService.SetSetting(String.Format(AzureBlobProviderPlugin.EndPointSettingsKey, providerType), endPoint);

            //check settings
            try
            {
                _blobStorageProviderFactory.Create(providerType);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
            return new NullJsonResult();
        }


        [HttpPost]
        [AdminAntiForgery]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _azureBlobProviderSettings.ConnectionString = model.ConnectionString;
            _settingService.SaveSetting(_azureBlobProviderSettings);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        #endregion
    }
}
