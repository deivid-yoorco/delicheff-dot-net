using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class MarketingExpenseController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly MarketingExpenseService _marketingExpenseService;
        private readonly MarketingExpenseTypeService _marketingExpenseTypeService;

        public MarketingExpenseController(IPermissionService permissionService, MarketingExpenseService marketingExpenseService,
            IWorkContext workContext, MarketingExpenseTypeService marketingExpenseTypeService)
        {
            _permissionService = permissionService;
            _marketingExpenseService = marketingExpenseService;
            _workContext = workContext;
            _marketingExpenseTypeService = marketingExpenseTypeService;

        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.MarketingDashboard/Views/MarketingExpense/List.cshtml");
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var model = new MarketingExpenseModel();
            model.InitDateString = DateTime.Now.ToString("dd-MM-yyyy");
            model.EndDateString = DateTime.Now.ToString("dd-MM-yyyy");
            PrepareModel(model);

            return View("~/Plugins/Teed.Plugin.MarketingDashboard/Views/MarketingExpense/Create.cshtml", model);
        }

        [HttpPost]
        public IActionResult Create(MarketingExpenseModel model)
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            model.InitDate = DateTime.ParseExact(model.InitDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            model.EndDate = DateTime.ParseExact(model.EndDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            MarketingExpense marketingExpense = new MarketingExpense()
            {
                Amount = model.Amount,
                Comment = model.Comment,
                EndDate = model.EndDate,
                InitDate = model.InitDate,
                ExpenseTypeId = model.ExpenseTypeId,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) creó el gasto publicitario.\n"
            };
            _marketingExpenseService.Insert(marketingExpense);

            return RedirectToAction("List");
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var marketingExpense = _marketingExpenseService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (marketingExpense == null) return NotFound();

            var model = new MarketingExpenseModel()
            {
                Id = marketingExpense.Id,
                Amount = marketingExpense.Amount,
                Log = marketingExpense.Log,
                Comment= marketingExpense.Comment,
                EndDate = marketingExpense.EndDate,
                EndDateString = marketingExpense.EndDate.ToString("dd-MM-yyyy"),
                ExpenseTypeId = marketingExpense.ExpenseTypeId,
                InitDate = marketingExpense.InitDate,
                InitDateString = marketingExpense.InitDate.ToString("dd-MM-yyyy"),
            };

            PrepareModel(model);

            return View("~/Plugins/Teed.Plugin.MarketingDashboard/Views/MarketingExpense/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult Edit(MarketingExpenseModel model)
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var marketingExpense = _marketingExpenseService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (marketingExpense == null) return NotFound();

            model.InitDate = DateTime.ParseExact(model.InitDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            model.EndDate = DateTime.ParseExact(model.EndDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            string log = PrepareLog(model, marketingExpense);
            marketingExpense.Amount = model.Amount;
            marketingExpense.Comment = model.Comment;
            marketingExpense.EndDate = model.EndDate;
            marketingExpense.InitDate = model.InitDate;
            marketingExpense.ExpenseTypeId = model.ExpenseTypeId;
            marketingExpense.Log += log;

            _marketingExpenseService.Update(marketingExpense);

            return RedirectToAction("Edit", new { id = model.Id });
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var MarketingExpense = _marketingExpenseService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (MarketingExpense == null) return NotFound();

            MarketingExpense.Log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - El elemento fue eliminado por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}).\n";

            _marketingExpenseService.Delete(MarketingExpense);

            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var query = _marketingExpenseService.GetAll();
            var pagedList = new PagedList<MarketingExpense>(query.OrderByDescending(m => m.CreatedOnUtc), command.Page - 1, command.PageSize);

            DataSourceResult gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    Amount = x.Amount.ToString("C"),
                    InitDate = x.InitDate.ToString("dd-MM-yyyy"),
                    EndDate = x.EndDate.ToString("dd-MM-yyyy"),
                    ExpenseType = x.ExpenseType.Name
                }),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        private void PrepareModel(MarketingExpenseModel model)
        {
            var types = _marketingExpenseTypeService.GetAll().ToList();
            model.MarketingExpenseTypes = types.Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();
        }

        private string PrepareLog(MarketingExpenseModel newValue, MarketingExpense currentValue)
        {
            string log = string.Empty;
            if (newValue.Amount != currentValue.Amount)
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el monto de {currentValue.Amount} a {newValue.Amount}.\n";
            }
            if (newValue.Comment != currentValue.Comment)
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el comentario de '{currentValue.Comment}' a '{newValue.Comment}'.\n";
            }
            if (newValue.EndDateString != currentValue.EndDate.ToString("dd-MM-yyyy"))
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió la fecha final de '{currentValue.EndDate.ToString("dd-MM-yyyy")}' a '{newValue.EndDateString}'.\n";
            }
            if (newValue.InitDateString != currentValue.InitDate.ToString("dd-MM-yyyy"))
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió la fecha inicial de '{currentValue.InitDate.ToString("dd-MM-yyyy")}' a '{newValue.InitDateString}'.\n";
            }
            if (newValue.ExpenseTypeId != currentValue.ExpenseTypeId)
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el tipo de gasto de ID: {currentValue.ExpenseTypeId} a ID: {newValue.ExpenseTypeId}.\n";
            }
            return log;
        }
    }
}
