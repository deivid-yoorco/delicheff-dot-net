using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Models.CategoryPrice;
using Teed.Plugin.Groceries.Security;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class CategoryPriceController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly ICategoryService _categoryService;

        public CategoryPriceController(IPermissionService permissionService, ICategoryService categoryService)
        {
            _permissionService = permissionService;
            _categoryService = categoryService;
        }

        public IActionResult Index()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.CategoryPrice))
                return AccessDeniedView();

            PrepareViewData();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/CategoryPrice/ConfigureCategoryPrice.cshtml");
        }

        [HttpPost]
        public IActionResult SaveData(List<DataModel> dataList)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.CategoryPrice))
                return AccessDeniedView();

            foreach (var data in dataList)
            {
                var category = _categoryService.GetCategoryById(int.Parse(data.Id));
                if (category == null) return NotFound();
                if (data.IsParent)
                    category.Coefficient = decimal.Parse(data.Value);
                else
                    category.PercentageOfUtility = decimal.Parse(data.Value);
                _categoryService.UpdateCategory(category);
            }
            return NoContent();
        }

        private void PrepareViewData()
        {
            ViewData["Categories"] = _categoryService.GetAllCategories().Select(x => new CategoryPriceModel()
            {
                Coefficient = x.Coefficient,
                Id = x.Id,
                Name = x.GetFormattedBreadCrumb(_categoryService),
                ParentCategoryId = x.ParentCategoryId,
                PercentageOfUtility = x.PercentageOfUtility
            }).ToList();
        }
    }

    public class DataModel
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public bool IsParent { get; set; }
    }
}