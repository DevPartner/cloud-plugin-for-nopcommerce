using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Domain;
using DevPartner.Nop.Plugin.CloudStorage.Models;
using DevPartner.Nop.Plugin.CloudStorage.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace DevPartner.Nop.Plugin.CloudStorage.Controllers
{
    public class MiscCloudStorageController : BasePluginController
    {
        #region Fileds
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly MovingItemService _movingService;
        private readonly CloudDownloadService _downloadService;
        /*        private readonly StoreContentService _storeContentService;*/
        private readonly IWebHelper _webHelper;
        private readonly CloudProviderFactory _cloudProviderFactory;
        private readonly ISettingModelFactory _settingModelFactory;
        private readonly INotificationService _notificationService;
        private readonly CloudPictureService _pictureService;
        #endregion

        #region Ctor
        public MiscCloudStorageController(ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            ISettingService settingService,
            IPermissionService permissiionService,
            CloudPictureService pictureService,
            MovingItemService movingService,
            IWebHelper webHelper,
            CloudProviderFactory cloudProviderFactory,
            CloudDownloadService downloadService,
            ISettingModelFactory settingModelFactory,
            INotificationService notificationService)
        {
            _settingService = settingService;
            _permissionService = permissiionService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _pictureService = pictureService;
            _downloadService = downloadService;
            /*_storeContentService = storeContentService;*/
            _movingService = movingService;
            _webHelper = webHelper;
            _cloudProviderFactory = cloudProviderFactory;
            _settingModelFactory = settingModelFactory;
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

            var pluginSetting = await _settingService.LoadSettingAsync<DevPartnerCloudStorageSetting>();
            var model = new ConfigurationModel
            {
                PictureStoreType = pluginSetting.PictureStoreType,
                //ThumbPictureStoreType = pluginSetting.ThumbPictureStoreType,
                DownloadStoreType = pluginSetting.DownloadStoreType,
                ContentStoreType = pluginSetting.ContentStoreType,
                AlwaysShowMainImage = pluginSetting.AlwaysShowMainImage,
                CheckIfImageExist = pluginSetting.CheckIfImageExist,
                StoreImageInDb = pluginSetting.StoreImageInDb,
                ArchiveDownloads = pluginSetting.ArchiveDownloads,
                LicenseKey = pluginSetting.LicenseKey,
                LicenseValid = true
            };

            return View("~/Plugins/DevPartner.CloudStorage/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure(ConfigurationModel model, IFormCollection form)
        {
            if (!ModelState.IsValid)
                return await Configure();
            if (!await _permissionService.AuthorizeAsync("DevPartner.DevCommerce.CloudStorage"))
                return AccessDeniedView();

            var pluginSetting = await _settingService.LoadSettingAsync<DevPartnerCloudStorageSetting>();
            pluginSetting.AlwaysShowMainImage = model.AlwaysShowMainImage;
            pluginSetting.CheckIfImageExist = model.CheckIfImageExist;
            pluginSetting.ArchiveDownloads = model.ArchiveDownloads;
            pluginSetting.StoreImageInDb = model.StoreImageInDb;
            if (pluginSetting.PictureStoreType != model.PictureStoreType)
            {
                SaveProviderSettings(DPCloudDefaults.PICTURE_PROVIDER_TYPE_NAME, model.PictureStoreType, form);
                if (!ValidateProvider(DPCloudDefaults.PICTURE_PROVIDER_TYPE_NAME, model.PictureStoreType))
                    return await Configure();
            }
            
            if (pluginSetting.ContentStoreType != model.ContentStoreType)
            {
                SaveProviderSettings(DPCloudDefaults.CONTENT_PROVIDER_TYPE_NAME, model.ContentStoreType, form);
                if (!ValidateProvider(DPCloudDefaults.CONTENT_PROVIDER_TYPE_NAME, model.ContentStoreType))
                    return await Configure();
            }
            if (pluginSetting.DownloadStoreType != model.DownloadStoreType)
            {
                SaveProviderSettings(DPCloudDefaults.DOWNLOAD_PROVIDER_TYPE_NAME, model.DownloadStoreType, form);
                if (!ValidateProvider(DPCloudDefaults.DOWNLOAD_PROVIDER_TYPE_NAME, model.DownloadStoreType))
                    return await Configure();
            }
            await MovePictureStorageFilesAsync(model, pluginSetting);
            await MoveDownloadStorageFilesAsync(model, pluginSetting);

            pluginSetting.PictureStoreType = model.PictureStoreType;
            pluginSetting.DownloadStoreType = model.DownloadStoreType;
            pluginSetting.ContentStoreType = model.ContentStoreType;
            pluginSetting.LicenseKey = model.LicenseKey;

            await _settingService.SaveSettingAsync(pluginSetting);

            CloudHelper.FileProvider = await _cloudProviderFactory
                        .Create(DPCloudDefaults.PICTURE_PROVIDER_TYPE_NAME);
            CloudHelper.DownloadProvider = await _cloudProviderFactory
                    .Create(DPCloudDefaults.DOWNLOAD_PROVIDER_TYPE_NAME);

            //_movingService.StartWorklflow();
            await _customerActivityService.InsertActivityAsync("EditSettings",
                            await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
            return await Configure();
        }

        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> GetSettings(string type, string providerSystemName)
        {
            var component = _cloudProviderFactory.GetProviderComponentName(providerSystemName);
            if (!String.IsNullOrEmpty(component))
                return ViewComponent(component, new { type = type });
            return Content("");
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("change-pictures-storage")]
        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> ChangePictureStorage(ConfigurationModel model)
        {
            var pluginSetting = await _settingService.LoadSettingAsync<DevPartnerCloudStorageSetting>();
            await MovePictureStorageFilesAsync(model, pluginSetting);
            pluginSetting.PictureStoreType = model.PictureStoreType;
            await _settingService.SaveSettingAsync(pluginSetting);
            _webHelper.RestartAppDomain();
            //_movingService.StartWorklflow();

            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("DevPartner.CloudStorage.ConfigureModel.ChangePicturesStorage"));
            return await Configure();
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("change-files-storage")]
        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> ChangeFilesStorage(ConfigurationModel model)
        {
            var pluginSetting = await _settingService.LoadSettingAsync<DevPartnerCloudStorageSetting>();
            await MoveDownloadStorageFilesAsync(model, pluginSetting);
            pluginSetting.DownloadStoreType = model.DownloadStoreType;
            await _settingService.SaveSettingAsync(pluginSetting);
            _webHelper.RestartAppDomain();
            //_movingService.StartWorklflow();
            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("DevPartner.CloudStorage.ConfigureModel.ChangeFilesStorage"));
            return await Configure();
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("change-content-storage")]
        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> ChangeContentStorage(ConfigurationModel model)
        {

            /*if (_movingFileService.AllItemsMoved())
                _storeContentService.StoreType = model.ContentStoreType;
            else
                ErrorNotification(await _localizationService.GetResourceAsync("DevPartner.CloudStorage.ConfigureModel.FileMovingInProgress"));

            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("DevPartner.CloudStorage.ConfigureModel.ChangeContentStorage"));*/
            return await Configure();
        }

        #endregion
        /*
        #region Log

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public ActionResult LogItemList(DataSourceRequest command, LogFilterModel model)
        {
            var listStatuses = new List<MovingItemStatus>();
            if (model.ShowPending)
                listStatuses.Add(MovingItemStatus.Pending);
            if (model.ShowProcessing)
                listStatuses.Add(MovingItemStatus.Processing);
            if (model.ShowSucceed)
                listStatuses.Add(MovingItemStatus.Succeed);
            if (model.ShowFailed)
                listStatuses.Add(MovingItemStatus.Failed);

            var listTypes = new List<MovingItemTypes>();
            if (model.ShowPictures)
                listTypes.Add(MovingItemTypes.Picture);
            if (model.ShowDownloads)
                listTypes.Add(MovingItemTypes.Download);
            if (model.ShowFiles)
                listTypes.Add(MovingItemTypes.File);
            var movingItems = new List<MovingItem>();
            //_movingService.GetLogItems(listStatuses, listTypes, command.Page - 1, command.PageSize);

            var movingItemModels = movingItems.Select(x => x.ToModel()).ToList();

            var gridModel = new DataSourceResult
            {
                Data = movingItemModels,
                Total = movingItemModels.Count()
            };

            return Json(gridModel);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public ActionResult Log()
        {
            var model = new LogFilterModel()
            {
                ShowPictures = true,
                ShowDownloads = true,
                ShowFiles = true,
                ShowPending = true,
                ShowProcessing = true,
                ShowSucceed = false,
                ShowFailed = true
            };

            return PartialView("~/Plugins/DevPartner.CloudStorage/Views/Log.cshtml", model);
        }

        [HttpPost]
        public ActionResult AjaxClearLog(bool clearPictures, bool clearDownloads, bool clearFiles)
        {
           
            return new NullJsonResult();
        }

        #endregion
        */
        /*
        #region MediaMethods

        public ActionResult Change()
        {
            return PartialView("~/Plugins/DevPartner.CloudStorage/Views/Change.cshtml");
        }

        public async Task<IActionResult> Media()
        {

            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var model = _settingModelFactory.PrepareMediaSettingsModel();
            return View("~/Plugins/DevPartner.CloudStorage/Views/Media.cshtml", model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> Media(MediaSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var shoppingCartSettings = await _settingService.LoadSettingAsync<MediaSettings>(storeScope);
            var mediaSettings = model.ToSettings(shoppingCartSettings);

            if (model.AvatarPictureSize_OverrideForStore || storeScope == 0)
                await _settingService.SaveSettingAsync(mediaSettings, x => x.AvatarPictureSize, storeScope, false);
            else if (storeScope > 0)
                await _settingService.DeleteSettingAsync(mediaSettings, x => x.AvatarPictureSize, storeScope);

            if (model.ProductThumbPictureSize_OverrideForStore || storeScope == 0)
                await _settingService.SaveSettingAsync(mediaSettings, x => x.ProductThumbPictureSize, storeScope, false);
            else if (storeScope > 0)
                await _settingService.DeleteSettingAsync(mediaSettings, x => x.ProductThumbPictureSize, storeScope);

            if (model.ProductDetailsPictureSize_OverrideForStore || storeScope == 0)
                await _settingService.SaveSettingAsync(mediaSettings, x => x.ProductDetailsPictureSize, storeScope, false);
            else if (storeScope > 0)
                await _settingService.DeleteSettingAsync(mediaSettings, x => x.ProductDetailsPictureSize, storeScope);

            if (model.ProductThumbPictureSizeOnProductDetailsPage_OverrideForStore || storeScope == 0)
                await _settingService.SaveSettingAsync(mediaSettings, x => x.ProductThumbPictureSizeOnProductDetailsPage,
                    storeScope, false);
            else if (storeScope > 0)
                await _settingService.DeleteSettingAsync(mediaSettings, x => x.ProductThumbPictureSizeOnProductDetailsPage,
                    storeScope);

            if (model.AssociatedProductPictureSize_OverrideForStore || storeScope == 0)
                await _settingService.SaveSettingAsync(mediaSettings, x => x.AssociatedProductPictureSize, storeScope, false);
            else if (storeScope > 0)
                await _settingService.DeleteSettingAsync(mediaSettings, x => x.AssociatedProductPictureSize, storeScope);

            if (model.CategoryThumbPictureSize_OverrideForStore || storeScope == 0)
                await _settingService.SaveSettingAsync(mediaSettings, x => x.CategoryThumbPictureSize, storeScope, false);
            else if (storeScope > 0)
                await _settingService.DeleteSettingAsync(mediaSettings, x => x.CategoryThumbPictureSize, storeScope);

            if (model.ManufacturerThumbPictureSize_OverrideForStore || storeScope == 0)
                await _settingService.SaveSettingAsync(mediaSettings, x => x.ManufacturerThumbPictureSize, storeScope, false);
            else if (storeScope > 0)
                await _settingService.DeleteSettingAsync(mediaSettings, x => x.ManufacturerThumbPictureSize, storeScope);

            if (model.CartThumbPictureSize_OverrideForStore || storeScope == 0)
                await _settingService.SaveSettingAsync(mediaSettings, x => x.CartThumbPictureSize, storeScope, false);
            else if (storeScope > 0)
                await _settingService.DeleteSettingAsync(mediaSettings, x => x.CartThumbPictureSize, storeScope);

            if (model.MiniCartThumbPictureSize_OverrideForStore || storeScope == 0)
                await _settingService.SaveSettingAsync(mediaSettings, x => x.MiniCartThumbPictureSize, storeScope, false);
            else if (storeScope > 0)
                await _settingService.DeleteSettingAsync(mediaSettings, x => x.MiniCartThumbPictureSize, storeScope);

            if (model.MaximumImageSize_OverrideForStore || storeScope == 0)
                await _settingService.SaveSettingAsync(mediaSettings, x => x.MaximumImageSize, storeScope, false);
            else if (storeScope > 0)
                await _settingService.DeleteSettingAsync(mediaSettings, x => x.MaximumImageSize, storeScope);

            await _settingService.ClearCacheAsync();

            await _customerActivityService.InsertActivityAsync("EditSettings",
                await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
            return RedirectToAction("Media");
        }

        [HttpPost, ActionName("Media")]
        [FormValueRequired("change-storage")]
        public ActionResult ChangeInformation(MediaModel model)
        {
            //if (_movingPictureService.AllItemsMoved())
            //    (_pictureService as StorePictureService).StoreType = model.PictureStoreType;
            //else
            //    ErrorNotification(await _localizationService.GetResourceAsync("DevPartner.CloudStorage.ConfigureModel.PictureMovingInProgress"));
                
            await _customerActivityService.InsertActivityAsync("EditSettings",
                await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));


            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Media");
        }

        #endregion
        */
        #region Methods

        private bool SaveProviderSettings(string providerType, string provider, IFormCollection form)
        {
            try
            {
                _cloudProviderFactory.SaveProviderSettings(providerType, provider, form);
            }
            catch (Exception ex)
            {
                _notificationService.ErrorNotification(ex.Message);
                return false;
            }
            return true;
        }

        private bool ValidateProvider(string providerType, string provider)
        {
            try
            {
                _cloudProviderFactory.Create(providerType, provider);
            }
            catch (Exception ex)
            {
                _notificationService.ErrorNotification(ex.Message);
                return false;
            }
            return true;
        }

        private async Task MovePictureStorageFilesAsync(ConfigurationModel model, DevPartnerCloudStorageSetting pluginSetting)
        {
            if (_movingService.AllItemsMoved(MovingItemTypes.Picture))
            {
                if (!await _pictureService.IsStoreInDbAsync() && pluginSetting.PictureStoreType == DPCloudDefaults.NULL_CLOUD_PROVIDER_NAME)
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("DevPartner.CloudStorage.ConfigureModel.ErrorNotification.StoreInDB"));
                    return;
                }
                if (pluginSetting.PictureStoreType != model.PictureStoreType)
                {
                    var picturesIds = await _pictureService.GetPicturesIdsAsync();
                    await _movingService.SaveAsync(picturesIds, MovingItemTypes.Picture, pluginSetting.PictureStoreType);
                }
            }
            else
                _notificationService.ErrorNotification(
                        await _localizationService.GetResourceAsync("DevPartner.CloudStorage.ConfigureModel.PictureMovingInProgress"));
        }

        private async Task MoveDownloadStorageFilesAsync(ConfigurationModel model, DevPartnerCloudStorageSetting pluginSetting)
        {
            if (_movingService.AllItemsMoved(MovingItemTypes.Download))
            {
                if (pluginSetting.DownloadStoreType != model.DownloadStoreType)
                {
                    //update all picture objects
                    var downloadsIds = await _downloadService.GetDownloadsIdsAsync();
                    await _movingService.SaveAsync(downloadsIds, MovingItemTypes.Download, pluginSetting.DownloadStoreType);
                }
            }
            else
                _notificationService.ErrorNotification(
                    await _localizationService.GetResourceAsync("DevPartner.CloudStorage.ConfigureModel.PictureMovingInProgress"));
        }


        private async Task MoveContentStorageFiles(ConfigurationModel model, DevPartnerCloudStorageSetting pluginSetting)
        {
            /*if (_movingService.AllItemsMoved(MovingItemTypes.File))
            {
                if (pluginSetting.ContentStoreType != model.ContentStoreType)
                {
                    var files = _storeContentService.ListDirTree();
                    _movingService.Save(files, MovingItemTypes.File, pluginSetting.ContentStoreType);
                }
            }
            else*/
            _notificationService.ErrorNotification(
                await _localizationService.GetResourceAsync("DevPartner.CloudStorage.ConfigureModel.FileMovingInProgress"));
        }

        #endregion
    }
}
