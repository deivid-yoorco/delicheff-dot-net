using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;
using System.Linq;

namespace Teed.Plugin.LamyShop.Security
{
    public partial class LamyShopPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ReportLamyShop = new PermissionRecord
        {
            Name = "Reportes de LamyShop",
            SystemName = "Lamyshop",
            Category = "Plugin"
        };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ReportLamyShop
            };
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return Enumerable.Empty<DefaultPermissionRecord>();
        }
    }
}