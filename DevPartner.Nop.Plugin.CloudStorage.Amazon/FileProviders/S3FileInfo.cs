using Amazon.S3;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;

namespace DevPartner.Nop.Plugin.CloudStorage.Amazon.FileProviders
{
    public class S3FileInfo : IFileInfo
    {
        private Func<IAmazonS3> _amazonS3Factory;
        private readonly string _bucketName;
        private readonly string _key;

        public S3FileInfo(Func<IAmazonS3> amazonS3Factory, string bucketName, string key)
        {
            _amazonS3Factory = amazonS3Factory;
            _bucketName = bucketName;
            _key = key;
            IsDirectory = key.EndsWith("/");
        }


        public bool Exists => throw new NotImplementedException();

        public long Length => throw new NotImplementedException();

        public string PhysicalPath => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public DateTimeOffset LastModified => throw new NotImplementedException();

        public bool IsDirectory { get; }

        public Stream CreateReadStream()
        {
            throw new NotImplementedException();
        }
    }
}
