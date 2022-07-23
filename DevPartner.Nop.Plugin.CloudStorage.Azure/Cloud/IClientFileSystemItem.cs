using System;
using System.Collections.Generic;

namespace DevPartner.Nop.Plugin.CloudStorage.Azure.Cloud
{
    public interface IClientFileSystemItem
    {
        string Name { get; set; }

        DateTime DateModified { get; set; }

        bool IsDirectory { get; set; }

        long Size { get; set; }

        bool HasSubDirectories { get; set; }

        IDictionary<string, object> CustomFields => new Dictionary<string, object>();
    }
}