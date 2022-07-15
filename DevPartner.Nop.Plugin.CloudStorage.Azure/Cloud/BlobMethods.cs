using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevPartner.Nop.Plugin.CloudStorage.Domain;
using Microsoft.WindowsAzure.Storage.Blob;
using CloudStorageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount;

//using Microsoft.WindowsAzure.StorageClient;

namespace DevPartner.Nop.Plugin.CloudStorage.Azure.Cloud
{
    public class BlobMethods
    {
        //256 * 1024
        public int BlockSize { get { return 256 * 1024; } }

        //these variables are used throughout the class 
        string ContainerName { get; set; }
        CloudBlobContainer CloudBlobContainer { get; set; }

        //this is the only public constructor; can't use this class without this info
        public BlobMethods()
        {
        }

        /// <summary>
        /// set up references to the container, etc.
        /// </summary>
        private CloudBlobContainer SetUpContainer(string azureBlobStorageConnectionString, string containerName)
        {
            string connectionString = azureBlobStorageConnectionString;
            /*    string.Format(@"DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
            storageAccountName, storageAccountKey);*/

            //get a reference to the container where you want to put the files
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            if (cloudStorageAccount == null)
                throw new Exception("Azure connection string for BLOB is wrong");

            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            var containerReference = cloudBlobClient.GetContainerReference(containerName);

            return containerReference;
        }

        public async void RunAtAppStartup(string azureBlobStorageConnectionString, string containerName)
        {
            ContainerName = containerName;
            CloudBlobContainer = SetUpContainer(azureBlobStorageConnectionString, containerName);
            //just in case, check to see if the container exists,
            //  and create it if it doesn't
            await CloudBlobContainer.CreateIfNotExistsAsync();
           
            //set access level to "blob", which means user can access the blob 
            //  but not look through the whole container
            //this means the user must have a URL to the blob to access it
            BlobContainerPermissions permissions = new BlobContainerPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
            await CloudBlobContainer.SetPermissionsAsync(permissions);
        }

        public async Task<bool> ExistsAsync(string thumbFileName)
        {
            //GetBlockBlobReference doesn't need to be async since it doesn't contact the server yet
            var blockBlob = CloudBlobContainer.GetBlockBlobReference(thumbFileName);

            return await blockBlob.ExistsAsync();
        }

