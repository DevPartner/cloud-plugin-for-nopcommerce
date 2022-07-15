using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Extensions;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Seo;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Linq;
using System.Threading;
using Image = SixLabors.ImageSharp.Image;

namespace DevPartner.Nop.Plugin.CloudStorage.Services
{
    public class StorePictureService : PictureService
    {

        #region Fields
        private readonly ICloudPictureProvider _pictureProvider;
        private readonly ICloudThumbPictureProvider _thumbPictureProvider;
        private readonly IRepository<Picture> _pictureRepository;
        private readonly ISettingService _settingService2;
        //private readonly IWebHelper _webHelper;
        private readonly IEventPublisher _eventPublisher;
        private readonly MediaSettings _mediaSettings;
        private readonly ILogger _logger;
        //private readonly IDataProvider _dataProvider;

        #endregion

        #region Ctor

        public StorePictureService(
            IRepository<Picture> pictureRepository,
            ILogger logger,
            ICloudPictureProvider pictureProvider,
            ICloudThumbPictureProvider thumbPictureProvider,
            IDataProvider dataProvider,
            IDbContext dbContext,
            IDownloadService downloadService,
            IEventPublisher eventPublisher,
            IHttpContextAccessor httpContextAccessor,
            INopFileProvider fileProvider,
            IProductAttributeParser productAttributeParser,
            IRepository<PictureBinary> pictureBinaryRepository,
            IRepository<ProductPicture> productPictureRepository,
            ISettingService settingService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            MediaSettings mediaSettings)
            : base(
            dataProvider,
            dbContext,
            downloadService,
            eventPublisher,
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
            _pictureProvider = pictureProvider.IsNotNull() as ICloudPictureProvider;
            _thumbPictureProvider = thumbPictureProvider.IsNotNull() as ICloudThumbPictureProvider;
            _pictureRepository = pictureRepository;
            _settingService2 = settingService;
            _logger = logger;
            _mediaSettings = mediaSettings;
            _eventPublisher = eventPublisher;
        }
        #endregion

        #region Utilities
        public string GetFileName(int id, string mimeType)
        {
            var lastPart = GetFileExtensionFromMimeType(mimeType);
            return $"{id.ToString("000000000")}_0.{lastPart}";
        }
        /*

        /// <summary>
        /// Gets a picture
        /// </summary>
        /// <param name="pictureId">Picture identifier</param>
        /// <returns>Picture</returns>
        public virtual Picture GetPictureById(int pictureId)
        {
            if (pictureId == 0)
                return null;

            return _pictureRepository.GetById(pictureId);
        }*/
        #endregion

        #region Base Methods

        public Picture InsertPictureBase(byte[] pictureBinary, string mimeType, string seoFilename,
            string altAttribute = null, string titleAttribute = null,
            bool isNew = true, bool validateBinary = true, string externalUrl = null)
        {
            mimeType = CommonHelper.EnsureNotNull(mimeType);
            mimeType = CommonHelper.EnsureMaximumLength(mimeType, 20);

            seoFilename = CommonHelper.EnsureMaximumLength(seoFilename, 100);


            if (validateBinary)
                pictureBinary = ValidatePicture(pictureBinary, mimeType);

            var picture = new Picture
            {
                //PictureBinary = pictureBinary,
                MimeType = mimeType,
                SeoFilename = seoFilename,
                AltAttribute = altAttribute,
                TitleAttribute = titleAttribute,
                IsNew = isNew,
                VirtualPath = externalUrl
            };
            _pictureRepository.Insert(picture);
            UpdatePictureBinary(picture, StoreInDb ? pictureBinary : new byte[0]);
            //UpdatePictureBinary(picture, pictureBinary);

            //event notification
            _eventPublisher.EntityInserted(picture);

            return picture;
        }



