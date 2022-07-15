using System.Collections.Generic;
using System.IO;
using System.Linq;
using DevPartner.Nop.Plugin.CloudStorage.Domain;
using DevPartner.Nop.Plugin.CloudStorage.Extensions;

namespace DevPartner.Nop.Plugin.CloudStorage.Cloud
{
    public abstract class BaseCloudStorageProvider : ICloudContentProvider, ICloudDownloadProvider, ICloudPictureProvider, ICloudThumbPictureProvider
    {
        #region Fields

        protected const string CLOUD_DIRECTORY_PLACEHOLDER_FILE_NAME = "directory_placeholder";

        #endregion

        #region Utilites

        protected virtual List<string> RemoveDirectoryPlacehoders(List<string> fileList)
        {
            fileList.RemoveAll(f => f.EndsWith(CLOUD_DIRECTORY_PLACEHOLDER_FILE_NAME));
            return fileList;
        }

        #endregion

        /// <summary>
        /// Insert file to the cloud storage
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <param name="contentType">Content Type</param>
        /// <param name="binary">Binary</param>
        public abstract string InsertFile(string fileName, string contentType, byte[] binary);
        /// <summary>
        /// Check if File exist
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <returns></returns>
        public abstract bool IsFileExist(string fileName);
        /// <summary>
        /// Get File 
        /// </summary>
        /// <param name="fileName">File Name</param>
        public abstract byte[] GetFile(string fileName);
        /// <summary>
        /// Delete file
        /// </summary>
        /// <param name="fileName">File Name</param>
        public abstract void DeleteFile(string fileName);

        /// <summary>
        /// Delete file by prefix
        /// </summary>
        /// <param name="filter">File Name Prefix</param>
        public abstract void DeleteFileByPrefix(string filter);
        /// <summary>
        /// Update file in the cloud storage
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <param name="contentType">Content Type</param>
        /// <param name="binary">Binary</param>
        /// <returns></returns>
        public abstract string UpdateFile(string fileName, string contentType, byte[] binary);

        public virtual void MoveFile(string sourcePath, string targetPath)
        {
            var bytes = GetFile(sourcePath);
            InsertFile(targetPath, targetPath.GetMimeFromExtension(), bytes);
            DeleteFile(sourcePath);
        }

        public virtual void CopyFile(string sourcePath, string targetPath)
        {
            InsertFile(targetPath, targetPath.GetMimeFromExtension(), GetFile(sourcePath));
        }

        public virtual void RenameFile(string path, string newName)
        {
            var newPath = $"{path.Substring(0, path.Length - Path.GetFileName(path).Length)}{newName}";
            MoveFile(path, newPath);
        }

        /// <summary>
        /// Get Url by File name
        /// </summary>
        /// <param name="fileName">URL. For example Blob URL</param>
        /// <returns>URL (blob ). It depends on provider settings</returns>
        public abstract string GetFileUrlByUniqName(string fileName);

        public List<string> GetFiles(string relativePath, string type, bool includeSubDirectories = false,
            bool includeDirectoryPlaceholders = false)
        {
            throw new System.NotImplementedException();
        }

        public abstract List<string> GetFiles(string relativePath, bool includeSubDirectories = false,
            bool includeDirectoryPlaceholders = false);

        public virtual List<string> GetDirectories(string relativePath = "", bool includeSubDirectories = false)
        {
            var files = GetFiles(relativePath, includeSubDirectories, true);
            var directories = files.Select(f => f.Substring(0, f.LastIndexOf('/'))).Distinct().ToList();
            directories.RemoveAll(d => d == relativePath);

            //Add empty directories to list
            var directoryList = new List<string>();
            directories.ForEach(d =>
            {
                directoryList.Add(d);
                var directoryNames = d.Split("/".ToCharArray());
                var directoryPath = string.Empty;
                for (int i = 0; i < directoryNames.Count(); i++)
                {
                    directoryPath += !string.IsNullOrEmpty(directoryPath) ? "/" : "";
                    directoryPath += directoryNames[i];
                    directoryList.Add(directoryPath);
                }
            });
            directoryList = directoryList.Distinct().OrderBy(d => d).ToList();
            directoryList.RemoveAll(d => !d.StartsWith(relativePath));
            directoryList.Remove(relativePath);

            return directoryList;
        }

        public abstract FileData GetFileData(string fileName);
        
        public abstract void CreateDirectory(string parentDirectoryPath, string name);

        public virtual void DeleteDirectory(string path)
        {
            var files = GetFiles(path, true, true);
            foreach (var file in files)
            {
                DeleteFile(file);
            }
        }

        public virtual void MoveDirectory(string sourcePath, string targetPath)
        {
            var dirName = sourcePath.Split("/".ToCharArray()).Last();
            var files = GetFiles(sourcePath, true, true);

            if (sourcePath != targetPath)
            {
                foreach (var filePath in files)
                {
                    var newFilePath = targetPath + "/" + dirName + filePath.Substring(sourcePath.Length);
                    MoveFile(filePath, newFilePath);
                }
            }
        }

        public virtual void CopyDirectory(string sourcePath, string targetPath)
        {
            var destinationDirectoryName = sourcePath.Split("/".ToCharArray()).Last();
            var filesToCopy = GetFiles(sourcePath, true, true);

            foreach (var filePath in filesToCopy)
            {
                var fileRelativePath = filePath.Substring(sourcePath.Length + 1);
                CopyFile(filePath, string.Format("{0}/{1}/{2}", targetPath, destinationDirectoryName, fileRelativePath));
            }
        }

        public virtual void RenameDirectory(string path, string newName)
        {
            var newPath = $"{path.Substring(0, path.Length - path.Split("/".ToCharArray()).Last().Length)}{newName}";
            foreach (var filePath in GetFiles(path, true, true))
            {
                var newFilePath = filePath.Replace(path, newPath);
                MoveFile(filePath, newFilePath);
            }
        }

        public virtual void DownloadToStream(string s, MemoryStream stream)
        {
            throw new System.NotImplementedException();
        }

        public virtual string GenerateUrl(string thumbFileName)
        {
            throw new System.NotImplementedException();
        }
    }
}
