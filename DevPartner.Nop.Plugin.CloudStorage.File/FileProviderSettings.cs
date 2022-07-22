using Nop.Core.Configuration;

namespace DevPartner.Nop.Plugin.CloudStorage.File
{
    public class FileProviderSettings : ISettings
    {
        public string DirectoryForFiles { get; set; }
    }
}
