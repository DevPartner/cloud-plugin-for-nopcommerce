using System.Collections.Generic;
using System.IO;
using DevPartner.Nop.Plugin.CloudStorage.Domain;

namespace DevPartner.Nop.Plugin.CloudStorage.Cloud
{
    /// <summary>
    /// Cloud Storage Provider Service Interface
    /// </summary>
    public interface ICloudStorageProvider
    {
        /// <summary>
        /// Insert file to the storage
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <param name="contentType">Content Type</param>
        /// <param name="binary">Binary</param>
        string InsertFile(string fileName, string contentType, byte[] binary);


        void DownloadToStream(string s, MemoryStream stream);

        /// <summary>
        /// Check if File exist
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <returns></returns>
        bool IsFileExist(string fileName);

        /// <summary>
        /// Get File 
        /// </summary>
        /// <param name="fileName">File Name</param>
        byte[] GetFile(string fileName);

        /// <summary>
        /// Delete file
        /// </summary>
        /// <param name="fileName">File Name</param>
        void DeleteFile(string fileName);

        /// <summary>
        /// Delete file prefix
        /// </summary>
        /// <param name="filter">File Prefix</param>
        void DeleteFileByPrefix(string filter);

        /// <summary>
        /// Update file in the storage
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <param name="contentType">Content Type</param>
        /// <param name="binary">Binary</param>
        /// <returns></returns>
        string UpdateFile(string fileName, string contentType, byte[] binary);

        /// <summary>
        /// Move file from source path to target
        /// </summary>
        /// <param name="sourcePath">Source File Path</param>
        /// <param name="targetPath">Target File Path</param>
        void MoveFile(string sourcePath, string targetPath);

        /// <summary>
        /// Copy file from source path to target
        /// </summary>
        /// <param name="sourcePath">Source File Path</param>
        /// <param name="targetPath">Target File Path</param>
        void CopyFile(string sourcePath, string targetPath);

        /// <summary>
        /// Renames file
        /// </summary>
        /// <param name="path">File Path</param>
        /// <param name="newName">New Name</param>
        void RenameFile(string path, string newName);

        /// <summary>
        /// Get Url by File name
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <returns>URL (blob ). It depends on provider settings</returns>
        string GetFileUrlByUniqName(string fileName);

        /// <summary>
        /// Get file URL 
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <returns>Local file path</returns>
        string GenerateUrl(string thumbFileName);


        /// <summary>
        /// Gets list of files in selected location on storage
        /// </summary>
        /// <param name="relativePath">path (starting from root)</param>
        /// <param name="includeSubDirectories">If true include files in subdirectories</param>
        /// <param name="includeDirectoryPlaceholders">If true include placeholders (used for creating directories on cloud)</param>
        /// <returns>List of files</returns>
        List<string> GetFiles(string relativePath, bool includeSubDirectories = false, 
            bool includeDirectoryPlaceholders = false);

        /// <summary>
        /// Gets list of directories in selected location on storage
        /// </summary>
        /// <param name="relativePath">path (starting from root)</param>
        /// <param name="includeSubDirectories">If true include directories in subdirectories</param>
        /// <returns>List of directories</returns>
        List<string> GetDirectories(string relativePath="", bool includeSubDirectories = false); 

        ///// <summary>
        ///// Get root url
        ///// </summary>
        ///// <returns>Root url</returns>
        //string GetRootUrl();

        /// <summary>
        /// Get file data (size, upload date and etc)
        /// </summary>
        /// <param name="filePath">path (starting from root)</param>
        /// <returns>File Data</returns>
        FileData GetFileData(string filePath);
        
        /// <summary>
        /// Creates new directory in storage.
        /// </summary>
        /// <param name="sourcePath">Source Directory Path</param>
        /// <param name="targetPath">Target Directory Path</param>
        void MoveDirectory(string sourcePath, string targetPath);

        /// <summary>
        /// Creates new directory in storage.
        /// </summary>
        /// <param name="sourcePath">Source Directory Path</param>
        /// <param name="targetPath">Target Directory Path</param>
        void CopyDirectory(string sourcePath, string targetPath);

        /// <summary>
        /// Renames directory in storage.
        /// </summary>
        /// <param name="path">path (starting from root)</param>
        /// <param name="newName">New directory name</param>
        void RenameDirectory(string path, string newName);
    }
}
