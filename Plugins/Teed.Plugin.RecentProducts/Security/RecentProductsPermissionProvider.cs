using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;
using System.Linq;

namespace Teed.Plugin.RecentProducts.Security
{
    public partial class RecentProductsPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord Recent = new PermissionRecord
        {
            Name = "Configuración de productos recientes",
            SystemName = "RecentProducts",
            Category = "Plugin"
        };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                Recent
            };
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return Enumerable.Empty<DefaultPermissionRecord>();
        }
    }
}