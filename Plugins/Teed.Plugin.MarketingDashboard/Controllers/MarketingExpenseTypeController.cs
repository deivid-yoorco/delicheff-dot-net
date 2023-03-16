using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.MarketingDashboard.Domain.MarketingExpenses;
using Teed.Plugin.MarketingDashboard.Models.MarketingExpenses;
using Teed.Plugin.MarketingDashboard.Security;
using Teed.Plugin.MarketingDashboard.Services;

namespace Teed.Plugin.MarketingDashboard.Controllers
{
    [Area(AreaNames.Admin)]
    public class MarketingExpenseTypeController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly MarketingExpenseTypeService _marketingExpenseTypeService;

        public MarketingExpenseTypeController(IPermissionService permissionService, MarketingExpenseTypeService marketingExpenseTypeService,
            IWorkContext workContext)
        {
            _permissionService = permissionService;
            _marketingExpenseTypeService = marketingExpenseTypeService;
            _workContext = workContext;
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.MarketingDashboard/Views/MarketingExpenseType/List.cshtml");
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.MarketingDashboard/Views/MarketingExpenseType/Create.cshtml");
        }

        [HttpPost]
        public IActionResult Create(MarketingExpenseTypeModel model)
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            MarketingExpenseType marketingExpenseType = new MarketingExpenseType()
            {
                Name = model.Name,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) creó el tipo de gasto {model.Name}.\n"
            };
            _marketingExpenseTypeService.Insert(marketingExpenseType);

            return RedirectToAction("List");
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var marketingExpense = _marketingExpenseTypeService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (marketingExpense == null) return NotFound();

            var model = new MarketingExpenseTypeModel()
            {
                Id = marketingExpense.Id,
                Name = marketingExpense.Name,
                Log = marketingExpense.Log
            };

            return View("~/Plugins/Teed.Plugin.MarketingDashboard/Views/MarketingExpenseType/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult Edit(MarketingExpenseTypeModel model)
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var marketingExpenseType = _marketingExpenseTypeService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (marketingExpenseType == null) return NotFound();

            string log = PrepareLog(model, marketingExpenseType);
            marketingExpenseType.Name = model.Name;
            marketingExpenseType.Log += log;
            _marketingExpenseTypeService.Update(marketingExpenseType);

            return RedirectToAction("Edit", new { id = model.Id });
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var marketingExpenseType = _marketingExpenseTypeService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (marketingExpenseType == null) return NotFound();

            marketingExpenseType.Log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - El elemento fue eliminado por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}).\n";

            _marketingExpenseTypeService.Delete(marketingExpenseType);

            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var query = _marketingExpenseTypeService.GetAll();
            var pagedList = new PagedList<MarketingExpenseType>(query.OrderByDescending(m => m.CreatedOnUtc), command.Page - 1, command.PageSize);

            DataSourceResult gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    x.Name
                }),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        private string PrepareLog(MarketingExpenseTypeModel newValue, MarketingExpenseType currentValue)
        {
            string log = string.Empty;
            if (newValue.Name != currentValue.Name)
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el nombre del tipo de gasto de {currentValue.Name} a {newValue.Name}.\n";
            }
            return log;
        }
    }
}
