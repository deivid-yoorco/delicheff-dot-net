using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Appointment;
using Teed.Plugin.Medical.Services;

namespace Teed.Plugin.Medical.Controllers
{
    [Area(AreaNames.Admin)]
    public class PublicController : BasePluginController
    {
        private readonly AppointmentService _appointmentService;
        private readonly PatientService _patientService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;

        public PublicController(AppointmentService appointmentService,
            PatientService patientService,
            IDateTimeHelper dateTimeHelper,
            IWorkContext workContext,
            ICustomerService customerService)
        {
            _appointmentService = appointmentService;
            _dateTimeHelper = dateTimeHelper;
            _workContext = workContext;
            _customerService = customerService;
            _patientService = patientService;
        }

        [HttpGet]
        [AllowAnonymous]
        public int ScheduledAppointments()
        {
            return _appointmentService.GetAll().Where(x => x.Status == AppointmentStatus.Scheduled).Count();
        }

        [HttpGet]
        [AllowAnonymous]
        public int ConfirmedAppointments()
        {
            return _appointmentService.GetAll().Where(x => x.Status == AppointmentStatus.Confirmed).Count();
        }

        [HttpGet]
        [AllowAnonymous]
        public int FirstTimeAppointments()
        {
            return _appointmentService.GetAll().Where(x => x.VisitType == VisitType.FirstTime && x.Status == AppointmentStatus.Scheduled).Count();
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult LoadNewPatientsStatistics(string period)
        {
            var result = new List<object>();

            var nowDt = DateTime.Now;
            var culture = new CultureInfo(_workContext.WorkingLanguage.LanguageCulture);

            switch (period)
            {
                case "year":
                    //year statistics
                    var yearAgoDt = nowDt.AddYears(-1).AddMonths(1);
                    var searchYearDateUser = new DateTime(yearAgoDt.Year, yearAgoDt.Month, 1);
                    for (var i = 0; i <= 12; i++)
                    {
                        result.Add(new
                        {
                            date = searchYearDateUser.Date.ToString("Y", culture),
                            value = _appointmentService.GetAll().AsEnumerable().Where(x => x.VisitType == VisitType.FirstTime && x.Status == AppointmentStatus.Scheduled && x.CreatedOnUtc.Month == DateTime.UtcNow.AddMonths(-i - 1).Month).Count()
                        });

                        searchYearDateUser = searchYearDateUser.AddMonths(1);
                    }
                    break;

                case "month":
                    //month statistics
                    var monthAgoDt = nowDt.AddDays(-30);
                    var searchMonthDateUser = new DateTime(monthAgoDt.Year, monthAgoDt.Month, monthAgoDt.Day);
                    for (var i = 0; i <= 30; i++)
                    {
                        result.Add(new
                        {
                            date = searchMonthDateUser.Date.ToString("M", culture),
                            value = _appointmentService.GetAll().AsEnumerable().Where(x => x.VisitType == VisitType.FirstTime && x.Status == AppointmentStatus.Scheduled && x.CreatedOnUtc.Date == searchMonthDateUser.Date).Count()
                        });

                        searchMonthDateUser = searchMonthDateUser.AddDays(1);
                    }
                    break;
                case "week":
                default:
                    //week statistics
                    var weekAgoDt = nowDt.AddDays(-7);
                    var searchWeekDateUser = new DateTime(weekAgoDt.Year, weekAgoDt.Month, weekAgoDt.Day);
                    for (var i = 0; i <= 7; i++)
                    {
                        result.Add(new
                        {
                            date = searchWeekDateUser.Date.ToString("d dddd", culture),
                            value = _appointmentService.GetAll().AsEnumerable().Where(x => x.VisitType == VisitType.FirstTime && x.Status == AppointmentStatus.Scheduled && x.CreatedOnUtc.Date == DateTime.UtcNow.AddDays(-i - 1).Date).Count()
                        });

                        searchWeekDateUser = searchWeekDateUser.AddDays(1);
                    }
                    break;
            }

            return Json(result);
        }
    }
}
