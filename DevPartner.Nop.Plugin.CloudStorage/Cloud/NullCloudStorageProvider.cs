using System.Collections.Generic;
using System.IO;
using DevPartner.Nop.Plugin.CloudStorage.Domain;

namespace DevPartner.Nop.Plugin.CloudStorage.Cloud
{
    /// <summary>
    /// Cloud Storage Provider Service
    /// </summary>
    public class NullCloudStorageProvider :  ICloudContentProvider, ICloudDownloadProvider, ICloudPictureProvider, ICloudThumbPictureProvider
    {
        public NullCloudStorageProvider()
        {
                
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
            throw new System.NotImplementedException();
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

        public void CopyFile(string sourcePath, string targetPath)
        {
            throw new System.NotImplementedException();
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

        public List<string> GetDirectories(string relativePath, bool includeSubDirectories = false)
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
            throw new System.NotImplementedException();
        }

        public string GenerateUrl(string thumbFileName)
        {
            throw new System.NotImplementedException();
        }
    }
}
