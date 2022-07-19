using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Domain;
using DevPartner.Nop.Plugin.CloudStorage.Extensions;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Logging;
using Nop.Services.Media;

namespace DevPartner.Nop.Plugin.CloudStorage.Services.NopServices
{
    public class CloudDownloadService : DownloadService
    {
        #region Fields
        private readonly IRepository<Download> _downloadRepository;
        private readonly ICloudStorageProvider _downloadProvider;
        private readonly ILogger _logger;
        private readonly INopFileProvider _fileProvider;
        private readonly ZipService _zipService;
        private readonly DevPartnerCloudStorageSetting _cloudStorageSetting;

        private object _mutex = new object();
        #endregion

        #region Ctor

        public CloudDownloadService(IRepository<Download> downloadRepository,
            ILogger logger,
            INopFileProvider fileProvider,
            ZipService zipService,
            DevPartnerCloudStorageSetting cloudStorageSetting)
            : base(downloadRepository)
        {
            _downloadRepository = downloadRepository;
            _cloudStorageSetting = cloudStorageSetting;
            _downloadProvider = CloudHelper.DownloadProvider;
            _logger = logger;
            _fileProvider = fileProvider;
            _zipService = zipService;
        }
        #endregion

        #region CRUD
        public async Task<List<int>> GetDownloadsIdsAsync()
        {
            var query = from p in _downloadRepository.Table
                        select p;
            return await query.Select(x => x.Id).ToListAsync();
        }

        #endregion

        #region Utilities
        public string GetFileName(Download download)
        {
            return $"{download.Id}_{download.Filename}";
        }

        protected virtual string GetStoredDownloadPath(string fileName, string extension)
        {
            var cloudFileName = _cloudStorageSetting.ArchiveDownloads && !extension.IsZip()
                ? Path.ChangeExtension(fileName,".zip")
                : Path.ChangeExtension(fileName, extension);

            var downloadDirectoryPath = _fileProvider.GetAbsolutePath(DPCloudDefaults.DownloadsPath);
            var cloudFilePath = _fileProvider.Combine(downloadDirectoryPath, cloudFileName);
            return cloudFilePath;
        }

        protected virtual byte[] GetSourceBinary(byte[] bytes, string fileName, string extension)
        {
            return _cloudStorageSetting.ArchiveDownloads && !extension.IsZip()
                ? _zipService.UnzipData(bytes)
                : bytes;
        }

        protected virtual byte[] DPProcessBinary(byte[] bytes, string fileName, string extension)
        {
            return _cloudStorageSetting.ArchiveDownloads && !extension.IsZip()
                ? _zipService.ZipData(bytes, fileName)
                : bytes;
        }

        protected async Task<Download> GetDownloadDataAsync(Download download)
        {
            var fileName = GetFileName(download);
            try
            {

                var downloadPath = GetStoredDownloadPath(fileName, download.Extension);

                //ensure \download directory exists
                //_fileProvider.CreateDirectory(_fileProvider.GetAbsolutePath(DPCloudDefaults.DownloadsPath));

                var cloudFile = await _fileProvider.ReadAllBytesAsync(
                    GetStoredDownloadPath(fileName, download.Extension));
                var downloadBinary = GetSourceBinary(cloudFile, fileName, download.Extension);
                
                download.DownloadBinary = downloadBinary;
                download.UseDownloadUrl = false;
                download.DownloadUrl = "";

                return download;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex, null);
            }
            return download;
        }

        /// <summary>
        /// Save a download to cloud
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <param name="mimeType">MIME type</param>
        /// <param name="binary">Picture binary</param>
        protected virtual async Task<string> SaveDownloadAsync(Download download, byte[] binary)
        {
            var fileName = GetFileName(download);
            var downloadPath = GetStoredDownloadPath(fileName, download.Extension);
            var processBinnary = DPProcessBinary(binary, fileName, download.Extension);

            //ensure \download directory exists
            _fileProvider.CreateDirectory(_fileProvider.GetAbsolutePath(DPCloudDefaults.DownloadsPath));

            //save
            await _fileProvider.WriteAllBytesAsync(downloadPath, processBinnary);

            return _fileProvider.GetVirtualPath(downloadPath);
        }
        #endregion

        #region Methods DownloadService
        /// <summary>
        /// Gets a download
        /// </summary>
        /// <param name="downloadId">Download identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the download
        /// </returns>
        public override async Task<Download> GetDownloadByIdAsync(int downloadId)
        {
            if (downloadId == 0)
                return null;

            var download = await base.GetDownloadByIdAsync(downloadId);
            if (!_downloadProvider.IsNull())
            {
                return await GetDownloadDataAsync(download);
            }
            return download;
        }

        /// <summary>
        /// Gets a download by GUID
        /// </summary>
        /// <param name="downloadGuid">Download GUID</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the download
        /// </returns>
        public override async Task<Download> GetDownloadByGuidAsync(Guid downloadGuid)
        {
            if (downloadGuid == Guid.Empty)
                return null;
            var download = await base.GetDownloadByGuidAsync(downloadGuid);
            if (!_downloadProvider.IsNull())
            {
                return await GetDownloadDataAsync(download);
            }
            return download;
        }


        /// <summary>
        /// Deletes a download
        /// </summary>
        /// <param name="download">Download</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task DeleteDownloadAsync(Download download)
        {
            if (download == null)
                throw new ArgumentNullException("download");

            if (!_downloadProvider.IsNull())
            {
                //ThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        var fileName = GetFileName(download);
                        var path = GetStoredDownloadPath(fileName, download.Extension);
                        _fileProvider.DeleteFile(path);
                    }
                    catch (Exception ex)
                    {
                        await _logger.ErrorAsync(ex.Message, ex, null);
                    }
                }
            }
            await base.DeleteDownloadAsync(download);
        }


        /// <summary>
        /// Inserts a download
        /// </summary>
        /// <param name="download">Download</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InsertDownloadAsync(Download download)
        {
            if (download == null)
                throw new ArgumentNullException("download");

            if (!_downloadProvider.IsNull() && download.DownloadBinary != null)
            {
                byte[] downloadBinary = download.DownloadBinary;
                download.DownloadBinary = null;

                await base.InsertDownloadAsync(download);

                download.DownloadUrl = await SaveDownloadAsync(download, downloadBinary);

                lock (_mutex)
                {
                    _downloadRepository.UpdateAsync(download); //save changes
                }
            }
            else
            {
                await base.InsertDownloadAsync(download);
            }
        }

        #endregion
    }
}