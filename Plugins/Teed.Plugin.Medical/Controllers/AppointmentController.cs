using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using Teed.Plugin.Medical.Helpers;
using System.Collections.Generic;
using System.Linq;
using Teed.Plugin.Medical.Models.Patients;
using Teed.Plugin.Medical.Services;
using Nop.Services.Catalog;
using Teed.Plugin.Medical.Models.Appointment;
using Teed.Plugin.Medical.Domain.Appointment;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Nop.Services.Security;
using Teed.Plugin.Medical.Security;
using Teed.Plugin.Medical.Models.Branches;
using System;
using Teed.Plugin.Medical.Domain;
using Teed.Plugin.Medical.Models.Doctors;
using Teed.Plugin.Medical.Domain.Doctor;
using Teed.Plugin.Medical.Domain.Branches;
using Nop.Services.Common;
using Microsoft.AspNetCore.Authorization;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.IO;
using iText.Kernel.Geom;
using System.Threading;
using System.Globalization;
using iText.Layout.Borders;
using iText.Kernel.Colors;
using System.Data.Entity.Core.Objects;
using System.Data.Entity;

namespace Teed.Plugin.Medical.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class AppointmentController : BasePluginController
    {
        private readonly AppointmentService _appointmentService;
        private readonly AppointmentItemService _appointmentItemService;
        private readonly PatientService _patientService;
        private readonly BranchService _branchService;
        private readonly HolidayService _holidayService;
        private readonly HolidayBranchService _holidayBranchService;
        private readonly OfficeService _officeService;
        private readonly DoctorScheduleService _doctorScheduleService;
        private readonly DoctorLockedDatesServicecs _doctorLockedDatesServicecs;
        private readonly VisitExtraUsersService _appointmentExtraUsersService;
        private readonly BranchWorkerService _branchWorkerService;
        private readonly DoctorAppointmentIntervalService _doctorAppointmentIntervalService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;

        public static List<ReservedTimeModel> reservedTimes = new List<ReservedTimeModel>();

        public AppointmentController(
            IWorkContext workContext,
            ICustomerService customerService,
            IProductService productService,
            IPermissionService permissionService,
            AppointmentService appointmentService,
            PatientService patientService,
            AppointmentItemService appointmentItemService,
            BranchService branchService,
            HolidayService holidayService,
            HolidayBranchService holidayBranchService,
            OfficeService officeService,
            DoctorScheduleService doctorScheduleService,
            BranchWorkerService branchWorkerService,
            DoctorLockedDatesServicecs doctorLockedDatesServicecs,
            VisitExtraUsersService appointmentExtraUsersService,
            DoctorAppointmentIntervalService doctorAppointmentIntervalService)
        {
            _workContext = workContext;
            _appointmentService = appointmentService;
            _patientService = patientService;
            _customerService = customerService;
            _productService = productService;
            _appointmentItemService = appointmentItemService;
            _permissionService = permissionService;
            _branchService = branchService;
            _holidayService = holidayService;
            _holidayBranchService = holidayBranchService;
            _officeService = officeService;
            _doctorScheduleService = doctorScheduleService;
            _doctorLockedDatesServicecs = doctorLockedDatesServicecs;
            _appointmentExtraUsersService = appointmentExtraUsersService;
            _branchWorkerService = branchWorkerService;
            _doctorAppointmentIntervalService = doctorAppointmentIntervalService;
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            ViewData["BranchesId"] = _branchWorkerService.GetByCustomerId(_workContext.CurrentCustomer.Id).ToList();
            ViewData["DoctorId"] = _workContext.CurrentCustomer.IsAdmin() || _workContext.CurrentCustomer.IsInCustomerRole("Receptionist") ? 0 : _workContext.CurrentCustomer.Id;
            ViewData["AllowedToFilterByDoctor"] = _workContext.CurrentCustomer.IsInCustomerRole("Doctor") ||
                                                  _workContext.CurrentCustomer.IsInCustomerRole("Receptionist") ||
                                                  _workContext.CurrentCustomer.IsInCustomerRole("Manager") ||
                                                  _workContext.CurrentCustomer.IsAdmin();

            if (_workContext.CurrentCustomer.IsInCustomerRole("CosmetologistJr") ||
            _workContext.CurrentCustomer.IsInCustomerRole("CosmetologistSr") ||
            _workContext.CurrentCustomer.IsInCustomerRole("NurseJr") ||
            _workContext.CurrentCustomer.IsInCustomerRole("NurseSr") ||
            _workContext.CurrentCustomer.IsInCustomerRole("Doctor") ||
            _workContext.CurrentCustomer.IsInCustomerRole("Assistant"))
            {
                return View("~/Plugins/Teed.Plugin.Medical/Views/Appointment/ListDoctorRole.cshtml");
            }
            else
            {
                return View("~/Plugins/Teed.Plugin.Medical/Views/Appointment/List.cshtml");
            }
        }

        public IActionResult Create(int id = 0)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            ViewData["Patients"] = _patientService.GetAll();
            ViewData["Treatments"] = SelectListHelper.GetProductsList(_productService);
            ViewData["DoctorsAndNurses"] = SelectListHelper.GetDoctorsAndNursesList(_customerService);
            var customerId = _workContext.CurrentCustomer.Id;
            var branchId = _branchWorkerService.GetByCustomerId(customerId).FirstOrDefault();

            AppointmentCreateModel model = new AppointmentCreateModel();

            if (_workContext.CurrentCustomer.IsInCustomerRole("Doctor"))
            {
                model.DoctorUserId = _workContext.CurrentCustomer.Id;
            }
            model.PatientId = id;
            model.BranchId = branchId;

            return View("~/Plugins/Teed.Plugin.Medical/Views/Appointment/Create.cshtml", model);

        }

        public IActionResult ManualCreate(int id = 0)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            ViewData["Patients"] = _patientService.GetAll();
            ViewData["Treatments"] = SelectListHelper.GetProductsList(_productService);
            ViewData["DoctorsAndNurses"] = SelectListHelper.GetDoctorsAndNursesList(_customerService);
            var customerId = _workContext.CurrentCustomer.Id;
            var branchId = _branchWorkerService.GetByCustomerId(customerId).FirstOrDefault();
            AppointmentCreateModel model = new AppointmentCreateModel();
            model.PatientId = id;
            model.BranchId = branchId;

            return View("~/Plugins/Teed.Plugin.Medical/Views/Appointment/ManualCreate.cshtml", model);

        }

        public IActionResult Edit(int id, bool rs = false)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Appointment appointment = _appointmentService.GetById(id);
            if (appointment == null) return NotFound();

            AppointmentEditModel model = new AppointmentEditModel()
            {
                Id = appointment.Id,
                Comments = appointment.Comments,
                PatientId = appointment.PatientId,
                VisitType = appointment.VisitType,
                Status = appointment.Status,
                SelectedProductsIds = _appointmentItemService.GetProductsIdsByAppointmentId(id).ToList(),
                BranchId = appointment.BranchId,
                DoctorUserId = appointment.DoctorId,
                SelectedDate = appointment.AppointmentDate.ToShortDateString(),
                AppointmentHour = appointment.AppointmentDate.ToLocalTime().Hour,
                AppointmentMinute = appointment.AppointmentDate.ToLocalTime().Minute,
                SelectedUsersIds = _appointmentExtraUsersService.GetAll().Where(x => x.VisitId == appointment.Id).Select(x => x.CustomerId).ToList(),
                IsManualAppointment = appointment.IsManualAppointment,
                Note = appointment.Note,
                AppointmentDate = appointment.AppointmentDate
            };

            var appointmentTimes = new List<TimeSpan>();
            appointmentTimes.Add(new TimeSpan(appointment.AppointmentDate.ToLocalTime().Ticks));
            foreach (var item in _appointmentService.GetAll().Where(x => x.ParentAppointmentId == appointment.Id).ToList().Select(x => x.AppointmentDate.ToLocalTime()))
            {
                appointmentTimes.Add(new TimeSpan(item.Ticks));
            }
            model.AppointmentTime = appointmentTimes;

            if (rs)
            {
                appointment.Note = appointment.Note + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Cita cancelada por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + ". Razón: Se reagendará la cita.";
                _appointmentService.Update(appointment);
                model.Status = AppointmentStatus.Rescheduled;
            }

            ViewData["Patients"] = _patientService.GetAll();
            ViewData["Treatments"] = SelectListHelper.GetProductsList(_productService);
            ViewData["DoctorsAndNurses"] = SelectListHelper.GetDoctorsAndNursesList(_customerService);

            return View("~/Plugins/Teed.Plugin.Medical/Views/Appointment/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command, int branchId = 0, int doctorId = 0, string filterDate = null)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            var count = _appointmentService.GetAll();
            var appointmentList = _appointmentService.ListAsNoTracking(command.Page - 1, command.PageSize, branchId, doctorId, filterDate)
                .AsEnumerable();

            if (appointmentList == null) return NotFound();

            if (branchId != 0)
            {
                count = count.Where(x => x.BranchId == branchId);
            }

            if (doctorId != 0)
            {
                count = count.Where(x => x.DoctorId == doctorId);
            }

            var appointments = appointmentList.ToList().OrderBy(x => x.AppointmentDate).Select(x => new AppointmentListModel()
            {
                Id = x.Id,
                Patient = _patientService.GetById(x.PatientId),
                Status = EnumHelper.GetDisplayName(x.Status),
                Branch = _branchService.GetById(x.BranchId).Name,
                AppointmentDate = x.AppointmentDate.ToLocalTime().ToString("dd MMMM yyyy - hh:mm tt"),
                Comments = x.Comments,
                VisitType = EnumHelper.GetDisplayName(x.VisitType),
                Doctor = _customerService.GetCustomerById(x.DoctorId)?.GetFullName(),
            });

            var gridModel = new DataSourceResult
            {
                Data = appointments.ToList(),
                Total = count.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ListDataToday(int branchId = 0, int doctorId = 0)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            var appointmentList = _appointmentService.GetAll();
            if (appointmentList == null) return NotFound();

            var appointments = appointmentList.ToList()
                .OrderBy(x => x.AppointmentDate)
                .Where(x => x.AppointmentDate.ToLocalTime().ToShortDateString() == DateTime.Now.ToShortDateString() && x.ParentAppointmentId == 0)
                .Select(x => new AppointmentListModel()
                {
                    Id = x.Id,
                    Patient = x.Patient,
                    Status = EnumHelper.GetDisplayName(x.Status),
                    Branch = _branchService.GetById(x.BranchId).Name,
                    AppointmentDate = x.AppointmentDate.ToLocalTime().ToString("hh:mm tt"),
                    BranchId = x.BranchId,
                    DoctorId = x.DoctorId,
                    VisitType = EnumHelper.GetDisplayName(x.VisitType),
                    Comments = x.Comments,
                    Doctor = _customerService.GetCustomerById(x.DoctorId).GetAttribute<string>(SystemCustomerAttributeNames.FirstName) +
                    " " + _customerService.GetCustomerById(x.DoctorId).GetAttribute<string>(SystemCustomerAttributeNames.LastName),
                });

            if (branchId != 0)
            {
                appointments = appointments.Where(x => x.BranchId == branchId);
            }

            if (doctorId != 0)
            {
                appointments = appointments.Where(x => x.DoctorId == doctorId);
            }

            var gridModel = new DataSourceResult
            {
                Data = appointments.ToList(),
                Total = appointments.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult PatientListData()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            var patients = _patientService.GetAll();
            var elements = patients.Select(x => new PatientsListModel
            {
                Id = x.Id,
                Patient = $"{x.FirstName} {x.LastName}"
            }).ToList();

            return Json(elements);
        }

        [HttpPost]
        public IActionResult BranchListData(int doctorId = 0, bool showAllAvailable = false)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            List<BranchesListModel> elements = new List<BranchesListModel>();
            var branches = _branchService.GetAll().AsEnumerable();
            if (doctorId != 0)
            {
                var doctorBranches = _doctorScheduleService.ListByDoctorId(doctorId).Select(x => x.BranchId);
                branches = _branchService.GetAll().Where(x => doctorBranches.Contains(x.Id));
            }

            elements = branches.Select(x => new BranchesListModel
            {
                Id = x.Id,
                Branch = $"{x.Name}"
            }).ToList();

            if (showAllAvailable)
            {
                elements.Add(new BranchesListModel
                {
                    Id = 0,
                    Branch = "Todas las sucursales"
                });
            }

            return Json(elements);
        }

        [HttpPost]
        public IActionResult DoctorListData()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();


            List<DoctorsListModel> elements = new List<DoctorsListModel>();
            var doctors = SelectListHelper.GetDoctorsAndNursesListCustomer(_customerService);
            elements = doctors.Select(x => new DoctorsListModel
            {
                Id = x.Id,
                Doctor = $"{x.GetAttribute<string>(SystemCustomerAttributeNames.FirstName)} {x.GetAttribute<string>(SystemCustomerAttributeNames.LastName)} ({x.Email})"
            }).ToList();

            elements.Add(new DoctorsListModel
            {
                Id = 0,
                Doctor = "Todas los encargados"
            });

            return Json(elements);
        }

        [HttpPost]
        public IActionResult GetDates(int branchId, int doctorId)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Branch branch = _branchService.GetById(branchId);
            IQueryable<int> holidayIds = _holidayBranchService.GetHolidayIdsByBranchId(branchId);
            IQueryable<Holiday> holidaysInBranch = _holidayService.GetByIds(holidayIds.ToArray());
            List<DoctorLockedDates> lockDates = _doctorLockedDatesServicecs.GetAllByDoctorId(doctorId).Where(x => x.EndDate >= DateTime.Now).ToList();


            List<object> data = new List<object>();
            List<string> holidays = new List<string>();
            List<string> doctorLockDates = new List<string>();
            List<int> weekDays = new List<int>();

            foreach (var holiday in holidaysInBranch)
            {
                holidays.Add(holiday.Date.ToString("yyyy-MM-dd"));
            }

            foreach (var rangeDates in lockDates)
            {
                DateTime aux = rangeDates.InitDate;
                do
                {
                    doctorLockDates.Add(aux.Date.ToString("yyyy-MM-dd"));
                    aux = aux.AddDays(1);
                } while (aux.Date <= rangeDates.EndDate.Date);
            }

            List<int> doctorSchedule = new List<int>();
            if (branch != null)
            {
                if (!branch.WorksOnSaturday) weekDays.Add((int)WeekDays.Saturday);
                if (!branch.WorksOnSunday) weekDays.Add((int)WeekDays.Sunday);
                doctorSchedule = _doctorScheduleService.GetAll()
                    .Where(x => x.BranchId == branchId && x.CustomerId == doctorId)
                    .Select(x => (int)x.DayOfTheWeek)
                    .ToList();
            }

            List<int> weekdaysEnum = Enum.GetValues(typeof(WeekDays)).Cast<int>().ToList();
            foreach (var day in weekdaysEnum)
            {
                if (!doctorSchedule.Contains(day) && !weekDays.Contains(day)) weekDays.Add(day);
            }

            data.Add(holidays);
            data.Add(weekDays);
            data.Add(doctorLockDates);

            return Json(data);
        }

        [HttpPost]
        public IActionResult GetTimes(int doctorId, string selectedDate, int branchId)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            DateTime selectedDayParsed = DateTime.ParseExact(selectedDate, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);

            var doctorSchedule = _doctorScheduleService.GetAll()
                .Where(x => x.CustomerId == doctorId && x.BranchId == branchId && (int)x.DayOfTheWeek == (int)selectedDayParsed.DayOfWeek)
                .FirstOrDefault();
            if (doctorSchedule == null) return NotFound();

            var takenTimes = _appointmentService.GetAll().AsEnumerable().Where(x =>
                x.DoctorId == doctorId &&
                x.BranchId == branchId &&
                x.AppointmentDate.ToLocalTime().Day == selectedDayParsed.Day &&
                x.AppointmentDate.ToLocalTime().Month == selectedDayParsed.Month &&
                x.AppointmentDate.ToLocalTime().Year == selectedDayParsed.Year &&
                x.Status != AppointmentStatus.Cancelled &&
                !x.Deleted)
                .Select(x => x.AppointmentDate.ToLocalTime().TimeOfDay)
                .ToList();

            var reservedToCurrent = reservedTimes.Where(x => x.BranchId == branchId &&
                                                             x.DoctorId == doctorId &&
                                                             x.SelectedDate.ToLocalTime().Day == selectedDayParsed.Day &&
                                                             x.SelectedDate.ToLocalTime().Month == selectedDayParsed.Month &&
                                                             x.SelectedDate.ToLocalTime().Year == selectedDayParsed.Year);
            foreach (var time in reservedToCurrent)
            {
                takenTimes.Add(new TimeSpan(time.SelectedDate.ToLocalTime().Hour, time.SelectedDate.ToLocalTime().Minute, 0));
            }

            var doctorAppointmentInterval = _doctorAppointmentIntervalService.GetByCustomerId(doctorId).FirstOrDefault();
            var doctorIntervalMinutes = doctorAppointmentInterval == null ? 30 : doctorAppointmentInterval.IntervalMinutes;

            List<TimeListModel> availableTimes = new List<TimeListModel>();
            for (TimeSpan i = doctorSchedule.StartTime; i < doctorSchedule.EndTime; i = i.Add(TimeSpan.FromMinutes(doctorIntervalMinutes)))
            {
                availableTimes.Add(new TimeListModel()
                {
                    TimeValue = i,
                    TimeText = new DateTime(i.Ticks).ToString("HH:mm"),
                    IsActive = !takenTimes.Contains(i)
                });
            }
            return Json(availableTimes);
        }

        [HttpPost]
        public IActionResult ReserveTime(TimeSpan time, int doctorId, int branchId, string selectedDate)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            DateTime selectedDayParsed = DateTime.ParseExact(selectedDate, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var checkDate = new DateTime(selectedDayParsed.Year, selectedDayParsed.Month, selectedDayParsed.Day, time.Hours, time.Minutes, time.Seconds).ToUniversalTime();

            Appointment appointment = _appointmentService.GetAll().Where(x => x.AppointmentDate == checkDate && x.DoctorId == doctorId && x.BranchId == branchId).FirstOrDefault();
            if (appointment != null) return BadRequest("Ya hay una cita para la hora seleccionada.");

            if (reservedTimes.Any(x => x.SelectedDate == checkDate)) return BadRequest("Ya hay una cita para la hora seleccionada.");

            reservedTimes.Add(new ReservedTimeModel()
            {
                SelectedDate = checkDate,
                ReservationDate = DateTime.UtcNow,
                BranchId = branchId,
                DoctorId = doctorId
            });
            return Ok();
        }

        public IActionResult Details(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Appointment appointment = _appointmentService.GetById(id);
            if (appointment == null) return NotFound();

            var extraCustomerIds = _appointmentExtraUsersService.GetAll().Where(x => x.VisitId == appointment.Id).Select(x => x.CustomerId).ToArray();
            AppointmentDetailsModel model = new AppointmentDetailsModel()
            {
                Id = appointment.Id,
                AppointmentDate = appointment.AppointmentDate.ToLocalTime().ToString("dd MMMM yyyy - hh:mm tt"),
                Comments = appointment.Comments,
                Patient = appointment.Patient,
                Status = appointment.Status,
                VisitType = appointment.VisitType,
                Products = _appointmentItemService.GetProductsByAppointmentId(appointment.Id),
                Doctor = _customerService.GetCustomerById(appointment.DoctorId).GetFullName(),
                Branch = _branchService.GetById(appointment.BranchId),
                Customers = _customerService.GetCustomersByIds(extraCustomerIds).Select(x => (x.GetFullName() + " (" + x.Email + ")")).ToList()
            };

            return View("~/Plugins/Teed.Plugin.Medical/Views/Appointment/Details.cshtml", model);
        }

        [HttpPost]
        public IActionResult Create(AppointmentCreateModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            if (!ModelState.IsValid) return BadRequest();

            if (model.PatientId == 0)
            {
                ViewData["Patients"] = _patientService.GetAll();
                ViewData["Treatments"] = SelectListHelper.GetProductsList(_productService);
                ViewData["DoctorsAndNurses"] = SelectListHelper.GetDoctorsAndNursesList(_customerService);

                ModelState.AddModelError("", "Debes seleccionar el paciente.");
                return View("~/Plugins/Teed.Plugin.Medical/Views/Appointment/Create.cshtml", model);
            };

            if (model.BranchId == 0)
            {
                ViewData["Patients"] = _patientService.GetAll();
                ViewData["Treatments"] = SelectListHelper.GetProductsList(_productService);
                ViewData["DoctorsAndNurses"] = SelectListHelper.GetDoctorsAndNursesList(_customerService);

                ModelState.AddModelError("", "Debes seleccionar la sucursal.");
                return View("~/Plugins/Teed.Plugin.Medical/Views/Appointment/Create.cshtml", model);
            };

            if (model.DoctorUserId == 0)
            {
                ViewData["Patients"] = _patientService.GetAll();
                ViewData["Treatments"] = SelectListHelper.GetProductsList(_productService);
                ViewData["DoctorsAndNurses"] = SelectListHelper.GetDoctorsAndNursesList(_customerService);

                ModelState.AddModelError("", "Debes seleccionar el encargado.");
                return View("~/Plugins/Teed.Plugin.Medical/Views/Appointment/Create.cshtml", model);
            };

            DateTime selectedDateParsed = DateTime.ParseExact(model.SelectedDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var appointmentsToUpload = new List<Appointment>();
            for (int i = 0; i < model.AppointmentTime.Count; i++)
            {
                var appointmentDate = new DateTime(selectedDateParsed.Year,
                                               selectedDateParsed.Month,
                                               selectedDateParsed.Day,
                                               model.AppointmentTime[i].Hours,
                                               model.AppointmentTime[i].Minutes, 0).ToUniversalTime();
                bool appointmentExist = _appointmentService.GetAll().Where(x => x.BranchId == model.BranchId && x.DoctorId == model.DoctorUserId && x.AppointmentDate == appointmentDate && x.Status != AppointmentStatus.Cancelled).Any();
                if (appointmentExist)
                {
                    ViewData["Patients"] = _patientService.GetAll();
                    ViewData["Treatments"] = SelectListHelper.GetProductsList(_productService);
                    ViewData["DoctorsAndNurses"] = SelectListHelper.GetDoctorsAndNursesList(_customerService);

                    ModelState.AddModelError("", "Ya se ha reservado una cita para la fecha, encargado y sucursal seleccionados.");
                    return View("~/Plugins/Teed.Plugin.Medical/Views/Appointment/Create.cshtml", model);
                }

                Appointment appointment = new Appointment()
                {
                    AppointmentDate = appointmentDate,
                    Comments = model.Comments,
                    VisitType = model.VisitType,
                    Status = AppointmentStatus.Scheduled,
                    PatientId = model.PatientId,
                    BranchId = model.BranchId,
                    DoctorId = model.DoctorUserId,
                    CheckInDate = null,
                    CheckOutDate = null,
                    IsManualAppointment = false,
                    Note = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Cita agendada por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + "."
                };

                appointmentsToUpload.Add(appointment);
            }

            int parentAppointmentId = 0;
            for (int i = 0; i < appointmentsToUpload.Count; i++)
            {
                if (i > 0) appointmentsToUpload[i].ParentAppointmentId = parentAppointmentId;
                _appointmentService.Insert(appointmentsToUpload[i]);
                if (i == 0 && appointmentsToUpload.Count > 1) parentAppointmentId = appointmentsToUpload[i].Id;
            }

            //foreach (var item in model.SelectedProductsIds)
            //{
            //    _appointmentItemService.Insert(new AppointmentItem()
            //    {
            //        AppointmentId = appointment.Id,
            //        ProductId = item
            //    });
            //}

            /*foreach (var item in model.SelectedUsersIds)
            {
                if (item != model.DoctorUserId)
                {
                    _appointmentExtraUsersService.Insert(new AppointmentExtraUsers()
                    {
                        AppointmentId = appointment.Id,
                        CustomerId = item
                    });
                }
            }*/


            return RedirectToAction("List");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateManual(AppointmentCreateModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            if (!ModelState.IsValid) return BadRequest();

            if (model.PatientId == 0)
            {
                ViewData["Patients"] = _patientService.GetAll();
                ViewData["Treatments"] = SelectListHelper.GetProductsList(_productService);
                ViewData["DoctorsAndNurses"] = SelectListHelper.GetDoctorsAndNursesList(_customerService);

                ModelState.AddModelError("", "Debes seleccionar el paciente.");
                return View("~/Plugins/Teed.Plugin.Medical/Views/Appointment/Create.cshtml", model);
            };

            if (model.BranchId == 0)
            {
                ViewData["Patients"] = _patientService.GetAll();
                ViewData["Treatments"] = SelectListHelper.GetProductsList(_productService);
                ViewData["DoctorsAndNurses"] = SelectListHelper.GetDoctorsAndNursesList(_customerService);

                ModelState.AddModelError("", "Debes seleccionar la sucursal.");
                return View("~/Plugins/Teed.Plugin.Medical/Views/Appointment/Create.cshtml", model);
            };

            if (model.DoctorUserId == 0)
            {
                ViewData["Patients"] = _patientService.GetAll();
                ViewData["Treatments"] = SelectListHelper.GetProductsList(_productService);
                ViewData["DoctorsAndNurses"] = SelectListHelper.GetDoctorsAndNursesList(_customerService);

                ModelState.AddModelError("", "Debes seleccionar el encargado.");
                return View("~/Plugins/Teed.Plugin.Medical/Views/Appointment/Create.cshtml", model);
            };

            DateTime selectedDateParsed = DateTime.ParseExact(model.SelectedDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var appointmentsToUpload = new List<Appointment>();
            for (int i = 0; i < model.AppointmentTime.Count; i++)
            {
                var appointmentDate = new DateTime(selectedDateParsed.Year,
                                               selectedDateParsed.Month,
                                               selectedDateParsed.Day,
                                               model.AppointmentTime[i].Hours,
                                               model.AppointmentTime[i].Minutes, 0).ToUniversalTime();

                Appointment appointment = new Appointment()
                {
                    AppointmentDate = appointmentDate,
                    Comments = model.Comments,
                    VisitType = model.VisitType,
                    Status = AppointmentStatus.Scheduled,
                    PatientId = model.PatientId,
                    BranchId = model.BranchId,
                    DoctorId = model.DoctorUserId,
                    CheckInDate = null,
                    CheckOutDate = null,
                    IsManualAppointment = true,
                    Note = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Cita manual agendada por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + "."
                };

                appointmentsToUpload.Add(appointment);
            }

            int parentAppointmentId = 0;
            for (int i = 0; i < appointmentsToUpload.Count; i++)
            {
                if (i > 0) appointmentsToUpload[i].ParentAppointmentId = parentAppointmentId;
                _appointmentService.Insert(appointmentsToUpload[i]);
                if (i == 0 && appointmentsToUpload.Count > 1) parentAppointmentId = appointmentsToUpload[i].Id;
            }

            //foreach (var item in model.SelectedProductsIds)
            //{
            //    _appointmentItemService.Insert(new AppointmentItem()
            //    {
            //        AppointmentId = appointment.Id,
            //        ProductId = item
            //    });
            //}

            /*foreach (var item in model.SelectedUsersIds)
            {
                if (item != model.DoctorUserId)
                {
                    _appointmentExtraUsersService.Insert(new AppointmentExtraUsers()
                    {
                        AppointmentId = appointment.Id,
                        CustomerId = item
                    });
                }
            }*/


            return RedirectToAction("List");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AppointmentEditModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            if (!ModelState.IsValid) return BadRequest();

            Appointment appointment = _appointmentService.GetById(model.Id);
            if (appointment == null) return NotFound();

            
            var childAppointments = _appointmentService.GetAll().Where(x => x.ParentAppointmentId == appointment.Id).ToList();
            if (!appointment.IsManualAppointment)
            {
                foreach (var child in childAppointments)
                {
                    if (_appointmentService.GetAll().Where(x => x.AppointmentDate == child.AppointmentDate && x.ParentAppointmentId != child.ParentAppointmentId && x.Status != AppointmentStatus.Cancelled).Any())
                    {
                        ViewData["Patients"] = _patientService.GetAll();
                        ViewData["Treatments"] = SelectListHelper.GetProductsList(_productService);
                        ViewData["DoctorsAndNurses"] = SelectListHelper.GetDoctorsAndNursesList(_customerService);

                        ModelState.AddModelError("", "Ya se ha reservado una cita para la fecha, encargado y sucursal seleccionados.");
                        return View("~/Plugins/Teed.Plugin.Medical/Views/Appointment/Edit.cshtml", model);
                    }
                }
            }

            DateTime selectedDateParsed = DateTime.ParseExact(model.SelectedDate, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
            DateTime selectedDateUTC = new DateTime(selectedDateParsed.Year,
                                                       selectedDateParsed.Month,
                                                       selectedDateParsed.Day,
                                                       model.AppointmentTime[0].Hours,
                                                       model.AppointmentTime[0].Minutes, 0).ToUniversalTime();
            if (selectedDateUTC != appointment.AppointmentDate && !appointment.IsManualAppointment)
            {
                bool appointmentExist = _appointmentService.GetAll().Where(x => x.BranchId == model.BranchId &&
                                                                           x.DoctorId == model.DoctorUserId &&
                                                                           x.AppointmentDate == selectedDateUTC &&
                                                                           x.Status != AppointmentStatus.Cancelled &&
                                                                           x.ParentAppointmentId != appointment.Id).Any();
                if (appointmentExist)
                {
                    ViewData["Patients"] = _patientService.GetAll();
                    ViewData["Treatments"] = SelectListHelper.GetProductsList(_productService);
                    ViewData["DoctorsAndNurses"] = SelectListHelper.GetDoctorsAndNursesList(_customerService);

                    ModelState.AddModelError("", "Ya se ha reservado una cita para la fecha, encargado y sucursal seleccionados.");
                    return View("~/Plugins/Teed.Plugin.Medical/Views/Appointment/Edit.cshtml", model);
                }
            }

            foreach (var child in childAppointments)
            {
                child.Deleted = true;
                _appointmentService.Delete(child);
            }

            for (int i = 0; i < model.AppointmentTime.Count; i++)
            {
                if (i == 0)
                {
                    if (model.Status != appointment.Status)
                    {
                        switch (model.Status)
                        {
                            case AppointmentStatus.Confirmed:
                                appointment.Note = appointment.Note + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Cita confirmada por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + ".";
                                break;
                            case AppointmentStatus.Scheduled:
                                appointment.Note = appointment.Note + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Cita marcada como agendada por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + ".";
                                break;
                            case AppointmentStatus.Cancelled:
                                appointment.Note = appointment.Note + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Cita cancelada por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + ". Razón: " + model.StatusNote;
                                break;
                            case AppointmentStatus.Complete:
                                appointment.Note = appointment.Note + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Cita marcada como completada por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + ".";
                                break;
                            case AppointmentStatus.Rescheduled:
                                appointment.Note = appointment.Note + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Cita reagendada por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + ".";
                                break;
                            case AppointmentStatus.NotConfirmed:
                                appointment.Note = appointment.Note + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Cita marcada como no confirmada por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + ". Razón: " + model.StatusNote;
                                break;
                            case AppointmentStatus.Registered:
                                appointment.Note = appointment.Note + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Cita marcada como registrada por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + ".";
                                break;
                            default:
                                break;
                        }
                    }

                    if ((model.Status == appointment.Status) && (selectedDateUTC != appointment.AppointmentDate))
                    {
                        appointment.Note = appointment.Note + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Cita reagendada por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + ".";
                    }

                    appointment.AppointmentDate = selectedDateUTC;
                    appointment.PatientId = model.PatientId;
                    appointment.Status = model.Status;
                    appointment.VisitType = model.VisitType;
                    appointment.Comments = model.Comments;
                    appointment.DoctorId = model.DoctorUserId;
                    appointment.BranchId = model.BranchId;

                    _appointmentService.Update(appointment);
                }
                else
                {
                    selectedDateUTC = new DateTime(selectedDateParsed.Year,
                                                   selectedDateParsed.Month,
                                                   selectedDateParsed.Day,
                                                   model.AppointmentTime[i].Hours,
                                                   model.AppointmentTime[i].Minutes, 0).ToUniversalTime();

                    Appointment childAppointment = new Appointment()
                    {
                        AppointmentDate = selectedDateUTC,
                        Comments = appointment.Comments,
                        VisitType = appointment.VisitType,
                        Status = appointment.Status,
                        PatientId = appointment.PatientId,
                        BranchId = appointment.BranchId,
                        DoctorId = appointment.DoctorId,
                        CheckInDate = appointment.CheckInDate,
                        CheckOutDate = appointment.CheckOutDate,
                        IsManualAppointment = appointment.IsManualAppointment,
                        Note = appointment.Note,
                        ParentAppointmentId = appointment.Id
                    };

                    _appointmentService.Insert(childAppointment);
                }
            }

            //var existingExtraUsers = _appointmentExtraUsersService.GetByAppointmentId(model.Id).ToList();
            //foreach (var extraUser in existingExtraUsers)
            //{
            //    if (!model.SelectedUsersIds.Contains(extraUser.Id))
            //    {
            //        _appointmentExtraUsersService.Delete(extraUser);
            //    }
            //}
            /*foreach (var extraUserId in model.SelectedUsersIds)
            {
                if (extraUserId != model.DoctorUserId &&
                    _appointmentExtraUsersService.GetAll().Where(x => x.AppointmentId == appointment.Id && x.CustomerId == extraUserId).FirstOrDefault() == null)
                {
                    _appointmentExtraUsersService.Insert(new AppointmentExtraUsers()
                    {
                        AppointmentId = appointment.Id,
                        CustomerId = extraUserId
                    });
                }
            }*/

            //var existingProducts = _appointmentItemService.GetByAppointmentId(model.Id).ToList();
            //foreach (var product in existingProducts)
            //{
            //    if (!model.SelectedProductsIds.Contains(product.ProductId))
            //    {
            //        _appointmentItemService.Delete(product);
            //    }
            //}
            //foreach (var productId in model.SelectedProductsIds)
            //{
            //    if (_appointmentItemService.GetByProductAndAppointmentId(productId, model.Id).FirstOrDefault() == null)
            //    {
            //        _appointmentItemService.Insert(new AppointmentItem()
            //        {
            //            AppointmentId = model.Id,
            //            ProductId = productId
            //        });
            //    }
            //}

            return RedirectToAction("List");
        }

        public IActionResult UpdateStatus(int id, AppointmentStatus status)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Appointment appointment = _appointmentService.GetById(id);
            if (appointment == null) return NotFound();

            appointment.Status = status;
            _appointmentService.Update(appointment);

            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
        public IActionResult CancelAppoiment(int id, string note)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Appointment appointment = _appointmentService.GetById(id);
            if (appointment == null) return NotFound();

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.Note = appointment.Note + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Cita cancelada por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + ". Razón: " + note;
            _appointmentService.Update(appointment);

            return Ok();
        }

        [HttpPost]
        public IActionResult NoConfirmedAppoiment(int id, string note)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Appointment appointment = _appointmentService.GetById(id);
            if (appointment == null) return NotFound();

            appointment.Status = AppointmentStatus.NotConfirmed;
            appointment.Note = appointment.Note + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Cita marcada como no confirmada por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + ". Razón: " + note;
            _appointmentService.Update(appointment);

            return Ok();
        }

        [HttpPost]
        public IActionResult RegisteredAppoiment(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Appointment appointment = _appointmentService.GetById(id);
            if (appointment == null) return NotFound();

            appointment.Status = AppointmentStatus.Registered;
            appointment.Note = appointment.Note + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Cita marcada como registrada por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + ".";
            appointment.CheckInDate = DateTime.UtcNow;
            _appointmentService.Update(appointment);

            return Ok();
        }

        [HttpPost]
        public IActionResult ConfirmedAppoiment(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Appointment appointment = _appointmentService.GetById(id);
            if (appointment == null) return NotFound();

            appointment.Status = AppointmentStatus.Confirmed;
            appointment.Note = appointment.Note + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Cita marcada como confirmada por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + ".";
            _appointmentService.Update(appointment);

            return Ok();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult PrintAppointments(int did = 0, int bid = 0, string d = null)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            if (d == "31-12-1969") d = null;

            MemoryStream stream = new MemoryStream();
            PdfWriter pdfWriter = new PdfWriter(stream);
            PdfDocument pdfDoc = new PdfDocument(pdfWriter);
            Document doc = new Document(pdfDoc, PageSize.A4);
            doc.SetMargins(20, 20, 20, 20);

            Paragraph dermalomas = new Paragraph("Citas Dermalomas");
            dermalomas.SetBold();
            dermalomas.SetFontSize(25);
            dermalomas.SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
            doc.Add(dermalomas);

            DateTime selectedDayParsed = string.IsNullOrWhiteSpace(d) ? DateTime.Now : DateTime.ParseExact(d, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            Paragraph date = new Paragraph(selectedDayParsed.ToString("D", new CultureInfo("es-MX")));
            date.SetBold();
            date.SetFontSize(20);
            date.SetMarginBottom(-5);
            doc.Add(date);

            string doctorName = _customerService.GetCustomerById(did)?.GetFullName();
            doctorName = string.IsNullOrWhiteSpace(doctorName) ? "Todos los encargados" : doctorName;
            Paragraph doctor = new Paragraph(doctorName);
            doctor.SetBold();
            doctor.SetFontSize(15);
            doctor.SetMarginBottom(0);
            doc.Add(doctor);

            string branchName = _branchService.GetById(bid)?.Name;
            branchName = string.IsNullOrWhiteSpace(branchName) ? "Todas las sucursales" : branchName;
            Paragraph branch = new Paragraph(branchName);
            branch.SetFontSize(11);
            branch.SetBold();
            branch.SetMarginBottom(20);
            doc.Add(branch);

            var appointments = _appointmentService.GetAll()
                .Where(x => DbFunctions.TruncateTime(x.AppointmentDate) == DbFunctions.TruncateTime(selectedDayParsed));

            if (did != 0)
            {
                appointments = appointments.Where(x => x.DoctorId == did);
            }
            if (bid != 0)
            {
                appointments = appointments.Where(x => x.BranchId == bid);
            }

            int numColums = 5;
            if (did == 0) numColums++;
            if (bid == 0) numColums++;
            Table table = new Table(numColums);
            table.SetFontSize(7);
            table.SetWidth(PageSize.A4.GetWidth() - 50);
            table.SetBorder(Border.NO_BORDER);

            foreach (var appointment in appointments.OrderBy(x => x.AppointmentDate).ToList())
            {
                table.AddCell(new Cell()
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetBorderRight(Border.NO_BORDER)
                    .SetWidth(40)
                    .SetHeight(20)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(new Paragraph(appointment.AppointmentDate.ToLocalTime().ToString("hh:mm tt"))));
                table.AddCell(new Cell()
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetBorderRight(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .SetWidth(150)
                    .Add(new Paragraph(appointment.Patient.FirstName + " " + appointment.Patient.LastName)));
                table.AddCell(new Cell()
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetBorderRight(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .SetWidth(50)
                    .Add(new Paragraph(EnumHelper.GetDisplayName(appointment.VisitType))));
                if (bid == 0)
                {
                    table.AddCell(new Cell()
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetBorderRight(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .SetWidth(50)
                    .Add(new Paragraph(_branchService.GetById(appointment.BranchId).Name)));
                }
                if (did == 0)
                {
                    table.AddCell(new Cell()
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetBorderRight(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .SetWidth(100)
                    .Add(new Paragraph(_customerService.GetCustomerById(appointment.DoctorId).GetFullName())));
                }
                table.AddCell(new Cell()
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetBorderRight(Border.NO_BORDER)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .SetWidth(80)
                    .Add(new Paragraph(appointment.Patient.Phone1)));
                table.AddCell(new Cell()
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetBorderRight(Border.NO_BORDER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                    .Add(new Paragraph(string.IsNullOrWhiteSpace(appointment.Comments) ? "" : appointment.Comments)));
            }

            doc.Add(table);
            doc.Close();

            return File(stream.ToArray(), "application/pdf");
        }
    }
}