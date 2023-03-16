using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;
using System.Linq;

namespace Teed.Plugin.TicketControl.Security
{
    public partial class TeedTicketControlPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord TicketControl = new PermissionRecord
        {
            Name = "Teed - Control de Tickets",
            SystemName = "TicketControl",
            Category = "Plugin"
        };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                TicketControl,
            };
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return Enumerable.Empty<DefaultPermissionRecord>();
        }
    }
}