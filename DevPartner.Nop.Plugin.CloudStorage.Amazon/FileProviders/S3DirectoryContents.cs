using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DevPartner.Nop.Plugin.CloudStorage.Amazon.FileProviders
{
    public class S3DirectoryContents : IDirectoryContents
    {
        private readonly IEnumerable<IFileInfo> _contents;

        /// <summary>
        /// Initializes a <see cref="S3DirectoryContents"/> instance.
        /// </summary>
        public S3DirectoryContents(Func<IAmazonS3> amazonS3Factory, string bucketName, string subpath)
        {
            bucketName = bucketName.ToLower();

            var request = new ListObjectsV2Request()
            {
                BucketName = bucketName,
                Delimiter = "/",
                Prefix = subpath
            };
            
            ListObjectsV2Response response;
            
            try
            {
                var contents = new List<IFileInfo>();
                
                using (var amazonS3 = amazonS3Factory())
                {
                    do
                    {
                        response = amazonS3.ListObjectsV2Async(request).Result;

                        var files = response.S3Objects
                            .Where(x => x.Key != subpath)
                            .Select(x => new S3FileInfo(amazonS3Factory, bucketName, x.Key));

                        var directories = response.CommonPrefixes
                            .Select(x => new S3FileInfo(amazonS3Factory, bucketName, x));

                        contents.AddRange(files);
                        contents.AddRange(directories);

                        request.ContinuationToken = response.NextContinuationToken;
                    }
                    while (response.IsTruncated);
                }

                _contents = contents;
                
                Exists = true;
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound)
                    throw ex;
            }
            catch(Exception)
            {
                throw;
            }
        }

        public IEnumerator<IFileInfo> GetEnumerator() => _contents.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _contents.GetEnumerator();

        public bool Exists { get; }
    }
}
