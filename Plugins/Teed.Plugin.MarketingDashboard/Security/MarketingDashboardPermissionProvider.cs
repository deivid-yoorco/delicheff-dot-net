using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;
using System.Linq;

namespace Teed.Plugin.MarketingDashboard.Security
{
    public partial class MarketingDashboardPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord MarketingExpenses = new PermissionRecord
        {
            Name = "Gasto publicitario",
            SystemName = "MarketingExpenses",
            Category = "Plugin"
        };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                MarketingExpenses
            };
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return Enumerable.Empty<DefaultPermissionRecord>();
        }
    }
}