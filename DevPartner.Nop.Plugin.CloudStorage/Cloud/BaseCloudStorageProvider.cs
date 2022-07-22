using DevPartner.Nop.Plugin.CloudStorage.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        #region Methods

        /// <summary>
        /// Check if File exist
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <returns></returns>
        public abstract bool FileExists(string fileName);

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
        public abstract FileData GetFileData(string fileName);

        public virtual async Task MoveFileAsync(string sourcePath, string targetPath)
        {
            var bytes = await ReadAllBytesAsync(sourcePath);
            var targetDirectory = Path.GetDirectoryName(targetPath);
            //ensure that directory exists
            CreateDirectory(targetDirectory);
            await WriteAllBytesAsync(targetPath, bytes);
            DeleteFile(sourcePath);
        }

        public virtual void DeleteDirectory(string path)
        {
            var files = GetFiles(path);
            foreach (var file in files)
            {
                DeleteFile(file);
            }
        }
       
        public virtual void CopyDirectory(string sourcePath, string targetPath)
        {
            var destinationDirectoryName = sourcePath.Split("/".ToCharArray()).Last();
            var filesToCopy = GetFiles(sourcePath);

            foreach (var filePath in filesToCopy)
            {
                var fileRelativePath = filePath.Substring(sourcePath.Length + 1);
                FileCopy(filePath, string.Format("{0}/{1}/{2}", targetPath, destinationDirectoryName, fileRelativePath));
            }
        }

        public virtual async Task RenameDirectoryAsync(string path, string newName)
        {
            var newPath = $"{path.Substring(0, path.Length - path.Split("/".ToCharArray()).Last().Length)}{newName}";
            foreach (var filePath in GetFiles(path))
            {
                var newFilePath = filePath.Replace(path, newPath);
                await MoveFileAsync(filePath, newFilePath);
            }
        }


        public abstract string[] GetFiles(string directoryPath, string searchPattern = "", bool topDirectoryOnly = true);

        public abstract void CreateDirectory(string path);

        public abstract string GetVirtualPath(string filePath);

        public abstract string[] GetDirectories(string path, string searchPattern = "", bool topDirectoryOnly = true);

        public abstract void DirectoryMove(string sourceDirName, string destDirName);

        public abstract bool DirectoryExists(string path);

        public abstract void FileCopy(string sourcePath, string targetPath, bool overwrite = false);

        public abstract long FileLength(string filePath);

        public abstract void FileMove(string filePath, string destFilePath);

        public abstract DateTime GetLastWriteTime(string path);

        public abstract string GetParentDirectory(string filePath);

        public virtual Task WriteAllBytesAsync(string filePath, byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public virtual Task<byte[]> ReadAllBytesAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
