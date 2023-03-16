using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teed.Plugin.TicketControl.Domain.DatePersonalizations;
using Teed.Plugin.TicketControl.Domain.Schedules;
using Teed.Plugin.TicketControl.Models.DatePersonalization;
using Teed.Plugin.TicketControl.Security;
using Teed.Plugin.TicketControl.Services;

namespace Teed.Plugin.TicketControl.Controllers
{
    [Area(AreaNames.Admin)]
    public class DatePersonalizationController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly DatePersonalizationService _datePersonalizationService;
        private readonly ScheduleService _scheduleService;

        public DatePersonalizationController(IPermissionService permissionService, IWorkContext workContext,
            DatePersonalizationService datePersonalizationService, ScheduleService scheduleService)
        {
            _permissionService = permissionService;
            _workContext = workContext;
            _datePersonalizationService = datePersonalizationService;
            _scheduleService = scheduleService;
        }

        [AuthorizeAdmin]
        public IActionResult Index()
        {
            if (!_permissionService.Authorize(TeedTicketControlPermissionProvider.TicketControl))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.TicketControl/Views/DatePersonalization/Index.cshtml");
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedTicketControlPermissionProvider.TicketControl))
                return AccessDeniedView();

            var dates = _datePersonalizationService.GetAll().GroupBy(x => x.Date).ToList();
            var schedules = _scheduleService.GetAll().ToList();

            var gridModel = new DataSourceResult
            {
                Data = dates.OrderByDescending(x => x.Key)
                .Select(x => new
                {
                    Date = x.Key.ToString("dd/MM/yyyy"),
                    Quantities = BuildQuantities(x, schedules),
                }).ToList(),
                Total = dates.Count()
            };

            return Json(gridModel);
        }

        protected virtual string BuildQuantities(IGrouping<DateTime, DatePersonalization> dates,
            List<Schedule> schedules)
        {
            var final = string.Empty;
            foreach (var date in dates)
            {
                var scheduleOfDate = _scheduleService.GetById(date.ScheduleId);
                final += $"{scheduleOfDate.Name}: {date.Quantity}<br />\n";
            }
            return final;
        }

        [AuthorizeAdmin]
        public IActionResult Create()
        {
            if (!_permissionService.Authorize(TeedTicketControlPermissionProvider.TicketControl))
                return AccessDeniedView();

            var model = new CreateOrEditModel();
            DateUpload(model, true);

            return View("~/Plugins/Teed.Plugin.TicketControl/Views/DatePersonalization/Create.cshtml", model);
        }

        [AuthorizeAdmin]
        public IActionResult Edit(string dateString)
        {
            if (!_permissionService.Authorize(TeedTicketControlPermissionProvider.TicketControl))
                return AccessDeniedView();

            var parseDate = DateTime.ParseExact(dateString, "dd/MM/yyyy", CultureInfo.InvariantCulture).Date;
            var date = _datePersonalizationService.GetAllByDate(parseDate).FirstOrDefault();
            if (date == null)
                return View("~/Plugins/Teed.Plugin.TicketControl/Views/DatePersonalization/Index.cshtml");

            var model = new CreateOrEditModel
            {
                Id = date.Id,
                Date = date.Date,
                OriginalDate = date.Date,
            };
            DateUpload(model, true);

            return View("~/Plugins/Teed.Plugin.TicketControl/Views/DatePersonalization/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [AuthorizeAdmin]
        public IActionResult Create(CreateOrEditModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedTicketControlPermissionProvider.TicketControl))
                return AccessDeniedView();

            DateUpload(model);

            if (continueEditing)
            {
                return RedirectToAction("Edit", new { dateString = model.Date.ToString("dd/MM/yyyy") });
            }
            return View("~/Plugins/Teed.Plugin.TicketControl/Views/DatePersonalization/Index.cshtml");
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [AuthorizeAdmin]
        public IActionResult Edit(CreateOrEditModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedTicketControlPermissionProvider.TicketControl))
                return AccessDeniedView();

            var date = _datePersonalizationService.GetAllByDate(model.OriginalDate.Date).FirstOrDefault();
            if (date == null)
                return View("~/Plugins/Teed.Plugin.TicketControl/Views/DatePersonalization/Index.cshtml");

            DateUpload(model);

            if (continueEditing)
            {
                return RedirectToAction("Edit", new { dateString = model.Date.ToString("dd/MM/yyyy") });
            }
            return View("~/Plugins/Teed.Plugin.TicketControl/Views/DatePersonalization/Index.cshtml");
        }

        protected virtual bool DateUpload(CreateOrEditModel model, bool getDates = false)
        {
            var schedules = _scheduleService.GetAll().ToList();
            List<DatePersonalization> currentDates = new List<DatePersonalization>();
            if (model.OriginalDate != DateTime.MinValue && model.OriginalDate != null)
                currentDates = _datePersonalizationService.GetAllByDate(model.OriginalDate.Date).ToList();
            var ok = false;
            try
            {
                if (!getDates)
                {
                    var split = model.ScheduleSplit;
                    if (!string.IsNullOrEmpty(split))
                    {
                        foreach (var currentDate in currentDates)
                        {
                            currentDate.Deleted = true;
                            currentDate.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó una fecha personalizada, original: {model.OriginalDate:dd/MM/yyyy}, nueva {model.Date:dd/MM/yyyy}, se eliminaron fechas con el dato original de forma automatica.\n";
                            _datePersonalizationService.Update(currentDate);
                        }
                        var dates = split.Split('|');
                        foreach (var date in dates)
                        {
                            var infoOfDate = date.Split('-');
                            var scheduleId = int.Parse(infoOfDate.FirstOrDefault());
                            var quantity = int.Parse(infoOfDate.LastOrDefault());

                            _datePersonalizationService.Insert(new DatePersonalization
                            {
                                Date = model.Date,
                                Quantity = quantity,
                                ScheduleId = scheduleId,
                                Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) creó una nueva fecha personalizada ({model.Date:dd/MM/yyyy}) con cantidad de {quantity} y horario con nombre \"{schedules.Where(x => x.Id == scheduleId).FirstOrDefault()?.Name}\".\n"
                            });
                        }
                        ok = true;
                    }
                }
                else
                {
                    var listOfSchedules = new List<string>();
                    foreach (var scheduleOfDate in schedules)
                    {
                        var currentDate = currentDates.Where(x => x.ScheduleId == scheduleOfDate.Id).FirstOrDefault();
                        if (currentDate != null)
                        {
                            var schedule = new Schedules
                            {
                                Id = currentDate.ScheduleId,
                                Name = schedules.Where(x => x.Id == currentDate.ScheduleId).FirstOrDefault()?.Name,
                                OriginalQuantity = currentDate.Quantity
                            };
                            model.Schedules.Add(schedule);
                            listOfSchedules.Add($"{schedule.Id}-{schedule.OriginalQuantity}");
                        }
                        else
                        {
                            var schedule = new Schedules
                            {
                                Id = scheduleOfDate.Id,
                                Name = scheduleOfDate.Name,
                                OriginalQuantity = scheduleOfDate.Quantity
                            };
                            model.Schedules.Add(schedule);
                            listOfSchedules.Add($"{schedule.Id}-{schedule.OriginalQuantity}");
                        }
                    }
                    model.ScheduleSplit = string.Join("|", listOfSchedules);
                    currentDates = _datePersonalizationService.GetAllByDate(model.OriginalDate.Date, true).ToList();
                    model.Log = string.Join("\n", currentDates.Select(x => x.Log));

                    ok = true;
                }
                var takenDates = _datePersonalizationService.GetAll().ToList().Where(x => x.Date != model.OriginalDate).Select(x => x.Date);
                if (takenDates.Any())
                    model.TakenDates.AddRange(takenDates);
            }
            catch (Exception e)
            {
                var err = e;
            }

            return ok;
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(TeedTicketControlPermissionProvider.TicketControl))
                return AccessDeniedView();

            var date = _datePersonalizationService.GetById(id);
            if (date == null)
                return RedirectToAction("Index");

            var datesOfDate = _datePersonalizationService.GetAllByDate(date.Date.Date)
                .ToList();
            foreach (var dateInner in datesOfDate)
            {
                dateInner.Deleted = true;
                dateInner.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) eliminó la fecha perzonalizada, por lo que se borró todas las fechas perzonalizadas con la fecha {date.Date.ToString("dd/MM/yyyy")} de automatica.\n";
                _datePersonalizationService.Update(dateInner);
            }
            return RedirectToAction("Index");
        }
    }
}