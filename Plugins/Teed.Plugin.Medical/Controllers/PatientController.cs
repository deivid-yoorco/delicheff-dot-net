using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Linq;
using Teed.Plugin.Medical.Services;
using Teed.Plugin.Medical.Domain;
using Teed.Plugin.Medical.Domain.Note;
using Teed.Plugin.Medical.Models;
using Teed.Plugin.Medical.Models.Appointment;
using Teed.Plugin.Medical.Models.Note;
using Teed.Plugin.Medical.Models.Patients;
using Teed.Plugin.Medical.Models.Prescription;
using Teed.Plugin.Medical.Models.Visit;
using Teed.Plugin.Medical.Security;
using Teed.Plugin.Medical.Utils;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Teed.Plugin.Medical.Models.File;

namespace Teed.Plugin.Medical.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class PatientController : BasePluginController
    {
        private readonly PatientService _patientService;
        private readonly DoctorPatientService _doctorPatientService;
        private readonly PrescriptionService _prescriptionService;
        private readonly NoteService _noteService;
        private readonly CustomerService _customerService;
        private readonly CustomerSettings _customerSettings;
        private readonly VisitService _visitService;
        private readonly AppointmentService _appointmentService;
        private readonly BranchService _branchService;
        private readonly PrescriptionItemService _prescriptionItemService;
        private readonly PatientFileService _patientFileService;
        private readonly IProductService _productService;
        private readonly ICountryService _countryService;
        private readonly ILocalizationService _localizationService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly ICustomerRegistrationService _customerRegistrationService;

        public PatientController(
            PatientService patientService,
            DoctorPatientService doctorPatientService,
            PrescriptionService prescriptionService,
            NoteService noteService,
            CustomerService customerService,
            CustomerSettings customerSettings,
            VisitService visitService,
            AppointmentService appointmentService,
            BranchService branchService,
            PrescriptionItemService prescriptionItemService,
            PatientFileService patientFileService,
            IPermissionService permissionService,
            ICountryService countryService,
            ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IGenericAttributeService genericAttributeService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            ICustomerRegistrationService customerRegistrationService,
            IProductService productService)
        {
            _patientService = patientService;
            _permissionService = permissionService;
            _doctorPatientService = doctorPatientService;
            _workContext = workContext;
            _prescriptionService = prescriptionService;
            _noteService = noteService;
            _customerService = customerService;
            _countryService = countryService;
            _localizationService = localizationService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _genericAttributeService = genericAttributeService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _customerSettings = customerSettings;
            _customerRegistrationService = customerRegistrationService;
            _visitService = visitService;
            _appointmentService = appointmentService;
            _branchService = branchService;
            _prescriptionItemService = prescriptionItemService;
            _productService = productService;
            _patientFileService = patientFileService;
        }

        public IActionResult List()
        {
            return View("~/Plugins/Teed.Plugin.Medical/Views/Patient/List.cshtml");
        }

        public IActionResult Details(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Patient patient = _patientService.GetById(id);
            if (patient == null) return BadRequest();

            Customer referedByCustomer = patient.ReferedBy == 0 ? null : _customerService.GetCustomerById(patient.ReferedBy);
            var url = "";
            if (patient.UpdatePageActive)
            {
                var host = Request.Host;
#if DEBUG
                url = "http://" + host.Host + ":" + host.Port + "/Admin/UpdatePatient/Update/" + id;
#else
                url = "http://" + host.Host + "/Admin/UpdatePatient/Update/" + id;
#endif
            }
            PatientsModel model = new PatientsModel()
            {
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Email = patient.Email,
                Id = patient.Id,
                DateOfBirthDay = patient.DateOfBirth.HasValue ? patient.DateOfBirth.Value.Day : 0,
                DateOfBirthMonth = patient.DateOfBirth.HasValue ? patient.DateOfBirth.Value.Month : 0,
                DateOfBirthYear = patient.DateOfBirth.HasValue ? patient.DateOfBirth.Value.Year : 0,
                City = patient.City,
                CountryId = patient.CountryId,
                CurrentConditions = patient.CurrentConditions,
                FamilyBackground = patient.FamilyBackground,
                Gender = patient.Gender,
                PersonalNonPathologicalHistory = patient.PersonalNonPathologicalHistory,
                PersonalPathologicalHistory = patient.PersonalPathologicalHistory,
                Phone1 = patient.Phone1,
                Phone2 = patient.Phone2,
                Phone3 = patient.Phone3,
                StateProvinceId = patient.StateProvinceId,
                StreetAddress = patient.StreetAddress,
                StreetAddress2 = patient.StreetAddress2,
                ZipPostalCode = patient.ZipPostalCode,
                LastUpdateDate = patient.UpdatedOnUtc,
                Allergies = patient.Allergies,
                ReferdBy = patient.ReferedBy,
                Commentary = patient.Commentary,
                ReferedByUser = referedByCustomer != null ? referedByCustomer.GetFullName() + " (" + referedByCustomer.Email + ")" : "",
                ActiveUpdate = patient.UpdatePageActive,
                UrlActive = url

            };

            ViewData["notesCount"] = _noteService.GetAll().Where(x => x.PatientId == patient.Id).Count();
            ViewData["filesCount"] = _patientFileService.GetAll().Where(x => x.PatientId == id).Count();

            return View("~/Plugins/Teed.Plugin.Medical/Views/Patient/Details.cshtml", model);
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            var patients = _patientService.ListAsNoTracking(command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = patients,
                Total = patients.TotalCount
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult PatientPrescriptions(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Patient patient = _patientService.GetById(id);
            if (patient == null) return BadRequest();

            var prescriptions = _prescriptionService.GetAllByPatientId(id).Select(x => new PrescriptionListModel
            {
                Id = x.Id,
                CreationDate = x.CreatedOnUtc.ToLocalTime().ToShortDateString(),
                CreationTime = x.CreatedOnUtc.ToLocalTime().ToLongTimeString()
            }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = prescriptions,
                Total = prescriptions.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult PatientFiles(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Patient patient = _patientService.GetById(id);
            if (patient == null) return BadRequest();

            var files = _patientFileService.GetAll()
                .Where(x => x.PatientId == id)
                .ToList()
                .OrderByDescending(x => x.UploadedDateUtc)
                .Select(x => new FilesListModel()
                {
                    Id = x.Id,
                    Description = x.Description,
                    FileType = x.Extension,
                    UploadedDate = x.UploadedDateUtc.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt"),
                    Url = x.FileUrl

                }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = files,
                Total = files.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult PatientsNotes(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Patient patient = _patientService.GetById(id);
            if (patient == null) return BadRequest();

            var notes = _noteService.GetAllByPatientId(id).Select(x => new NotesListModel
            {
                Id = x.Id,
                CreationDate = x.CreatedOnUtc.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt"),
                Text = x.Text

            }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = notes,
                Total = notes.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult PatientVisits(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Patient patient = _patientService.GetById(id);
            if (patient == null) return BadRequest();

            var visits = _visitService.GetAll()
                .AsEnumerable()
                .Where(x => x.PatientId == patient.Id)
                .OrderByDescending(x => x.VisitDate)
                .Select(x => new VisitListModel
                {
                    Id = x.Id,
                    CreationDate = x.VisitDate.ToLongTimeString() == "12:00:00 AM" ? x.VisitDate.ToString("dd-MM-yyyy") : x.VisitDate.ToLocalTime().ToString("dd-MM-yyyy"),
                    CreationTime = x.VisitDate.ToLongTimeString() == "12:00:00 AM" ? "Hora no disponible" : x.VisitDate.ToLongTimeString(),
                    //Branch = x.BranchId == 0 ? null : _branchService.GetById(x.BranchId).Name,
                    Doctor = x.DoctorId == 0 ? null : _customerService.GetCustomerById(x.DoctorId).GetFullName()
                }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = visits,
                Total = visits.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult PatientAppointments(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Patient patient = _patientService.GetById(id);
            if (patient == null) return BadRequest();

            var appointments = _appointmentService.GetAll()
                .AsEnumerable()
                .Where(x => x.PatientId == patient.Id && x.ParentAppointmentId == 0)
                .OrderByDescending(x => x.AppointmentDate)
                .Select(x => new AppointmentListModel
                {
                    Id = x.Id,
                    AppointmentDate = x.AppointmentDate.ToLocalTime().ToShortDateString(),
                    AppointmentTime = x.AppointmentDate.ToLocalTime().ToLongTimeString(),
                    Doctor = _customerService.GetCustomerById(x.DoctorId).GetFullName(),
                    Branch = _branchService.GetById(x.BranchId).Name,
                    Patient = x.Patient
                }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = appointments,
                Total = appointments.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult PatientPastAppointments(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Patient patient = _patientService.GetById(id);
            if (patient == null) return BadRequest();

            var appointments = _appointmentService.GetAll()
                .AsEnumerable()
                .Where(x => x.PatientId == patient.Id && x.AppointmentDate.ToLocalTime() < (DateTime.Now - TimeSpan.FromMinutes(30)))
                .Select(x => new AppointmentListModel
                {
                    Id = x.Id,
                    AppointmentDate = x.AppointmentDate.ToLocalTime().ToShortDateString(),
                    AppointmentTime = x.AppointmentDate.ToLocalTime().ToLongTimeString(),
                    Doctor = _customerService.GetCustomerById(x.DoctorId).GetFullName(),
                    Branch = _branchService.GetById(x.BranchId).Name
                }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = appointments,
                Total = appointments.Count
            };
            return Json(gridModel);
        }


        public IActionResult VerifyAndCreate()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Medical/Views/Patient/VerifyAndCreate.cshtml");
        }

        [HttpPost]
        public IActionResult VerifyAndCreate(VerifyModel verify)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            if (!string.IsNullOrWhiteSpace(verify.EmailPhone))
            {
                string[] EmailPhone = verify.EmailPhone.Split('(', ' ', ')');
                var email = EmailPhone[0];
                bool isNumeric = email.All(char.IsDigit);
                if (isNumeric)
                {
                    verify.Phone = email;
                }
                else
                {
                    verify.Email = email;
                    bool isValidEmail = StringUtils.IsValidEmail(email);
                    if (!isValidEmail)
                    {
                        ModelState.AddModelError("", "Debes ingresar un correo electrónico válido.");
                        verify.EmailPhone = null;
                        verify.Email = null;
                        return View("~/Plugins/Teed.Plugin.Medical/Views/Patient/VerifyAndCreate.cshtml", verify);
                    }
                }

            }
            else if (verify.CustomerId > 0)
            {
                Customer selectedCustomer = _customerService.GetCustomerById(verify.CustomerId);
                if (selectedCustomer == null) return NotFound();
                verify.Email = selectedCustomer.Email;
            }
            else
            {
                //ModelState.AddModelError("", "Debes ingresar un correo electrónico válido.");
                //verify.EmailPhone = null;
                //verify.Email = null;
                //return View("~/Plugins/Teed.Plugin.Medical/Views/Patient/VerifyAndCreate.cshtml", verify);
                var patientModel = new PatientsModel();
                PreparePatientModel(patientModel);
                return View("~/Plugins/Teed.Plugin.Medical/Views/Patient/Create.cshtml", patientModel);
            }

            Patient patient = string.IsNullOrWhiteSpace(verify.Email) ? null : _patientService.FindByEmail(verify.Email);
            if (patient != null)
            {
                ModelState.AddModelError("", "Ya existe un paciente con ese correo electrónico.");
                verify.EmailPhone = null;
                verify.Email = null;
                return View("~/Plugins/Teed.Plugin.Medical/Views/Patient/VerifyAndCreate.cshtml", verify);
            }

            Customer customer = string.IsNullOrWhiteSpace(verify.Email) ? null : _customerService.GetCustomerByEmail(verify.Email);
            PatientsModel model = new PatientsModel();

            if (customer != null)
            {
                DateTime date = customer.GetAttribute<DateTime>(SystemCustomerAttributeNames.DateOfBirth);
                model = new PatientsModel
                {
                    FirstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                    LastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName),
                    StreetAddress = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress),
                    StreetAddress2 = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2),
                    CountryId = customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId),
                    StateProvinceId = customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId),
                    City = customer.GetAttribute<string>(SystemCustomerAttributeNames.City),
                    ZipPostalCode = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode),
                    Gender = customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender),
                    Phone1 = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                    Email = customer.Email,
                    DateOfBirthDay = date.Day,
                    DateOfBirthMonth = date.Month,
                    DateOfBirthYear = date.Year,
                    IsExistingUser = true
                };
            }
            else
            {
                model.Email = verify.Email;
                model.Phone1 = verify.Phone;
            }

            PreparePatientModel(model);
            return View("~/Plugins/Teed.Plugin.Medical/Views/Patient/Create.cshtml", model);
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Patient patient = _patientService.GetById(id);
            if (patient == null) return BadRequest();

            PatientsModel model = new PatientsModel()
            {
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Email = patient.Email,
                Id = patient.Id,
                DateOfBirthDay = patient.DateOfBirth.HasValue ? patient.DateOfBirth.Value.Day : 0,
                DateOfBirthMonth = patient.DateOfBirth.HasValue ? patient.DateOfBirth.Value.Month : 0,
                DateOfBirthYear = patient.DateOfBirth.HasValue ? patient.DateOfBirth.Value.Year : 0,
                City = patient.City,
                CountryId = patient.CountryId,
                CurrentConditions = patient.CurrentConditions,
                FamilyBackground = patient.FamilyBackground,
                Gender = patient.Gender,
                PersonalNonPathologicalHistory = patient.PersonalNonPathologicalHistory,
                PersonalPathologicalHistory = patient.PersonalPathologicalHistory,
                Phone1 = patient.Phone1,
                Phone2 = patient.Phone2,
                Phone3 = patient.Phone3,
                StateProvinceId = patient.StateProvinceId,
                StreetAddress = patient.StreetAddress,
                StreetAddress2 = patient.StreetAddress2,
                ZipPostalCode = patient.ZipPostalCode,
                Allergies = patient.Allergies,
                Commentary = patient.Commentary
            };

            PreparePatientModel(model);
            return View("~/Plugins/Teed.Plugin.Medical/Views/Patient/Edit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PatientsModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            if (!string.IsNullOrWhiteSpace(model.Email) && !model.IsExistingUser)
            {
                Patient patientDb = _patientService.FindByEmail(model.Email);
                if (patientDb != null)
                {
                    ModelState.AddModelError("", "Ya existe un paciente registrado con ese correo electrónico.");
                    return View("~/Plugins/Teed.Plugin.Medical/Views/Patient/Create.cshtml", model);
                }
            }

            Patient patient = new Patient()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                DateOfBirth = model.DateOfBirthYear == 0 ||
                              model.DateOfBirthMonth == 0 ||
                              model.DateOfBirthDay == 0 ? null :
                              new DateTime?(new DateTime(model.DateOfBirthYear, model.DateOfBirthMonth, model.DateOfBirthDay)),
                StreetAddress = model.StreetAddress,
                StateProvinceId = model.StateProvinceId,
                StreetAddress2 = model.StreetAddress2,
                City = model.City,
                CountryId = model.CountryId,
                FamilyBackground = model.FamilyBackground,
                Gender = model.Gender,
                PersonalNonPathologicalHistory = model.PersonalNonPathologicalHistory,
                PersonalPathologicalHistory = model.PersonalPathologicalHistory,
                ZipPostalCode = model.ZipPostalCode,
                Phone1 = model.Phone1,
                Phone2 = model.Phone2,
                Phone3 = model.Phone3,
                CurrentConditions = model.CurrentConditions,
                Allergies = model.Allergies,
                ReferedBy = model.ReferdBy,
                ReferedByExternal = model.ReferedByExternal,
                Commentary = model.Commentary

            };
            _patientService.Insert(patient);

            if (!model.IsExistingUser && !string.IsNullOrWhiteSpace(model.Email) && model.StreetAddress != null && model.ZipPostalCode != null && model.City != null && model.CountryId != 0f)
            {
                var customer = new Customer
                {
                    CustomerGuid = Guid.NewGuid(),
                    Email = model.Email,
                    Username = model.Email,
                    VendorId = 0,
                    AdminComment = null,
                    IsTaxExempt = false,
                    Active = true,
                    CreatedOnUtc = DateTime.UtcNow,
                    LastActivityDateUtc = DateTime.UtcNow,
                    RegisteredInStoreId = _storeContext.CurrentStore.Id
                };
                _customerService.InsertCustomer(customer);

                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, model.FirstName);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, model.LastName);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, model.DateOfBirth);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress, model.StreetAddress);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress2, model.StreetAddress2);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZipPostalCode, model.ZipPostalCode);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.City, model.City);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CountryId, model.CountryId);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StateProvinceId, model.StateProvinceId);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, model.Phone1);

                _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                {
                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                    Email = customer.Email,
                    Active = true,
                    StoreId = _storeContext.CurrentStore.Id,
                    CreatedOnUtc = DateTime.UtcNow
                });

                var changePassRequest = new ChangePasswordRequest(model.Email, false, _customerSettings.DefaultPasswordFormat, "teed1234");
                var changePassResult = _customerRegistrationService.ChangePassword(changePassRequest);
                if (!changePassResult.Success)
                {
                    foreach (var changePassError in changePassResult.Errors)
                        ErrorNotification(changePassError);
                }

                customer.CustomerRoles.Add(_customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered));
                _customerService.UpdateCustomer(customer);
            }

            //DoctorPatient doctorPatient = new DoctorPatient()
            //{
            //    DoctorId = _workContext.CurrentCustomer.Id,
            //    PatientId = patient.Id
            //};
            //_doctorPatientService.Insert(doctorPatient);

            return RedirectToAction("Details", new { id = patient.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(PatientsModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Patient patient = _patientService.GetById(model.Id);
            if (patient == null) return BadRequest();

            patient.FirstName = model.FirstName;
            patient.LastName = model.LastName;
            patient.Email = model.Email;
            patient.DateOfBirth = model.DateOfBirthYear == 0 ||
                              model.DateOfBirthMonth == 0 ||
                              model.DateOfBirthDay == 0 ? null :
                              new DateTime?(new DateTime(model.DateOfBirthYear, model.DateOfBirthMonth, model.DateOfBirthDay));
            patient.City = model.City;
            patient.CountryId = model.CountryId;
            patient.CurrentConditions = model.CurrentConditions;
            patient.FamilyBackground = model.FamilyBackground;
            patient.Gender = model.Gender;
            patient.PersonalNonPathologicalHistory = model.PersonalNonPathologicalHistory;
            patient.PersonalPathologicalHistory = model.PersonalPathologicalHistory;
            patient.Phone1 = model.Phone1;
            patient.Phone2 = model.Phone2;
            patient.Phone3 = model.Phone3;
            patient.StateProvinceId = model.StateProvinceId;
            patient.StreetAddress = model.StreetAddress;
            patient.StreetAddress2 = model.StreetAddress2;
            patient.ZipPostalCode = model.ZipPostalCode;
            patient.Allergies = model.Allergies;
            patient.Commentary = model.Commentary;

            _patientService.Update(patient);

            return RedirectToAction("Details", new { id = patient.Id });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Patient patient = _patientService.GetById(id);
            if (patient == null) return BadRequest();

            _patientService.Delete(patient);

            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult VerifyCustomerListData()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            var customers = _customerService.GetAllCustomers().Where(x => !string.IsNullOrWhiteSpace(x.Email) && !string.IsNullOrWhiteSpace(x.GetFullName()));
            var elements = customers.Select(x => new CustomerListModel
            {
                Id = x.Id,
                Customer = x.GetFullName() + " " + x.Email + " (" + x.GetAttribute<string>(SystemCustomerAttributeNames.Phone) + ")"
            }).ToList();

            return Json(elements);
        }

        [HttpPost]
        public IActionResult GetAllPatients()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            var patients = _patientService.GetAll();
            var elements = patients.Select(x => new GetAllPatientsListModel
            {
                Id = x.Id,
                Patient = $"{x.FirstName} {x.LastName} ({x.Email})({x.Phone1})"
            }).ToList();

            return Json(elements);
        }

        [HttpPost]
        public IActionResult ReferedListData()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            var customers = _customerService.GetAllCustomers().Where(x => !string.IsNullOrWhiteSpace(x.Email) && !string.IsNullOrWhiteSpace(x.GetFullName()));
            var patients = _patientService.GetAll().GroupBy(x => x.ReferedByExternal).ToList();
            var elements = customers.Select(x => new CustomerListModel
            {
                Id = x.Id,
                Customer = x.GetFullName() + " (" + x.Email + ")"
            }).ToList();
            foreach (var item in patients)
            {
                if (item.Key != null)
                {
                    CustomerListModel model = new CustomerListModel()
                    {
                        Id = 0,
                        Customer = item.Key
                    };
                    elements.Add(model);
                }


            }
            return Json(elements);
        }

        [HttpPost]
        public IActionResult CustomerListData()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            var customers = _customerService.GetAllCustomers().Where(x => !string.IsNullOrWhiteSpace(x.Email) && !string.IsNullOrWhiteSpace(x.GetFullName()));
            var elements = customers.Select(x => new CustomerListModel
            {
                Id = x.Id,
                Customer = x.GetFullName() + " (" + x.Email + ")"
            }).ToList();

            return Json(elements);
        }

        [HttpPost]
        public IActionResult GetPrescriptionProductsList(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            var products = _prescriptionItemService.ListByPrescriptionId(id)
                .Select(x => new PrescriptionItemModel
                {
                    Id = x.Id,
                    Dosage = x.Dosage,
                    ItemName = x.ProductId > 0 ? _productService.GetProductById(x.ProductId.GetValueOrDefault()).Name : x.ItemName,
                    PrescriptionId = x.PrescriptionId,
                    ProductId = x.ProductId
                }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = products,
                Total = products.Count
            };
            return Json(gridModel);
        }

        private void PreparePatientModel(PatientsModel model)
        {
            // GET COUNTRIES
            model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries(showHidden: true))
            {
                model.AvailableCountries.Add(new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(),
                    Selected = c.Id == model.CountryId
                });
            }
            model.AvailableCountries.Add(new SelectListItem { Text = "Otro", Value = "999" });

            // GET STATES
            var states = _stateProvinceService.GetStateProvincesByCountryId(Int32.Parse(model.AvailableCountries[1].Value)).ToList();
            if (states.Any())
            {
                model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectState"), Value = "0" });
                foreach (var s in states)
                {
                    model.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
                }
                model.AvailableStates.Add(new SelectListItem { Text = "Otro", Value = "999" });
            }
            else
            {
                var anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);
                model.AvailableStates.Add(new SelectListItem
                {
                    Text = _localizationService.GetResource(anyCountrySelected ? "Admin.Address.OtherNonUS" : "Admin.Address.SelectState"),
                    Value = "0"
                });
            }
        }

        public IActionResult ActivePatient(int id)
        {

            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Patient patient = _patientService.GetById(id);
            if (patient == null) return BadRequest();

            Customer referedByCustomer = patient.ReferedBy == 0 ? null : _customerService.GetCustomerById(patient.ReferedBy);
            Note note = _noteService.GetLast(id);
            patient.UpdatePageActive = true;
            patient.UpdatePageActivationDateOnUtc = DateTime.UtcNow;
            _patientService.Update(patient);


            return RedirectToAction("Details", new { id = patient.Id });
        }
    }
}