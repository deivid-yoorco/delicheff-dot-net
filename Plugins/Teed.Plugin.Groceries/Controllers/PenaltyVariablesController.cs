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
using Teed.Plugin.Groceries.Domain.ShippingRoutes;
using Teed.Plugin.Groceries.Models.Franchise;
using Teed.Plugin.Groceries.Models.RatesAndBonuses;
using Teed.Plugin.Groceries.Models.ShippingUserRoute;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class PenaltyVariablesController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly ShippingRouteService _shippingRouteService;
        private readonly ShippingRouteUserService _shippingRouteUserService;
        private readonly IOrderService _orderService;
        private readonly FranchiseService _franchiseService;
        private readonly ISettingService _settingService;
        private readonly PenaltiesVariablesService _penaltiesVariablesService;
        private readonly BillingService _billingService;
        private readonly PenaltiesCatalogService _penaltiesCatalogService;

        public PenaltyVariablesController(IPermissionService permissionService, ShippingRouteService shippingRouteService,
            ShippingRouteUserService shippingRouteUserService, IWorkContext workContext, ICustomerService customerService,
            IOrderService orderService, FranchiseService franchiseService, ISettingService settingService,
            PenaltiesVariablesService penaltiesVariablesService, BillingService billingService,
            PenaltiesCatalogService penaltiesCatalogService)
        {
            _permissionService = permissionService;
            _shippingRouteService = shippingRouteService;
            _shippingRouteUserService = shippingRouteUserService;
            _workContext = workContext;
            _customerService = customerService;
            _orderService = orderService;
            _franchiseService = franchiseService;
            _settingService = settingService;
            _penaltiesVariablesService = penaltiesVariablesService;
            _billingService = billingService;
            _penaltiesCatalogService = penaltiesCatalogService;
        }

        public IActionResult PenaltiesList()
        {
            return View("~/Plugins/Teed.Plugin.Groceries/Views/PenaltiesVariables/PenaltiesList.cshtml");
        }

        [HttpPost]
        public IActionResult PenaltiesListData(DataSourceRequest command)
        {
            var query = _penaltiesVariablesService.GetAll().OrderBy(m => m.PenaltyCustomId).ThenBy(x => x.ApplyDate);
            var pagedList = new PagedList<PenaltiesVariables>(query, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    x.PenaltyCustomId,
                    x.IncidentCode,
                    x.Coefficient,
                    Date = x.ApplyDate.ToString("dd-MM-yyyy")
                }).ToList(),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        public IActionResult Create()
        {
            PenaltyVariablesModel model = new PenaltyVariablesModel();

            model.ApplyDateString = DateTime.Now.ToString("dd-MM-yyyy");
            PrepareModel(model);

            return View("~/Plugins/Teed.Plugin.Groceries/Views/PenaltiesVariables/Create.cshtml", model);
        }

        [HttpPost]
        public IActionResult Create(PenaltyVariablesModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            if (!string.IsNullOrEmpty(model.ApplyDateString))
                model.ApplyDate = DateTime.ParseExact(model.ApplyDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            PenaltiesVariables penaltyVariables = new PenaltiesVariables()
            {
                Coefficient = model.Coefficient,
                IncidentCode = model.IncidentCode,
                PenaltyCustomId = model.PenaltyCustomId,
                ApplyDate = model.ApplyDate,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) creó la variable de incidencia.\n"
            };
            _penaltiesVariablesService.Insert(penaltyVariables);

            _billingService.RecalculateIncidents(penaltyVariables.ApplyDate, penaltyVariables.PenaltyCustomId, model.IncidentCode);

            return RedirectToAction("Edit", new { Id = penaltyVariables.Id });
        }

        public IActionResult Edit(int id)
        {
            PenaltiesVariables penalty = _penaltiesVariablesService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (penalty == null) return NotFound();

            PenaltyVariablesModel model = new PenaltyVariablesModel()
            {
                Id = penalty.Id,
                Coefficient = penalty.Coefficient,
                IncidentCode = penalty.IncidentCode,
                PenaltyCustomId = penalty.PenaltyCustomId,
                ApplyDate = penalty.ApplyDate,
                ApplyDateString = penalty.ApplyDate.ToString("dd-MM-yyyy"),
                Log = penalty.Log
            };

            PrepareModel(model);

            return View("~/Plugins/Teed.Plugin.Groceries/Views/PenaltiesVariables/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult Edit(PenaltyVariablesModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            PenaltiesVariables penalty = _penaltiesVariablesService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (penalty == null) return NotFound();

            if (!string.IsNullOrEmpty(model.ApplyDateString))
                model.ApplyDate = DateTime.ParseExact(model.ApplyDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            bool shouldRecalculate = false;
            string log = string.Empty;

            if (penalty.PenaltyCustomId != model.PenaltyCustomId)
            {
                shouldRecalculate = true;
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el indicador base de {penalty.PenaltyCustomId} a {model.PenaltyCustomId}.\n";
                penalty.Log += log;

                penalty.PenaltyCustomId = model.PenaltyCustomId;
            }
            if (penalty.ApplyDate != model.ApplyDate)
            {
                shouldRecalculate = true;
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió la fecha de aplicación de {penalty.ApplyDate.ToString("dd/MM/yyyy")} por {model.ApplyDate.ToString("dd/MM/yyyy")}.\n";
                penalty.Log += log;

                penalty.ApplyDate = model.ApplyDate;
            }
            if (penalty.Coefficient != model.Coefficient)
            {
                shouldRecalculate = true;
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el coeficiente de {penalty.Coefficient} por {model.Coefficient}.\n";
                penalty.Log += log;

                penalty.Coefficient = model.Coefficient;
            }
            if (penalty.IncidentCode != model.IncidentCode)
            {
                shouldRecalculate = true;
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el código de la incidencia  de {penalty.IncidentCode} por {model.IncidentCode}.\n";
                penalty.Log += log;

                penalty.IncidentCode = model.IncidentCode;
            }

            _penaltiesVariablesService.Update(penalty);

            //if (shouldRecalculate)
            _billingService.RecalculateIncidents(penalty.ApplyDate, penalty.PenaltyCustomId, model.IncidentCode);

            return RedirectToAction("Edit", new { Id = penalty.Id });
        }

        //delete
        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            var penalty = _penaltiesVariablesService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (penalty == null)
                //No recurring payment found with the specified id
                return RedirectToAction("PenaltiesList");

            _penaltiesVariablesService.Delete(penalty);

            return RedirectToAction("PenaltiesList");
        }

        private void PrepareModel(PenaltyVariablesModel model)
        {
            var penalties = IncidentCodes.CodeDictionary();
            model.Penalties = penalties.Select(x => new SelectListItem()
            {
                Value = x.Key.ToString(),
                Text = $"{x.Key} - {x.Value}"
            }).ToList();

            model.PenaltiesCatalog = _penaltiesCatalogService.GetAll().GroupBy(x => x.PenaltyCustomId).Select(x => new SelectListItem()
            {
                Value = x.Select(y => y.PenaltyCustomId).FirstOrDefault(),
                Text = x.Select(y => y.PenaltyCustomId).FirstOrDefault() + " - " + x.OrderByDescending(y => y.ApplyDate).Select(y => y.Amount).FirstOrDefault() + " MXN",
            }).ToList();
        }
    }
}
