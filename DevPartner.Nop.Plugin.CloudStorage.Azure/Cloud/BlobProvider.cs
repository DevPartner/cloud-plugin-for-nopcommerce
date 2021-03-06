using DevPartner.Nop.Plugin.CloudStorage.Azure.Infrastructure;
using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Domain;
using Nop.Core.Caching;
using Nop.Core.Domain.Media;
using Nop.Services.Localization;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DevPartner.Nop.Plugin.CloudStorage.Azure.Cloud
{
    public class BlobProvider : BaseCloudStorageProvider, ICloudStorageProvider
    {
        #region Fields
        private readonly BlobMethods _blobMethods;
        private readonly IStaticCacheManager _cacheManager;
        private readonly MediaSettings _mediaSettings;
        private readonly AzureBlobProviderSettings _azureBlobProviderSettings;
        private string _container;
        private string _endPoint;
        #endregion

        #region Ctor
        public BlobProvider(AzureBlobProviderSettings azureBlobProviderSettings, ILocalizationService localizationService,
            IStaticCacheManager cacheManager, MediaSettings mediaSettings)
        {
            _blobMethods = new BlobMethods();
            _azureBlobProviderSettings = azureBlobProviderSettings;
            _cacheManager = cacheManager;
            _mediaSettings = mediaSettings;
        }

        #endregion

        #region Utils
        public static byte[] ReadToEnd(MemoryStream stream)
        {
            return stream.ToArray();
            /*
            using(var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }*/
            /*
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }*/
        }

        protected virtual async Task<bool> IsFileDirExsitAsync(string thumbFileName)
        {
            try
            {
                var cacheKey = _cacheManager.PrepareKeyForDefaultCache(AzureDefaults.BlobExistsCacheKey, _container, thumbFileName);
                return await _cacheManager.Get(cacheKey, async () =>
                {
                    return await _blobMethods.ExistsAsync(thumbFileName);
                });
            }
            catch { return false; }
        }


        #endregion

        #region Methods
        public void RunAtAppStartup(string container, string endPoint)
        {
            if (string.IsNullOrEmpty(_azureBlobProviderSettings.ConnectionString))
                throw new Exception("Azure connection string for BLOB is not specified");
            _container = container;
            _endPoint = endPoint;
            if (string.IsNullOrEmpty(_container))
                throw new Exception("Azure container name for BLOB is not specified");

            if (string.IsNullOrEmpty(_endPoint))
                throw new Exception("Azure end point for BLOB is not specified");

            try
            {
                _blobMethods.RunAtAppStartup(_azureBlobProviderSettings.ConnectionString, _container);
            }
            catch (Exception ex)
            {
                throw new Exception(_container + " " + ex.Message, ex);
            }
        }
        /*
        /// <summary>
        /// Insert picture to blob
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <param name="contentType">Content Type</param>
        /// <param name="binary">Binary</param>
        public override string InsertFile(string fileName, string contentType, byte[] binary)
        {
            var result = _blobMethods.UploadFromByteArrayAsync(fileName, contentType, binary, _mediaSettings.AzureCacheControlHeader);

            //_cacheManager.RemoveByPattern(String.Format(CONTAINER_PATTERN_KEY, _container));
            _cacheManager.RemoveByPrefix(String.Format(CONTAINER_PATTERN_KEY, _container));

            return result.Result;
        }*/


        /// <summary>
        /// Delete file from blob
        /// </summary>
        /// <param name="fileName">Blob ID</param>
        public override void DeleteFile(string fileName)
        {
            _blobMethods.DeleteBlob(fileName);
        }

        /// <summary>
        /// Delete file from blob
        /// </summary>
        /// <param name="filter">Blob ID</param>
        public override void DeleteFileByPrefix(string filter)
        {
            _blobMethods.DeleteBlobByPrefixAsync(filter);

            _cacheManager.RemoveByPrefixAsync(String.Format(AzureDefaults.CONTAINER_PATTERN_KEY, _container));
        }
        /*
        public override void MoveFile(string sourcePath, string targetPath)
        {
            _blobMethods.CopyBlob(sourcePath, targetPath, true);
        }*/

        //public override void CopyFile(string sourcePath, string targetPath)
        //{
        //    _blobMethods.CopyBlob(sourcePath, targetPath);
        //}
        /*
        public override async Task RenameFileAsync(string path, string newName)
        {
            var newPath = string.Format("{0}{1}", path.Substring(0, path.Length - Path.GetFileName(path).Length), newName);
            _blobMethods.CopyBlob(path, newPath, true);
        }*/
        /*
        public override string GetFileUrlByUniqName(string fileName)
        {
            return _blobMethods.GetUrlByUniqName(fileName);
        }*/
        /// <summary>
        /// Get picture (thumb) URL 
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <returns>Local picture thumb path</returns>
        /*public override string GenerateUrl(string thumbFileName)
        {
            var url = _endPoint + _container + "/";

            url = url + thumbFileName;
            return url;
        }*/

        /*
        public override bool IsFileExist(string fileName)
        {
            return IsFileExsitAsync(fileName).Result;
        }*/
        /*
        public override string UpdateFile(string fileName, string contentType, byte[] binary)
        {
            using (var stream = new MemoryStream(binary))
            {
                //stream.Write(binary, 0, binary.Length);
                var res = _blobMethods.UploadFromStream(stream, fileName, contentType);
                res.Wait();

                return _blobMethods.GetUrlByUniqName(fileName);
            }
        }*/

        public override FileData GetFileData(string fileName)
        {
            return _blobMethods.GetFileData(fileName);
        }
        /*
        public override void MoveDirectory(string sourcePath, string targetPath)
        {
            var destinationDirectoryName = sourcePath.Split("/".ToCharArray()).Last();
            foreach (var filePath in GetFiles(sourcePath))
            {
                var fileRelativePath = filePath.Substring(sourcePath.Length + 1);
                var newFilePath = string.Format("{0}/{1}/{2}",
                    targetPath, destinationDirectoryName, fileRelativePath);
                _blobMethods.CopyBlob(filePath, newFilePath, true);
            }
        }*/

        public override void CopyDirectory(string sourcePath, string targetPath)
        {
            var destinationDirectoryName = sourcePath.Split("/".ToCharArray()).Last();
            foreach (var filePath in GetFiles(sourcePath))
            {
                var fileRelativePath = filePath.Substring(sourcePath.Length + 1);
                var newFilePath = string.Format("{0}/{1}/{2}",
                    targetPath, destinationDirectoryName, fileRelativePath);
                _blobMethods.CopyBlob(filePath, newFilePath, false);
            }
        }

        public override async Task RenameDirectoryAsync(string path, string newName)
        {
            foreach (var filePath in GetFiles(path))
            {
                var newFilePath = string.Format("{0}{1}{2}",
                    path.Substring(0, path.Length - Path.GetFileName(path).Length), newName, filePath.Substring(path.Length));
                _blobMethods.CopyBlob(filePath, newFilePath, true);
            }
        }

        public override bool FileExists(string fileName)
        {
            return IsFileDirExsitAsync(fileName).Result;
        }

        public override async Task WriteAllBytesAsync(string filePath, byte[] bytes)
        {
            await Task.Run(() => {
                using (var stream = new MemoryStream(bytes))
                {
                    var exists = _blobMethods.ExistsAsync(filePath);
                    exists.Wait();
                    if (exists.Result)
                        _blobMethods.DeleteBlob(filePath);

                    //stream.Write(binary, 0, binary.Length);
                    var res = _blobMethods.UploadFromStream(stream, filePath);
                    res.Wait();
                }
            });
        }

        public override async Task<byte[]> ReadAllBytesAsync(string fileName)
        {

            var stream = _blobMethods.DownloadToStream(fileName);

            return ReadToEnd(stream.Result);
        }

        public override string[] GetFiles(string directoryPath, string searchPattern = "", bool topDirectoryOnly = true)
        {
            //set relative path
            var relativePath = directoryPath;
            var files =  _blobMethods.GetFileListForRelPath(relativePath).Result;
            if (topDirectoryOnly)
                files.RemoveAll(f => f.Substring(relativePath.Length + 1).Contains(@"/"));
            return files.ToArray();
        }

        public override void CreateDirectory(string path)
        {
            var bytes = new byte[0];
            if (!path.EndsWith("/"))
                path = string.Concat(path, "/");

            var exists = _blobMethods.ExistsAsync(path);
            exists.Wait();
            if (!exists.Result)
            {
                using (var stream = new MemoryStream(bytes))
                {
                    var res = _blobMethods.UploadFromStream(stream, path);
                    res.Wait();
                }
            }
        }

        public override string GetVirtualPath(string filePath)
        {
            var url = _endPoint + _container + "/";

            url = url + filePath;
            return url;
        }

        public override string[] GetDirectories(string path, string searchPattern = "", bool topDirectoryOnly = true)
        {
            var directoryPath = path;
            //set relative path
            var relativePath = directoryPath;
            var files = _blobMethods.GetFileListForRelPath(relativePath).Result;
            if (topDirectoryOnly)
                files.RemoveAll(f => f.Substring(relativePath.Length + 1).Contains(@"/"));
            return files.ToArray();
        }

        public override void DirectoryMove(string sourceDirName, string destDirName)
        {
            throw new NotImplementedException();
        }

        public override bool DirectoryExists(string path)
        {
            if (!path.EndsWith("/"))
                path = string.Concat(path, "/");
            return IsFileDirExsitAsync(path).Result;
        }

        public override void FileCopy(string sourcePath, string targetPath, bool overwrite = false)
        {
            throw new NotImplementedException();
        }

        public override long FileLength(string filePath)
        {
            throw new NotImplementedException();
        }

        public override void FileMove(string filePath, string destFilePath)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetLastWriteTime(string path)
        {
            throw new NotImplementedException();
        }

        public override string GetParentDirectory(string filePath)
        {
            throw new NotImplementedException();
        }



        #endregion
    }
}

