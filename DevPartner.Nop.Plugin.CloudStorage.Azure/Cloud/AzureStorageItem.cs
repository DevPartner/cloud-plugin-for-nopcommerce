using System;
using System.Collections.Generic;

namespace DevPartner.Nop.Plugin.CloudStorage.Azure.Cloud
{
    public class AzureStorageItem : IClientFileSystemItem
    {
        public string Name { get; set; }

        public DateTime DateModified { get; set; }

        public bool IsDirectory { get; set; }

        public long Size { get; set; }

        public bool HasSubDirectories { get; set; }

        public IDictionary<string, object> CustomFields => new Dictionary<string, object>();
    }
}
