using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Configuration;
using DevPartner.Nop.Plugin.CloudStorage.Extensions;
using Microsoft.AspNetCore.Hosting;
using Nop.Core.Infrastructure;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DevPartner.Nop.Plugin.CloudStorage.Services.NopServices
{
    /// <summary>
    /// IO functions using cloud file system
    /// </summary>
    public class CloudFileProvider : NopFileProvider
    {
        #region Properties
        /// <summary>
        /// A value indicating cloud config rules
        /// </summary>
        private CloudConfig _cloudConfig { get; set; }
        #endregion

        #region Utils
        protected string GetCloudPath(string fileOrDirPath)
        {
            if (!CloudHelper.FileProvider.IsNull())
            {
                var rule = _cloudConfig.FileProviderRuleConfig.FirstOrDefault(x => Regex.IsMatch(fileOrDirPath, x.Pattern));
                if (rule != null)
                {
                    var path = Regex.Replace(fileOrDirPath, rule.Pattern, rule.Replace);
                    return path;
                }
            }
            return null;
        }
        #endregion

        #region Ctor
        public CloudFileProvider(IWebHostEnvironment webHostEnvironment,
            CloudConfig cloudConfig
            ) : base(webHostEnvironment)
        {
            _cloudConfig = cloudConfig;
        }
        #endregion

        #region Override Base 
        /// <summary>
        /// Writes the specified byte array to the file
        /// </summary>
        /// <param name="filePath">The file to write to</param>
        /// <param name="bytes">The bytes to write to the file</param>
        public override async Task WriteAllBytesAsync(string filePath, byte[] bytes)
        {
            var path = GetCloudPath(filePath);
            if (!String.IsNullOrEmpty(path))
            {
                await CloudHelper.FileProvider.WriteAllBytesAsync(path, bytes);
                return;
            }
            await base.WriteAllBytesAsync(filePath, bytes);
        }

        /// <summary>
        /// Writes the specified byte array to the file
        /// </summary>
        /// <param name="filePath">The file to write to</param>
        /// <param name="bytes">The bytes to write to the file</param>
        public override async Task<byte[]> ReadAllBytesAsync(string filePath)
        {
            var path = GetCloudPath(filePath);
            if (!String.IsNullOrEmpty(path))
            {
                return await CloudHelper.FileProvider.ReadAllBytesAsync(path);
            }

            return await base.ReadAllBytesAsync(filePath);
        }

        public override void DeleteFile(string filePath)
        {
            var path = GetCloudPath(filePath);
            if (!String.IsNullOrEmpty(path))
            {
                CloudHelper.FileProvider.DeleteFile(path);
                return;
            }
            base.DeleteFile(filePath);
        }


        /// <summary>
        /// Returns the names of files (including their paths) that match the specified search
        /// pattern in the specified directory, using a value to determine whether to search subdirectories.
        /// </summary>
        /// <param name="directoryPath">The path to the directory to search</param>
        /// <param name="searchPattern">
        /// The search string to match against the names of files in path. This parameter
        /// can contain a combination of valid literal path and wildcard (* and ?) characters
        /// , but doesn't support regular expressions.
        /// </param>
        /// <param name="topDirectoryOnly">
        /// Specifies whether to search the current directory, or the current directory and all
        /// subdirectories
        /// </param>
        /// <returns>
        /// An array of the full names (including paths) for the files in the specified directory
        /// that match the specified search pattern, or an empty array if no files are found.
        /// </returns>
        public override string[] GetFiles(string directoryPath, string searchPattern = "", bool topDirectoryOnly = true)
        {
            var path = GetCloudPath(directoryPath);
            if (!String.IsNullOrEmpty(path))
            {
                return CloudHelper.FileProvider.GetFiles(path, searchPattern, topDirectoryOnly);
            }

            return base.GetFiles(directoryPath, searchPattern, topDirectoryOnly);
        }

        /// <summary>
        /// Returns the absolute path to the directory
        /// </summary>
        /// <param name="paths">An array of parts of the path</param>
        /// <returns>The absolute path to the directory</returns>
        /*public override string GetAbsolutePath(params string[] paths)
        {
            var imagesPath = base.Combine(WebRootPath, "\\images");
            var absolutePath = base.GetAbsolutePath(paths);
            if (!CloudHelper.FileProvider.IsNull() && absolutePath.StartsWith(imagesPath, StringComparison.InvariantCulture))
            {

                var path = absolutePath.Substring(imagesPath.Length);
                absolutePath = CloudHelper.FileProvider.GetAbsolutePath(path);
            }
            return absolutePath;
        }*/

        /// <summary>
        /// Creates all directories and subdirectories in the specified path unless they already exist
        /// </summary>
        /// <param name="path">The directory to create</param>
        public override void CreateDirectory(string directoryPath)
        {
            var path = GetCloudPath(directoryPath);
            if (!String.IsNullOrEmpty(path))
            {
                CloudHelper.FileProvider.CreateDirectory(path);
                return;
            }

            base.CreateDirectory(directoryPath);
        }



        /// <summary>
        /// Determines whether the specified file exists
        /// </summary>
        /// <param name="filePath">The file to check</param>
        /// <returns>
        /// True if the caller has the required permissions and path contains the name of an existing file; otherwise,
        /// false.
        /// </returns>
        public override bool FileExists(string filePath)
        {
            var path = GetCloudPath(filePath);
            if (!String.IsNullOrEmpty(path))
            {
                return CloudHelper.FileProvider.FileExists(path);
            }

            return base.FileExists(filePath);
        }

        /// <summary>
        /// Gets a virtual path from a physical disk path.
        /// </summary>
        /// <param name="path">The physical disk path</param>
        /// <returns>The virtual path. E.g. "~/bin"</returns>
        public override string GetVirtualPath(string path)
        {
            var cloudPath = GetCloudPath(path);
            if (!String.IsNullOrEmpty(cloudPath))
            {
                return CloudHelper.FileProvider.GetVirtualPath(cloudPath);
            }

            return base.GetVirtualPath(path);
        }

        public new void DeleteDirectory(string path)
        {
            var cloudPath = GetCloudPath(path);
            if (!String.IsNullOrEmpty(cloudPath))
            {
                CloudHelper.FileProvider.DeleteDirectory(cloudPath);
                return;
            }
            base.DeleteDirectory(path);
        }

        public new bool DirectoryExists(string path)
        {
            var cloudPath = GetCloudPath(path);
            if (!String.IsNullOrEmpty(cloudPath))
            {
                return CloudHelper.FileProvider.DirectoryExists(cloudPath);
            }

            return base.DirectoryExists(path);
        }

        public override void FileCopy(string sourceFileName, string destFileName, bool overwrite = false)
        {
            var sourceCloudPath = GetCloudPath(sourceFileName);
            if (!String.IsNullOrEmpty(sourceCloudPath))
            {
                var destCloudPath = GetCloudPath(destFileName);
                if (!String.IsNullOrEmpty(destCloudPath))
                {
                    CloudHelper.FileProvider.FileCopy(sourceCloudPath, destCloudPath, overwrite);
                    return;
                }
            }

            base.FileCopy(sourceFileName, destFileName, overwrite);
        }

        public override long FileLength(string path)
        {
            var сloudPath = GetCloudPath(path);
            if (!String.IsNullOrEmpty(сloudPath))
            {
                return CloudHelper.FileProvider.FileLength(сloudPath);
            }

            return base.FileLength(path);
        }

        public override void FileMove(string sourceFileName, string destFileName)
        {
            var sourceCloudPath = GetCloudPath(sourceFileName);
            if (!String.IsNullOrEmpty(sourceCloudPath))
            {
                var destCloudPath = GetCloudPath(destFileName);
                if (!String.IsNullOrEmpty(destCloudPath))
                {
                    CloudHelper.FileProvider.FileMove(sourceCloudPath, destCloudPath);
                    return;
                }
            }

            base.FileMove(sourceFileName, destFileName);
        }

        public override string[] GetDirectories(string path, string searchPattern = "", bool topDirectoryOnly = true)
        {
            var сloudPath = GetCloudPath(path);
            if (!String.IsNullOrEmpty(сloudPath))
            {
                return CloudHelper.FileProvider.GetDirectories(сloudPath, searchPattern, topDirectoryOnly);
            }

            return base.GetDirectories(path, searchPattern, topDirectoryOnly);
        }
        public override DateTime GetLastWriteTime(string path)
        {
            var сloudPath = GetCloudPath(path);
            if (!String.IsNullOrEmpty(сloudPath))
            {
                return CloudHelper.FileProvider.GetLastWriteTime(сloudPath);
            }

            return base.GetLastWriteTime(path);
        }


        public override string GetParentDirectory(string directoryPath)
        {
            var сloudPath = GetCloudPath(directoryPath);
            if (!String.IsNullOrEmpty(сloudPath))
            {
                return CloudHelper.FileProvider.GetParentDirectory(сloudPath);
            }

            return base.GetParentDirectory(directoryPath);
        }

        /*
        public IFileInfo GetFileInfo(string subpath)
        {
            throw new NotImplementedException();
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            throw new NotImplementedException();
        }

        public IChangeToken Watch(string filter)
        {
            throw new NotImplementedException();
        }
        */
        #endregion

        #region Extend Methods to copy files from one provider to another
        public async Task WriteAllBytesAsync(string fullPath, byte[] bytes, ICloudStorageProvider providerService)
        {
            if (!providerService.IsNull())
            {
                await providerService.WriteAllBytesAsync(fullPath, bytes);
            }
            else
            {
                await base.WriteAllBytesAsync(fullPath, bytes);
            }
        }


        public async Task<byte[]> ReadAllBytesAsync(string path, ICloudStorageProvider providerService)
        {
            if (!providerService.IsNull())
            {
                return await providerService.ReadAllBytesAsync(path);
            }
            else
            {
                return await base.ReadAllBytesAsync(path);
            }
        }

        public string[] GetFiles(string directoryPath, string searchPattern, bool topDirectoryOnly, ICloudStorageProvider providerService)
        {
            if (!providerService.IsNull())
            {
                return providerService.GetFiles(directoryPath, searchPattern, topDirectoryOnly);
            }
            else
            {
                return base.GetFiles(directoryPath, searchPattern, topDirectoryOnly);
            }
        }

        public void CreateDirectory(string path, ICloudStorageProvider providerService)
        {
            if (!providerService.IsNull())
            {
                providerService.CreateDirectory(path);
            }
            else
            {
                base.CreateDirectory(path);
            }
        }

        public bool FileExists(string path, ICloudStorageProvider providerService)
        {
            if (!providerService.IsNull())
            {
                return providerService.FileExists(path);
            }
            else
            {
                return base.FileExists(path);
            }
        }

        public string GetVirtualPath(string filePath, ICloudStorageProvider providerService)
        {
            if (!providerService.IsNull())
            {
                return providerService.GetVirtualPath(filePath);
            }
            else
            {
                return base.GetVirtualPath(filePath);
            }
        }

        public string[] GetDirectories(string path, string searchPattern = "", bool topDirectoryOnly = true, ICloudStorageProvider providerService = null)
        {
            if (!providerService.IsNull())
            {
                return providerService.GetDirectories(path, searchPattern, topDirectoryOnly);
            }
            else
            {
                return base.GetDirectories(path, searchPattern, topDirectoryOnly);
            }
        }

        public void DirectoryMove(string sourcePath, string destPath, ICloudStorageProvider providerService = null)
        {
            if (!providerService.IsNull())
            {
                providerService.DirectoryMove(sourcePath, destPath);
            }
            else
            {
                base.DirectoryMove(sourcePath, destPath);
            }
        }


        public bool DirectoryExists(string fullPath, ICloudStorageProvider providerService = null)
        {
            return !providerService.IsNull() ? providerService.DirectoryExists(fullPath) : base.DirectoryExists(fullPath);
        }

        public void FileCopy(string source, string target, bool overwrite, ICloudStorageProvider providerService = null)
        {
            if (!providerService.IsNull())
            {
                providerService.FileCopy(source, target, overwrite);
            }
            else
            {
                base.FileCopy(source, target, overwrite);
            }
        }

        public long FileLength(string fullPath, ICloudStorageProvider providerService = null)
        {
            if (!providerService.IsNull())
            {
                return providerService.FileLength(fullPath);
            }
            else
            {
                return base.FileLength(fullPath);
            }
        }

        public void FileMove(string fullPath, string destPath, ICloudStorageProvider providerService = null)
        {
            if (!providerService.IsNull())
            {
                providerService.FileMove(fullPath, destPath);
            }
            else
            {
                base.FileMove(fullPath, destPath);
            }
        }

        public DateTime GetLastWriteTime(string fullPath, ICloudStorageProvider providerService = null)
        {
            if (!providerService.IsNull())
            {
                return providerService.GetLastWriteTime(fullPath);
            }
            else
            {
                return base.GetLastWriteTime(fullPath);
            }
        }

        public string GetParentDirectory(string fullPath, ICloudStorageProvider providerService = null)
        {
            if (!providerService.IsNull())
            {
                return providerService.GetParentDirectory(fullPath);
            }
            else
            {
                return base.GetParentDirectory(fullPath);
            }
        }


        public void DeleteFile(string filePath, ICloudStorageProvider providerService = null)
        {
            if (!providerService.IsNull())
            {
                providerService.DeleteFile(filePath);
            }
            else
            {
                base.DeleteFile(filePath);
            }
        }

        #endregion
    }
}
