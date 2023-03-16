using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components
{
    public class SearchBoxViewComponent : NopViewComponent
    {
        private readonly ICatalogModelFactory _catalogModelFactory;

        public SearchBoxViewComponent(ICatalogModelFactory catalogModelFactory)
        {
            this._catalogModelFactory = catalogModelFactory;
        }

        public IViewComponentResult Invoke(bool showAsNavBar = false, bool showAsDiv = false)
        {
            var model = _catalogModelFactory.PrepareSearchBoxModel();
            model.ShowAsNavBar = showAsNavBar;
            model.ShowAsDiv = showAsDiv;
            return View(model);
        }
    }
}