        public Picture UpdatePictureBase(int pictureId, byte[] pictureBinary, string mimeType,
            string seoFilename, string altAttribute = null, string titleAttribute = null,
            bool isNew = true, bool validateBinary = true, string externalUrl = null)
        {
            mimeType = CommonHelper.EnsureNotNull(mimeType);
            mimeType = CommonHelper.EnsureMaximumLength(mimeType, 20);

            seoFilename = CommonHelper.EnsureMaximumLength(seoFilename, 100);


            if (validateBinary)
                pictureBinary = ValidatePicture(pictureBinary, mimeType);

            var picture = GetPictureById(pictureId);
            if (picture == null)
                return null;

            //delete old thumbs if a picture has been changed
            if (seoFilename != picture.SeoFilename)
                DeletePictureThumbs(picture);

            //picture.PictureBinary = pictureBinary;
            picture.MimeType = mimeType;
            picture.SeoFilename = seoFilename;
            picture.AltAttribute = altAttribute;
            picture.TitleAttribute = titleAttribute;
            picture.IsNew = isNew;
            picture.VirtualPath = externalUrl;
            //picture.ExternalUrl = externalUrl;

            _pictureRepository.Update(picture);
            UpdatePictureBinary(picture, StoreInDb ? pictureBinary : new byte[0]);

            //event notification
            _eventPublisher.EntityUpdated(picture);

            return picture;
        }



        /// <summary>
        /// Deletes a picture
        /// </summary>
        /// <param name="picture">Picture</param>
        public virtual void DeletePictureBase(Picture picture)
        {
            if (picture == null)
                throw new ArgumentNullException("picture");

            //delete thumbs
            DeletePictureThumbs(picture);

            //delete from file system
            /*if (!this.StoreInDb)
                DeletePictureOnFileSystem(picture);*/


            //delete from database
            _pictureRepository.Delete(picture);

            //event notification
            _eventPublisher.EntityDeleted(picture);
        }

        private string GetPictureUrlBase(Picture picture, int targetSize, string storeLocation, string url, ICloudStorageProvider provider)
        {
            byte[] pictureBinary = null;

            if (picture.IsNew)
            {
                DeletePictureThumbs(picture);
                pictureBinary = LoadPictureBinary(picture, provider);
                //we do not validate picture binary here to ensure that no exception ("Parameter is not valid") will be thrown
                picture = UpdatePicture(picture.Id,
                    pictureBinary,
                    picture.MimeType,
                    picture.SeoFilename,
                    picture.AltAttribute,
                    picture.TitleAttribute,
                    false,
                    false);
            }

            var seoFileName = picture.SeoFilename; // = GetPictureSeName(picture.SeoFilename); //just for sure

            string lastPart = GetFileExtensionFromMimeType(picture.MimeType);
            string thumbFileName;
            if (targetSize == 0)
            {
                thumbFileName = !String.IsNullOrEmpty(seoFileName)
                    ? string.Format("{0}_{1}.{2}", picture.Id.ToString("0000000"), seoFileName, lastPart)
                    : string.Format("{0}.{1}", picture.Id.ToString("0000000"), lastPart);
            }
            else
            {
                thumbFileName = !String.IsNullOrEmpty(seoFileName)
                    ? string.Format("{0}_{1}_{2}.{3}", picture.Id.ToString("0000000"), seoFileName, targetSize, lastPart)
                    : string.Format("{0}_{1}.{2}", picture.Id.ToString("0000000"), targetSize, lastPart);
            }
            string thumbFilePath = GetThumbLocalPath(thumbFileName);


            //the named mutex helps to avoid creating the same files in different threads,
            //and does not decrease performance significantly, because the code is blocked only for the specific file.
            using (var mutex = new Mutex(false, thumbFileName))
            {
                if (!GeneratedThumbExists(thumbFilePath, thumbFileName))
                {
                    mutex.WaitOne();

                    //check, if the file was created, while we were waiting for the release of the mutex.
                    if (!GeneratedThumbExists(thumbFilePath, thumbFileName))
                    {
                        byte[] pictureBinaryResized;

                        if (picture != null && pictureBinary == null)
                            pictureBinary = LoadPictureBinary(picture);
                        if (picture == null || pictureBinary == null || pictureBinary.Length == 0)
                        {
                            return url;
                        }
                        //resizing required
                        if (targetSize != 0)
                        {
                            //resizing required
                            using (var image = Image.Load(pictureBinary, out var imageFormat))
                            {
                                image.Mutate(imageProcess => imageProcess.Resize(new ResizeOptions
                                {
                                    Mode = ResizeMode.Max,
                                    Size = CalculateDimensions(image.Size(), targetSize)
                                }));

                                pictureBinaryResized = EncodeImage(image, imageFormat);
                            }
                        }
                        else
                        {
                            //create a copy of pictureBinary
                            pictureBinary = LoadPictureBinary(picture, provider);
                            pictureBinaryResized = pictureBinary.ToArray();
                        }

                        SaveThumb(thumbFilePath, thumbFileName, picture.MimeType, pictureBinaryResized);
                    }

                    mutex.ReleaseMutex();
                }
            }
            url = GetThumbUrl(thumbFileName, storeLocation);
            return url;
        }

