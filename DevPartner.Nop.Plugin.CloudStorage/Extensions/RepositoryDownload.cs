using System;
using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Media;

namespace DevPartner.Nop.Plugin.CloudStorage.Extensions
{
    public static class RepositoryDownloadExtension
    {
        public static Download GetByGuid(this IRepository<Download> stdownloadRepository, Guid guid)
        {
            var query = from o in stdownloadRepository.Table
                        where o.DownloadGuid == guid
                        select o;
            return query.FirstOrDefault();
        }
    }
}