        public string GetUrlByUniqName(string targetBlobName)
        {
            try
            {
                CloudBlockBlob blob = CloudBlobContainer.GetBlockBlobReference(targetBlobName);
                blob.FetchAttributesAsync();
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
                CloudBlockBlob blob = CloudBlobContainer.GetBlockBlobReference(targetBlobName);
                blob.FetchAttributesAsync();
                
                return new FileData()
                {
                    Length = blob.Properties.Length,
                    
                    LastWriteTime =  blob.Properties.LastModified.GetValueOrDefault().DateTime
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

        protected virtual void UploadFile(CloudBlockBlob blob, byte[] bytes, string blockId)
        {
            string blockHash = GetMD5HashFromStream(bytes);
            blob.PutBlockAsync(blockId, new MemoryStream(bytes), blockHash);
        }

        public string UploadAsyncFromStream(Stream stream, string targetBlobName, string contentType)
        {
            stream.Position = 0;
            CloudBlockBlob blob = CloudBlobContainer.GetBlockBlobReference(targetBlobName);
            
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

            blob.Properties.ContentType = contentType;
            blob.SetPropertiesAsync();

            return blob.Uri.ToString();
        }

        public async Task<string> UploadFromByteArrayAsync(string fileName, string contentType, byte[] binary, string cacheControl)
        {
            //GetBlockBlobReference doesn't need to be async since it doesn't contact the server yet
            var blob = CloudBlobContainer.GetBlockBlobReference(fileName);

            //set mime type
            if (!string.IsNullOrEmpty(contentType))
                blob.Properties.ContentType = contentType;

            //set cache control
            if (!string.IsNullOrEmpty(cacheControl))
                blob.Properties.CacheControl = cacheControl;

            await blob.UploadFromByteArrayAsync(binary, 0, binary.Length);
            return blob.Uri.ToString();

        }

        public async Task UploadFromStream(Stream stream, string targetBlobName, string contentType)
        {
            //string status = string.Empty;
            //reset the stream back to its starting point (no partial saves)
            stream.Position = 0;
            CloudBlockBlob blob = CloudBlobContainer.GetBlockBlobReference(targetBlobName);
            blob.Properties.ContentType = contentType;
            await blob.UploadFromStreamAsync(stream);
            //status = "Uploaded successfully.";
            //return blob.Uri.ToString();
        }

        public  async Task<MemoryStream> DownloadToStream(string sourceBlobName)
        {
            CloudBlockBlob blob = CloudBlobContainer.GetBlockBlobReference(sourceBlobName);
            var ms = new MemoryStream();
            await blob.DownloadToStreamAsync(ms);
            return ms;
        }
        
        internal string CopyBlob(string sourcePath, string targetPath, bool deleteAfterFinish = false)
        {
            string status = string.Empty;
            CloudBlockBlob blobSource = CloudBlobContainer.GetBlockBlobReference(sourcePath);
            if (blobSource.ExistsAsync().Result)
            {
                CloudBlockBlob blobTarget = CloudBlobContainer.GetBlockBlobReference(targetPath);
                blobTarget.StartCopyAsync(blobSource);
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
            BlobContinuationToken continuationToken = null;
            do
            {
                var resultSegment = await CloudBlobContainer.ListBlobsSegmentedAsync(filter, true, BlobListingDetails.All, null, continuationToken, null, null);

                //delete files in result segment
                await Task.WhenAll(resultSegment.Results.Select(blobItem => ((CloudBlockBlob)blobItem).DeleteAsync()));

                //get the continuation token.
                continuationToken = resultSegment.ContinuationToken;
            }
            while (continuationToken != null);

        }


        //if the blob is there, delete it 
        //check returning value to see if it was there or not
        internal string DeleteBlob(string blobName)
        {
            string status = string.Empty;
            CloudBlockBlob blobSource = CloudBlobContainer.GetBlockBlobReference(blobName);
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

        internal List<string> GetDirectoryListForRelPath(string relativePath)
        {
            //first, check the slashes and change them if necessary
            //second, remove leading slash if it's there
            relativePath = relativePath.Replace(@"\", @"/");
            if (relativePath.Substring(0, 1) == @"/")
                relativePath = relativePath.Substring(1, relativePath.Length - 1);

            CloudBlobDirectory cloudBlobDirectory = CloudBlobContainer.GetDirectoryReference(relativePath);
            var directories = cloudBlobDirectory.ListBlobsSegmentedAsync(true, BlobListingDetails.All,null,null,null,null).Result.Results
                .Select(blobItem => GetFileNameFromBlobURI(blobItem.Uri, ContainerName)).ToList();

            throw new NotImplementedException();
            return directories;
        }

        internal List<string> GetFileListForRelPath(string relativePath)
        {
            //first, check the slashes and change them if necessary
            //second, remove leading slash if it's there
            relativePath = relativePath.Replace(@"\", @"/");
            if (relativePath.Substring(0, 1) == @"/")
                relativePath = relativePath.Substring(1, relativePath.Length - 1);

            List<string> listOBlobs = new List<string>();
            foreach (IListBlobItem blobItem in CloudBlobContainer.ListBlobsSegmentedAsync(relativePath, true, BlobListingDetails.All, null, null, null, null).Result.Results)
            {
                string oneFile = GetFileNameFromBlobURI(blobItem.Uri, ContainerName);
                listOBlobs.Add(oneFile);
            }
            return listOBlobs;
        }

    }

}
