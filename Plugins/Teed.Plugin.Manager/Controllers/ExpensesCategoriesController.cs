using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Manager.Domain.ExpensesCategories;
using Teed.Plugin.Manager.Models.ExpensesCategories;
using Teed.Plugin.Manager.Services;

namespace Teed.Plugin.Manager.Controllers
{
    [Area(AreaNames.Admin)]
    public class ExpensesCategoriesController : BasePluginController
    {
        private readonly ExpensesCategoriesService _expensesCategoriesService;
        private readonly IPermissionService _permissionService;

        public ExpensesCategoriesController(ExpensesCategoriesService expensesCategoriesService, IPermissionService permissionService)
        {
            _expensesCategoriesService = expensesCategoriesService;
            _permissionService = permissionService;
        }

        public IActionResult Index()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Manager/Views/ExpensesCategories/Index.cshtml");
        }

        [HttpPost]
        public IActionResult SaveCategories(ExpensesCategoriesModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid) return BadRequest();

            var categoriesToDelete = _expensesCategoriesService.GetAll().ToList().Where(x => !model.ExpensesCategories.Select(y => y.CategoryId).ToList().Contains(x.ExpenseCategoryId));
            foreach (var item in categoriesToDelete)
            {
                _expensesCategoriesService.Delete(item);
            }

            foreach (var item in model.ExpensesCategories)
            {
                var dbCategory = _expensesCategoriesService.GetAll().Where(x => x.ExpenseCategoryId == item.CategoryId).FirstOrDefault();
                if (dbCategory == null)
                {
                    _expensesCategoriesService.Insert(new ExpensesCategories()
                    {
                        ExpenseCategoryId = item.CategoryId,
                        ParentId = item.ParentCategoryId,
                        Value = item.Value,
                        ValueTitle = string.IsNullOrWhiteSpace(item.ValueTitle) ? "" : item.ValueTitle
                    });
                }
                else
                {
                    dbCategory.Value = item.Value;
                    dbCategory.ValueTitle = string.IsNullOrWhiteSpace(item.ValueTitle) ? "" : item.ValueTitle;
                    _expensesCategoriesService.Update(dbCategory);
                }
            }

            return NoContent();
        }

        [HttpGet]
        public IActionResult GetCategories()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = new ExpensesCategoriesModel();
            model.ExpensesCategories = _expensesCategoriesService.GetAll().ToList().Select(x => new ExpenseCategory()
            {
                CategoryId = x.ExpenseCategoryId,
                ParentCategoryId = x.ParentId,
                Value = x.Value,
                ValueTitle = string.IsNullOrWhiteSpace(x.ValueTitle) ? "" : x.ValueTitle
            }).ToList();

            return Ok(model);
        }
    }
}
