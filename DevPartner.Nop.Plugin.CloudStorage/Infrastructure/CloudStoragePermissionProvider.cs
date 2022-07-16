using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace DevPartner.Nop.Plugin.CloudStorage.Infrastructure
{
    /// <summary>
    /// DevPartner.CloudStorage permission provider
    /// </summary>
    public partial class CloudStoragePermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord DevCommerce = new PermissionRecord { Name = "DevPartner.DevCommerce: Admin area.", SystemName = "DevPartner.DevCommerce", Category = "DevPartner.DevCommerce" };
        public static readonly PermissionRecord CloudStorage = new PermissionRecord { Name = "DevPartner.DevCommerce.CloudStorage: Admin area.", SystemName = "DevPartner.DevCommerce.CloudStorage", Category = "DevPartner.DevCommerce" };

        /// <summary>
        /// Get permissions
        /// </summary>
        /// <returns>Permissions</returns>
        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                DevCommerce,
                CloudStorage
            };
        }

        /// <summary>
        /// Get default permissions
        /// </summary>
        /// <returns>Permissions</returns>
        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        DevCommerce,
                        CloudStorage
                    }
                ),
            };
        }
    }
}
