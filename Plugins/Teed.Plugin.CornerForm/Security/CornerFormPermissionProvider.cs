using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;
using System.Linq;

namespace Teed.Plugin.CornerForm.Security
{
    public partial class CornerFormPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord CornerForm = new PermissionRecord
        {
            Name = "Administración de formulario público (Corner form)",
            SystemName = "CornerForm",
            Category = "Plugin"
        };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                CornerForm,
            };
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return Enumerable.Empty<DefaultPermissionRecord>();
        }
    }
}