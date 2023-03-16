using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Helper
{
    public static class SelectListHelper
    {
        public static List<SelectListItem> GetProductsList(IProductService productService)
        {
            if (productService == null)
                throw new ArgumentNullException(nameof(productService));

            List<SelectListItem> result = new List<SelectListItem>();
            var listItems = productService.SearchProducts();
            foreach (var item in listItems)
            {
                result.Add(new SelectListItem
                {
                    Text = $"{item.Name}",
                    Value = item.Id.ToString()
                });
            }
            return result;
        }

        public static List<SelectListItem> GetCategoriesList(ICategoryService categoryService)
        {
            if (categoryService == null)
                throw new ArgumentNullException(nameof(categoryService));

            List<SelectListItem> result = new List<SelectListItem>();
            var listItems = categoryService.GetAllCategories();
            foreach (var item in listItems)
            {
                result.Add(new SelectListItem
                {
                    Text = $"{item.Name}",
                    Value = item.Id.ToString()
                });
            }
            return result;
        }
    }
}
