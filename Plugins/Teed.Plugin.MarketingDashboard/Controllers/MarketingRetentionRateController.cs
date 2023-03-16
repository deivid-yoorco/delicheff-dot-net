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
    public class MarketingRetentionRateController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly MarketingRetentionRateService _marketingRetentionRateService;

        public MarketingRetentionRateController(IPermissionService permissionService, MarketingRetentionRateService marketingRetentionRateService,
            IWorkContext workContext)
        {
            _permissionService = permissionService;
            _marketingRetentionRateService = marketingRetentionRateService;
            _workContext = workContext;
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.MarketingDashboard/Views/MarketingRetentionRate/List.cshtml");
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var model = new MarketingRetentionRateModel();
            model.ApplyDateString = DateTime.Now.ToString("dd-MM-yyyy");

            return View("~/Plugins/Teed.Plugin.MarketingDashboard/Views/MarketingRetentionRate/Create.cshtml", model);
        }

        [HttpPost]
        public IActionResult Create(MarketingRetentionRateModel model)
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            model.ApplyDate = DateTime.ParseExact(model.ApplyDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            MarketingRetentionRate marketingRetentionRate = new MarketingRetentionRate()
            {
                ApplyDate = model.ApplyDate,
                Value = model.Value,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) creó el registro de tasa de retención.\n"
            };
            _marketingRetentionRateService.Insert(marketingRetentionRate);

            return RedirectToAction("List");
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var marketingRetentionRate = _marketingRetentionRateService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (marketingRetentionRate == null) return NotFound();

            var model = new MarketingRetentionRateModel()
            {
                Id = marketingRetentionRate.Id,
                ApplyDate = marketingRetentionRate.ApplyDate,
                ApplyDateString = marketingRetentionRate.ApplyDate.ToString("dd-MM-yyyy"),
                Value = marketingRetentionRate.Value,
                Log = marketingRetentionRate.Log
            };

            return View("~/Plugins/Teed.Plugin.MarketingDashboard/Views/MarketingRetentionRate/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult Edit(MarketingRetentionRateModel model)
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var MarketingRetentionRate = _marketingRetentionRateService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (MarketingRetentionRate == null) return NotFound();

            model.ApplyDate = DateTime.ParseExact(model.ApplyDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            string log = PrepareLog(model, MarketingRetentionRate);
            MarketingRetentionRate.ApplyDate = model.ApplyDate;
            MarketingRetentionRate.Value = model.Value;
            MarketingRetentionRate.Log += log;
            _marketingRetentionRateService.Update(MarketingRetentionRate);

            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var MarketingRetentionRate = _marketingRetentionRateService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (MarketingRetentionRate == null) return NotFound();

            MarketingRetentionRate.Log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - El elemento fue eliminado por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}).\n";

            _marketingRetentionRateService.Delete(MarketingRetentionRate);

            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(MarketingDashboardPermissionProvider.MarketingExpenses))
                return AccessDeniedView();

            var query = _marketingRetentionRateService.GetAll();
            var pagedList = new PagedList<MarketingRetentionRate>(query.OrderByDescending(m => m.CreatedOnUtc), command.Page - 1, command.PageSize);

            DataSourceResult gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    Value = x.Value,
                    ApplyDate = x.ApplyDate.ToString("dd-MM-yyyy")
                }),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        private string PrepareLog(MarketingRetentionRateModel newValue, MarketingRetentionRate currentValue)
        {
            string log = string.Empty;
            if (newValue.ApplyDate != currentValue.ApplyDate)
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió la fecha a partir de la que aplica de {currentValue.ApplyDate.ToString("dd-MM-yyyy")} a {newValue.ApplyDate.ToString("dd-MM-yyyy")}.\n";
            }
            if (newValue.Value != currentValue.Value)
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió la tasa de retención de {currentValue.Value} a {newValue.Value}.\n";
            }
            return log;
        }
    }
}
