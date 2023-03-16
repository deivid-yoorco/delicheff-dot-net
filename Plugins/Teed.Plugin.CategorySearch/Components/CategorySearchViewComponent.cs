using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.CategorySearch.Dtos.Categories;
using Nop.Services.Catalog;
using Teed.Plugin.CategorySearch.Models;
using Nop.Services.Media;
using Nop.Services.Seo;

namespace Teed.Plugin.CategorySearch.Components
{
    [ViewComponent(Name = "CategorySearch")]
    public class CategorySearchViewComponent : NopViewComponent
    {
        private readonly ICategoryService _categorySearchService;
        private readonly IPictureService _pictureService;

        public CategorySearchViewComponent(ICategoryService categorySearchService, IPictureService pictureService)
        {
            _categorySearchService = categorySearchService;
            _pictureService = pictureService;
        }

        public virtual List<CategorySearchModel> SearchCategories(string searchTerm)
        {
            var searchResults = _categorySearchService.GetAllCategories(categoryName: searchTerm)
                .Select(x => new CategorySearchModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    SeName = x.GetSeName(),
                    ParentCategoryId = x.ParentCategoryId,
                    PictureUrl = _pictureService.GetPictureUrl(x.PictureId, showDefaultPicture: false) ?? ""
        }).ToList();

            return searchResults;
        }

        public IViewComponentResult Invoke(string widgetZone, string additionalData)
        {
            var searchResutls = SearchCategories(additionalData);
           return View("~/Plugins/Teed.Plugin.CategorySearch/Views/Shared/Components/CategorySearch.cshtml", searchResutls);
        }
    }
}