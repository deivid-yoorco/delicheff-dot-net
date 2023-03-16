using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;
using System.Linq;

namespace Teed.Plugin.CustomerComments.Security
{
    public partial class TeedCustomerCommentsPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord CustomerComments = new PermissionRecord
        {
            Name = "Teed - Comentarios para clientes",
            SystemName = "CustomerComments",
            Category = "Plugin"
        };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                CustomerComments,
            };
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return Enumerable.Empty<DefaultPermissionRecord>();
        }
    }
}