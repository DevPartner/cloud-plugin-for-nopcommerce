using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Extensions;
using Nop.Services.Configuration;

namespace DevPartner.Nop.Plugin.CloudStorage.Services
{
    public class StoreContentService
    {
        #region Const

        #endregion

        #region Fields

        private ICloudStorageProvider _oldProviderService;
        private readonly ICloudContentProvider _contentProvider;
        #endregion

        #region Properties

        #endregion

        #region Ctor

        public StoreContentService(
            ISettingService settingService,
            ICloudContentProvider contentProvider, MovingItemService movingFileService)
        {
            _contentProvider = contentProvider;
        }

        #endregion

        #region Methods

        public List<String> ListDirTree()
        {
            var relativePath = "/" /*GetFilesRelativeRootCloud()*/;
            var directories = new ArrayList(GetDirectoryList(relativePath, true));
            directories.Insert(0, relativePath);
            var output = new List<string>();
            for (var i = 0; i < directories.Count; i++)
            {
                string dir = (string) directories[i];

                output.AddRange(GetFileList(dir, false));
            }
            return output;
        }


        private string FixPath(string path)
        {
            if (path == null)
                path = "/";

            return path;
        }

        public List<string> GetFileList(string relativePath, bool includeSubDirectories = false,
            bool getFullPaths = false)
        {
            var contentProvider = _contentProvider;
            var files = contentProvider.GetFiles(FixPath(relativePath), includeSubDirectories);
            if (getFullPaths)
                files = files.Select(f => contentProvider.GetFileUrlByUniqName(f)).ToList();

            return files;
        }

        public List<string> GetDirectoryList(string relativePath, bool includeSubDirectories = false)
        {
            var contentProvider = _contentProvider;
            var directories = contentProvider.GetDirectories(relativePath, includeSubDirectories);
            //var directories = ProviderService.GetDirectoryListForRelPath(relativePath, includeSubDirectories);
            return directories;
        }

        #endregion
        
        ///// <summary>
        ///// Get file local path. 
        ///// </summary>
        ///// <param name="fileName">Filename</param>
        ///// <returns>Local picture path</returns>
        //protected virtual string GetPictureLocalPath(string fileName)
        //{
        //    return Path.Combine(CommonHelper.MapPath("~"), fileName);
        //}
        /// <summary>
        /// Loads a picture from file
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns>File binary</returns>
        protected virtual byte[] LoadFile(string filePath)
        {
            if (!File.Exists(filePath))
                return new byte[0];
            return File.ReadAllBytes(filePath);
        }

        private byte[] LoadFileFromStorage(string filePath, ICloudStorageProvider fileProvider)
        {

            var cloudFile = fileProvider.GetFile(filePath);
            return cloudFile;
        }

        private byte[] LoadFileBinary(string filePath, ICloudStorageProvider fileProvider)
        {
            var result = fileProvider == null
                ? LoadFile(filePath)
                : LoadFileFromStorage(filePath, fileProvider);
            return result;
        }

        public void MoveFile(string movingItemFilePath, ICloudStorageProvider provider)
        {
            provider = provider.IsNotNull() as ICloudPictureProvider;
            if (provider == null)
            {
                var file = LoadFileBinary(filePath: movingItemFilePath, fileProvider: provider);
                UpdateFile(movingItemFilePath, file);
            }
        }

        private void UpdateFile(string movingItemFilePath, byte[] file)
        {
            var provider = _contentProvider;
            if (provider != null)
            {

                var fileName = Path.GetFileName(movingItemFilePath);
                var extension = Path.GetExtension(movingItemFilePath);

                string url = null;
                if (file != null)
                {
                    url = provider.UpdateFile(movingItemFilePath, null, file);
                }
            }
        }
    }
}