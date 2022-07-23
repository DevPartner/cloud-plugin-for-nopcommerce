using Amazon;
using Amazon.S3;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;

namespace DevPartner.Nop.Plugin.CloudStorage.Amazon.FileProviders
{
    public class S3FileProvider : IFileProvider
    {
        private readonly string _awsAccessKeyId;
        private readonly string _awsSecretAccessKey;

        public S3FileProvider(
            string awsAccessKeyId, 
            string awsSecretAccessKey)
        {
            _awsAccessKeyId = awsAccessKeyId;
            _awsSecretAccessKey = awsSecretAccessKey;
        }

        public RegionEndpoint RegionEndpoint { get; set; } = RegionEndpoint.EUWest2;

        public string BucketName { get; set; } = "DevPartner.Bucket";

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return new S3DirectoryContents(GetAmazonS3Instance, BucketName, subpath);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            throw new NotImplementedException();
        }

        public IChangeToken Watch(string filter)
        {
            throw new NotImplementedException();
        }

        private IAmazonS3 GetAmazonS3Instance() => new AmazonS3Client(_awsAccessKeyId, _awsSecretAccessKey, RegionEndpoint);
    }
}
