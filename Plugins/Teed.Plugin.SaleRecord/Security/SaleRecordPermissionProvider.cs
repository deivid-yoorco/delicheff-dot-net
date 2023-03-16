using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;
using System.Linq;

namespace Teed.Plugin.SaleRecord.Security
{
    public partial class SaleRecordPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord SaleRecords = new PermissionRecord
        {
            Name = "Registro de venta",
            SystemName = "SaleRecords",
            Category = "Plugin"
        };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                SaleRecords
            };
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return Enumerable.Empty<DefaultPermissionRecord>();
        }
    }
}