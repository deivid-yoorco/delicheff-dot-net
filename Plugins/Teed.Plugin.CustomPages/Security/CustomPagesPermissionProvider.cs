using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;
using System.Linq;

namespace Teed.Plugin.CustomPages.Security
{
    public partial class CustomPagesPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord CustomPages = new PermissionRecord
        {
            Name = "Administración de micrositios",
            SystemName = "CustomPages",
            Category = "Plugin"
        };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                CustomPages,
            };
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return Enumerable.Empty<DefaultPermissionRecord>();
        }
    }
}