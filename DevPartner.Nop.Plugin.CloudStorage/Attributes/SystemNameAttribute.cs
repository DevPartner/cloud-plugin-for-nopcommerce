using System;
using System.ComponentModel.DataAnnotations;

namespace DevPartner.Nop.Plugin.CloudStorage.Attributes
{
    //[MetadataAttribute]
    public class SystemNameAttribute : Attribute
    {
        public SystemNameAttribute(string systemName)
        {
            SystemName = systemName;
        }
        public string SystemName { get; set; }
    }
}
