using Microsoft.AspNetCore.Mvc;
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
    public class MarketingGrossMarginController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly MarketingGrossMarginService _marketingGrossMarginService;

        public MarketingGrossMarginController(IPermissionService permissionService, MarketingGrossMarginService marketingGrossMarginService,
            IWorkContext workContext)
        {
            _permissionService = permissionService;
            _marketingGrossMarginService = marketingGrossMarginService;
            _workContext = workContext;
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.MarketingDashboard/Views/MarketingGrossMargin/List.cshtml");
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var model = new MarketingGrossMarginModel();
            model.ApplyDateString = DateTime.Now.ToString("dd-MM-yyyy");

            return View("~/Plugins/Teed.Plugin.MarketingDashboard/Views/MarketingGrossMargin/Create.cshtml", model);
        }

        [HttpPost]
        public IActionResult Create(MarketingGrossMarginModel model)
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            model.ApplyDate = DateTime.ParseExact(model.ApplyDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            MarketingGrossMargin marketingGrossMargin = new MarketingGrossMargin()
            {
                ApplyDate = model.ApplyDate,
                Value = model.Value,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) creó el registro de margen bruto.\n"
            };
            _marketingGrossMarginService.Insert(marketingGrossMargin);

            return RedirectToAction("List");
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var marketingGrossMargin = _marketingGrossMarginService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (marketingGrossMargin == null) return NotFound();

            var model = new MarketingGrossMarginModel()
            {
                Id = marketingGrossMargin.Id,
                ApplyDate = marketingGrossMargin.ApplyDate,
                ApplyDateString = marketingGrossMargin.ApplyDate.ToString("dd-MM-yyyy"),
                Value = marketingGrossMargin.Value,
                Log = marketingGrossMargin.Log
            };

            return View("~/Plugins/Teed.Plugin.MarketingDashboard/Views/MarketingGrossMargin/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult Edit(MarketingGrossMarginModel model)
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var MarketingGrossMargin = _marketingGrossMarginService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (MarketingGrossMargin == null) return NotFound();

            model.ApplyDate = DateTime.ParseExact(model.ApplyDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            string log = PrepareLog(model, MarketingGrossMargin);
            MarketingGrossMargin.ApplyDate = model.ApplyDate;
            MarketingGrossMargin.Value = model.Value;
            MarketingGrossMargin.Log += log;
            _marketingGrossMarginService.Update(MarketingGrossMargin);

            return RedirectToAction("Edit", new { id = model.Id });
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var MarketingGrossMargin = _marketingGrossMarginService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (MarketingGrossMargin == null) return NotFound();

            MarketingGrossMargin.Log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - El elemento fue eliminado por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}).\n";

            _marketingGrossMarginService.Delete(MarketingGrossMargin);

            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var query = _marketingGrossMarginService.GetAll();
            var pagedList = new PagedList<MarketingGrossMargin>(query.OrderByDescending(m => m.CreatedOnUtc), command.Page - 1, command.PageSize);

            DataSourceResult gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    Value = x.Value + "%",
                    ApplyDate = x.ApplyDate.ToString("dd-MM-yyyy")
                }),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        private string PrepareLog(MarketingGrossMarginModel newValue, MarketingGrossMargin currentValue)
        {
            string log = string.Empty;
            if (newValue.ApplyDate != currentValue.ApplyDate)
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió la fecha a partir de la que aplica de {currentValue.ApplyDate.ToString("dd-MM-yyyy")} a {newValue.ApplyDate.ToString("dd-MM-yyyy")}.\n";
            }
            if (newValue.Value != currentValue.Value)
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el margen bruto de {currentValue.Value} a {newValue.Value}.\n";
            }
            return log;
        }
    }
}
