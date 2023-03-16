using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;
using System.Linq;

namespace Teed.Plugin.Facturify.Security
{
    public partial class FacturifyPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord Facturify = new PermissionRecord
        {
            Name = "Facturación - Facturify",
            SystemName = "Facturify",
            Category = "Plugin"
        };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                Facturify,
            };
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return Enumerable.Empty<DefaultPermissionRecord>();
        }
    }
}