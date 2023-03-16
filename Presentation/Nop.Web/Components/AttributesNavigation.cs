using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Components
{
    public class AttributesNavigationViewComponent : NopViewComponent
    {
        private readonly ICatalogModelFactory _catalogModelFactory;
        
        public AttributesNavigationViewComponent(ICatalogModelFactory catalogModelFactory)
        {
            this._catalogModelFactory = catalogModelFactory;
        }

        public IViewComponentResult Invoke(int currentCategoryId, int currentProductId)
        {
            //var model = _catalogModelFactory.PrepareCategoryNavigationModel(currentCategoryId, currentProductId);
            var model = new CatalogPagingFilteringModel.SpecificationFilterModel();
            return View(model);
        }
    }
}
