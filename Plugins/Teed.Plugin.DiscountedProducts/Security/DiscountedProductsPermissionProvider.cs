using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;
using System.Linq;

namespace Teed.Plugin.DiscountedProducts.Security
{
    public partial class DiscountedProductsPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord Product = new PermissionRecord
        {
            Name = "Configuración de Productos con descuento",
            SystemName = "DiscountedProducts",
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