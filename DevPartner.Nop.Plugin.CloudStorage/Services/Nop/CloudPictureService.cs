using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Extensions;
using DevPartner.Nop.Plugin.CloudStorage.Nop.Services;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Seo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DevPartner.Nop.Plugin.CloudStorage.Services.Nop
{
    public class CloudPictureService : PictureService
    {

        #region Fields
        private readonly ILogger _logger;
        private readonly CloudFileProvider _coudFileProvider;
        private readonly MediaSettings _mediaSettings2;
        private readonly IRepository<Picture> _pictureRepository2;
        private readonly INopFileProvider _fileProvider2;
        #endregion

        #region Ctor
        public CloudPictureService(
            ILogger logger,
            IDownloadService downloadService,
            IHttpContextAccessor httpContextAccessor,
            INopFileProvider fileProvider,
            CloudFileProvider coudFileProvider,
            IProductAttributeParser productAttributeParser,
            IRepository<Picture> pictureRepository,
            IRepository<PictureBinary> pictureBinaryRepository,
            IRepository<ProductPicture> productPictureRepository,
            ISettingService settingService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            MediaSettings mediaSettings)
            : base(
             downloadService,
             httpContextAccessor,
             fileProvider,
             productAttributeParser,
             pictureRepository,
             pictureBinaryRepository,
             productPictureRepository,
             settingService,
             urlRecordService,
             webHelper,
             mediaSettings)
        {
            _logger = logger;
            _coudFileProvider = coudFileProvider;
            _mediaSettings2 = mediaSettings;
            _pictureRepository2 = pictureRepository;
            _fileProvider2 = fileProvider;
        }
        #endregion

        #region Utils
        public async Task<string> GetFileNameAsync(int id, string mimeType)
        {
            var lastPart = await GetFileExtensionFromMimeTypeAsync(mimeType);
            return $"{id:0000000}_0.{lastPart}";
        }

        /// <summary>
        /// Loads a picture from file
        /// </summary>
        /// <param name="pictureId">Picture identifier</param>
        /// <param name="mimeType">MIME type</param>
        /// <returns>Picture binary</returns>
        private async Task<byte[]> LoadPictureFromStorageAsync(int pictureId, string mimeType, ICloudStorageProvider providerService)
        {
            try
            {
                var fileName = await GetFileNameAsync(pictureId, mimeType);
                var filePath = await GetPictureLocalPathAsync(fileName);
                return await _coudFileProvider.ReadAllBytesAsync(filePath, providerService);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex, null);
            }
            return null;
        }

        /// <summary>
        /// Get images path URL 
        /// </summary>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns></returns>
        protected virtual async Task<string> GetImagesPathUrlAsync(ICloudStorageProvider cloudStorageProvider, string fileName = "", string storeLocation = null)
        {

            var path = await GetPictureLocalPathAsync(fileName);
            var virtualPath = _coudFileProvider.GetVirtualPath(path);
            return virtualPath;
        }
        #endregion 

        #region Methods

        /// <summary>
        /// Get images path URL 
        /// </summary>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns></returns>
        protected override async Task<string> GetImagesPathUrlAsync(string storeLocation = null)
        {
            if (CloudHelper.FileProvider.IsNull())
                return await base.GetImagesPathUrlAsync(storeLocation);
            var path = await GetImagesPathUrlAsync(CloudHelper.FileProvider);
            return path;
        }

        /// <summary>
        /// Get picture (thumb) URL 
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns>Local picture thumb path</returns>
        protected override async Task<string> GetThumbUrlAsync(string thumbFileName, string storeLocation = null)
        {
            if (CloudHelper.FileProvider == null)
                return await base.GetThumbUrlAsync(thumbFileName, storeLocation);
            var url = await GetImagesPathUrlAsync(CloudHelper.FileProvider, "thumbs/", storeLocation);

            if (_mediaSettings2.MultipleThumbDirectories)
            {
                //get the first two letters of the file name
                var fileNameWithoutExtension = _coudFileProvider.GetFileNameWithoutExtension(thumbFileName);
                if (fileNameWithoutExtension != null && fileNameWithoutExtension.Length > NopMediaDefaults.MultipleThumbDirectoriesLength)
                {
                    var subDirectoryName = fileNameWithoutExtension.Substring(0, NopMediaDefaults.MultipleThumbDirectoriesLength);
                    url = url + subDirectoryName + "/";
                }
            }

            url = url + thumbFileName;
            return url;
        }
        #endregion

        #region Methods Ext

        /// <summary>
        /// Gets the loaded picture binary depending on picture storage settings
        /// </summary>
        /// <param name="picture">Picture</param>
        /// <param name="fromDb">Load from database; otherwise, from file system</param>
        /// <returns>Picture binary</returns>
        private async Task<byte[]> LoadPictureBinaryAsync(Picture picture, ICloudStorageProvider pictureProvider)
        {
            if (picture == null)
                throw new ArgumentNullException("picture");

            var result = await IsStoreInDbAsync() ?
                (await GetPictureBinaryByPictureIdAsync(picture.Id))?.BinaryData ?? Array.Empty<byte>()
                : await LoadPictureFromStorageAsync(picture.Id, picture.MimeType, pictureProvider);
            return result;
        }

        public async Task MovePictureAsync(Picture picture, ICloudStorageProvider pictureProvider)
        {
            var pictureBinary = await LoadPictureBinaryAsync(picture, pictureProvider);

            //delete from file system
            if (!pictureProvider.IsNull() || (pictureProvider.IsNull() && !await IsStoreInDbAsync()))
            {
                var fileName = await GetFileNameAsync(picture.Id, picture.MimeType);
                var filePath = await GetPictureLocalPathAsync(fileName);
                _coudFileProvider.DeleteFile(filePath, pictureProvider);
            }

            //just update a picture (all required logic is in UpdatePicture method)
            await UpdatePictureAsync(picture.Id,
                pictureBinary, picture.MimeType, picture.SeoFilename, picture.AltAttribute,
                picture.TitleAttribute, true, false);
            //we do not validate picture binary here to ensure that no exception ("Parameter is not valid") will be thrown when "moving" pictures
        }

        public async Task<List<int>> GetPicturesIdsAsync()
        {
            var query = from p in _pictureRepository2.Table
                        select p;
            return await query.Select(x => x.Id).ToListAsync();
        }
        #endregion
    }
}
