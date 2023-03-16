using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;
using System.Linq;

namespace Teed.Plugin.Medical.Security
{
    public partial class TeedManagerPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord TeedManager = new PermissionRecord
        {
            Name = "Teed - ManagerPlugin",
            SystemName = "TeedManagerPlugin",
            Category = "Plugin"
        };

        public static readonly PermissionRecord TeedPurchaseOrders = new PermissionRecord
        {
            Name = "Teed - PurchaseOrders",
            SystemName = "TeedPurchaseOrders",
            Category = "Plugin"
        };

        public static readonly PermissionRecord TeedPartnerLiabilities = new PermissionRecord
        {
            Name = "Teed - PartnerLiabilities",
            SystemName = "TeedPartnerLiabilities",
            Category = "Plugin"
        };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                TeedManager,
                TeedPurchaseOrders,
                TeedPartnerLiabilities
            };
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return Enumerable.Empty<DefaultPermissionRecord>();
        }
    }
}