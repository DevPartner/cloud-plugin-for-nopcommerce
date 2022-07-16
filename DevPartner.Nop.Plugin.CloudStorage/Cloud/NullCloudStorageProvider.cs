using System;
using System.Threading.Tasks;
using DevPartner.Nop.Plugin.CloudStorage.Domain;

namespace DevPartner.Nop.Plugin.CloudStorage.Cloud
{
    /// <summary>
    /// Cloud Storage Provider Service
    /// </summary>
    public class NullCloudStorageProvider : ICloudContentProvider, ICloudDownloadProvider, ICloudPictureProvider, ICloudThumbPictureProvider
    {
        public NullCloudStorageProvider()
        {

        }

        public bool FileExists(string fileName)
        {
            throw new System.NotImplementedException();
        }

        public byte[] GetFile(string fileName)
        {
            throw new System.NotImplementedException();
        }


        public void DeleteFile(string fileName)
        {
            throw new System.NotImplementedException();
        }

        public void DeleteFileByPrefix(string filter)
        {
            throw new System.NotImplementedException();
        }

        public void MoveFile(string sourcePath, string targetPath)
        {
            throw new System.NotImplementedException();
        }

        public void FileCopy(string sourcePath, string targetPath)
        {
            throw new System.NotImplementedException();
        }

        public void RenameFile(string path, string newName)
        {
            throw new System.NotImplementedException();
        }

        public FileData GetFileData(string filePath)
        {
            throw new System.NotImplementedException();
        }

        public void CreateDirectory(string path)
        {
            throw new System.NotImplementedException();
        }

        public void DeleteDirectory(string path)
        {
            throw new System.NotImplementedException();
        }

        public void CopyDirectory(string sourcePath, string targetPath)
        {
            throw new System.NotImplementedException();
        }

        public void RenameDirectoryAsync(string path, string newName)
        {
            throw new System.NotImplementedException();
        }

        public void WriteAllBytes(string filePath, byte[] bytes)
        {
            throw new System.NotImplementedException();
        }

        public byte[] ReadAllBytes(string path)
        {
            throw new System.NotImplementedException();
        }

        public string[] GetFiles(string directoryPath, string searchPattern = "", bool topDirectoryOnly = true)
        {
            throw new System.NotImplementedException();
        }

        public string GetVirtualPath(string filePath)
        {
            throw new System.NotImplementedException();
        }

        public string[] GetDirectories(string path, string searchPattern = "", bool topDirectoryOnly = true)
        {
            throw new System.NotImplementedException();
        }

        public string GetDirectoryNameOnly(string path)
        {
            throw new System.NotImplementedException();
        }

        public void DirectoryMove(string sourceDirName, string destDirName)
        {
            throw new System.NotImplementedException();
        }

        public bool DirectoryExists(string path)
        {
            throw new System.NotImplementedException();
        }

        public void FileCopy(string sourcePath, string targetPath, bool overwrite = false)
        {
            throw new NotImplementedException();
        }

        public long FileLength(string filePath)
        {
            throw new NotImplementedException();
        }

        public void FileMove(string filePath, string destFilePath)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastWriteTime(string path)
        {
            throw new NotImplementedException();
        }

        public string GetParentDirectory(string filePath)
        {
            throw new NotImplementedException();
        }

        public Task WriteAllBytesAsync(string filePath, byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> ReadAllBytesAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        Task ICloudStorageProvider.RenameDirectoryAsync(string path, string newName)
        {
            throw new NotImplementedException();
        }
    }
}
