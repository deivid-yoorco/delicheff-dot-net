using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Models.Branch;
using Teed.Plugin.Medical.Services;
using Teed.Plugin.Medical.Domain;
using Teed.Plugin.Medical.Domain.Doctor;
using Teed.Plugin.Medical.Helpers;
using Teed.Plugin.Medical.Models.Doctors;
using Teed.Plugin.Medical.Security;

namespace Teed.Plugin.Medical.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class DoctorController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly BranchService _branchService;
        private readonly DoctorScheduleService _doctorScheduleService;
        private readonly DoctorLockedDatesServicecs _doctorLockedDatesServicecs;
        private readonly AppointmentService _appointmentService;
        private readonly DoctorAppointmentIntervalService _doctorAppointmentIntervalService;

        public DoctorController(
            IPermissionService permissionService,
            IWorkContext workContext,
            ICustomerService customerService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            BranchService branchService,
            DoctorLockedDatesServicecs doctorLockedDatesServicecs,
            AppointmentService appointmentService,
            DoctorScheduleService doctorScheduleService,
            DoctorAppointmentIntervalService doctorAppointmentIntervalService)
        {
            _permissionService = permissionService;
            _workContext = workContext;
            _customerService = customerService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _branchService = branchService;
            _doctorScheduleService = doctorScheduleService;
            _doctorLockedDatesServicecs = doctorLockedDatesServicecs;
            _appointmentService = appointmentService;
            _doctorAppointmentIntervalService = doctorAppointmentIntervalService;
        }

        public IActionResult List()
        {
            if (_workContext.CurrentCustomer.IsAdmin() || _workContext.CurrentCustomer.IsInCustomerRole("Receptionist"))
            {
                return View("~/Plugins/Teed.Plugin.Medical/Views/Doctor/List.cshtml");
            }
            else
            {
                return RedirectToAction("Details", new { id = _workContext.CurrentCustomer.Id });
            }
        }

        public IActionResult Details(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            var doctor = _customerService.GetCustomerById(id);
            if (doctor == null) return NotFound();

            var doctorAppointmentInterval = _doctorAppointmentIntervalService.GetByCustomerId(id).FirstOrDefault();

            DoctorsModel model = new DoctorsModel()
            {
                Id = doctor.Id,
                Email = doctor.Email,
                Address = doctor.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress),
                Address2 = doctor.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2),
                City = doctor.GetAttribute<string>(SystemCustomerAttributeNames.City),
                Country = _countryService.GetCountryById(int.Parse(doctor.GetAttribute<string>(SystemCustomerAttributeNames.CountryId)))?.Name,
                DateOfBirth = doctor.GetAttribute<string>(SystemCustomerAttributeNames.DateOfBirth),
                FirstName = doctor.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                LastName = doctor.GetAttribute<string>(SystemCustomerAttributeNames.LastName),
                Phone = doctor.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                PostalCode = doctor.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode),
                State = _stateProvinceService.GetStateProvinceById(int.Parse(doctor.GetAttribute<string>(SystemCustomerAttributeNames.StateProvinceId)))?.Name,
                ElementsCount = _doctorScheduleService.GetAll().Where(x => x.CustomerId == doctor.Id && !x.Deleted).Count(),
                LockDatesCount = _doctorLockedDatesServicecs.GetAllByDoctorId(doctor.Id).Count(),
                IntervalMinutes = doctorAppointmentInterval == null ? 0 : doctorAppointmentInterval.IntervalMinutes
            };

            ViewData["IsAdmin"] = _workContext.CurrentCustomer.IsAdmin();

            return View("~/Plugins/Teed.Plugin.Medical/Views/Doctor/Details.cshtml", model);
        }

        [HttpPost]
        public IActionResult BranchesListData()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            var products = _branchService.GetAll().Select(x => new ListViewModel
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();

            return Json(products);
        }

        [HttpPost]
        public IActionResult ListData()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            var doctorsAndNurses = SelectListHelper.GetDoctorsAndNursesList(_customerService);
            var gridModel = new DataSourceResult
            {
                Data = doctorsAndNurses,
                Total = doctorsAndNurses.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult GetScheduleList(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            var schedule = _doctorScheduleService.ListByDoctorId(id)
                .Select(x => new DoctorScheduleModel
                {
                    Id = x.Id,
                    WeekDay = EnumHelper.GetDisplayName(x.DayOfTheWeek),
                    StartTime = new DateTime(x.StartTime.Ticks).ToString("HH:mm"),
                    EndTime = new DateTime(x.EndTime.Ticks).ToString("HH:mm"),
                    BranchName = _branchService.GetById(x.BranchId).Name,
                    BranchId = x.BranchId
                })
                .OrderBy(x => x.BranchName)
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = schedule,
                Total = schedule.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(DoctorsModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            DoctorAppointmentInterval dai = _doctorAppointmentIntervalService.GetAll().Where(x => x.CustomerId == model.Id).FirstOrDefault();
            if (dai == null)
            {
                DoctorAppointmentInterval newDai = new DoctorAppointmentInterval()
                {
                    IntervalMinutes = model.IntervalMinutes,
                    CustomerId = model.Id
                };
                _doctorAppointmentIntervalService.Insert(newDai);
            }
            else
            {
                dai.IntervalMinutes = model.IntervalMinutes;
                _doctorAppointmentIntervalService.Update(dai);
            }

            return RedirectToAction("List");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddScheduleToDoctor(int branchId, WeekDays day, int startHour, int startMinute, int endHour, int endMinute, int doctorId)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            DoctorSchedule doctorSchedule = new DoctorSchedule()
            {
                BranchId = branchId,
                CustomerId = doctorId,
                DayOfTheWeek = day,
                StartTime = new TimeSpan(startHour, startMinute, 0),
                EndTime = new TimeSpan(endHour, endMinute, 0)
            };
            _doctorScheduleService.Insert(doctorSchedule);

            return Json(new { Result = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteScheduleElement(DoctorScheduleModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            DoctorSchedule doctorSchedule = _doctorScheduleService.GetById(model.Id);
            if (doctorSchedule == null) return NotFound();

            _doctorScheduleService.Delete(doctorSchedule);

            return new NullJsonResult();
        }

        [HttpGet]
        public IActionResult DoctorByBranchListData(int branchId)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            var doctorsInBranch = _doctorScheduleService.GetAll().Where(x => x.BranchId == branchId && !x.Deleted).Select(x => x.CustomerId).Distinct().AsEnumerable();
            var elements = doctorsInBranch.Select(x => new DoctorsListModel
            {
                Id = x,
                Doctor = $"{_customerService.GetCustomerById(x).GetFullName()} ({_customerService.GetCustomerById(x).Email})"
            }).ToList();

            return Json(elements);
        }

        [HttpGet]        
        public IActionResult DoctorByDateListData(string date)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();
            int[] ignoreIds;            
            //var doctors = SelectListHelper.GetDoctorsAndNursesListCustomer(_customerService);
            var docIds = _doctorScheduleService.GetIdsDoctor(date).ToArray();
            List<int> auxIds = _doctorLockedDatesServicecs.GetAllByDate(date).ToList();
            var doc = SelectListHelper.GetDoctorsAndNursesListCustomer(_customerService,docIds);
            
            foreach (var item in doc)
            {
                auxIds.Add(item.Id);
            }

            ignoreIds = auxIds.ToArray();

            var doctors = SelectListHelper.GetDoctorsAndNursesList(_customerService, ignoreIds);

            var elements = doctors.Select(x => new DoctorsListModel
            {
                Id = int.Parse(x.Value),
                Doctor = $"{x.Text}"
            }).ToList();

            return Json(elements);
        }

        [HttpGet]
        public IActionResult DoctorByTimeAndDateListData(string date, TimeSpan time,int branchId)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            int[] ignoreIds;
            //var doctors = SelectListHelper.GetDoctorsAndNursesListCustomer(_customerService);
            var docIds = _doctorScheduleService.GetIdsDoctorBYDateAndTime(date,time).ToArray();
            List<int> auxIds = _doctorLockedDatesServicecs.GetAllByDate(date).ToList();
            var doc = SelectListHelper.GetDoctorsAndNursesListCustomer(_customerService, docIds);
            DateTime selectedDateParsed = DateTime.ParseExact(date, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var appointmentDate = new DateTime(selectedDateParsed.Year,
                                              selectedDateParsed.Month,
                                              selectedDateParsed.Day,
                                              time.Hours,
                                              time.Minutes, 0).ToUniversalTime();

            for (int i = 0; i < docIds.Length; i++)
            {
                if (_appointmentService.CheckAppoinment(appointmentDate, docIds[i], branchId))
                {
                    auxIds.Add(docIds[i]);
                }
            }

            foreach (var item in doc)
            {
                auxIds.Add(item.Id);
            }

            ignoreIds = auxIds.ToArray();

            var doctors = SelectListHelper.GetDoctorsAndNursesList(_customerService, ignoreIds);

            var elements = doctors.Select(x => new DoctorsListModel
            {
                Id = int.Parse(x.Value),
                Doctor = $"{x.Text}"
            }).ToList();

            return Json(elements);
        }

        [HttpGet]
        public IActionResult DoctorByTimeListData(TimeSpan time)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            int[] ignoreIds;
            List<int> auxIds = new List<int>();
            //var doctors = SelectListHelper.GetDoctorsAndNursesListCustomer(_customerService);
            var docIds = _doctorScheduleService.GetIdsDoctorBYTime(time).ToArray();
            //List<int> auxIds = _doctorLockedDatesServicecs.GetAllByDate(date).ToList();
            var doc = SelectListHelper.GetDoctorsAndNursesListCustomer(_customerService, docIds);

            foreach (var item in doc)
            {
                auxIds.Add(item.Id);
            }

            ignoreIds = auxIds.ToArray();

            var doctors = SelectListHelper.GetDoctorsAndNursesList(_customerService, ignoreIds);

            var elements = doctors.Select(x => new DoctorsListModel
            {
                Id = int.Parse(x.Value),
                Doctor = $"{x.Text}"
            }).ToList();

            return Json(elements);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ListLockDate(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();



            var list = _doctorLockedDatesServicecs.GetAllByDoctorId(id).ToList();
            var elements = list.Select(x => new DoctorLockDateModel
            {
                Id = x.Id,
                InitDate = x.InitDate.ToString("dd/MM/yyyy"),
                EndDate = x.EndDate.ToString("dd/MM/yyyy")
            }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = elements,
                Total = elements.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveLockDate(DoctorLockDateModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            DoctorLockedDates doctorLockedDates = new DoctorLockedDates
            {
                DoctorId = model.Id,
                InitDate = DateTime.ParseExact(model.InitDate, "MM/dd/yyyy", CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact(model.EndDate, "MM/dd/yyyy", CultureInfo.InvariantCulture)

            };

            _doctorLockedDatesServicecs.Insert(doctorLockedDates);

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteLockDate(DoctorLockDateModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            DoctorLockedDates doctorLockedDates = _doctorLockedDatesServicecs.GetById(model.Id);
            if (doctorLockedDates == null) return NotFound();

            _doctorLockedDatesServicecs.Delete(doctorLockedDates);

            return new NullJsonResult();
        }
    }
}