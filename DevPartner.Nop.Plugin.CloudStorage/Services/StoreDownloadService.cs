using System;
using System.Collections.Generic;
using System.Linq;
using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Extensions;
using DevPartner.Nop.Plugin.CloudStorage;
using Nop.Core.Data;
using Nop.Core.Domain.Media;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Services.Media;

namespace DevPartner.Nop.Plugin.CloudStorage.Services
{
    public class StoreDownloadService : DownloadService
    {

        #region Fields

        private readonly IRepository<Download> _downloadRepository;
        private readonly ICloudDownloadProvider _downloadProvider;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;
        private readonly ZipService _zipService;

        private object _mutex = new object();

        #endregion

        #region Ctor

        public StoreDownloadService(IRepository<Download> downloadRepository,
            IEventPublisher eventPublisher,
            ICloudDownloadProvider downloadProvider,
            ISettingService settingService,
            ILogger logger,
            ZipService zipService)
            : base( eventPublisher, downloadRepository)
        {
            _downloadRepository = downloadRepository;
            _downloadProvider = downloadProvider.IsNotNull() as ICloudDownloadProvider;
            _settingService = settingService;
            _logger = logger;
            _zipService = zipService;
        }
        #endregion

        #region CRUD
        public List<Download> GetDownloads()
        {
            var query = from p in _downloadRepository.Table
                        select p;
            return query.ToList();
        }

        #endregion

        #region Utilities
        public static string GetFileName(Download download)
        {
            return download.DownloadGuid.ToString();
        }


        protected virtual string GetStoredDownloadName(string fileName, string extension)
        {
            var cloudFileName = _settingService.LoadSetting<DevPartnerCloudStorageSetting>().ArchiveDownloads && !extension.IsZip()
                ? $"{fileName}.zip"
                : fileName;
            return cloudFileName;
        }

        protected virtual byte[] GetSourceBinary(byte[] bytes, string fileName, string extension)
        {
            return _settingService.LoadSetting<DevPartnerCloudStorageSetting>().ArchiveDownloads && !extension.IsZip()
                ? _zipService.UnzipData(bytes)
                : bytes;
        }

        protected virtual byte[] GetUploadBinary(byte[] bytes, string fileName, string extension)
        {
            return _settingService.LoadSetting<DevPartnerCloudStorageSetting>().ArchiveDownloads && !extension.IsZip()
                ? _zipService.ZipData(bytes, fileName)
                : bytes;
        }

        public Download GetDownloadDataFromCloud(Download download, ICloudStorageProvider downloadProvider)
        {
            var fileName = GetFileName(download);
            try
            {
                var cloudFile = downloadProvider.GetFile(
                    GetStoredDownloadName(fileName, download.Extension));
                var downloadBinary = GetSourceBinary(cloudFile, fileName, download.Extension);

                download.DownloadBinary = downloadBinary;
                download.UseDownloadUrl = false;
                download.DownloadUrl = "";

                return download;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, null);
            }
            return download;
        }
        #endregion

        #region Methods DownloadService
        public override Download GetDownloadById(int downloadId)
        {
            if (downloadId == 0)
                return null;

            var download = base.GetDownloadById(downloadId);
            var downloadProvider = _downloadProvider;
            if (downloadProvider != null)
            {
                return GetDownloadDataFromCloud(download, downloadProvider);
            }
            return download;
        }

        public override Download GetDownloadByGuid(Guid downloadGuid)
        {
            if (downloadGuid == Guid.Empty) return null;
            var download = base.GetDownloadByGuid(downloadGuid);
            var downloadProvider = _downloadProvider;
            if (downloadProvider != null)
            {
                return GetDownloadDataFromCloud(download, downloadProvider);
            }
            return download;
        }

        public override void DeleteDownload(Download download)
        {
            if (download == null)
                throw new ArgumentNullException("download");

            var downloadProvider = _downloadProvider;
            if (downloadProvider != null)
            {
                //ThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        var fileName = GetFileName(download);
                        downloadProvider.DeleteFile(GetStoredDownloadName(fileName, download.Extension));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message, ex, null);
                    }
                }
            }
            base.DeleteDownload(download);
        }

        public override void InsertDownload(Download download)
        {
            if (download == null)
                throw new ArgumentNullException("download");
            var downloadProvider = _downloadProvider;


            if (downloadProvider != null && download.DownloadBinary!=null)
            {
                byte[] downloadBinary = download.DownloadBinary;
                download.DownloadBinary = null;

                base.InsertDownload(download);
                var fileName = GetFileName(download);
                string url = downloadProvider.InsertFile(GetStoredDownloadName(fileName, download.Extension), download.ContentType,
                                                GetUploadBinary(downloadBinary, fileName, download.Extension));

                download.DownloadUrl = url;
                lock (_mutex)
                {
                    _downloadRepository.Update(download); //save changes
                }
            }
            else
            {
                base.InsertDownload(download);
            }
        }

        public override void UpdateDownload(Download download)
        {
            if (download == null)
                throw new ArgumentNullException("download");

            var downloadProvider = _downloadProvider;
            if (downloadProvider != null && download.DownloadBinary != null)
            {

                byte[] downloadBinary = download.DownloadBinary;
                download.DownloadBinary = null;

                var fileName = GetFileName(download);
                string url = downloadProvider.UpdateFile(GetStoredDownloadName(fileName, download.Extension), download.ContentType,
                    GetUploadBinary(downloadBinary, fileName, download.Extension));
                download.DownloadUrl = url;
                lock (_mutex)
                {
                    _downloadRepository.Update(download); //save changes
                }
            }
            else
            {
                base.UpdateDownload(download);
            }
        }
        #endregion

        #region Move
        public void MoveDownload(int downloadId, ICloudStorageProvider provider)
        {
            provider = provider.IsNotNull() as ICloudPictureProvider;
            var download = base.GetDownloadById(downloadId);
            if (provider != null)
            {
                download = GetDownloadDataFromCloud(download, provider);
            }
            UpdateDownload(download);
        }
        #endregion
    }
}