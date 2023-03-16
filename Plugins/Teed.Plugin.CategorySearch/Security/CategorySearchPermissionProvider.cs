using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;
using System.Linq;

namespace Teed.Plugin.CategorySearch.Security
{
    public partial class CategorySearchPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord CategorySearchPermission = new PermissionRecord
        {
            Name = "Busqueda de categoria",
            SystemName = "CategorySearch",
            Category = "Plugin"
        };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                CategorySearchPermission
            };
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return Enumerable.Empty<DefaultPermissionRecord>();
        }
    }
}