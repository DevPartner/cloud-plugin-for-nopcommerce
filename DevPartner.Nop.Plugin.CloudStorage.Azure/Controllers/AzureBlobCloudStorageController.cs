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
using System.Threading.Tasks;

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
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync("DevPartner.DevCommerce.CloudStorage"))
                return AccessDeniedView();

            var model = new ConfigurationModel { ConnectionString = _azureBlobProviderSettings.ConnectionString };

            return View("~/Plugins/DevPartner.CloudStorage.Azure/Views/Configure.cshtml", model);
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
            await _settingService.SetSettingAsync(String.Format(AzureBlobProviderPlugin.ContainerSettingsKey, providerType), container);
            await _settingService.SetSettingAsync(String.Format(AzureBlobProviderPlugin.EndPointSettingsKey, providerType), endPoint);

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
        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            //save settings
            _azureBlobProviderSettings.ConnectionString = model.ConnectionString;
            await _settingService.SaveSettingAsync(_azureBlobProviderSettings);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        #endregion
    }
}
