using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tasks;
using Nop.Core.Infrastructure;
using Nop.Services.Customers;
using Nop.Services.Security;

namespace DevPartner.Nop.Plugin.CloudStorage.Extensions
{
    public static class PermissionServiceExtensions
    {
        public static void Save(this IPermissionService permissionService, string permSytemName, string permName, string permCategory)
        {
            var customerService = EngineContext.Current.Resolve<ICustomerService>();
            var perm = permissionService.GetPermissionRecordBySystemName(permSytemName);
            if (perm == null)
            {
                perm = new PermissionRecord { Name = permName, SystemName = permSytemName, Category = permCategory };
                var customerRole = customerService.GetCustomerRoleBySystemName(NopCustomerDefaults.AdministratorsRoleName);
                perm.PermissionRecordCustomerRoleMappings.Add(new PermissionRecordCustomerRoleMapping { CustomerRole = customerRole });
                permissionService.InsertPermissionRecord(perm);
            }
            else
            {
                perm.Name = permName;
                permissionService.UpdatePermissionRecord(perm);
            }
        }

    }
}
