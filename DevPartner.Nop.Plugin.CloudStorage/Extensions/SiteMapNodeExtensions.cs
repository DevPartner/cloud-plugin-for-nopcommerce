using Nop.Web.Framework.Menu;
using System.Linq;

namespace DevPartner.Nop.Plugin.CloudStorage.Extensions
{

    public static class SiteMapNodeExtensions
    {
        public static SiteMapNode AddIfNotExist(this SiteMapNode siteMapNode, SiteMapNode childNode)
        {
            var childNodeExist = siteMapNode.ChildNodes.FirstOrDefault(x => x.SystemName == childNode.SystemName);

            if (childNodeExist == null)
            {
                childNodeExist = childNode;
                siteMapNode.ChildNodes.Add(childNode);
            }
            return childNodeExist;
        }
    }
}
