using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;
using System.Linq;

namespace Teed.Plugin.Medical.Security
{
    public partial class TeedMedicalPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord TeedMedical = new PermissionRecord
        {
            Name = "Teed - MedicalPlugin",
            SystemName = "TeedMedicalPlugin",
            Category = "Plugin"
        };

        public static readonly PermissionRecord TeedBranches = new PermissionRecord
        {
            Name = "Teed - Branches",
            SystemName = "TeedBranches",
            Category = "Plugin"
        };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                TeedMedical,
                TeedBranches
            };
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return Enumerable.Empty<DefaultPermissionRecord>();
        }
    }
}