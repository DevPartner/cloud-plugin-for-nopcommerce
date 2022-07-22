using DevPartner.Nop.Plugin.CloudStorage.Domain;
using DevPartner.Nop.Plugin.CloudStorage.Extensions;
using DevPartner.Nop.Plugin.CloudStorage.Infrastructure;
using Nop.Core;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevPartner.Nop.Plugin.CloudStorage
{
    public class CloudStoragePlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin
    {
        #region Fields
        private readonly ISettingService _settingService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly IPluginService _pluginService;
        #endregion

        #region Ctor
        public CloudStoragePlugin(ISettingService settingService,
            IScheduleTaskService scheduleTaskService,
            IWebHelper webHelper,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            IPluginService pluginService
            )
        {
            _settingService = settingService;
            _scheduleTaskService = scheduleTaskService;
            _webHelper = webHelper;
            _permissionService = permissionService;
            _localizationService = localizationService;
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
        public override async Task InstallAsync()
        {
            //locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
//                ["DevPartner.CloudStorage.ConfigureModel.Note.PictureStoreType"] = "NOTE: Do not forget to backup your database and files before changing this option",
                ["DevPartner.CloudStorage.ConfigureModel.Note.DownloadStoreType"] = "NOTE: Do not forget to backup your database before changing this option",
                ["DevPartner.CloudStorage.ConfigureModel.Note.FileStoreType"] = "NOTE: Do not forget to backup your files before changing this option",
                ["DevPartner.CloudStorage.ForumSupport"] = "Forum Support",
                ["DevPartner.CloudStorage.ConfigureModel.ButtonSave"] = "Save",
//                ["DevPartner.CloudStorage.ConfigureModel.PictureStoreType"] = "Pictures are stored into...",
//                ["DevPartner.CloudStorage.ConfigureModel.PictureStoreType.Hint"] = "Chose pictures storage type",
                ["DevPartner.CloudStorage.ConfigureModel.DownloadStoreType"] = "Downloads are stored into...",
                ["DevPartner.CloudStorage.ConfigureModel.DownloadStoreType.Hint"] = "Chose downloads storage type",
                ["DevPartner.CloudStorage.ConfigureModel.FileStoreType"] = "Files are stored into...",
                ["DevPartner.CloudStorage.ConfigureModel.FileStoreType.Hint"] = "Chose storage type for files(js, css, images). By default plugin works just with wwwroot\\images folder but you can extent FileProviderRuleConfig in App_Data\\appsettings.json and copy other folders",
                ["DevPartner.CloudStorage.ConfigureModel.ChangeFilesStorage"] = "Files storage has been changed successfully.",
                ["DevPartner.CloudStorage.ConfigureModel.ChangePicturesStorage"] = "Pictures storage has been changed successfully.",
                ["DevPartner.CloudStorage.ConfigureModel.ChangeContentStorage"] = "Content storage has been changed successfully.",
                ["DevPartner.CloudStorage.ConfigureModel.AlwaysShowMainImage"] = "Always show main image",
                ["DevPartner.CloudStorage.ConfigureModel.AlwaysShowMainImage.Hint"] = "Please check this checkbox if you want to improve performance. Plugin will not generate thumbnails and will return main image",
                ["DevPartner.CloudStorage.ConfigureModel.CheckIfImageExist"] = "Check if image exist",
                ["DevPartner.CloudStorage.ConfigureModel.CheckIfImageExist.Hint"] = "Please uncheck this checkbox if you want to improve performance. Plugin will not check existence of images and image thumbnails",
                ["DevPartner.CloudStorage.ConfigureModel.ArchiveDownloads"] = "Save downloads as .zip archives",
                ["DevPartner.CloudStorage.ConfigureModel.ArchiveDownloads.Hint"] = "Save downloads as .zip archives",
                ["DevPartner.CloudStorage.ConfigureModel.ValidateSettings"] = "Storage settings are empty",
                ["DevPartner.CloudStorage.ConfigureModel.PictureMovingInProgress"] = "Could not change picture storage type before the moving completed",
                ["DevPartner.CloudStorage.ConfigureModel.DownloadMovingInProgress"] = "Could not change download storage type before the moving completed",
                ["DevPartner.CloudStorage.ConfigureModel.FileMovingInProgress"] = "Could not change file storage type before the moving complited",
                ["DevPartner.CloudStorage.ConfigureModel.StoreImageInDb"] = "Store Images In DB",
                ["DevPartner.CloudStorage.ConfigureModel.StoreImageInDb.Hint"] = "Store Images In DB",
                ["DevPartner.CloudStorage.ConfigureModel.LicenseKey"] = "License Key",
                ["DevPartner.CloudStorage.ConfigureModel.LicenseKey.Hint"] = "Enter license key here",
                ["DevPartner.CloudStorage.ConfigureModel.Note.LicenseKey"] = "You can buy this product <a href='https://www.dev-partner.biz/nopcommerce-cloud-storage-azure-blobcdn-amazon-s3cloudfront-plugin'>here</a>",
                ["DevPartner.CloudStorage.Configure.FailedToSave"] = "Failed to save",
                ["DevPartner.CloudStorage.ConfigureModel.ErrorNotification.StoreInDB"] = "Please move your pictures to DB",
                ["DevPartner.CloudStorage.Configure.Settings"] = "Cloud Settings",
                ["DevPartner.CloudStorage.Configure.Settings.BackToList"] = "back to cloud settings",
                ["DevPartner.CloudStorage.LogFilterModel.Log"] = "Cloud Log",
                ["DevPartner.CloudStorage.LogFilterModel.Items"] = "Items to show",
                ["DevPartner.CloudStorage.LogFilterModel.ShowPictures"] = "Show Pictures to move",
                ["DevPartner.CloudStorage.LogFilterModel.ShowPictures.Hint"] = "Show Pictures to move.",
                ["DevPartner.CloudStorage.LogFilterModel.ShowDownloads"] = "Show Downloads to move",
                ["DevPartner.CloudStorage.LogFilterModel.ShowDownloads.Hint"] = "Show Downloads to move.",
                ["DevPartner.CloudStorage.LogFilterModel.ShowFiles"] = "Show Files to move",
                ["DevPartner.CloudStorage.LogFilterModel.ShowFiles.Hint"] = "Show Files to move",
                ["DevPartner.CloudStorage.LogFilterModel.Statuses"] = "Statuses",
                ["DevPartner.CloudStorage.LogFilterModel.ShowPending"] = "Pending",
                ["DevPartner.CloudStorage.LogFilterModel.ShowPending.Hint"] = "Show Pending items.",
                ["DevPartner.CloudStorage.LogFilterModel.ShowProcessing"] = "Processing",
                ["DevPartner.CloudStorage.LogFilterModel.ShowProcessing.Hint"] = "Show Processing items.",
                ["DevPartner.CloudStorage.LogFilterModel.ShowSucceed"] = "Succeed",
                ["DevPartner.CloudStorage.LogFilterModel.ShowSucceed.Hint"] = "Show Succeed items.",
                ["DevPartner.CloudStorage.LogFilterModel.ShowFailed"] = "Failed",
                ["DevPartner.CloudStorage.LogFilterModel.ClearLog"] = "Clear Log",
                ["DevPartner.CloudStorage.LogFilterModel.ClearLog.Hint"] = "Clear Log from succeed records.",
                ["DevPartner.CloudStorage.MovingItemModel.Id"] = "Id",
                ["DevPartner.CloudStorage.MovingItemModel.Item"] = "Item",
                ["DevPartner.CloudStorage.MovingItemModel.StoreType"] = "Store Type",
                ["DevPartner.CloudStorage.MovingItemModel.Status"] = "Status",
                ["DevPartner.CloudStorage.MovingItemModel.CreatedOnUtc"] = "Created On",
                ["DevPartner.CloudStorage.MovingItemModel.ChangedOnUtc"] = "Changed On",
            });

            //settings
            var settings = new DevPartnerCloudStorageSetting
            {
                AlwaysShowMainImage = false,
                ArchiveDownloads = false,
                CheckIfImageExist = false,
                StoreImageInDb = false,
                DownloadStoreType = DPCloudDefaults.NULL_CLOUD_PROVIDER_NAME,
                FileStoreType = DPCloudDefaults.NULL_CLOUD_PROVIDER_NAME,
            };

            await _settingService.SaveSettingAsync(settings);

            //install synchronization task
            await _scheduleTaskService.InsertTasksIfDoesntExistAsync(GetScheduleTaskList());

            //permitions
            var permissionProviders = new List<Type> { typeof(CloudStoragePermissionProvider) };
            foreach (var providerType in permissionProviders)
            {
                var provider = (IPermissionProvider)Activator.CreateInstance(providerType);
                await EngineContext.Current.Resolve<IPermissionService>().InstallPermissionsAsync(provider);
            }

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override async Task UninstallAsync()
        {
            //locales
            await _localizationService.DeleteLocaleResourcesAsync("DevPartner.CloudStorage");

            //remove ScheduleTasks
            var allTasks = await _scheduleTaskService.GetAllTasksAsync();
            foreach (var task in GetScheduleTaskList())
            {
                var scheduleTask = allTasks.FirstOrDefault(t => t.Name == task.Name);
                if (scheduleTask != null)
                    await _scheduleTaskService.DeleteTaskAsync(scheduleTask);
            }

            await base.UninstallAsync();
        }
        #endregion

        #region IAdminMenuPlugin
        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return;

            var pluginDescriptor = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("DevPartner.CloudStorage", LoadPluginsMode.All);
            if (pluginDescriptor == null || !pluginDescriptor.Installed)
                return;

            var devCommerceNode = rootNode.AddIfNotExist(new SiteMapNode
            {
                Title = "DevCommerce",
                SystemName = "DevCommerce",
                IconClass = "fa fa-random",
                Visible = await _permissionService.AuthorizeAsync("DevPartner.DevCommerce"),
            });
            if (devCommerceNode.Visible)
            {
                var pluginCMS = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("DevPartner.CMS", LoadPluginsMode.All);
                if (pluginCMS == null)
                {
                    devCommerceNode.AddIfNotExist(new SiteMapNode
                    {
                        Visible = true,
                        Title = "CMS",
                        SystemName = "DP_CMS",
                        IconClass = "fa fa-cog",
                        Url = "https://www.dev-partner.biz/nopcommerce-cms"
                    });
                }

                var cloudNode = new SiteMapNode
                {
                    Visible = true,
                    Title = "Cloud",
                    SystemName = "DP_Cloud",
                    IconClass = "fa fa-cloud",
                };

                devCommerceNode.ChildNodes.Add(cloudNode);
                cloudNode.Visible = await _permissionService.AuthorizeAsync("DevPartner.DevCommerce.CloudStorage");

                if (cloudNode.Visible)
                {
                    cloudNode.ChildNodes.Add(new SiteMapNode
                    {
                        SystemName = "DP_CloudSettings",
                        Visible = true,
                        Title = await _localizationService.GetResourceAsync("DevPartner.Cloud.Configure.Settings"),
                        IconClass = "fa fa-dot-circle",
                        Url = GetConfigurationPageUrl()
                    });

                    //cloudNode.ChildNodes.Add(new SiteMapNode
                    //{
                    //    SystemName = "DP_CloudLog",
                    //    Visible = true,
                    //    Title = await _localizationService.GetResourceAsync("DevPartner.CloudStorage.LogFilterModel.Log"),
                    //    IconClass = "far fa-dot-circle",
                    //    Url = _webHelper.GetStoreLocation() + "Admin/MiscCloudStorage/Log"
                    //});

                    cloudNode.ChildNodes.Add(new SiteMapNode
                    {
                        Visible = true,
                        IconClass = "far fa-dot-circle",
                        Title = await _localizationService.GetResourceAsync("DevPartner.CloudStorage.ForumSupport"),
                        Url = "https://www.dev-partner.biz/boards/forum/5/cloud-storage-plugin"
                    });
                }

                var pluginSearch = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("DevPartner.Search", LoadPluginsMode.All);
                if (pluginSearch == null)
                {
                    devCommerceNode.AddIfNotExist(new SiteMapNode
                    {
                        Visible = true,
                        Title = "Search",
                        SystemName = "DP_Search",
                        IconClass = "fa fa-search",
                        Url = "https://www.dev-partner.biz/nopcommerce-search-solr-plugin"
                    });
                }

                var pluginSync = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("DevPartner.Sync", LoadPluginsMode.All);
                if (pluginSync == null)
                {
                    devCommerceNode.AddIfNotExist(new SiteMapNode
                    {
                        Visible = true,
                        Title = "Sync",
                        SystemName = "DP_Sync",
                        IconClass = "fa fa-plug",
                        Url = "https://www.dev-partner.biz/nopcommerce-sync-plugin"
                    });
                }
            }
        }
        #endregion

        #region Utils
        private List<ScheduleTask> GetScheduleTaskList()
        {
            var tasks = new List<ScheduleTask>
            {
                //pictures
                new ScheduleTask
                {
                    Name = "DevPartner CloudStorage: Move Entities",
                    Type = DPCloudDefaults.SchedultTaskType,
                    Seconds = DPCloudDefaults.SCHEDULE_TASK_SECONDS,
                    Enabled = false,
                    StopOnError = false,
                },
            };

            return tasks;
        }

        #endregion
    }
}
