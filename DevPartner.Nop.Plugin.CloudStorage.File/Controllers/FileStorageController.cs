using DevPartner.Nop.Plugin.CloudStorage.File.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.File.Models;
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

namespace DevPartner.Nop.Plugin.CloudStorage.File.Controllers
{
    public class FileStorageController : BasePluginController
    {
        #region Fileds
        private readonly ISettingService _settingService;
        private readonly FileProviderSettings _azureFileProviderSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly FileStorageProviderFactory _blobStorageProviderFactory;
        private readonly INotificationService _notificationService;
        #endregion

        #region Ctor
        public FileStorageController(
            ISettingService settingService,
            FileProviderSettings azureFileProviderSettings,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            FileStorageProviderFactory blobStorageProviderFactory,
            INotificationService notificationService)
        {
            _settingService = settingService;
            _azureFileProviderSettings = azureFileProviderSettings;
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

            var model = new ConfigurationModel { };

            return View("~/Plugins/DevPartner.CloudStorage.File/Views/Configure.cshtml", model);
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> SaveSettings(string directoryForFiles,
            string providerType)
        {
            if (!await _permissionService.AuthorizeAsync("DevPartner.DevCommerce.CloudStorage"))
                return AccessDeniedView();

            //save settings
            await _settingService.SetSettingAsync(String.Format(FileProviderPlugin.DIRECTORY_FOR_FILES_SETTINGS_KEY, providerType), directoryForFiles);

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


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            //save settings
            await _settingService.SaveSettingAsync(_azureFileProviderSettings);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        #endregion
    }
}
