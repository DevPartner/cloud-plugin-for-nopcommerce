using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using System.Collections.Generic;
using System.Linq;
using Autofac.Features.Metadata;

namespace DevPartner.Nop.Plugin.CloudStorage.Extensions
{
    public static class EnumerableExtensions
    {
        public static string GetComponentSettings(this IEnumerable<Meta<ICloudStorageProviderFactory>> providers, string systemName)
        {
            var firstOrDefault = providers
                .FirstOrDefault(x => x.Metadata["SystemName"].Equals(systemName));
            if (firstOrDefault!=null&&firstOrDefault.Metadata.ContainsKey("Settings"))
                return firstOrDefault.Metadata["Settings"]
                    .ToString();
            return null;
        }

        public static string GetComponentSettings(this Meta<ICloudStorageProviderFactory> provider, string systemName)
        {
            if (provider != null && provider.Metadata.ContainsKey("Settings"))
                return provider.Metadata["Settings"]
                    .ToString();
            return null;
        }

    }
}
