using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;
using System.Linq;

namespace Teed.Plugin.Careers.Security
{
    public partial class CareersPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord Careers = new PermissionRecord
        {
            Name = "Configuración de Postulación",
            SystemName = "Careers",
            Category = "Plugin"
        };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                Careers
            };
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return Enumerable.Empty<DefaultPermissionRecord>();
        }
    }
}