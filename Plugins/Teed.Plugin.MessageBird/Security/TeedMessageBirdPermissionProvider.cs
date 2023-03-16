using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;
using System.Linq;

namespace Teed.Plugin.MessageBird.Security
{
    public partial class TeedMessageBirdPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord MessageBird = new PermissionRecord
        {
            Name = "Teed - MessageBird",
            SystemName = "MessageBird",
            Category = "Plugin"
        };

        public static readonly PermissionRecord MessageBirdAdmin = new PermissionRecord
        {
            Name = "Teed - Administrador MessageBird",
            SystemName = "MessageBirdAdmin",
            Category = "Plugin"
        };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                MessageBird,
                MessageBirdAdmin
            };
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return Enumerable.Empty<DefaultPermissionRecord>();
        }
    }
}