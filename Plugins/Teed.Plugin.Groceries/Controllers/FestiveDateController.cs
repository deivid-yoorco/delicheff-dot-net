using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.FestiveDates;
using Teed.Plugin.Groceries.Models.FestiveDate;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class FestiveDateController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly FestiveDateService _festiveDateService;

        public FestiveDateController(IPermissionService permissionService,
            IWorkContext workContext,
            IOrderService orderService,
            FestiveDateService festiveDateService)
        {
            _permissionService = permissionService;
            _workContext = workContext;
            _orderService = orderService;
            _festiveDateService = festiveDateService;
        }

        public void PrepareModel(FestiveDateModel model)
        {
            var dates = _festiveDateService.GetAll().ToList();
            if (dates.Any())
                model.TakenDates = dates;
        }

        [AuthorizeAdmin]
        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.FestiveDates))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/FestiveDate/List.cshtml");
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.FestiveDates))
                return AccessDeniedView();

            var festiveDates = _festiveDateService.GetAll().OrderByDescending(x => x.Date).ToList();
            var pagedList = new PagedList<FestiveDate>(festiveDates, command.Page - 1, command.PageSize);

            var data = pagedList.Select(x => new {
                x.Id,
                Date = x.AppliesYearly ? x.Date.ToString("dd/MM") : x.Date.ToString("dd/MM/yyyy"),
                x.AppliesYearly,
                x.DontApplyToPayroll
            }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = data,
                Total = festiveDates.Count()
            };

            return Json(gridModel);
        }

        [AuthorizeAdmin]
        public IActionResult Create()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.FestiveDates))
                return AccessDeniedView();

            var model = new FestiveDateModel();
            PrepareModel(model);

            return View("~/Plugins/Teed.Plugin.Groceries/Views/FestiveDate/Create.cshtml", model);
        }

        [AuthorizeAdmin]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(FestiveDateModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.FestiveDates))
                return AccessDeniedView();

            var festiveDate = new FestiveDate
            {
                Date = model.Date,
                AppliesYearly = model.AppliesYearly,
                DontApplyToPayroll = model.DontApplyToPayroll,
                Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) agregó una nueva fecha festiva {(model.AppliesYearly ? model.Date.ToString("MM/yyyy") : model.Date.ToString("dd/MM/yyyy"))}\n"
            };

            _festiveDateService.Insert(festiveDate);

            if (continueEditing)
            {
                SuccessNotification("Se guardaron los cambios de la fecha festiva de forma correcta.");
                PrepareModel(model);
                return RedirectToAction("Edit", new { id = festiveDate.Id });
            }
            return RedirectToAction("List");
        }

        [AuthorizeAdmin]
        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.FestiveDates))
                return AccessDeniedView();

            var festiveDate = _festiveDateService.GetById(id);
            if (festiveDate == null)
                return RedirectToAction("List");

            var model = new FestiveDateModel
            {
                Id = festiveDate.Id,
                AppliesYearly = festiveDate.AppliesYearly,
                DontApplyToPayroll = festiveDate.DontApplyToPayroll,
                Date = festiveDate.Date,
                Log = festiveDate.Log
            };
            PrepareModel(model);

            return View("~/Plugins/Teed.Plugin.Groceries/Views/FestiveDate/Edit.cshtml", model);
        }

        [AuthorizeAdmin]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(FestiveDateModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.FestiveDates))
                return AccessDeniedView();

            var festiveDate = _festiveDateService.GetById(model.Id);
            if (festiveDate == null)
                return RedirectToAction("List");

            if (festiveDate.Date != model.Date)
            {
                festiveDate.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió la fecha de {(festiveDate.AppliesYearly ? festiveDate.Date.ToString("dd/MM") : festiveDate.Date.ToString("dd/MM/yyyy"))} a {(model.AppliesYearly ? model.Date.ToString("dd/MM") : model.Date.ToString("dd/MM/yyyy"))}\n";
                festiveDate.Date = model.Date;
            }
            if (festiveDate.AppliesYearly != model.AppliesYearly)
            {
                festiveDate.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió si aplica de forma anual de {(festiveDate.AppliesYearly ? "\"Si\"" : "\"No\"")} a {(model.AppliesYearly ? "\"Si\"" : "\"No\"")}\n";
                festiveDate.AppliesYearly = model.AppliesYearly;
            }
            if (festiveDate.DontApplyToPayroll != model.DontApplyToPayroll)
            {
                festiveDate.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió si no aplica para el area de nómina {(festiveDate.DontApplyToPayroll ? "\"Si\"" : "\"No\"")} a {(model.DontApplyToPayroll ? "\"Si\"" : "\"No\"")}\n";
                festiveDate.DontApplyToPayroll = model.DontApplyToPayroll;
            }

            _festiveDateService.Update(festiveDate);

            if (continueEditing)
            {
                SuccessNotification("Se guardaron los cambios de la fecha festiva de forma correcta.");
                PrepareModel(model);
                return RedirectToAction("Edit", new { id = festiveDate.Id });
            }
            return RedirectToAction("List");
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.FestiveDates))
                return AccessDeniedView();

            var onboarding = _festiveDateService.GetById(id);
            if (onboarding == null)
                return RedirectToAction("List");

            _festiveDateService.Delete(onboarding);

            return RedirectToAction("List");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetFestiveDates()
        {
            var dates = _festiveDateService.GetAll().ToList();
            if (!dates.Any())
                return Ok("");
            else
            {
                return Ok(JsonConvert.SerializeObject(dates));
            }
        }
    }
}
