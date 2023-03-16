using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;
using System.Linq;

namespace Teed.Plugin.ShoppingCartUrlGenerator.Security
{
    public partial class TeedShoppingCartUrlPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ShoppingCartUrl = new PermissionRecord
        {
            Name = "Url de carrito de compras",
            SystemName = "ShoppingCartUrl",
            Category = "Plugin"
        };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ShoppingCartUrl
            };
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return Enumerable.Empty<DefaultPermissionRecord>();
        }
    }
}