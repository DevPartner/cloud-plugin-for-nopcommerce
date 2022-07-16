using System;

namespace DevPartner.Nop.Plugin.CloudStorage.Attributes
{
    public class SystemNameAttribute : Attribute
    {
        public SystemNameAttribute(string systemName)
        {
            SystemName = systemName;
        }
        public string SystemName { get; set; }
    }
}
