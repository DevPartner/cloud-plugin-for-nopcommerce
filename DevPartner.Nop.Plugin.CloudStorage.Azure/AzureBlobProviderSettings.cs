using Nop.Core.Configuration;

namespace DevPartner.Nop.Plugin.CloudStorage.Azure
{
    public class AzureBlobProviderSettings : ISettings
    {
        public string ConnectionString { get; set; }
    }
}
