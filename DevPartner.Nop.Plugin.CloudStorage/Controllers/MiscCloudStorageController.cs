using Autofac.Features.Metadata;
using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Domain;
using DevPartner.Nop.Plugin.CloudStorage.Extensions;
using DevPartner.Nop.Plugin.CloudStorage.Kendoui;
using DevPartner.Nop.Plugin.CloudStorage.Models;
using DevPartner.Nop.Plugin.CloudStorage.Services;
using DevPartner.Nop.Plugin.CloudStorage.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Settings;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevPartner.Nop.Plugin.CloudStorage.Controllers
{
    public class MiscCloudStorageController : BasePluginController
    {
        #region Fileds
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly StoreDownloadService _downloadService;
        private readonly MovingItemService _movingService;
        private readonly StoreContentService _storeContentService;
        private readonly IWebHelper _webHelper;
        private readonly IEnumerable<Meta<ICloudStorageProviderFactory>> _cloudStorageProviders;
        private readonly CloudProviderFactory _cloudProviderFactory;
        private readonly ISettingModelFactory _settingModelFactory;
        private readonly INotificationService _notificationService;

        #endregion

        #region Ctor
        public MiscCloudStorageController(ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            IPermissionService permissiionService,
            StoreDownloadService downloadService,
            StoreContentService storeContentService,
            MovingItemService movingService,
            IWebHelper webHelper,
            IEnumerable<Meta<ICloudStorageProviderFactory>> cloudStorageProviders,
            CloudProviderFactory cloudProviderFactory,
            ISettingModelFactory settingModelFactory,
            INotificationService notificationService)
        {
            _settingService = settingService;
            _storeService = storeService;
            _permissionService = permissiionService;
            _workContext = workContext;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _downloadService = downloadService;
            _storeContentService = storeContentService;
            _movingService = movingService;
            _webHelper = webHelper;
            _cloudStorageProviders = cloudStorageProviders;
            _cloudProviderFactory = cloudProviderFactory;
            _settingModelFactory = settingModelFactory;
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

            var pluginSetting = _settingService.LoadSetting<DevPartnerCloudStorageSetting>();
            var model = new ConfigurationModel
            {
                PictureStoreType = pluginSetting.PictureStoreType,
                ThumbPictureStoreType = pluginSetting.ThumbPictureStoreType,
                DownloadStoreType = pluginSetting.DownloadStoreType,
                ContentStoreType = pluginSetting.ContentStoreType,
                AlwaysShowMainImage = pluginSetting.AlwaysShowMainImage,
                MainImageUrlFormat = pluginSetting.MainImageUrlFormat,
                CheckIfImageExist = pluginSetting.CheckIfImageExist,
                StoreImageInDb = pluginSetting.StoreImageInDb,
                ArchiveDownloads = pluginSetting.ArchiveDownloads,
                LicenseKey = pluginSetting.LicenseKey,
                LicenseValid = true
                //LicenseValid = CloudStoragePlugin.IsValidLicense(pluginSetting.LicenseKey)
            };
            if (!model.LicenseValid)
            {

                var httpContextAccessor = EngineContext.Current.Resolve<IHttpContextAccessor>();
                var host = httpContextAccessor.HttpContext.Request.Host.Host;
      
                var ip = NetworkUtil.GetIPAddress().ToString();

                _notificationService.ErrorNotification(String.Format(_localizationService.GetResource("DevPartner.CloudStorage.LicenseKey.Invalid"), host, ip));
            }
            return View("~/Plugins/DevPartner.CloudStorage/Views/Configure.cshtml", model);
        }
        
        [HttpPost]
        [AdminAntiForgery]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model, IFormCollection form)
        {
            if (!ModelState.IsValid)
                return Configure();
            if (!_permissionService.Authorize("DevPartner.DevCommerce.CloudStorage"))
                return AccessDeniedView();

            var pluginSetting = _settingService.LoadSetting<DevPartnerCloudStorageSetting>();
            pluginSetting.AlwaysShowMainImage = model.AlwaysShowMainImage;
            pluginSetting.MainImageUrlFormat = model.MainImageUrlFormat;
            pluginSetting.CheckIfImageExist = model.CheckIfImageExist;
            pluginSetting.ArchiveDownloads = model.ArchiveDownloads;
            pluginSetting.StoreImageInDb = model.StoreImageInDb;
            if (pluginSetting.PictureStoreType != model.PictureStoreType)
            {
                SaveProviderSettings(CloudStoragePlugin.PICTURE_PROVIDER_TYPE_NAME, model.PictureStoreType, form);
                if (!ValidateProvider(CloudStoragePlugin.PICTURE_PROVIDER_TYPE_NAME, model.PictureStoreType))
                    return Configure();
            }
            if (pluginSetting.ThumbPictureStoreType != model.ThumbPictureStoreType)
            {
                SaveProviderSettings(CloudStoragePlugin.THUMB_PICTURE_PROVIDER_TYPE_NAME, model.ThumbPictureStoreType, form);
                if (!ValidateProvider(CloudStoragePlugin.THUMB_PICTURE_PROVIDER_TYPE_NAME, model.ThumbPictureStoreType))
                    return Configure();
            }
            if (pluginSetting.ContentStoreType != model.ContentStoreType)
            {
                SaveProviderSettings(CloudStoragePlugin.CONTENT_PROVIDER_TYPE_NAME, model.ContentStoreType, form);
                if (!ValidateProvider(CloudStoragePlugin.CONTENT_PROVIDER_TYPE_NAME, model.ContentStoreType))
                    return Configure();
            }
            if (pluginSetting.DownloadStoreType != model.DownloadStoreType)
            {
                SaveProviderSettings(CloudStoragePlugin.DOWNLOAD_PROVIDER_TYPE_NAME, model.DownloadStoreType, form);
                if (!ValidateProvider(CloudStoragePlugin.DOWNLOAD_PROVIDER_TYPE_NAME, model.DownloadStoreType))
                    return Configure();
            }
            MovePictureStorageFiles(model, pluginSetting);
            MoveDownloadStorageFiles(model, pluginSetting);
 
            pluginSetting.PictureStoreType = model.PictureStoreType;
            pluginSetting.ThumbPictureStoreType = model.ThumbPictureStoreType;
            pluginSetting.DownloadStoreType = model.DownloadStoreType;
            pluginSetting.ContentStoreType = model.ContentStoreType;
            pluginSetting.LicenseKey = model.LicenseKey;

            _settingService.SaveSetting(pluginSetting);

            _webHelper.RestartAppDomain();
            //_movingService.StartWorklflow();
            _customerActivityService.InsertActivity("EditSettings",
                            _localizationService.GetResource("ActivityLog.EditSettings"));
            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return Configure();
        }

        [AdminAntiForgery]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult GetSettings(string type, string providerSystemName)
        {
            var provider = _cloudStorageProviders
                .FirstOrDefault(a => a.Metadata["SystemName"].Equals(providerSystemName));
            var component = provider?.GetComponentSettings(type);
            if(!String.IsNullOrEmpty(component))
                return ViewComponent(component, new {type = type});
            return Content("");
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("change-pictures-storage")]
        [AdminAntiForgery]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult ChangePictureStorage(ConfigurationModel model)
        {
            var pluginSetting = _settingService.LoadSetting<DevPartnerCloudStorageSetting>();
            MovePictureStorageFiles(model, pluginSetting);
            pluginSetting.PictureStoreType = model.PictureStoreType;
            _settingService.SaveSetting(pluginSetting);
            _webHelper.RestartAppDomain();
            _movingService.StartWorklflow();

            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));
            _notificationService.SuccessNotification(_localizationService.GetResource("DevPartner.CloudStorage.ConfigureModel.ChangePicturesStorage"));
            return Configure();
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("change-files-storage")]
        [AdminAntiForgery]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult ChangeFilesStorage(ConfigurationModel model)
        {
            var pluginSetting = _settingService.LoadSetting<DevPartnerCloudStorageSetting>();
            MoveDownloadStorageFiles(model, pluginSetting);
            pluginSetting.DownloadStoreType = model.DownloadStoreType;
            _settingService.SaveSetting(pluginSetting);
            _webHelper.RestartAppDomain();
            _movingService.StartWorklflow();
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));
            _notificationService.SuccessNotification(_localizationService.GetResource("DevPartner.CloudStorage.ConfigureModel.ChangeFilesStorage"));
            return Configure();
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("change-content-storage")]
        [AdminAntiForgery]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult ChangeContentStorage(ConfigurationModel model)
        {
            /*
            if (_movingFileService.AllItemsMoved())
                _storeContentService.StoreType = model.ContentStoreType;
            else
                ErrorNotification(_localizationService.GetResource("DevPartner.CloudStorage.ConfigureModel.FileMovingInProgress"));
                */
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));
            _notificationService.SuccessNotification(_localizationService.GetResource("DevPartner.CloudStorage.ConfigureModel.ChangeContentStorage"));
            return Configure();
        }

        #endregion

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
            var movingItems = _movingService.GetLogItems(listStatuses, listTypes, command.Page - 1, command.PageSize);

            var movingItemModels = movingItems.Select(x => x.ToModel()).ToList();

            var gridModel = new DataSourceResult
            {
                Data = movingItemModels,
                Total = movingItemModels.Count
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
            if (clearPictures)
                _movingService.ClearSucceedRecords(MovingItemTypes.Picture);

            if (clearDownloads)
                _movingService.ClearSucceedRecords(MovingItemTypes.Download);

            if (clearFiles)
                _movingService.ClearSucceedRecords(MovingItemTypes.File);

            return new NullJsonResult();
        }

        #endregion

        #region MediaMethods

        public ActionResult Change()
        {
            return PartialView("~/Plugins/DevPartner.CloudStorage/Views/Change.cshtml");
        }

        public IActionResult Media()
        {

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var model = _settingModelFactory.PrepareMediaSettingsModel();
            return View("~/Plugins/DevPartner.CloudStorage/Views/Media.cshtml", model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult Media(MediaSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();
            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var shoppingCartSettings = _settingService.LoadSetting<MediaSettings>(storeScope);
            var mediaSettings = model.ToSettings(shoppingCartSettings);

            if (model.AvatarPictureSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mediaSettings, x => x.AvatarPictureSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mediaSettings, x => x.AvatarPictureSize, storeScope);

            if (model.ProductThumbPictureSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mediaSettings, x => x.ProductThumbPictureSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mediaSettings, x => x.ProductThumbPictureSize, storeScope);

            if (model.ProductDetailsPictureSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mediaSettings, x => x.ProductDetailsPictureSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mediaSettings, x => x.ProductDetailsPictureSize, storeScope);

            if (model.ProductThumbPictureSizeOnProductDetailsPage_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mediaSettings, x => x.ProductThumbPictureSizeOnProductDetailsPage,
                    storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mediaSettings, x => x.ProductThumbPictureSizeOnProductDetailsPage,
                    storeScope);

            if (model.AssociatedProductPictureSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mediaSettings, x => x.AssociatedProductPictureSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mediaSettings, x => x.AssociatedProductPictureSize, storeScope);

            if (model.CategoryThumbPictureSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mediaSettings, x => x.CategoryThumbPictureSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mediaSettings, x => x.CategoryThumbPictureSize, storeScope);

            if (model.ManufacturerThumbPictureSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mediaSettings, x => x.ManufacturerThumbPictureSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mediaSettings, x => x.ManufacturerThumbPictureSize, storeScope);

            if (model.CartThumbPictureSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mediaSettings, x => x.CartThumbPictureSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mediaSettings, x => x.CartThumbPictureSize, storeScope);

            if (model.MiniCartThumbPictureSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mediaSettings, x => x.MiniCartThumbPictureSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mediaSettings, x => x.MiniCartThumbPictureSize, storeScope);

            if (model.MaximumImageSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mediaSettings, x => x.MaximumImageSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mediaSettings, x => x.MaximumImageSize, storeScope);

            _settingService.ClearCache();

            _customerActivityService.InsertActivity("EditSettings",
                _localizationService.GetResource("ActivityLog.EditSettings"));
            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Media");
        }

        [HttpPost, ActionName("Media")]
        [FormValueRequired("change-storage")]
        public ActionResult ChangeInformation(MediaModel model)
        {
            /*if (_movingPictureService.AllItemsMoved())
                (_pictureService as StorePictureService).StoreType = model.PictureStoreType;
            else
                ErrorNotification(_localizationService.GetResource("DevPartner.CloudStorage.ConfigureModel.PictureMovingInProgress"));
                */
            _customerActivityService.InsertActivity("EditSettings",
                _localizationService.GetResource("ActivityLog.EditSettings"));


            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));

            return RedirectToAction("Media");
        }

        #endregion

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


        private void MovePictureStorageFiles(ConfigurationModel model, DevPartnerCloudStorageSetting pluginSetting)
        {
            if (_movingService.AllItemsMoved(MovingItemTypes.Picture))
            {

                if (pluginSetting.PictureStoreType != model.PictureStoreType)
                {
                    _movingService.SaveAllPictures(pluginSetting.PictureStoreType);
                }
            }
            else
                _notificationService.ErrorNotification(
                    _localizationService.GetResource("DevPartner.CloudStorage.ConfigureModel.PictureMovingInProgress"));
        }

        private void MoveDownloadStorageFiles(ConfigurationModel model, DevPartnerCloudStorageSetting pluginSetting)
        {
            if (_movingService.AllItemsMoved(MovingItemTypes.Download))
            {
                if (pluginSetting.DownloadStoreType != model.DownloadStoreType)
                {
                    //update all picture objects
                    var downloads = _downloadService.GetDownloads();
                    var ids = downloads.Select(x => x.Id);
                    _movingService.Save(ids, MovingItemTypes.Download, pluginSetting.DownloadStoreType);
                }
            }
            else
                _notificationService.ErrorNotification(
                    _localizationService.GetResource("DevPartner.CloudStorage.ConfigureModel.PictureMovingInProgress"));
        }


        private void MoveContentStorageFiles(ConfigurationModel model, DevPartnerCloudStorageSetting pluginSetting)
        {
            if (_movingService.AllItemsMoved(MovingItemTypes.File))
            {
                if (pluginSetting.ContentStoreType != model.ContentStoreType)
                {
                    var files = _storeContentService.ListDirTree();
                    _movingService.Save(files, MovingItemTypes.File, pluginSetting.ContentStoreType);
                }
            }
            else
                _notificationService.ErrorNotification(
                    _localizationService.GetResource("DevPartner.CloudStorage.ConfigureModel.FileMovingInProgress"));
        }

        #endregion
    }
}
