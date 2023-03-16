using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;
using System.Linq;

namespace Teed.Plugin.Api.Security
{
    public partial class TeedApiPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord Configure = new PermissionRecord
        {
            Name = "API - Configuración",
            SystemName = "ApiConfigure",
            Category = "Plugin"
        };

        public static readonly PermissionRecord Notifications = new PermissionRecord
        {
            Name = "API - Notificaciones",
            SystemName = "ApiNotifications",
            Category = "Plugin"
        };

        public static readonly PermissionRecord PopupConfig = new PermissionRecord
        {
            Name = "API - Popup",
            SystemName = "ApiPopupConfig",
            Category = "Plugin"
        };

        public static readonly PermissionRecord TaggableBoxConfig = new PermissionRecord
        {
            Name = "API - Cajas etiquetables",
            SystemName = "ApiTaggableBoxConfig",
            Category = "Plugin"
        };

        public static readonly PermissionRecord OnboardingConfig = new PermissionRecord
        {
            Name = "API - Onboarding",
            SystemName = "ApiOnboardingConfig",
            Category = "Plugin"
        };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                Configure,
                Notifications,
                PopupConfig,
                TaggableBoxConfig,
                OnboardingConfig
            };
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return Enumerable.Empty<DefaultPermissionRecord>();
        }
    }
}