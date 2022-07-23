using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Domain;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DevPartner.Nop.Plugin.CloudStorage.Amazon.Cloud
{
    public class BucketProvider : BaseCloudStorageProvider, ICloudStorageProvider
    {
        private readonly string _awsAccessKeyId;
        private readonly string _awsSecretAccessKey;
        private readonly RegionEndpoint _regionEndpoint;
        private readonly string _domainNameForCDN;
        private string _bucketName;
        private readonly string _directoryPlaceholderName = "placeholder.mock";
        private readonly ILogger _logger;

        public BucketProvider(
            AmazonCloudStorageSettings amazonCloudStorageSettings,
            ILogger logger)
        {
            _awsAccessKeyId = amazonCloudStorageSettings.AwsAccessKeyId;
            if (string.IsNullOrEmpty(_awsAccessKeyId))
                throw new ArgumentOutOfRangeException($"Check if {nameof(amazonCloudStorageSettings.AwsAccessKeyId)} is set in plugin settings!");

            _awsSecretAccessKey = amazonCloudStorageSettings.AwsSecretAccessKey;
            if (string.IsNullOrEmpty(_awsSecretAccessKey))
                throw new ArgumentOutOfRangeException($"Check if {nameof(amazonCloudStorageSettings.AwsSecretAccessKey)} is set in plugin settings!");

            _regionEndpoint = amazonCloudStorageSettings.RegionEndpoint;

            _domainNameForCDN = amazonCloudStorageSettings.DomainNameForCDN;

            _logger = logger;
        }



        public void RunAtAppStartup(string backet)
        {

            if (string.IsNullOrEmpty(backet))
                throw new ArgumentOutOfRangeException($"Check if {nameof(backet)} is set in plugin settings!");

            _bucketName = backet.ToLower();
        }

        public override void CreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            if (Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out Uri uriResult) && uriResult.IsAbsoluteUri)
                throw new ArgumentException("Absolute path is not supposed to be passed to S3!", nameof(path));

            var newObjectPath = Path.Join(path, _directoryPlaceholderName)
                .Replace(@"\", "/")
                .Replace("//", "/");

            if (DirectoryExists(path))
                return;

            try
            {
                using var amazonS3 = GetAmazonS3();

                var response = amazonS3.PutObjectAsync(
                        new PutObjectRequest
                        {
                            BucketName = _bucketName,
                            Key = newObjectPath,
                            ContentBody = string.Empty
                        }
                    ).Result;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is AmazonS3Exception amazonS3Exception)
                {
                    _logger.ErrorAsync($"Failed to create {newObjectPath} directory!", amazonS3Exception);
                    throw amazonS3Exception;
                }
                else
                    throw;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.ErrorAsync($"Failed to create {newObjectPath} directory!", ex);
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override void DeleteFile(string fileName)
        {
            fileName = UnifyPath(fileName);

            try
            {
                using var amazonS3 = GetAmazonS3();

                var response = amazonS3.DeleteObjectAsync(
                        new DeleteObjectRequest
                        {
                            BucketName = _bucketName,
                            Key = fileName
                        }
                    ).Result;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is AmazonS3Exception amazonS3Exception)
                {
                    _logger.ErrorAsync($"Failed to delete {fileName} file!", amazonS3Exception);
                    throw amazonS3Exception;
                }
                else
                    throw;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.ErrorAsync($"Failed to delete {fileName} file!", ex);
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override void DeleteFileByPrefix(string prefix) =>
            DeleteS3Objects(ListS3ObjectsByBrefix(UnifyPath(prefix), false, out int pagesCount), pagesCount);

        public override bool DirectoryExists(string path)
        {
            path = UnifyPath(path);

            try
            {
                using var amazonS3 = GetAmazonS3();

                return amazonS3.ListObjectsV2Async(
                        new ListObjectsV2Request
                        {
                            BucketName = _bucketName,
                            MaxKeys = 1,
                            Prefix = path
                        }
                    )
                    .Result.S3Objects.Count > 0;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is AmazonS3Exception amazonS3Exception)
                {
                    _logger.ErrorAsync($"Failed to check if directory {path} exists!", amazonS3Exception);
                    throw amazonS3Exception;
                }
                else
                    throw;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.ErrorAsync($"Failed to check if directory {path} exists!", ex);
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override void DirectoryMove(string sourceDirName, string destDirName)
        {
            sourceDirName = UnifyPath(sourceDirName);
            destDirName = UnifyPath(destDirName);
            
            
            var dirName = sourceDirName.Split("/", StringSplitOptions.RemoveEmptyEntries).Last();
            var sourceDirNameLength = sourceDirName.Length;

            var sourceS3Objects = ListS3ObjectsByBrefix(sourceDirName, false, out int pagesCount);

            try
            {
                var sourceS3ObjectsCount = sourceS3Objects.Count;

                var taskPool = new Task[sourceS3ObjectsCount];

                using var amazonS3 = GetAmazonS3();

                for (var i = 0; i < sourceS3ObjectsCount; i++)
                {
                    var sourceKey = sourceS3Objects[i].Key;

                    var destinationKey = Path.Join(destDirName, dirName, sourceKey.Substring(sourceDirNameLength))
                        .Replace(@"\", "/")
                        .Replace("//", "/");

                    taskPool[i] =
                        amazonS3.CopyObjectAsync(
                            new CopyObjectRequest
                            {
                                SourceBucket = _bucketName,
                                DestinationBucket = _bucketName,
                                SourceKey = sourceKey,
                                DestinationKey = destinationKey,
                                CannedACL = S3CannedACL.PublicRead
                            }
                        );
                }

                Task.WaitAll(taskPool);
            }
            catch (AmazonS3Exception ex)
            {
                _logger.ErrorAsync($"Failed to copy directory {sourceDirName} to {destDirName}!", ex);
                throw;
            }
            catch (Exception)
            {
                throw;
            }

            DeleteS3Objects(sourceS3Objects, pagesCount);
        }

        public override bool FileExists(string fileName)
        {
            fileName = UnifyPath(fileName);
            
            try
            {
                using var amazonS3 = GetAmazonS3();

                var response = amazonS3.GetObjectMetadataAsync(
                        new GetObjectMetadataRequest
                        {
                            BucketName = _bucketName,
                            Key = fileName
                        }
                    ).Result;

                return true;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is AmazonS3Exception amazonS3Exception)
                {
                    if (amazonS3Exception.StatusCode != HttpStatusCode.NotFound)
                    {
                        _logger.ErrorAsync($"Failed to check if file: {fileName} exists!", amazonS3Exception);
                        throw amazonS3Exception;
                    }
                }
                else
                    throw;
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound)
                {
                    _logger.ErrorAsync($"Failed to check if file: {fileName} exists!", ex);
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return false;
        }

        public override string[] GetDirectories(string path, string searchPattern = "", bool topDirectoryOnly = true)
        {
            path = UnifyPath(path);

            if (string.IsNullOrEmpty(searchPattern))
                return ListS3ObjectsByBrefix(path, topDirectoryOnly, out int pagesCount)
                    .Select(p => Path.GetDirectoryName(p.Key).Replace(@"\", "/"))
                    .Distinct()
                    .ToArray();
            else
                return ListS3ObjectsByBrefix(path, topDirectoryOnly, out int pagesCount)
                    .Select(p => Path.GetDirectoryName(p.Key).Replace(@"\", "/"))
                    .Distinct()
                    .Where(p => LikeOperator.LikeString(p, searchPattern, CompareMethod.Text))
                    .ToArray();
        }

        public override FileData GetFileData(string fileName)
        {
            fileName = UnifyPath(fileName);

            var s3Object = ListS3ObjectsByBrefix(fileName, false, out _).FirstOrDefault();

            if (s3Object == null)
                return default;

            return new FileData
            {
                LastWriteTime = s3Object.LastModified,
                Length = s3Object.Size
            };
        }

        public override string[] GetFiles(string directoryPath, string searchPattern = "", bool topDirectoryOnly = true)
        {
            directoryPath = UnifyPath(directoryPath);

            if (string.IsNullOrEmpty(searchPattern))
                return ListS3ObjectsByBrefix(directoryPath, topDirectoryOnly, out int _)
                    .Select(p => p.Key)
                    .ToArray();
            else
                return ListS3ObjectsByBrefix(directoryPath, topDirectoryOnly, out int _)
                    .Select(p => p.Key)
                    .Where(p => LikeOperator.LikeString(p, searchPattern, CompareMethod.Text))
                    .ToArray();
        }

        public override async Task<byte[]> ReadAllBytesAsync(string path)
        {
            path = UnifyPath(path);
            
            try
            {
                using var amazonS3 = GetAmazonS3();

                using var getObjectResponse = amazonS3.GetObjectAsync(
                        new GetObjectRequest
                        {
                            BucketName = _bucketName,
                            Key = path
                        }
                    ).Result;

                using var responseStream = getObjectResponse.ResponseStream;

                using var memoryStream = new MemoryStream();

                await responseStream.CopyToAsync(memoryStream);

                return memoryStream.ToArray();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is AmazonS3Exception amazonS3Exception)
                {
                    await _logger.ErrorAsync($"Failed to read file: {path}!", amazonS3Exception);
                    throw amazonS3Exception;
                }
                else
                    throw;
            }
            catch (AmazonS3Exception ex)
            {
                await _logger.ErrorAsync($"Failed to read file: {path}!", ex);
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override async Task WriteAllBytesAsync(string filePath, byte[] bytes)
        {
            filePath = UnifyPath(filePath);
            
            var existingS3Object = ListS3ObjectsByBrefix(filePath, false, out _).FirstOrDefault();

            if (existingS3Object != null)
                DeleteFile(filePath);

            try
            {
                using var amazonS3 = GetAmazonS3();

                _ = amazonS3.PutObjectAsync(
                        new PutObjectRequest
                        {
                            BucketName = _bucketName,
                            Key = filePath,
                            InputStream = new MemoryStream(bytes),
                            CannedACL = S3CannedACL.PublicRead
                        }
                    ).Result;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is AmazonS3Exception amazonS3Exception)
                {
                    await _logger.ErrorAsync($"Failed to write file: {filePath}!", amazonS3Exception);
                    throw amazonS3Exception;
                }
                else
                    throw;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.ErrorAsync($"Failed to write file: {filePath}!", ex);
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override void FileCopy(string sourcePath, string targetPath, bool overwrite = false)
        {
            sourcePath = UnifyPath(sourcePath);
            targetPath = UnifyPath(targetPath);
            
            if (FileExists(targetPath))
            {
                if (overwrite)
                    DeleteFile(targetPath);
                else
                    return;
            }

            try
            {
                using var amazonS3 = GetAmazonS3();

                _ = amazonS3.CopyObjectAsync(
                        new CopyObjectRequest
                        {
                            SourceBucket = _bucketName,
                            DestinationBucket = _bucketName,
                            SourceKey = sourcePath,
                            DestinationKey = targetPath,
                            CannedACL = S3CannedACL.PublicRead
                        }
                    ).Result;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is AmazonS3Exception amazonS3Exception)
                {
                    _logger.ErrorAsync($"Failed to copy {sourcePath} to {targetPath}!", amazonS3Exception);
                    throw amazonS3Exception;
                }
                else
                    throw;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.ErrorAsync($"Failed to copy {sourcePath} to {targetPath}!", ex);
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override long FileLength(string filePath)
        {
            filePath = UnifyPath(filePath);
            var fileData = GetFileData(filePath);
            return fileData.Length;
        }

        public override void FileMove(string filePath, string destFilePath)
        {
            FileCopy(filePath, destFilePath, true);
            DeleteFile(filePath);
        }

        public override DateTime GetLastWriteTime(string path)
        {
            if (FileExists(path))
            {
                var fileData = GetFileData(path);
                return fileData.LastWriteTime;
            }
            else
            {
                path = UnifyPath(path);

                var s3Objects = ListS3ObjectsByBrefix(path, false, out int pagesCount);

                if (s3Objects.Count == 0)
                    throw new ArgumentException("Invalid Path!", nameof(path));

                return s3Objects
                    .OrderByDescending(p => p.LastModified)
                    .First()
                    .LastModified;
            }
        }

        public override string GetVirtualPath(string filePath)
        {
            if (string.IsNullOrEmpty(_domainNameForCDN))
                return
                    string.Format(
                        "https://{0}.s3.{1}.amazonaws.com/{2}",
                        _bucketName, _regionEndpoint.SystemName, UnifyPath(filePath));
            else
                return string.Format(
                    "{0}/{1}",
                    _domainNameForCDN.TrimEnd('/'), UnifyPath(filePath));
        }

        public override string GetParentDirectory(string filePath)
        {
            throw new NotImplementedException();
        }


        private IAmazonS3 GetAmazonS3() => 
            new AmazonS3Client(_awsAccessKeyId, _awsSecretAccessKey, _regionEndpoint);

        private List<S3Object> ListS3ObjectsByBrefix(string prefix, bool useDelimiter, out int pagesCount)
        {
            var s3Objects = new List<S3Object>();

            var request = new ListObjectsV2Request()
            {
                BucketName = _bucketName,
                Prefix = prefix, 
            };

            if (useDelimiter)
                request.Delimiter = "/";

            ListObjectsV2Response response;

            pagesCount = 0;

            try
            {
                using var amazonS3 = GetAmazonS3();

                do
                {
                    response = amazonS3.ListObjectsV2Async(request).Result;

                    request.ContinuationToken = response.NextContinuationToken;

                    s3Objects.AddRange(response.S3Objects);

                    pagesCount += 1;
                }
                while (response.IsTruncated);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is AmazonS3Exception amazonS3Exception)
                {
                    _logger.ErrorAsync($"Failed to list S3Objects by prefix: {prefix}!", amazonS3Exception);
                    throw amazonS3Exception;
                }
                else
                    throw;
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound)
                    throw;
            }
            catch (Exception)
            {
                throw;
            }

            return s3Objects;
        }

        private void DeleteS3Objects(List<S3Object> s3Objects, int pagesCount)
        {
            try
            {
                var taskPool = new Task[pagesCount];

                using var amazonS3 = GetAmazonS3();

                do
                {
                    pagesCount -= 1;

                    var keyVersionsToDelete = s3Objects
                        .Skip(pagesCount * 1000)
                        .Take(1000)
                        .Select(p => new KeyVersion { Key = p.Key })
                        .ToList();

                    taskPool[pagesCount] = amazonS3.DeleteObjectsAsync(
                            new DeleteObjectsRequest
                            {
                                BucketName = _bucketName,
                                Objects = keyVersionsToDelete,
                                Quiet = true
                            }
                        );
                }
                while (pagesCount > 0);

                Task.WaitAll(taskPool);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is AmazonS3Exception amazonS3Exception)
                {
                    _logger.ErrorAsync("Failed to delete S3 objects!", amazonS3Exception);
                    throw amazonS3Exception;
                }
                else
                    throw;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.ErrorAsync("Failed to delete S3 objects!", ex);
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string UnifyPath(string path) => path.Replace(@"\", "/");

    }
}

