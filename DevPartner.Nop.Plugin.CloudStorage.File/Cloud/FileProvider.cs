using System;
using System.Collections.Generic;
using System.IO;
using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Domain;
using Nop.Core.Caching;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;

namespace DevPartner.Nop.Plugin.CloudStorage.File.Cloud
{
    public class FileProvider : BaseCloudStorageProvider, ICloudStorageProvider
    {
        #region Constants
        /// <summary>
        /// Key to cache whether file exists
        /// </summary>
        /// <remarks>
        /// {0} : file name
        /// </remarks>
        private const string FILE_EXISTS_KEY = "DP.cloud.file.{0}.exists-{1}";

        /// <summary>
        /// Key pattern to clear container cache
        /// </summary>
        private const string CONTAINER_PATTERN_KEY = "DP.cloud.file.{0}";
        #endregion

        #region Fields
        private readonly IStaticCacheManager _cacheManager;
        private readonly MediaSettings _mediaSettings;
        private readonly FileProviderSettings _fileProviderSettings;
        private readonly INopFileProvider _fileProvider;
        private string _directory;

        #endregion

        #region Ctor

        public FileProvider(FileProviderSettings fileProviderSettings, 
            IStaticCacheManager cacheManager, MediaSettings mediaSettings,
            INopFileProvider fileProvider)
        {
            _fileProviderSettings = fileProviderSettings;
            _cacheManager = cacheManager;
            _mediaSettings = mediaSettings;
            _fileProvider = fileProvider;
        }

        #endregion

        #region Utils


        #endregion

        #region Methods

        public void RunAtAppStartup(string directory)
        {
            _directory = directory;
            if (string.IsNullOrEmpty(_directory))
                throw new Exception("Directory is not specified");

            try
            {
                string path = _fileProvider.MapPath(_directory);
            }
            catch (Exception ex)
            {
                throw new Exception(_directory + " " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Insert picture to blob
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <param name="contentType">Content Type</param>
        /// <param name="binary">Binary</param>
        public override string InsertFile(string fileName, string contentType, byte[] binary)
        {
            string fullPath = _fileProvider.GetAbsolutePath(_directory, fileName);

            using (FileStream fs = new FileStream(fullPath, FileMode.Create))
            {
                fs.Write(binary, 0, binary.Length);
            }

            var downloadUrl = _fileProvider.GetVirtualPath(fullPath);
            downloadUrl = $"{downloadUrl.TrimStart('~')}/{fileName}";
            return  downloadUrl;
        }

        /// <summary>
        /// Get File 
        /// </summary>
        /// <param name="fileName">File Name</param>
        public override byte[] GetFile(string fileName)
        {
            var fullPath = _fileProvider.GetAbsolutePath(_directory, fileName);
            return _fileProvider.ReadAllBytes(fullPath);
        }

        /// <summary>
        /// Delete file from directory
        /// </summary>
        /// <param name="fileName">fileName</param>
        public override void DeleteFile(string fileName)
        {
            var fullPath = _fileProvider.GetAbsolutePath(_directory, fileName);
            _fileProvider.DeleteFile(fullPath);
        }


        /// <summary>
        /// Delete file from blob
        /// </summary>
        /// <param name="filter">Blob ID</param>
        public override void DeleteFileByPrefix(string filter)
        {
            throw new NotImplementedException();
        }

        public override void MoveFile(string sourcePath, string targetPath)
        {
            base.MoveFile(sourcePath, targetPath);
        }

        public override void CopyFile(string sourcePath, string targetPath)
        {
            base.CopyFile(sourcePath, targetPath);
        }

        public override void RenameFile(string path, string newName)
        {
            base.RenameFile(path, newName);
        }

        public override string GetFileUrlByUniqName(string fileName)
        {
            string fullPath = _fileProvider.GetAbsolutePath(_directory, fileName);

            var downloadUrl = _fileProvider.GetVirtualPath(fullPath);
            downloadUrl = $"{downloadUrl.TrimStart('~')}/{fileName}";
            return downloadUrl;
        }
        

        public override List<string> GetFiles(string relativePath, bool includeSubDirectories = false,
            bool includeDirectoryPlaceholders = false)
        {
            throw new NotImplementedException();
        }

        public override bool IsFileExist(string fileName)
        {
            string fullPath = _fileProvider.GetAbsolutePath(_directory, fileName);
            return _fileProvider.FileExists(fullPath);

        }

        public override string UpdateFile(string fileName, string contentType, byte[] binary)
        {
            this.DeleteFile(fileName);
            return this.InsertFile(fileName, contentType, binary);
        }

        public override FileData GetFileData(string fileName)
        {
            throw new NotImplementedException();
        }

        public override void CreateDirectory(string parentDirectoryPath, string name)
        {
            throw new NotImplementedException();
        }

        public override void MoveDirectory(string sourcePath, string targetPath)
        {
            throw new NotImplementedException();
        }

        public override void CopyDirectory(string sourcePath, string targetPath)
        {
            throw new NotImplementedException();
        }

        public override void RenameDirectory(string path, string newName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

