using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Domain;
using DevPartner.Nop.Plugin.CloudStorage.Services.NopServices;
using System;
using System.Threading.Tasks;

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
        private readonly CloudFileProvider _fileProvider;
        private string _directory;
        #endregion

        #region Ctor
        public FileProvider(CloudFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }
        #endregion

        #region Methods
        public void RunAtAppStartup(string directory)
        {
            _directory = directory;
            if (string.IsNullOrEmpty(_directory))
                throw new Exception("Directory is not specified");

            try
            {
                //ensure _directory exists
                _fileProvider.CreateDirectory(_fileProvider.GetAbsolutePath(_directory));
            }
            catch (Exception ex)
            {
                throw new Exception(_directory + " " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Delete file from directory
        /// </summary>
        /// <param name="fileName">fileName</param>
        public override void DeleteFile(string fileName)
        {
            var fullPath = _fileProvider.GetAbsolutePath(_directory, fileName);
            _fileProvider.DeleteFile(fullPath, null);
        }


        /// <summary>
        /// Delete file from blob
        /// </summary>
        /// <param name="filter">Blob ID</param>
        public override void DeleteFileByPrefix(string filter)
        {
            throw new NotImplementedException();
        }

        /*
        public override async Task FileCopyAsync(string sourcePath, string targetPath)
        {
            base.FileCopy(sourcePath, targetPath);
        }*/
        /*
        public override void RenameFile(string path, string newName)
        {
            base.RenameFile(path, newName);
        }*/


        public override bool FileExists(string fileName)
        {
            string fullPath = _fileProvider.GetAbsolutePath(_directory, fileName);
            return _fileProvider.FileExists(fullPath, null);
        }

        public override FileData GetFileData(string fileName)
        {
            throw new NotImplementedException();
        }

        public override void CopyDirectory(string sourcePath, string targetPath)
        {
            throw new NotImplementedException();
        }

        public override Task RenameDirectoryAsync(string path, string newName)
        {
            throw new NotImplementedException();
        }

        public override async Task WriteAllBytesAsync(string filePath, byte[] bytes)
        {
            string fullPath = _fileProvider.GetAbsolutePath(_directory, filePath);
            await _fileProvider.WriteAllBytesAsync(fullPath, bytes, null);
        }

        public override async Task<byte[]> ReadAllBytesAsync(string path)
        {
            string fullPath = _fileProvider.GetAbsolutePath(_directory, path);
            return await _fileProvider.ReadAllBytesAsync(fullPath, null);
        }

        public override string[] GetFiles(string directoryPath, string searchPattern = "", bool topDirectoryOnly = true)
        {
            string fullPath = _fileProvider.GetAbsolutePath(_directory, directoryPath);
            //ensure _directory exists
            _fileProvider.CreateDirectory(fullPath, null);
            return _fileProvider.GetFiles(fullPath, searchPattern, topDirectoryOnly, null);
        }

        public override void CreateDirectory(string path)
        {
            string fullPath = _fileProvider.GetAbsolutePath(_directory, path);
            _fileProvider.CreateDirectory(fullPath, null);
        }

        public override string GetVirtualPath(string filePath)
        {
            string fullPath = _fileProvider.GetAbsolutePath(_directory, filePath);
            return _fileProvider.GetVirtualPath(fullPath, null);
        }

        public override string[] GetDirectories(string path, string searchPattern = "", bool topDirectoryOnly = true)
        {
            string fullPath = _fileProvider.GetAbsolutePath(_directory, path);
            //ensure _directory exists
            _fileProvider.CreateDirectory(fullPath);
            return _fileProvider.GetDirectories(fullPath, searchPattern, topDirectoryOnly, null);
        }

        public override void DirectoryMove(string sourceDirName, string destDirName)
        {
            string sourcePath = _fileProvider.GetAbsolutePath(_directory, sourceDirName);
            string destPath = _fileProvider.GetAbsolutePath(_directory, destDirName);
            _fileProvider.DirectoryMove(sourcePath, destPath, null);
        }

        public override bool DirectoryExists(string path)
        {
            string fullPath = _fileProvider.GetAbsolutePath(_directory, path);
            return _fileProvider.DirectoryExists(fullPath, null);
        }

        public override void FileCopy(string sourcePath, string targetPath, bool overwrite = false)
        {
            string source = _fileProvider.GetAbsolutePath(_directory, sourcePath);
            string target = _fileProvider.GetAbsolutePath(_directory, targetPath);

            _fileProvider.FileCopy(source, target, overwrite, null);
        }

        public override long FileLength(string filePath)
        {
            string fullPath = _fileProvider.GetAbsolutePath(_directory, filePath);
            return _fileProvider.FileLength(fullPath, null);
        }

        public override void FileMove(string filePath, string destFilePath)
        {
            string fullPath = _fileProvider.GetAbsolutePath(_directory, filePath);
            string destPath = _fileProvider.GetAbsolutePath(_directory, filePath);
            _fileProvider.FileMove(fullPath, destPath, null);
        }

        public override DateTime GetLastWriteTime(string path)
        {
            string fullPath = _fileProvider.GetAbsolutePath(_directory, path);
            return _fileProvider.GetLastWriteTime(fullPath, null);
        }

        public override string GetParentDirectory(string filePath)
        {
            string fullPath = _fileProvider.GetAbsolutePath(_directory, filePath);
            return _fileProvider.GetParentDirectory(fullPath, null);
        }

        #endregion
    }
}