        #endregion

        #region Methods Pictures Thumbs
        /// <summary>
        /// Delete picture thumbs
        /// </summary>
        /// <param name="picture">Picture</param>
        protected override void DeletePictureThumbs(Picture picture)
        {
            var provider = _thumbPictureProvider;
            if (provider != null)
            {
                string filter = $"{picture.Id.ToString("0000000")}";
                provider.DeleteFileByPrefix(filter);
            }
            else
            {
                base.DeletePictureThumbs(picture);
            }
        }

        /// <summary>
        /// Get picture (thumb) local path
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <returns>Local picture thumb path</returns>
        protected override string GetThumbLocalPath(string thumbFileName)
        {

            var provider = _thumbPictureProvider;
            return provider != null ? provider.GetFileUrlByUniqName(thumbFileName) : base.GetThumbLocalPath(thumbFileName);
        }

        /// <summary>
        /// Get picture (thumb) URL 
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns>Local picture thumb path</returns>
        protected override string GetThumbUrl(string thumbFileName, string storeLocation = null)
        {
            var provider = _thumbPictureProvider;
            return provider != null ? provider.GenerateUrl(thumbFileName) : base.GetThumbUrl(thumbFileName);
        }

        /// <summary>
        /// Get a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <returns>Result</returns>
        protected override bool GeneratedThumbExists(string thumbFilePath, string thumbFileName)
        {
            try
            {
                var provider = _thumbPictureProvider;
                return provider != null ? provider.IsFileExist(thumbFileName) : base.GeneratedThumbExists(thumbFilePath, thumbFileName);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Save a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <param name="binary">Picture binary</param>
        protected override void SaveThumb(string thumbFilePath, string thumbFileName, string mimeType, byte[] binary)
        {
            var provider = _thumbPictureProvider;
            if (provider != null)
            {
                provider.InsertFile(thumbFileName, mimeType, binary);
            }
            else
            {
                base.SaveThumb(thumbFilePath, thumbFileName, mimeType, binary);
            }
        }
        #endregion

        #region Methods Pictures
        
        public override Picture UpdatePicture(int pictureId, byte[] pictureBinary, string mimeType,
            string seoFilename, string altAttribute = null, string titleAttribute = null,
            bool isNew = true, bool validateBinary = true)
        {
            var provider = _pictureProvider;
            if (provider != null)
            {

                var fileName = GetFileName(pictureId, mimeType);
                string url = null;
                if (pictureBinary != null)
                {
                    url = provider.UpdateFile(fileName, mimeType, pictureBinary);
                }

                var cloudSetting = _settingService2.LoadSetting<DevPartnerCloudStorageSetting>();
                return UpdatePictureBase(pictureId, cloudSetting.StoreImageInDb ? pictureBinary :
                    null, mimeType, seoFilename, altAttribute, titleAttribute, isNew, false, externalUrl: url);
            }
            return base.UpdatePicture(pictureId, pictureBinary, mimeType, seoFilename, altAttribute, titleAttribute, isNew, validateBinary);
        }


        public override Picture InsertPicture(byte[] pictureBinary, string mimeType, string seoFilename,
            string altAttribute = null, string titleAttribute = null,
            bool isNew = true, bool validateBinary = true)
        {
            var provider = _pictureProvider;
            if (provider != null)
            {
                var cloudSetting = _settingService2.LoadSetting<DevPartnerCloudStorageSetting>();
                var picture = InsertPictureBase(cloudSetting.StoreImageInDb ? pictureBinary :
                    null, mimeType, seoFilename, altAttribute, titleAttribute, isNew, false);
                var fileName = GetFileName(picture.Id, mimeType);

                if (validateBinary)
                    pictureBinary = ValidatePicture(pictureBinary, mimeType);

                string url = provider.InsertFile(fileName, mimeType, pictureBinary);

                return picture;
            }
            return base.InsertPicture(pictureBinary, mimeType, seoFilename, altAttribute, titleAttribute, isNew, validateBinary);
        }

        public override void DeletePicture(Picture picture)
        {
            var provider = _pictureProvider;
            if (provider != null)
            {
                var fileName = GetFileName(picture.Id, picture.MimeType);
                provider.DeleteFile(fileName);

                DeletePictureBase(picture);
            }
            else
            {
                base.DeletePicture(picture);
            }
        }


        public override string GetPictureUrl(Picture picture,
            int targetSize = 0,
            bool showDefaultPicture = true,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity)
        {
            var provider = _pictureProvider;
            if (picture != null && provider != null)
            {
                byte[] pictureBinary = null;
                try
                {
                    //ExternalUrl feature
                    //Picture picture = null;
                    /*
                    if (picture.IsStoreInProvider() && !string.IsNullOrEmpty(picture.VirtualPath))
                    {
                        return picture.VirtualPath;
                    }*/
                    //AlwaysShowMainImage feuture
                    var cloudSetting = _settingService2.LoadSetting<DevPartnerCloudStorageSetting>();
                    if (picture != null && cloudSetting.AlwaysShowMainImage)
                    {
                        var fileName = GetFileName(picture.Id, picture.MimeType);
                        return String.Format(cloudSetting.MainImageUrlFormat, GetThumbUrl(fileName, storeLocation), targetSize);
                    }

                    string url = string.Empty;
                    if (picture.IsStoreInProvider() && !provider.IsFileExist(GetFileName(picture.Id, picture.MimeType)))
                    {
                        if (showDefaultPicture)
                        {
                            url = GetDefaultPictureUrl(targetSize, defaultPictureType, storeLocation);
                        }
                        return url;
                    }

                    //pictureBinary = LoadPictureBinary(picture);
                    return GetPictureUrlBase(picture, targetSize, storeLocation, url, provider);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, ex, null);
                    string url = string.Empty;
                    //if (showDefaultPicture)
                    //{
                    //    url = GetDefaultPictureUrl(targetSize, defaultPictureType, storeLocation);
                    //}
                    //if (picture != null) _logger.Information("GetPictureUrl " + targetSize + " " + picture.Id + " " + picture.SeoFilename);
                    return url;
                }
            }
            return base.GetPictureUrl(picture, targetSize, showDefaultPicture, storeLocation, defaultPictureType);
        }

        /// <summary>
        /// Gets the loaded picture binary depending on picture storage settings
        /// </summary>
        /// <param name="picture">Picture</param>
        /// <returns>Picture binary</returns>
        public override byte[] LoadPictureBinary(Picture picture)
        {
            var provider = _pictureProvider;
            if (provider != null)
            {
                return LoadPictureBinary(picture, provider);
            }
            return base.LoadPictureBinary(picture);
        }

        #endregion

        #region Methods Move Pictures

        private byte[] LoadPictureBinary(Picture picture, ICloudStorageProvider pictureProvider)
        {
            if (picture == null)
                throw new ArgumentNullException("picture");
            /*if (picture.PictureBinary != null && picture.PictureBinary.BinaryData != null)
                return picture.PictureBinary.BinaryData;*/
            var result = pictureProvider == null
                ? base.LoadPictureBinary(picture)
                : LoadPictureFromStorage(picture.Id, picture.MimeType, pictureProvider);
            return result;
        }
        private byte[] LoadPictureFromStorage(int id, string mimeType, ICloudStorageProvider providerService)
        {
            try
            {
                var fileName = GetFileName(id, mimeType);
                return providerService.GetFile(fileName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, null);
            }
            return null;
        }

        public void MovePicture(Picture picture, ICloudStorageProvider pictureProvider)
        {
            pictureProvider = pictureProvider.IsNotNull() as ICloudPictureProvider;
            var pictureBinary = LoadPictureBinary(picture, pictureProvider);

            //delete from file system
            if (pictureProvider == null)
            {
                if (!StoreInDb)
                {
                    DeletePictureOnFileSystem(picture);
                }
            }
            else
            {
                var fileName = GetFileName(picture.Id, picture.MimeType);
                pictureProvider.DeleteFile(fileName);
            }

            //just update a picture (all required logic is in UpdatePicture method)
            UpdatePicture(picture.Id,
                pictureBinary, picture.MimeType, picture.SeoFilename, picture.AltAttribute,
                picture.TitleAttribute, true, false);
            //we do not validate picture binary here to ensure that no exception ("Parameter is not valid") will be thrown when "moving" pictures
        }
        #endregion

        #region Methods to extend Picture Service

        public int[] GetPicturesIds()
        {
            return _pictureRepository.Table.Select(x => x.Id).ToArray();
        }
        #endregion

    }
}
