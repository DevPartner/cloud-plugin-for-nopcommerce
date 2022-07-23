using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DevPartner.Nop.Plugin.CloudStorage.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;


//using Microsoft.WindowsAzure.StorageClient;

namespace DevPartner.Nop.Plugin.CloudStorage.Azure.Cloud
{
    public class BlobMethods
    {
        //256 * 1024
        public int BlockSize { get { return 256 * 1024; } }

        //these variables are used throughout the class 
        string ContainerName { get; set; }
        BlobContainerClient BlobContainerClient { get; set; }

        //this is the only public constructor; can't use this class without this info
        public BlobMethods()
        {
        }

        /// <summary>
        /// set up references to the container, etc.
        /// </summary>
        private BlobContainerClient SetUpContainer(string azureBlobStorageConnectionString, string containerName)
        {
            string connectionString = azureBlobStorageConnectionString;

            //get a reference to the container where you want to put the files
            var blobServiceClient = new BlobServiceClient(connectionString);
            if (blobServiceClient == null)
                throw new Exception("Azure connection string for BLOB is wrong");
            var containerReference = blobServiceClient.GetBlobContainerClient(containerName);

            return containerReference;
        }

        public async void RunAtAppStartup(string azureBlobStorageConnectionString, string containerName)
        {
            ContainerName = containerName;
            BlobContainerClient = SetUpContainer(azureBlobStorageConnectionString, containerName);
            //just in case, check to see if the container exists,
            //  and create it if it doesn't
            //set access level to "blob", which means user can access the blob 
            //  but not look through the whole container
            //this means the user must have a URL to the blob to access it
            await BlobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        }

        public async Task<bool> ExistsAsync(string targetBlobName)
        {
            var correctBlobName = targetBlobName.TrimStart('\\');

            //GetBlobClient doesn't need to be async since it doesn't contact the server yet
            var blob = BlobContainerClient.GetBlobClient(correctBlobName);

            return await blob.ExistsAsync();
        }

        public string GetUrlByUniqName(string targetBlobName)
        {
            try
            {
                var correctBlobName = targetBlobName.TrimStart('\\');
                var blob = BlobContainerClient.GetBlobClient(correctBlobName);
                //blob.attr.FetchAttributesAsync();
                return blob.Uri.ToString();
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public FileData GetFileData(string targetBlobName)
        {
            try
            {
                var correctBlobName = targetBlobName.TrimStart('\\');
                var blob = BlobContainerClient.GetBlobClient(correctBlobName);
                //blob.FetchAttributesAsync();
                var properties = blob.GetProperties();
                return new FileData()
                {
                    Length = properties.Value.ContentLength,

                    LastWriteTime = properties.Value.LastModified.DateTime
                };
            }
            catch (Exception e)
            {
                return null;
            }
        }

        protected string GetMD5HashFromStream(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(stream);
                return Convert.ToBase64String(hash);
            }
        }

        protected virtual void UploadFile(BlobClient blob, byte[] bytes, string blockId)
        {
            blob.UploadAsync(new MemoryStream(bytes));
        }
        /*
        public string UploadAsyncFromStream(Stream stream, string targetBlobName, string contentType)
        {
            stream.Position = 0;
            var blob = BlobContainerClient.GetBlobClient(targetBlobName);

            var threads = new List<Thread>();
            var tasks = new List<Task>();
            List<string> blockIDs = new List<string>();
            int blockNumber = 0;
            int fileSize = (int)stream.Length;
            int offset = 0;
            do
            {
                blockNumber++;
                int bytesLeft = fileSize - offset;
                int bytesToUpload = Math.Min(bytesLeft, BlockSize);

                string blockId =
                    Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(string.Format("BlockId{0}", blockNumber.ToString("0000000"))));
                blockIDs.Add(blockId);

                byte[] bytes = new byte[bytesToUpload];
                stream.Read(bytes, 0, bytesToUpload);

                var task = Task.Factory.StartNew(() =>
                {
                    UploadFile(blob, bytes, blockId);
                });
                tasks.Add(task);

                offset += bytesToUpload;
            } while (offset < fileSize);

            Task.WaitAll(tasks.ToArray());

            //commit the blocks
            blob.PutBlockListAsync(blockIDs);

            properties.ContentType = contentType;
            blob.SetPropertiesAsync();

            return blob.Uri.ToString();
        }*/

        public async Task<string> UploadFromByteArrayAsync(string targetBlobName, string contentType, byte[] binary, string cacheControl)
        {
            var correctBlobName = targetBlobName.TrimStart('\\');

            //GetBlobClient doesn't need to be async since it doesn't contact the server yet
            var blob = BlobContainerClient.GetBlobClient(correctBlobName);

            BlobHttpHeaders headers = null;
            //set mime type
            if (!string.IsNullOrWhiteSpace(contentType))
            {
                headers = new BlobHttpHeaders
                {
                    ContentType = contentType
                };
            }

            //set cache control
            if (!string.IsNullOrWhiteSpace(cacheControl))
            {
                headers ??= new BlobHttpHeaders();
                headers.CacheControl = cacheControl;
            }
            await using var ms = new MemoryStream(binary);
            if (headers is null)
                await blob.UploadAsync(ms);
            else
                await blob.UploadAsync(ms, new BlobUploadOptions { HttpHeaders = headers });
            return blob.Uri.ToString();
        }

        public async Task UploadFromStream(Stream stream, string targetBlobName, string contentType = null)
        {
            //string status = string.Empty;
            //reset the stream back to its starting point (no partial saves)
            stream.Position = 0;
            var correctBlobName = targetBlobName.TrimStart('\\');
            var blob = BlobContainerClient.GetBlobClient(correctBlobName);

            BlobHttpHeaders headers = null;
            //set mime type
            if (!string.IsNullOrWhiteSpace(contentType))
            {
                headers = new BlobHttpHeaders
                {
                    ContentType = contentType
                };
            }
            if (headers is null)
                await blob.UploadAsync(stream);
            else
                await blob.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = headers });
            //status = "Uploaded successfully.";
            //return blob.Uri.ToString();
        }

