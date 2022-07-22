using DevPartner.Nop.Plugin.CloudStorage.Attributes;
using System;
using System.Linq;
using System.Reflection;

namespace DevPartner.Nop.Plugin.CloudStorage.Extensions
{
    public static class TypeExtensions
    {
        public static string GetSystemName(this Type t)
        {
            var attSystemName = t.GetCustomAttributes(typeof(SystemNameAttribute)).FirstOrDefault() as SystemNameAttribute;
            var name = attSystemName != null ? attSystemName.SystemName : t.FullName;
            return name;
        }

        public static string GetComponentName(this Type t)
        {
            var attComponentName = t.GetCustomAttributes(typeof(ComponentNameAttribute)).FirstOrDefault() as ComponentNameAttribute;
            return attComponentName?.Name;
        }
    }
}
