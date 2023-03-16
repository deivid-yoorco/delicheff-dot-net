using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Linq;
using Teed.Plugin.TicketControl.Models.Schedule;
using Teed.Plugin.TicketControl.Services;
using Teed.Plugin.TicketControl.Security;
using Teed.Plugin.TicketControl.Domain.Schedules;
using System.Globalization;
using Nop.Web.Utils;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Nop.Services.Tasks;
using Nop.Core.Domain.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Teed.Plugin.TicketControl.Controllers
{
    [Area(AreaNames.Admin)]
    public class ScheduleController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ScheduleService _scheduleService;
        private readonly DatePersonalizationService _datePersonalizationService;
        private readonly IOrderService _orderService;
        private readonly IScheduleTaskService _scheduleTaskService;

        public ScheduleController(IPermissionService permissionService, IWorkContext workContext, ScheduleService scheduleService,
            DatePersonalizationService datePersonalizationService, IOrderService orderService,
            IScheduleTaskService scheduleTaskService)
        {
            _permissionService = permissionService;
            _workContext = workContext;
            _scheduleService = scheduleService;
            _datePersonalizationService = datePersonalizationService;
            _orderService = orderService;
            _scheduleTaskService = scheduleTaskService;
        }

        [AuthorizeAdmin]
        public IActionResult Index()
        {
            if (!_permissionService.Authorize(TeedTicketControlPermissionProvider.TicketControl))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.TicketControl/Views/Schedule/Index.cshtml");
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedTicketControlPermissionProvider.TicketControl))
                return AccessDeniedView();

            var schedules = _scheduleService.GetAll().ToList();

            var gridModel = new DataSourceResult
            {
                Data = schedules.OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name).Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.DisplayOrder,
                    x.Quantity,
                    Hour = ReturnHourString(x.Hour)
                }).ToList(),
                Total = schedules.Count()
            };

            return Json(gridModel);
        }

        protected virtual string ReturnHourString(int hour)
        {
            try
            {
                var date = new DateTime(2000, 1, 1, hour, 0, 0);
                return date.ToString("h:mm tt");
            }
            catch (Exception e)
            {
                return hour.ToString();
            }
        }

        [AuthorizeAdmin]
        public IActionResult Create()
        {
            if (!_permissionService.Authorize(TeedTicketControlPermissionProvider.TicketControl))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.TicketControl/Views/Schedule/Create.cshtml", new CreateOrEditModel());
        }

        [AuthorizeAdmin]
        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedTicketControlPermissionProvider.TicketControl))
                return AccessDeniedView();

            var schedule = _scheduleService.GetById(id);
            if (schedule == null)
                return View("~/Plugins/Teed.Plugin.TicketControl/Views/Schedule/Index.cshtml");

            var model = new CreateOrEditModel
            {
                Id = schedule.Id,
                Name = schedule.Name,
                DisplayOrder = schedule.DisplayOrder,
                Quantity = schedule.Quantity,
                Log = schedule.Log,
                Hour = schedule.Hour
            };

            return View("~/Plugins/Teed.Plugin.TicketControl/Views/Schedule/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [AuthorizeAdmin]
        public IActionResult Create(CreateOrEditModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedTicketControlPermissionProvider.TicketControl))
                return AccessDeniedView();

            var schedule = new Schedule
            {
                Name = model.Name,
                DisplayOrder = model.DisplayOrder,
                Quantity = model.Quantity,
                Hour = model.Hour,
                Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) creó un nuevo horario ({model.Name}).\n"
            };
            _scheduleService.Insert(schedule);

            if (continueEditing)
            {
                return RedirectToAction("Edit", new { id = schedule.Id });
            }
            return View("~/Plugins/Teed.Plugin.TicketControl/Views/Schedule/Index.cshtml");
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [AuthorizeAdmin]
        public IActionResult Edit(CreateOrEditModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedTicketControlPermissionProvider.TicketControl))
                return AccessDeniedView();

            var schedule = _scheduleService.GetById(model.Id);
            if (schedule == null)
                return View("~/Plugins/Teed.Plugin.TicketControl/Views/Schedule/Index.cshtml");

            // Add of log
            if (schedule.Name != model.Name)
            {
                schedule.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el nombre de {schedule.Name} a {model.Name}.\n";
                schedule.Name = model.Name;
            }
            if (schedule.DisplayOrder != model.DisplayOrder)
            {
                schedule.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el orden para mostrar de {schedule.DisplayOrder} a {model.DisplayOrder}.\n";
                schedule.DisplayOrder = model.DisplayOrder;
            }
            if (schedule.Quantity != model.Quantity)
            {
                schedule.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió la cantidad de {schedule.Quantity} a {model.Quantity}.\n";
                schedule.Quantity = model.Quantity;
            }
            if (schedule.Hour != model.Hour)
            {
                schedule.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió la hora de {schedule.Hour} a {model.Hour}.\n";
                schedule.Hour = model.Hour;
            }
            _scheduleService.Update(schedule);

            if (continueEditing)
            {
                return RedirectToAction("Edit", new { id = schedule.Id });
            }
            return View("~/Plugins/Teed.Plugin.TicketControl/Views/Schedule/Index.cshtml");
        }


        [HttpGet]
        [Route("[controller]/[action]")]
        [AllowAnonymous]
        public IActionResult GetWebTimes(string date)
        {
            DateTime parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;
            var personalizedDates = _datePersonalizationService.GetAllByDate(parsedDate);
            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate == parsedDate);
            var schedules = _scheduleService.GetAll().OrderBy(x => x.DisplayOrder).ToList();

            var allOrdersScheduleQuery = ordersQuery
                .Select(x => new { SelectedShippingTime = x.SelectedShippingTime, x.CustomerId })
                .ToList();

            var options = new List<string>
            {
                "<option disabled value=\"0\">Selecciona el horario para tu ticket...</option>"
            };
            var anyDisabled = false;

            foreach (var schedule in schedules)
            {
                var hourAlreadyPassed = DateTime.Now.Hour >= schedule.Hour;
                if (parsedDate.Date != DateTime.Now.Date)
                    hourAlreadyPassed = false;
                if (hourAlreadyPassed)
                {
                    options.Add($"<option disabled value=\"{schedule.Name} [{schedule.Id}]\">{schedule.Name} (FUERA DE HORARIO)</option>");
                }
                else
                {
                    var optionTimeDisabled = false;
                    var ordersOfSchedule = 0;
                    var ordersWithSchedule = allOrdersScheduleQuery
                        .Where(x => x.SelectedShippingTime.Contains('[') && x.SelectedShippingTime.Contains(']'))
                        .ToList();
                    if (ordersWithSchedule.Any())
                    {
                        ordersOfSchedule = ordersWithSchedule
                            .Where(x => x.SelectedShippingTime.Split('[', ']')[1] == schedule.Id.ToString()).Count();
                    }
                    var personalizedDate = personalizedDates.Where(x => x.ScheduleId == schedule.Id).FirstOrDefault();
                    if (personalizedDate != null)
                        optionTimeDisabled = ordersOfSchedule >= personalizedDate.Quantity;
                    else
                        optionTimeDisabled = ordersOfSchedule >= schedule.Quantity;

                    options.Add($"<option {(optionTimeDisabled ? "disabled" : "")} value=\"{schedule.Name} [{schedule.Id}]\">{schedule.Name}{(optionTimeDisabled ? " (HORARIO LLENO)" : "")}</option>");
                    if (!anyDisabled && optionTimeDisabled)
                        anyDisabled = true;
                }
            }

            return Ok(new
            {
                optionValue = string.Join("\n", options),
                anyDisabled = anyDisabled
            });
        }

        [HttpGet]
        [Route("[controller]/[action]")]
        [AllowAnonymous]
        public IActionResult GetAllTimesSelectList()
        {
            var times = _scheduleService.GetAll().Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            return Ok(times);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(TeedTicketControlPermissionProvider.TicketControl))
                return AccessDeniedView();

            var schedule = _scheduleService.GetById(id);
            if (schedule == null)
                return RedirectToAction("Index");

            schedule.Deleted = true;
            schedule.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) eliminó el horario.\n";
            var datesWithSchedule = _datePersonalizationService.GetAll()
                .Where(x => x.ScheduleId == schedule.Id).ToList();
            foreach (var date in datesWithSchedule)
            {
                date.Deleted = true;
                date.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) eliminó el horario de la fecha personalizada, por lo que se borró esta fecha de forma automatica.\n";
                _datePersonalizationService.Update(date);
            }
            _scheduleService.Update(schedule);
            return RedirectToAction("Index");
        }
    }
}