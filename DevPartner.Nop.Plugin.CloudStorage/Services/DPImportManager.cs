using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace DevPartner.Nop.Plugin.CloudStorage.Services
{

    /// <summary>
    /// Import manager
    /// </summary>
    public partial class DPImportManager : ImportManager
    {
        #region Fields
        private readonly IProductService _productService2;
        private readonly IProductAttributeService _productAttributeService2;
        private readonly ICategoryService _categoryService2;
        private readonly IManufacturerService _manufacturerService2;
        private readonly IPictureService _pictureService2;
        private readonly IUrlRecordService _urlRecordService2;
        private readonly IStoreContext _storeContext2;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService2;
        private readonly ICountryService _countryService2;
        private readonly IStateProvinceService _stateProvinceService2;
        private readonly IEncryptionService _encryptionService2;
        private readonly IDataProvider _dataProvider2;
        private readonly MediaSettings _mediaSettings2;
        private readonly IVendorService _vendorService2;
        private readonly IProductTemplateService _productTemplateService2;
        private readonly IShippingService _shippingService2;
        private readonly ITaxCategoryService _taxCategoryService2;
        private readonly IMeasureService _measureService2;
        private readonly CatalogSettings _catalogSettings2;
        #endregion

        #region Ctor
        public DPImportManager(IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IPictureService pictureService,
            IUrlRecordService urlRecordService,
            IStoreContext storeContext,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IEncryptionService encryptionService,
            IHttpClientFactory httpClientFactory,
            IDataProvider dataProvider,
            MediaSettings mediaSettings,
            IVendorService vendorService,
            IProductTemplateService productTemplateService,
            IShippingService shippingService,
            IDateRangeService dateRangeService,
            ITaxCategoryService taxCategoryService,
            IMeasureService measureService,
            IProductAttributeService productAttributeService,
            CatalogSettings catalogSettings,
            IProductTagService productTagService,
            IWorkContext workContext,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            VendorSettings vendorSettings,
            ISpecificationAttributeService specificationAttributeService,
            ILogger logger,
            INopFileProvider fileProvider,
            IServiceScopeFactory serviceScopeFactory,
            IStoreMappingService storeMappingService,
            IStoreService storeService)
            : base(
                  catalogSettings,
                  categoryService,
                  countryService,
                  customerActivityService,
                  dataProvider,
                  dateRangeService,
                  encryptionService,
                  httpClientFactory,
                  localizationService,
                  logger,
                  manufacturerService,
                  measureService,
                  newsLetterSubscriptionService,
                  fileProvider,
                  pictureService,
                  productAttributeService,
                  productService,
                  productTagService,
                  productTemplateService,
                  serviceScopeFactory,
                  shippingService,
                  specificationAttributeService,
                  stateProvinceService,
                  storeContext,
                  storeMappingService,
                  storeService,
                  taxCategoryService,
                  urlRecordService,
                  vendorService,
                  workContext,
                  mediaSettings,
                  vendorSettings)
        {
            _productService2 = productService;
            _productAttributeService2 = productAttributeService;
            _categoryService2 = categoryService;
            _manufacturerService2 = manufacturerService;
            _pictureService2 = pictureService;
            _urlRecordService2 = urlRecordService;
            _storeContext2 = storeContext;
            _newsLetterSubscriptionService2 = newsLetterSubscriptionService;
            _countryService2 = countryService;
            _stateProvinceService2 = stateProvinceService;
            _encryptionService2 = encryptionService;
            _dataProvider2 = dataProvider;
            _mediaSettings2 = mediaSettings;
            _vendorService2 = vendorService;
            _productTemplateService2 = productTemplateService;
            _shippingService2 = shippingService;
            _taxCategoryService2 = taxCategoryService;
            _measureService2 = measureService;
            _catalogSettings2 = catalogSettings;
        }
        #endregion


        #region Utilities

        /// <summary>
        /// Creates or loads the image
        /// </summary>
        /// <param name="picturePath">The path to the image file</param>
        /// <param name="name">The name of the object</param>
        /// <param name="picId">Image identifier, may be null</param>
        /// <returns>The image or null if the image has not changed</returns>
        protected override Picture LoadPicture(string picturePath, string name, int? picId = null)
        {

            if (String.IsNullOrEmpty(picturePath))
                return null;

            byte[] newPictureBinary = null;
            if (Uri.IsWellFormedUriString(picturePath, UriKind.RelativeOrAbsolute) && Regex.IsMatch(picturePath, "^http"))
            {
                newPictureBinary = new System.Net.WebClient().DownloadData(picturePath);
            }
            else
            {

                if (!File.Exists(picturePath))
                    return null;

                newPictureBinary = File.ReadAllBytes(picturePath);
            }
            var mimeType = GetMimeTypeFromFilePath(picturePath);

            var pictureAlreadyExists = false;
            if (picId != null)
            {
                //compare with existing product pictures
                var existingPicture = _pictureService2.GetPictureById(picId.Value);

                var existingBinary = _pictureService2.LoadPictureBinary(existingPicture);
                //picture binary after validation (like in database)
                var validatedPictureBinary = _pictureService2.ValidatePicture(newPictureBinary, mimeType);
                if (existingBinary.SequenceEqual(validatedPictureBinary) ||
                    existingBinary.SequenceEqual(newPictureBinary))
                {
                    pictureAlreadyExists = true;
                }
            }

            if (pictureAlreadyExists) return null;

            var newPicture = _pictureService2.InsertPicture(newPictureBinary, mimeType,
                _pictureService2.GetPictureSeName(name));
            return newPicture;
        }

        protected override void ImportProductImagesUsingServices(IList<ProductPictureMetadata> productPictureMetadata)
        {
            foreach (var product in productPictureMetadata)
            {
                foreach (var picturePath in new[] { product.Picture1Path, product.Picture2Path, product.Picture3Path })
                {
                    if (String.IsNullOrEmpty(picturePath))
                        continue;

                    var mimeType = GetMimeTypeFromFilePath(picturePath);
                    byte[] newPictureBinary = null;
                    if (Uri.IsWellFormedUriString(picturePath, UriKind.RelativeOrAbsolute) && Regex.IsMatch(picturePath, "^http"))
                    {
                        newPictureBinary = new System.Net.WebClient().DownloadData(picturePath);
                    }
                    else
                    {
                        newPictureBinary = File.ReadAllBytes(picturePath);
                    }
                    var pictureAlreadyExists = false;
                    if (!product.IsNew)
                    {
                        //compare with existing product pictures
                        var existingPictures = _pictureService2.GetPicturesByProductId(product.ProductItem.Id);
                        foreach (var existingPicture in existingPictures)
                        {
                            var existingBinary = _pictureService2.LoadPictureBinary(existingPicture);
                            //picture binary after validation (like in database)
                            var validatedPictureBinary = _pictureService2.ValidatePicture(newPictureBinary, mimeType);
                            if (!existingBinary.SequenceEqual(validatedPictureBinary) &&
                                !existingBinary.SequenceEqual(newPictureBinary))
                                continue;
                            //the same picture content
                            pictureAlreadyExists = true;
                            break;
                        }
                    }

                    if (pictureAlreadyExists)
                        continue;
                    var newPicture = _pictureService2.InsertPicture(newPictureBinary, mimeType, _pictureService2.GetPictureSeName(product.ProductItem.Name));
                    product.ProductItem.ProductPictures.Add(new ProductPicture
                    {
                        //EF has some weird issue if we set "Picture = newPicture" instead of "PictureId = newPicture.Id"
                        //pictures are duplicated
                        //maybe because entity size is too large
                        PictureId = newPicture.Id,
                        DisplayOrder = 1,
                    });
                    _productService2.UpdateProduct(product.ProductItem);
                }
            }
        }

        protected override void ImportProductImagesUsingHash(IList<ProductPictureMetadata> productPictureMetadata, IList<Product> allProductsBySku)
        {
            ImportProductImagesUsingServices(productPictureMetadata);
            /*
            //performance optimization, load all pictures hashes
            //it will only be used if the images are stored in the SQL Server database (not compact)
            var takeCount = _dataProvider2.SupportedLengthOfBinaryHash() - 1;
            var productsImagesIds = _productService2.GetProductsImagesIds(allProductsBySku.Select(p => p.Id).ToArray());
            IDictionary<int, string> allPicturesHashes = new Dictionary<int, string>();
            if (productsImagesIds != null)
            {
                allPicturesHashes = _pictureService2.GetPicturesHash(productsImagesIds.SelectMany(p => p.Value).ToArray());
            }
            foreach (var product in productPictureMetadata)
            {
                foreach (var picturePath in new[] { product.Picture1Path, product.Picture2Path, product.Picture3Path })
                {
                    if (String.IsNullOrEmpty(picturePath))
                        continue;

                    var mimeType = GetMimeTypeFromFilePath(picturePath);
                    byte[] newPictureBinary = null;
                    if (Uri.IsWellFormedUriString(picturePath, UriKind.RelativeOrAbsolute) && Regex.IsMatch(picturePath, "^http"))
                    {
                        newPictureBinary = new System.Net.WebClient().DownloadData(picturePath);
                    }
                    else
                    {
                        newPictureBinary = File.ReadAllBytes(picturePath);
                    }
                    var pictureAlreadyExists = false;
                    if (!product.IsNew)
                    {
                        var newImageHash = _encryptionService2.CreateHash(newPictureBinary.Take(takeCount).ToArray());
                        var newValidatedImageHash = _encryptionService2.CreateHash(_pictureService2.ValidatePicture(newPictureBinary, mimeType).Take(takeCount).ToArray());

                        var imagesIds = productsImagesIds.ContainsKey(product.ProductItem.Id)
                            ? productsImagesIds[product.ProductItem.Id]
                            : new int[0];

                        pictureAlreadyExists = allPicturesHashes.Where(p => imagesIds.Contains(p.Key)).Select(p => p.Value).Any(p => p == newImageHash || p == newValidatedImageHash);
                    }

                    if (pictureAlreadyExists)
                        continue;
                    var newPicture = _pictureService2.InsertPicture(newPictureBinary, mimeType, _pictureService2.GetPictureSeName(product.ProductItem.Name));
                    product.ProductItem.ProductPictures.Add(new ProductPicture
                    {
                        //EF has some weird issue if we set "Picture = newPicture" instead of "PictureId = newPicture.Id"
                        //pictures are duplicated
                        //maybe because entity size is too large
                        PictureId = newPicture.Id,
                        DisplayOrder = 1,
                    });
                    _productService2.UpdateProduct(product.ProductItem);
                }
            }*/
        }

        #endregion


        #region Methods



        #endregion

    }
}