        public async Task<MemoryStream> DownloadToStream(string sourceBlobName)
        {
            var correctBlobName = sourceBlobName.TrimStart('\\');
            var blob = BlobContainerClient.GetBlobClient(correctBlobName);
            var ms = new MemoryStream();
            await blob.DownloadToAsync(ms);
            return ms;
        }

        internal string CopyBlob(string sourcePath, string targetPath, bool deleteAfterFinish = false)
        {
            string status = string.Empty;
            var correctSourceBlobName = sourcePath.TrimStart('\\');
            var blobSource = BlobContainerClient.GetBlobClient(correctSourceBlobName);
            if (blobSource.ExistsAsync().Result)
            {
                var correctTargetBlobName = targetPath.TrimStart('\\');
                var blobTarget = BlobContainerClient.GetBlobClient(correctTargetBlobName);
                blobTarget.StartCopyFromUriAsync(blobSource.Uri);
                if (deleteAfterFinish)
                    blobSource.DeleteAsync();
            }

            status = "Finished copying the blob.";
            return status;
        }

        //if the blob is there, delete it 
        //check returning value to see if it was there or not
        public async void DeleteBlobByPrefixAsync(string filter)
        {
            //get result segment
            //listing snapshots is only supported in flat mode, so set the useFlatBlobListing parameter to true.
            var resultSegments = BlobContainerClient.GetBlobsByHierarchyAsync(traits: BlobTraits.All, prefix: filter);

            //delete files in result segment
            resultSegments.Select(blobItem => DeleteBlob(blobItem.Blob.Name));

/*            BlobContinuationToken continuationToken = null;
            while(var  = BlobContainerClient.GetBlobsByHierarchyAsync(traits: BlobTraits.All, prefix: filter))
            {

                await Task.WhenAll(resultSegment.Results.Select(blobItem => ((var)blobItem).DeleteAsync()));

            }
            while (continuationToken != null);*/

        }

        //if the blob is there, delete it 
        //check returning value to see if it was there or not
        internal string DeleteBlob(string blobName)
        {
            string status = string.Empty;
            var correctBlobName = blobName.TrimStart('\\');
            var blobSource = BlobContainerClient.GetBlobClient(correctBlobName);
            var blobExisted = blobSource.DeleteIfExistsAsync();
            if (blobExisted.Result)
            {
                status = "Blob existed; deleted.";
            }
            else
            {
                status = "Blob did not exist.";
            }
            return status;
        }

        /// <summary>
        /// parse the blob URI to get just the file name of the blob 
        /// after the container. So this will give you /directory1/directory2/filename if it's in a "subfolder"
        /// </summary>
        /// <param name="theUri"></param>
        /// <returns>name of the blob including subfolders (but not container)</returns>
        private string GetFileNameFromBlobURI(Uri theUri, string containerName)
        {
            string theFile = theUri.ToString();
            int dirIndex = theFile.IndexOf(containerName);
            string oneFile = theFile.Substring(dirIndex + containerName.Length + 1,
                theFile.Length - (dirIndex + containerName.Length + 1));
            return oneFile;
        }
        /*
        internal List<string> GetDirectoryListForRelPath(string relativePath)
        {
            //first, check the slashes and change them if necessary
            //second, remove leading slash if it's there
            relativePath = relativePath.Replace(@"\", @"/");
            if (relativePath.Substring(0, 1) == @"/")
                relativePath = relativePath.Substring(1, relativePath.Length - 1);

            var cloudBlobDirectory = BlobContainerClient.GetBlobClient(relativePath);
            var directories = cloudBlobDirectory.GetBlobsByHierarchyAsync(true, BlobListingDetails.All, null, null, null, null).Result.Results
                .Select(blobItem => GetFileNameFromBlobURI(blobItem.Uri, ContainerName)).ToList();

            throw new NotImplementedException();
            return directories;
        }*/

        internal async Task<List<string>> GetFileListForRelPath(string relativePath)
        {
            //first, check the slashes and change them if necessary
            //second, remove leading slash if it's there
            relativePath = relativePath.Replace(@"\", @"/");
            relativePath = relativePath.TrimStart('\\');
            if (relativePath.Substring(0, 1) == @"/")
                relativePath = relativePath.Substring(1, relativePath.Length - 1);

            List<string> listOBlobs = new List<string>();
            await foreach (var blobItem in BlobContainerClient.GetBlobsAsync(BlobTraits.All, BlobStates.All, relativePath))
            {
                //string oneFile = GetFileNameFromBlobURI(blobItem..Name, ContainerName);
                listOBlobs.Add(blobItem.Name);
            }
            return listOBlobs;
        }

    }

}
