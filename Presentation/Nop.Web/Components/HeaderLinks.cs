using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components
{
    public class HeaderLinksViewComponent : NopViewComponent
    {
        private readonly ICommonModelFactory _commonModelFactory;

        public HeaderLinksViewComponent(ICommonModelFactory commonModelFactory)
        {
            this._commonModelFactory = commonModelFactory;
        }

        public IViewComponentResult Invoke(bool HideExtra = false, bool? ParentIsLogoContainer = null)
        {
            var model = _commonModelFactory.PrepareHeaderLinksModel();
            model.HideExtra = HideExtra;
            model.ParentIsLogoContainer = ParentIsLogoContainer;
            return View(model);
        }
    }
}
