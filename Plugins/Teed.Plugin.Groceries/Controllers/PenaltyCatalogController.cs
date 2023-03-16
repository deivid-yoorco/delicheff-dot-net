using iText.Forms.Xfdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.Franchise;
using Teed.Plugin.Groceries.Domain.PenaltiesCatalog;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;
using Teed.Plugin.Groceries.Models.Franchise;
using Teed.Plugin.Groceries.Models.RatesAndBonuses;
using Teed.Plugin.Groceries.Models.ShippingUserRoute;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class PenaltyCatalogController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly ShippingRouteService _shippingRouteService;
        private readonly ShippingRouteUserService _shippingRouteUserService;
        private readonly IOrderService _orderService;
        private readonly FranchiseService _franchiseService;
        private readonly ISettingService _settingService;
        private readonly PenaltiesCatalogService _penaltiesCatalogService;
        private readonly BillingService _billingService;
        private readonly FranchiseBonusService _franchiseBonusService;

        public PenaltyCatalogController(IPermissionService permissionService, ShippingRouteService shippingRouteService,
            ShippingRouteUserService shippingRouteUserService, IWorkContext workContext, ICustomerService customerService,
            IOrderService orderService, FranchiseService franchiseService, ISettingService settingService,
            PenaltiesCatalogService penaltiesCatalogService, BillingService billingService, FranchiseBonusService franchiseBonusService)
        {
            _permissionService = permissionService;
            _shippingRouteService = shippingRouteService;
            _shippingRouteUserService = shippingRouteUserService;
            _workContext = workContext;
            _customerService = customerService;
            _orderService = orderService;
            _franchiseService = franchiseService;
            _settingService = settingService;
            _penaltiesCatalogService = penaltiesCatalogService;
            _billingService = billingService;
            _franchiseBonusService = franchiseBonusService;
        }

        public IActionResult PenaltiesList()
        {
            return View("~/Plugins/Teed.Plugin.Groceries/Views/PenaltiesCatalog/PenaltiesList.cshtml");
        }

        [HttpPost]
        public IActionResult PenaltiesListData(DataSourceRequest command)
        {
            var query = _penaltiesCatalogService.GetAll().OrderBy(m => m.PenaltyCustomId).ThenBy(x => x.ApplyDate);
            var pagedList = new PagedList<PenaltiesCatalog>(query, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    x.Concepto,
                    x.Unit,
                    x.PenaltyCustomId,
                    x.Amount,
                    Date = x.ApplyDate.ToString("dd-MM-yyyy")
                }).ToList(),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        public IActionResult Create()
        {
            PenaltyCatalogModel model = new PenaltyCatalogModel();

            model.ApplyDateString = DateTime.Now.ToString("dd-MM-yyyy");

            return View("~/Plugins/Teed.Plugin.Groceries/Views/PenaltiesCatalog/Create.cshtml", model);
        }

        [HttpPost]
        public IActionResult Create(PenaltyCatalogModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            if (!string.IsNullOrEmpty(model.ApplyDateString))
                model.ApplyDate = DateTime.ParseExact(model.ApplyDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            PenaltiesCatalog penaltyCatalog = new PenaltiesCatalog()
            {
                Amount = model.Amount,
                Concepto = model.Concepto,
                PenaltyCustomId = model.PenaltyCustomId,
                Unit = model.Unit,
                ApplyDate = model.ApplyDate,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) creó la penalización {model.PenaltyCustomId}.\n"
            };
            _penaltiesCatalogService.Insert(penaltyCatalog);

            _billingService.RecalculateIncidents(penaltyCatalog.ApplyDate, penaltyCatalog.PenaltyCustomId);

            return RedirectToAction("Edit", new { Id = penaltyCatalog.Id });
        }

        public IActionResult Edit(int id)
        {
            PenaltiesCatalog penalty = _penaltiesCatalogService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (penalty == null) return NotFound();

            PenaltyCatalogModel model = new PenaltyCatalogModel()
            {
                Id = penalty.Id,
                Amount = penalty.Amount,
                Concepto = penalty.Concepto,
                PenaltyCustomId = penalty.PenaltyCustomId,
                Unit = penalty.Unit,
                ApplyDate = penalty.ApplyDate,
                ApplyDateString = penalty.ApplyDate.ToString("dd-MM-yyyy"),
                Log = penalty.Log
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/PenaltiesCatalog/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult Edit(PenaltyCatalogModel model)
        {
            PenaltiesCatalog penalty = _penaltiesCatalogService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (penalty == null) return NotFound();

            if (!ModelState.IsValid) return BadRequest();

            if (!string.IsNullOrEmpty(model.ApplyDateString))
                model.ApplyDate = DateTime.ParseExact(model.ApplyDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            bool shouldRecalculate = false;
            string log = string.Empty;
            if (penalty.Unit != model.Unit)
            {
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió la unidad de la penalización de {penalty.Unit} por {model.Unit}.\n";
                penalty.Log += log;

                penalty.Unit = model.Unit;
            }
            if (penalty.Amount != model.Amount)
            {
                shouldRecalculate = true;
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el monto de la penalización de {penalty.Amount} por {model.Amount}.\n";
                penalty.Log += log;

                penalty.Amount = model.Amount;
            }
            if (penalty.Concepto != model.Concepto)
            {
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el concepto de la penalización de {penalty.Concepto} por {model.Concepto}.\n";
                penalty.Log += log;

                penalty.Concepto = model.Concepto;
            }
            if (penalty.PenaltyCustomId != model.PenaltyCustomId)
            {
                shouldRecalculate = true;
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el Id personalizable de la penalización de {penalty.PenaltyCustomId} por {model.PenaltyCustomId}.\n";
                penalty.Log += log;

                penalty.PenaltyCustomId = model.PenaltyCustomId;
            }
            if (penalty.ApplyDate != model.ApplyDate)
            {
                shouldRecalculate = true;
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió la fecha de aplicación de la penalización de {penalty.ApplyDate.ToString("dd/MM/yyyy")} por {model.ApplyDate.ToString("dd/MM/yyyy")}.\n";
                penalty.Log += log;

                penalty.ApplyDate = model.ApplyDate;
            }

            _penaltiesCatalogService.Update(penalty);

            _billingService.RecalculateIncidents(penalty.ApplyDate, penalty.PenaltyCustomId);
            if (penalty.PenaltyCustomId == "I01" || penalty.PenaltyCustomId == "I04")
            {
                var penalties = _penaltiesCatalogService.GetAll().ToList();
                _franchiseBonusService.RecalculateBonus(penalties, penalty.ApplyDate);
            }

            return RedirectToAction("Edit", new { Id = penalty.Id });
        }

        //delete
        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            var penalty = _penaltiesCatalogService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (penalty == null)
                //No recurring payment found with the specified id
                return RedirectToAction("PenaltiesList");

            _penaltiesCatalogService.Delete(penalty);

            return RedirectToAction("PenaltiesList");
        }
    }
}
