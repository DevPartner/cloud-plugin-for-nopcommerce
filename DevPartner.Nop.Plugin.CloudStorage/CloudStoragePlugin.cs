using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Data;
using DevPartner.Nop.Plugin.CloudStorage.Extensions;
using DevPartner.Nop.Plugin.CloudStorage.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Tasks;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Tasks;
using Nop.Web.Framework.Menu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DevPartner.Nop.Plugin.CloudStorage
{
    public class CloudStoragePlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin
    {
        #region Consts

        public static string SCHEDULE_TASKS_TYPE =
            "DevPartner.Nop.Plugin.CloudStorage.Infrastructure.UpdateTask";

        public const string NULL_CLOUD_PROVIDER_NAME = "nopCommerce";
        public const string PICTURE_PROVIDER_TYPE_NAME = "PictureStoreType";
        public const string THUMB_PICTURE_PROVIDER_TYPE_NAME = "ThumbPictureStoreType";
        public const string DOWNLOAD_PROVIDER_TYPE_NAME = "DownloadStoreType";
        public const string CONTENT_PROVIDER_TYPE_NAME = "ContentStoreType";

        private const int SCHEDULE_TASKS_SECONDS = 60;

        /// <summary>
        /// Path to Roxy_Fileman configuration file
        /// </summary>
        private const string CONFIGURATION_FILE = "/lib/Roxy_Fileman/conf.json";

        /// <summary>
        /// Path to Roxy_Fileman backup configuration file
        /// </summary>
        private const string BACKUP_CONFIGURATION_FILE = "/lib/Roxy_Fileman/nop-conf.json";
        #endregion

        #region Fields
        private readonly ISettingService _settingService;
        private readonly PictureFileObjectContext _pictureFileObjectContext;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly FileCloudStorageProvider _fileCloudStorageProvider;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IWorkContext _workContext;
        private readonly IPluginService _pluginService;
        #endregion

        #region Ctor
        public CloudStoragePlugin(ISettingService settingService,
            PictureFileObjectContext pictureFileObjectContext,
            IScheduleTaskService scheduleTaskService,
            IWebHelper webHelper,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            FileCloudStorageProvider fileCloudStorageProvider,
            IHostingEnvironment hostingEnvironment,
            IWorkContext workContext,
            IPluginService pluginService
            )
        {
            _settingService = settingService;
            _pictureFileObjectContext = pictureFileObjectContext;
            _scheduleTaskService = scheduleTaskService;
            _webHelper = webHelper;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _fileCloudStorageProvider = fileCloudStorageProvider;
            _hostingEnvironment = hostingEnvironment;
            _workContext = workContext;
            _pluginService = pluginService;
        }
        #endregion

        #region IMiscPlugin
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/MiscCloudStorage/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            GetLocalResourcesList().AddOrUpdatePluginLocaleResource();

            //settings
            var settings = new DevPartnerCloudStorageSetting
            {
                AlwaysShowMainImage = false,
                MainImageUrlFormat = "{0}",
                ArchiveDownloads = false,
                CheckIfImageExist = false,
                StoreImageInDb = false,
                UseDPImportManager = true,
                PictureStoreType = NULL_CLOUD_PROVIDER_NAME,
                DownloadStoreType = NULL_CLOUD_PROVIDER_NAME,
                ContentStoreType = NULL_CLOUD_PROVIDER_NAME,
                ThumbPictureStoreType = NULL_CLOUD_PROVIDER_NAME
            };

            //SetDPRoxySettings();

            _settingService.SaveSetting(settings);

            _pictureFileObjectContext.Install();

            _scheduleTaskService.InsertTasksIfDoesntExist(GetScheduleTaskList());

            _permissionService.Save("DevPartner.DevCommerce", "DevPartner.DevCommerce: Admin area.", "DevPartner.DevCommerce");
            _permissionService.Save("DevPartner.DevCommerce.CloudStorage", "DevPartner.DevCommerce.CloudStorage: Admin area.", "DevPartner.DevCommerce");

            base.Install();
        }


        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            RestoreNopRoxySettings();

            foreach (var localResourse in GetLocalResourcesList())
            {
                _localizationService.DeletePluginLocaleResource(localResourse.Key);
            }

            GetScheduleTaskList().ForEach(task =>
            {
                var scheduleTask = _scheduleTaskService.GetAllTasks().FirstOrDefault(t => t.Name == task.Name);
                if (scheduleTask != null)
                    _scheduleTaskService.DeleteTask(scheduleTask);
            });

            base.Uninstall();
        }
        #endregion

        #region  IAdminMenuPlugin
        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var pluginCloudStorage = _pluginService.GetPluginDescriptorBySystemName<IPlugin>("DevPartner.CloudStorage", LoadPluginsMode.All);
            if (pluginCloudStorage == null || !pluginCloudStorage.Installed)
                return;

            var devCommerceNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "DevCommerce");
            if (devCommerceNode == null)
            {
                devCommerceNode = new SiteMapNode
                {
                    Title = "DevCommerce",
                    SystemName = "DevCommerce",
                    IconClass = "fa fa-random",
                    Visible = _permissionService.Authorize("DevPartner.DevCommerce"),
                };

                rootNode.ChildNodes.Add(devCommerceNode);
            }
            if (devCommerceNode.Visible)
            {
                var pluginCMS = _pluginService.GetPluginDescriptorBySystemName<IPlugin>("DevPartner.CMS", LoadPluginsMode.All);
                if (pluginCMS == null)
                {
                    var cmsNode = new SiteMapNode
                    {
                        Visible = true,
                        Title = "CMS",
                        SystemName = "DP_CMS",
                        IconClass = "fa fa-cog",
                        Url = "http://dev-partner.biz/nopcommerce-cms"
                    };

                    devCommerceNode.ChildNodes.Add(cmsNode);
                }

                var cloudNode = new SiteMapNode
                {
                    Visible = true,
                    Title = "Cloud",
                    SystemName = "DP_Cloud",
                    IconClass = "fa fa-cloud-download",
                };

                devCommerceNode.ChildNodes.Add(cloudNode);
                cloudNode.Visible = _permissionService.Authorize("DevPartner.DevCommerce.CloudStorage");

                if (cloudNode.Visible)
                {
                    cloudNode.ChildNodes.Add(new SiteMapNode
                    {
                        SystemName = "DP_CloudSettings",
                        Visible = true,
                        Title = _localizationService.GetResource("DevPartner.Cloud.Configure.Settings"),
                        IconClass = "fa-gears",
                        Url = GetConfigurationPageUrl()
                    });

                    cloudNode.ChildNodes.Add(new SiteMapNode
                    {
                        SystemName = "DP_CloudLog",
                        Visible = true,
                        Title = _localizationService.GetResource("DevPartner.CloudStorage.LogFilterModel.Log"),
                        IconClass = "fa fa-dot-circle-o",
                        Url = _webHelper.GetStoreLocation() + "Admin/MiscCloudStorage/Log"
                    });
					
					cloudNode.ChildNodes.Add(new SiteMapNode
                    {
                        Visible = true,
                        IconClass = "fa fa-dot-circle-o",
                        Title = _localizationService.GetResource("DevPartner.CloudStorage.ForumSupport"),
                        Url = "http://dev-partner.biz/boards/forum/5/cloud-storage-plugin"
                    });
                }

                var pluginSearch = _pluginService.GetPluginDescriptorBySystemName<IPlugin>("DevPartner.Search", LoadPluginsMode.All);
                if (pluginSearch == null)
                {
                    var searchNode = new SiteMapNode
                    {
                        Visible = true,
                        Title = "Search",
                        SystemName = "DP_Search",
                        IconClass = "fa fa-search",
                        Url = "http://dev-partner.biz/nopcommerce-search-solr-plugin"
                    };

                    devCommerceNode.ChildNodes.Add(searchNode);
                }

                var pluginSync = _pluginService.GetPluginDescriptorBySystemName<IPlugin>("DevPartner.Sync", LoadPluginsMode.All);
                if (pluginSync == null)
                {
                    var syncNode = new SiteMapNode
                    {
                        Visible = true,
                        Title = "Sync",
                        SystemName = "DP_Sync",
                        IconClass = "fa fa-plug",
                        Url = "http://dev-partner.biz/nopcommerce-sync-plugin"
                    };

                    devCommerceNode.ChildNodes.Add(syncNode);
                }
            }
        }
        #endregion

        #region Methods

        private void RestoreNopRoxySettings()
        {
            var filePath = _hostingEnvironment.WebRootPath + CONFIGURATION_FILE;
            var backupConfFile = _hostingEnvironment.WebRootPath + BACKUP_CONFIGURATION_FILE;
            _fileCloudStorageProvider.DeleteFile(filePath);
            _fileCloudStorageProvider.CopyFile(backupConfFile, filePath);
        }

        private void SetDPRoxySettings()
        {
            var filePath = _hostingEnvironment.WebRootPath + CONFIGURATION_FILE;
            var backupConfFile = _hostingEnvironment.WebRootPath + BACKUP_CONFIGURATION_FILE;
            _fileCloudStorageProvider.CopyFile(filePath, backupConfFile);


            //try to read existing configuration
            var existingText = File.ReadAllText(filePath);
            var existingConfiguration = JsonConvert.DeserializeAnonymousType(existingText, new
            {
                FILES_ROOT = string.Empty,
                SESSION_PATH_KEY = string.Empty,
                THUMBS_VIEW_WIDTH = string.Empty,
                THUMBS_VIEW_HEIGHT = string.Empty,
                PREVIEW_THUMB_WIDTH = string.Empty,
                PREVIEW_THUMB_HEIGHT = string.Empty,
                MAX_IMAGE_WIDTH = string.Empty,
                MAX_IMAGE_HEIGHT = string.Empty,
                DEFAULTVIEW = string.Empty,
                FORBIDDEN_UPLOADS = string.Empty,
                ALLOWED_UPLOADS = string.Empty,
                FILEPERMISSIONS = string.Empty,
                DIRPERMISSIONS = string.Empty,
                LANG = string.Empty,
                DATEFORMAT = string.Empty,
                OPEN_LAST_DIR = string.Empty,
                INTEGRATION = string.Empty,
                RETURN_URL_PREFIX = string.Empty,
                DIRLIST = string.Empty,
                CREATEDIR = string.Empty,
                DELETEDIR = string.Empty,
                MOVEDIR = string.Empty,
                COPYDIR = string.Empty,
                RENAMEDIR = string.Empty,
                FILESLIST = string.Empty,
                UPLOAD = string.Empty,
                DOWNLOAD = string.Empty,
                DOWNLOADDIR = string.Empty,
                DELETEFILE = string.Empty,
                MOVEFILE = string.Empty,
                COPYFILE = string.Empty,
                RENAMEFILE = string.Empty,
                GENERATETHUMB = string.Empty,
            });


            //create configuration
            var configuration = new
            {
                FILES_ROOT = existingConfiguration?.FILES_ROOT ?? "/images/uploaded",
                SESSION_PATH_KEY = existingConfiguration?.SESSION_PATH_KEY ?? string.Empty,
                THUMBS_VIEW_WIDTH = existingConfiguration?.THUMBS_VIEW_WIDTH ?? "140",
                THUMBS_VIEW_HEIGHT = existingConfiguration?.THUMBS_VIEW_HEIGHT ?? "120",
                PREVIEW_THUMB_WIDTH = existingConfiguration?.PREVIEW_THUMB_WIDTH ?? "300",
                PREVIEW_THUMB_HEIGHT = existingConfiguration?.PREVIEW_THUMB_HEIGHT ?? "200",
                MAX_IMAGE_WIDTH = existingConfiguration?.MAX_IMAGE_WIDTH ?? "1000",
                MAX_IMAGE_HEIGHT = existingConfiguration?.MAX_IMAGE_HEIGHT ?? "1000",
                DEFAULTVIEW = existingConfiguration?.DEFAULTVIEW ?? "list",
                FORBIDDEN_UPLOADS = existingConfiguration?.FORBIDDEN_UPLOADS ?? "zip js jsp jsb mhtml mht xhtml xht php phtml " +
                                    "php3 php4 php5 phps shtml jhtml pl sh py cgi exe application gadget hta cpl msc jar vb jse ws wsf wsc wsh " +
                                    "ps1 ps2 psc1 psc2 msh msh1 msh2 inf reg scf msp scr dll msi vbs bat com pif cmd vxd cpl htpasswd htaccess",
                ALLOWED_UPLOADS = existingConfiguration?.ALLOWED_UPLOADS ?? string.Empty,
                FILEPERMISSIONS = existingConfiguration?.FILEPERMISSIONS ?? "0644",
                DIRPERMISSIONS = existingConfiguration?.DIRPERMISSIONS ?? "0755",
                LANG = existingConfiguration?.LANG ?? _workContext.WorkingLanguage.UniqueSeoCode,
                DATEFORMAT = existingConfiguration?.DATEFORMAT ?? "dd/MM/yyyy HH:mm",
                OPEN_LAST_DIR = existingConfiguration?.OPEN_LAST_DIR ?? "yes",

                //no need user to configure
                INTEGRATION = "tinymce4",
                RETURN_URL_PREFIX = "",
                DIRLIST = $"/Admin/DPRoxyFileman/ProcessRequest?a=DIRLIST",
                CREATEDIR = $"/Admin/DPRoxyFileman/ProcessRequest?a=CREATEDIR",
                DELETEDIR = $"/Admin/DPRoxyFileman/ProcessRequest?a=DELETEDIR",
                MOVEDIR = $"/Admin/DPRoxyFileman/ProcessRequest?a=MOVEDIR",
                COPYDIR = $"/Admin/DPRoxyFileman/ProcessRequest?a=COPYDIR",
                RENAMEDIR = $"/Admin/DPRoxyFileman/ProcessRequest?a=RENAMEDIR",
                FILESLIST = $"/Admin/DPRoxyFileman/ProcessRequest?a=FILESLIST",
                UPLOAD = $"/Admin/DPRoxyFileman/ProcessRequest?a=UPLOAD",
                DOWNLOAD = $"/Admin/DPRoxyFileman/ProcessRequest?a=DOWNLOAD",
                DOWNLOADDIR = $"/Admin/DPRoxyFileman/ProcessRequest?a=DOWNLOADDIR",
                DELETEFILE = $"/Admin/DPRoxyFileman/ProcessRequest?a=DELETEFILE",
                MOVEFILE = $"/Admin/DPRoxyFileman/ProcessRequest?a=MOVEFILE",
                COPYFILE = $"/Admin/DPRoxyFileman/ProcessRequest?a=COPYFILE",
                RENAMEFILE = $"/Admin/DPRoxyFileman/ProcessRequest?a=RENAMEFILE",
                GENERATETHUMB = $"/Admin/DPRoxyFileman/ProcessRequest?a=GENERATETHUMB",
            };

            //save the file
            var text = JsonConvert.SerializeObject(configuration, Formatting.Indented);
            File.WriteAllText(filePath, text);
        }

        private List<ScheduleTask> GetScheduleTaskList()
        {
            var tasks = new List<ScheduleTask>
            {
                //pictures
                new ScheduleTask
                {
                    Name = "DevPartner CloudStorage: Move Entities",
                    Type = SCHEDULE_TASKS_TYPE,
                    Seconds = SCHEDULE_TASKS_SECONDS,
                    Enabled = false,
                    StopOnError = false,
                },
            };

            return tasks;
        }

        protected static Dictionary<string, string> GetLocalResourcesList()
        {
            var result = new Dictionary<string, string>
            {
                {"DevPartner.Cloud.NopProvider", "Nop Provider"},
				{"DevPartner.CloudStorage.ForumSupport", "Forum support"},
                {"DevPartner.CloudStorage.Configure.SettingsSaved", "Settings Saved" },
                {"DevPartner.CloudStorage.ConfigureModel.ButtonSave", "Save"},
                {"DevPartner.CloudStorage.ConfigureModel.Database", "Database"},
                {"DevPartner.CloudStorage.ConfigureModel.FileSystem", "File system"},
                {"DevPartner.CloudStorage.ConfigureModel.Note.PictureStoreType", "NOTE: Do not forget to backup your database and files before changing this option"},
                {"DevPartner.CloudStorage.ConfigureModel.Note.ThumbPictureStoreType", "NOTE: Do not forget to backup your database before changing this option"},
                {"DevPartner.CloudStorage.ConfigureModel.Note.DownloadStoreType", "NOTE: Do not forget to backup your database before changing this option"},
                {"DevPartner.CloudStorage.ConfigureModel.Note.ContentStoreType", "NOTE: Do not forget to backup your files before changing this option"},
                {"DevPartner.CloudStorage.ConfigureModel.PictureStoreType", "Pictures are stored into..."},
                {"DevPartner.CloudStorage.ConfigureModel.PictureStoreType.Hint", "Chose pictures storage type"},
                {"DevPartner.CloudStorage.ConfigureModel.ThumbPictureStoreType", "Thumb pictures are stored into..." },
                {"DevPartner.CloudStorage.ConfigureModel.ThumbPictureStoreType.Hint", "Chose thumb pictures storage type" },
                {"DevPartner.CloudStorage.ConfigureModel.DownloadStoreType", "Files are stored into..."},
                {"DevPartner.CloudStorage.ConfigureModel.DownloadStoreType.Hint", "Chose files storage type"},
                {"DevPartner.CloudStorage.ConfigureModel.ContentStoreType", "Content are stored into..."},
                {"DevPartner.CloudStorage.ConfigureModel.ContentStoreType.Hint", "Chose content(js, css, images) storage type"},
                {"DevPartner.CloudStorage.ConfigureModel.ChangeStorage", "Change"},
                {"DevPartner.CloudStorage.ConfigureModel.ChangeFilesStorage", "Files storage has been changed successfully."},
                {"DevPartner.CloudStorage.ConfigureModel.ChangePicturesStorage", "Pictures storage has been changed successfully."},
                {"DevPartner.CloudStorage.ConfigureModel.ChangeContentStorage", "Content storage has been changed successfully."},
                {"DevPartner.CloudStorage.ConfigureModel.AlwaysShowMainImage", "Always show main image"},
                {"DevPartner.CloudStorage.ConfigureModel.AlwaysShowMainImage.Hint", "Please check this checkbox if you want to improve performance. Plugin will not generate thumbnails and will return main image"},
                {"DevPartner.CloudStorage.ConfigureModel.MainImageUrlFormat", "Main Image Url Format"},
                {"DevPartner.CloudStorage.ConfigureModel.MainImageUrlFormat.Hint", "\"{0}?w={1}\", 0 - URL, 1 - targetSize" },
                              
                {"DevPartner.CloudStorage.ConfigureModel.CheckIfImageExist", "Check if image exist"},
                {"DevPartner.CloudStorage.ConfigureModel.CheckIfImageExist.Hint", "Please uncheck this checkbox if you want to improve performance. Plugin will not check existence of images and image thumbnails"},
                {"DevPartner.CloudStorage.ConfigureModel.ArchiveDownloads", "Save downloads as .zip archives"},
                {"DevPartner.CloudStorage.ConfigureModel.ArchiveDownloads.Hint", "Save downloads as .zip archives"},
                {"DevPartner.CloudStorage.ConfigureModel.ValidateSettings", "Storage settings are empty"},
                {"DevPartner.CloudStorage.ConfigureModel.PictureMovingInProgress", "Could not change picture storage type before the moving complited"},
                {"DevPartner.CloudStorage.ConfigureModel.DownloadMovingInProgress", "Could not change download storage type before the moving complited"},
                {"DevPartner.CloudStorage.ConfigureModel.FileMovingInProgress", "Could not change file storage type before the moving complited"},
                {"DevPartner.CloudStorage.ConfigureModel.StoreImageInDb", "Store Images In DB" },
                {"DevPartner.CloudStorage.ConfigureModel.StoreImageInDb.Hint", "Store Images In DB" },
                {"DevPartner.CloudStorage.ConfigureModel.LicenseKey", "License Key" },
                {"DevPartner.CloudStorage.ConfigureModel.LicenseKey.Hint", "Enter license key here" },
                {"DevPartner.CloudStorage.ConfigureModel.Note.LicenseKey", "You can buy this product <a href='http://shop.dev-partner.biz/nopcommerce-cloud-storage-azure-blobcdn-amazon-s3cloudfront-plugin'>here</a>"},
                {"DevPartner.CloudStorage.Configure.FailedToSave", "Failed to save" },

                {"DevPartner.CloudStorage.FileProvider.CopyFileInvalisPath","Cannot copy file - path doesn't exist"},
                {"DevPartner.CloudStorage.FileProvider.CopyFile","Error copying file"},
                {"DevPartner.CloudStorage.FileProvider.DeleteFile", "Delete file"},
                {"DevPartner.CloudStorage.FileProvider.DeleteFileInvalidPath", "Cannot delete file - path doesn't exist"},

                { "DevPartner.Cloud.Configure.Settings", "Cloud Settings" },
                {"DevPartner.Cloud.Configure.Settings.BackToList", "back to cloud settings" },
                {"DevPartner.CloudStorage.LogFilterModel.Log", "Cloud Log"},
                {"DevPartner.CloudStorage.LogFilterModel.Items", "Items to show"},
                {"DevPartner.CloudStorage.LogFilterModel.ShowPictures", "Show Pictures to move"},
                {"DevPartner.CloudStorage.LogFilterModel.ShowPictures.Hint", "Show Pictures to move."},
                {"DevPartner.CloudStorage.LogFilterModel.ShowDownloads", "Show Downloads to move"},
                {"DevPartner.CloudStorage.LogFilterModel.ShowDownloads.Hint", "Show Downloads to move."},
                {"DevPartner.CloudStorage.LogFilterModel.ShowFiles", "Show Files to move"},
                {"DevPartner.CloudStorage.LogFilterModel.ShowFiles.Hint", "Show Files to move"},
                {"DevPartner.CloudStorage.LogFilterModel.Statuses", "Statuses"},
                {"DevPartner.CloudStorage.LogFilterModel.ShowPending", "Pending"},
                {"DevPartner.CloudStorage.LogFilterModel.ShowPending.Hint", "Show Pending items."},
                {"DevPartner.CloudStorage.LogFilterModel.ShowProcessing", "Processing"},
                {"DevPartner.CloudStorage.LogFilterModel.ShowProcessing.Hint", "Show Processing items."},
                {"DevPartner.CloudStorage.LogFilterModel.ShowSucceed", "Succeed"},
                {"DevPartner.CloudStorage.LogFilterModel.ShowSucceed.Hint", "Show Succeed items."},
                {"DevPartner.CloudStorage.LogFilterModel.ShowFailed", "Failed"},
                {"DevPartner.CloudStorage.LogFilterModel.ClearLog", "Clear Log"},
                {"DevPartner.CloudStorage.LogFilterModel.ClearLog.Hint", "Clear Log from succeed records."},
                {"DevPartner.CloudStorage.MovingItemModel.Id", "Id"},
                {"DevPartner.CloudStorage.MovingItemModel.Item", "Item"},
                {"DevPartner.CloudStorage.MovingItemModel.StoreType", "Store Type"},
                {"DevPartner.CloudStorage.MovingItemModel.Status", "Status"},
                {"DevPartner.CloudStorage.MovingItemModel.CreatedOnUtc", "Created On"},
                {"DevPartner.CloudStorage.MovingItemModel.ChangedOnUtc", "Changed On"},
                {"DevPartner.CloudStorage.LicenseKey.Invalid", "Invalid license key for {0} ({1})" }
            };

            return result;
        }

        public static bool IsActive(int storeId = 0)
        {
            var pluginService = EngineContext.Current.Resolve<IPluginService>();

            //is plugin installed?
            var pluginDescriptor = pluginService.GetPluginDescriptorBySystemName<IPlugin>("DevPartner.CloudStorage", LoadPluginsMode.All);
            if (pluginDescriptor == null || !pluginDescriptor.Installed)
                return false;

            var plugin = pluginDescriptor.Instance<CloudStoragePlugin>();
            if (plugin == null)
                return false;

            //no validation required
            if (storeId == 0)
                return true;

            if (!pluginDescriptor.LimitedToStores.Any())
                return true;

            return pluginDescriptor.LimitedToStores.Contains(storeId);
        }

        public static bool IsValidLicense(string licenseKey)
        {
            /*var httpContextAccessor = EngineContext.Current.Resolve<IHttpContextAccessor>();

            var host = httpContextAccessor.HttpContext.Request.Host.Host;
            if (host == "localhost")
                return true;
            if (String.IsNullOrWhiteSpace(licenseKey))
                return false;
            var ip = NetworkUtil.GetIPAddress().ToString();*/

            return true;//JWTService.ValidateLicense(licenseKey, "DevPartner.CloudStorage", host, ip);
        }

        public static bool IsValidLicense()
        {
            /*var settingService = EngineContext.Current.Resolve<ISettingService>();
            var setting = settingService.GetSetting("devpartnercloudstoragesetting.licensekey");
            return IsValidLicense(setting?.Value);*/
            return true;
        }

        #endregion

    }
}
