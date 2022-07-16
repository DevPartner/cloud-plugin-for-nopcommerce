using System;

namespace DevPartner.Nop.Plugin.CloudStorage.Attributes
{
    public class ComponentNameAttribute : Attribute
    {
        public ComponentNameAttribute(string systemName)
        {
            Name = systemName;
        }
        public string Name { get; set; }
    }
}
