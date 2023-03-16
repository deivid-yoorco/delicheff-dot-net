using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;
using System.Linq;

namespace Teed.Plugin.FavoriteProducts.Security
{
    public partial class FavoriteProductsPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord Product = new PermissionRecord
        {
            Name = "Configuración de Productos favoritos",
            SystemName = "FavoriteProducts",
            Category = "Plugin"
        };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                Product
            };
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return Enumerable.Empty<DefaultPermissionRecord>();
        }
    }
}