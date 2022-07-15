using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DevPartner.Nop.Plugin.CloudStorage.Attributes;
using DevPartner.Nop.Plugin.CloudStorage.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;

namespace DevPartner.Nop.Plugin.CloudStorage.Cloud
{
    /// <summary>
    /// Cloud Storage Provider Service
    /// </summary>
    public class FileCloudStorageProvider :  ICloudContentProvider, ICloudDownloadProvider, ICloudPictureProvider, ICloudThumbPictureProvider
    {

        #region Fields
        private readonly ILocalizationService _localizationService;
        private readonly INopFileProvider _fileProvider;
        #endregion
        public FileCloudStorageProvider(
            ILocalizationService localizationService, INopFileProvider fileProvider)
        {
            _localizationService = localizationService;
            _fileProvider = fileProvider;
        }


        /// <summary>
        /// Get the unique name of the file (add -copy-(N) to the file name if there is already a file with that name in the directory)
        /// </summary>
        /// <param name="directoryPath">Path to the file directory</param>
        /// <param name="fileName">Original file name</param>
        /// <returns>Unique name of the file</returns>
        protected virtual string GetUniqueFileName(string directoryPath, string fileName)
        {
            var uniqueFileName = fileName;

            var i = 0;
            while (System.IO.File.Exists(Path.Combine(directoryPath, uniqueFileName)))
            {
                uniqueFileName = $"{Path.GetFileNameWithoutExtension(fileName)}-Copy-{++i}{Path.GetExtension(fileName)}";
            }

            return uniqueFileName;
        }

        public string InsertFile(string fileName, string contentType, byte[] binary)
        {
            throw new System.NotImplementedException();
        }

        public bool IsFileExist(string fileName)
        {
            throw new System.NotImplementedException();
        }

        public byte[] GetFile(string fileName)
        {
            throw new System.NotImplementedException();
        }

        public void DeleteFile(string fileName)
        {
            DeleteFileAsync(fileName).Wait();
        }


        /// <summary>
        /// Delete the file
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>A task that represents the completion of the operation</returns>
        protected virtual async Task DeleteFileAsync(string path)
        {
            path = _fileProvider.MapPath(path);
            //if (!File.Exists(path))
            //    throw new Exception(_localizationService.GetResource("DevPartner.CloudStorage.FileProvider.DeleteFileInvalidPath"));

            try
            {
                File.Delete(path);
            }
            catch
            {
                //throw new Exception(_localizationService.GetResource("DevPartner.CloudStorage.FileProvider.DeletеFile"));
            }
        }


        public void DeleteFileByPrefix(string filter)
        {
            throw new System.NotImplementedException();
        }

        public string UpdateFile(string fileName, string contentType, byte[] binary)
        {
            throw new System.NotImplementedException();
        }

        public void MoveFile(string sourcePath, string targetPath)
        {
            throw new System.NotImplementedException();
        }

        public void CopyFile(string sourcePath, string destinationPath)
        {
            CopyFileAsync(sourcePath, destinationPath).Wait();
        }


        /// <summary>
        /// Copy the file
        /// </summary>
        /// <param name="sourcePath">Path to the source file</param>
        /// <param name="destinationPath">Path to the destination file</param>
        /// <returns>A task that represents the completion of the operation</returns>
        protected virtual async Task CopyFileAsync(string sourcePath, string destinationPath)
        {
            var filePath = _fileProvider.MapPath(sourcePath);
            var file = new FileInfo(filePath);
            //if (!file.Exists)
            //    throw new Exception(_localizationService.GetResource("DevPartner.CloudStorage.FileProvider.CopyFileInvalisPath"));

            destinationPath = _fileProvider.MapPath(destinationPath);
            var newFileName = GetUniqueFileName(destinationPath, file.Name);
            try
            {
                File.Copy(file.FullName, Path.Combine(destinationPath, newFileName));
            }
            catch
            {
                //throw new Exception(_localizationService.GetResource("DevPartner.CloudStorage.FileProvider.CopyFile"));
            }
        }

        public void RenameFile(string path, string newName)
        {
            throw new System.NotImplementedException();
        }
        
        public List<string> GetFiles(string relativePath, bool includeSubDirectories = false,
            bool includeDirectoryPlaceholders = false)
        {
            throw new System.NotImplementedException();
        }

        public List<string> GetDirectories(string relativePath = "", bool includeSubDirectories = false)
        {
            throw new System.NotImplementedException();
        }
        
        public FileData GetFileData(string filePath)
        {
            throw new System.NotImplementedException();
        }

        public void DeleteDirectory(string path)
        {
            throw new System.NotImplementedException();
        }

        public void MoveDirectory(string sourcePath, string targetPath)
        {
            throw new System.NotImplementedException();
        }

        public void CopyDirectory(string sourcePath, string targetPath)
        {
            throw new System.NotImplementedException();
        }

        public void RenameDirectory(string path, string newName)
        {
            throw new System.NotImplementedException();
        }

        public string GetFileUrlByUniqName(string fileName)
        {
            throw new System.NotImplementedException();
        }

        public void DownloadToStream(string s, MemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public string GenerateUrl(string thumbFileName)
        {
            throw new NotImplementedException();
        }
    }
}
