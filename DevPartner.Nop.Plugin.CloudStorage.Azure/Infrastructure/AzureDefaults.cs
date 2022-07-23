using Nop.Core.Caching;

namespace DevPartner.Nop.Plugin.CloudStorage.Azure.Infrastructure
{
    public class AzureDefaults
    {
        /// <summary>
        /// Key to cache whether file exists
        /// </summary>
        /// <remarks>
        /// {0} : container
        /// {1} : file name
        /// </remarks>
        public static CacheKey BlobExistsCacheKey => new CacheKey("dp.cloud.azure.blob.{0}.exists-{1}");



        /// <summary>
        /// Key pattern to clear container cache
        /// </summary>
        public const string CONTAINER_PATTERN_KEY = "dp.cloud.azure.blob.{0}";
    }
}
