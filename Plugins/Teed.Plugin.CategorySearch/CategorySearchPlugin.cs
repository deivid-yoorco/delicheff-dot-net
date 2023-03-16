using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Security;
using Nop.Services.Tasks;
using Nop.Web.Framework.Menu;
using System.Collections.Generic;
using System.Linq;
using Teed.Plugin.CategorySearch.Security;

namespace Teed.Plugin.CategorySearch
{
    public class CategorySearchPlugin : BasePlugin, IMiscPlugin, IWidgetPlugin
    {
        private readonly IPermissionService _categorySearchPermission;

        public CategorySearchPlugin(IPermissionService categorySearchPermission)
        {
            _categorySearchPermission = categorySearchPermission;

            _categorySearchPermission.InstallPermissions(new CategorySearchPermissionProvider());
        }
        public void GetPublicViewComponent(string widgetZone, out string viewComponentName)
        {
            switch (widgetZone)
            {
                case "productsearch_page_before_results":
                    viewComponentName = "CategorySearch";
                    break;
                default:
                    viewComponentName = "";
                    break;
            }
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string> { "productsearch_page_before_results" };
        }


        public override void Install()
        {
            _categorySearchPermission.InstallPermissions(new CategorySearchPermissionProvider());

            base.Install();
        }

        public override void Uninstall()
        {
            base.Uninstall();
        }
    }
}
