using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Models;
using Teed.Plugin.Medical.Services;
using Teed.Plugin.Medical.Domain;
using Teed.Plugin.Medical.Domain.Prescription;
using Teed.Plugin.Medical.Domain.Visit;
using Teed.Plugin.Medical.Helpers;
using Teed.Plugin.Medical.Models.Appointment;
using Teed.Plugin.Medical.Models.Doctors;
using Teed.Plugin.Medical.Models.Patients;
using Teed.Plugin.Medical.Models.Prescription;
using Teed.Plugin.Medical.Models.Visit;
using Teed.Plugin.Medical.Security;
using Teed.Plugin.Medical.Domain.Appointment;
using System.IO;
using Teed.Plugin.Medical.Domain.Patients;

namespace Teed.Plugin.Medical.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class VisitController : BasePluginController
    {
        private readonly PatientService _patientService;
        private readonly BranchService _branchService;
        private readonly VisitService _visitService;
        private readonly VisitExtraUsersService _visitExtraUsersService;
        private readonly VisitProductService _visitProductService;
        private readonly AppointmentService _appointmentService;
        private readonly PrescriptionItemService _prescriptionItemService;
        private readonly PrescriptionService _prescriptionService;
        private readonly BranchWorkerService _branchWorkerService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IProductService _productService;

        public VisitController(PatientService patientService,
            BranchService branchService,
            VisitService visitService,
            VisitExtraUsersService visitExtraUsersService,
            AppointmentService appointmentService,
            VisitProductService visitProductService,
            PrescriptionItemService prescriptionItemService,
            PrescriptionService prescriptionService,
            BranchWorkerService branchWorkerService,
            IPermissionService permissionService,
            IWorkContext workContext,
            ICustomerService customerService,
            IProductService productService)
        {
            _patientService = patientService;
            _permissionService = permissionService;
            _workContext = workContext;
            _branchService = branchService;
            _visitService = visitService;
            _customerService = customerService;
            _visitProductService = visitProductService;
            _productService = productService;
            _appointmentService = appointmentService;
            _visitExtraUsersService = visitExtraUsersService;
            _prescriptionItemService = prescriptionItemService;
            _prescriptionService = prescriptionService;
            _branchWorkerService = branchWorkerService;
        }

        public IActionResult Create(int id = 0, int aid = 0)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            var customerId = _workContext.CurrentCustomer.Id;
            var branchId = _branchWorkerService.GetByCustomerId(customerId).FirstOrDefault();
            ViewData["BranchId"] = branchId;
            ViewData["SelectedUser"] = id;
            ViewData["AppointmentId"] = aid;
            ViewData["IsAdmin"] = _workContext.CurrentCustomer.IsAdmin();
            ViewData["IsDoctor"] = _workContext.CurrentCustomer.IsInCustomerRole("Doctor");
            ViewData["DoctorsAndNurses"] = SelectListHelper.GetDoctorsAndNursesList(_customerService);
            ViewData["Treatments"] = SelectListHelper.GetProductsList(_productService);
            return View("~/Plugins/Teed.Plugin.Medical/Views/Visit/Create.cshtml");
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Visit visit = _visitService.GetById(id);
            if (visit == null) return NotFound();
            Patient patient = _patientService.GetById(visit.PatientId);
            var branch = _branchService.GetById(visit.BranchId);
            var appoiment = _appointmentService.GetById(visit.AppointmentId);
            var extraCustomerIds = _visitExtraUsersService.GetAll().Where(x => x.VisitId == visit.Id).Select(x => x.CustomerId).ToArray();
            var model = new VisitModel()
            {
                Id = visit.Id,
                BranchId = visit.BranchId,
                BranchTitle = branch?.Name,
                Comment = visit.Comment,
                CurrentCondition = visit.CurrentCondition,
                Diagnosis = visit.Diagnosis,
                Evolution = visit.Evolution,
                ImportantRecord = visit.ImportantRecord,
                LastUpdate = visit.LastUpdate.ToLocalTime(),
                PatientId = visit.PatientId,
                Patient = patient,
                PreviousTx = visit.PreviousTx,
                Price = visit.Price,
                ProductsCount = _visitProductService.GetAll().Where(x => x.VisitId == visit.Id).Count(),
                DoctorId = visit.DoctorId,
                AppointmentId = visit.AppointmentId,
                Appoiment = appoiment?.AppointmentDate.ToLocalTime().ToShortDateString() + " " + appoiment?.AppointmentDate.ToLocalTime().ToShortTimeString(),
                Products = _visitProductService.GetProductsByVisitId(visit.Id),
                SelectedUsersIds = _visitExtraUsersService.GetAll().Where(x => x.VisitId == visit.Id).Select(x => x.CustomerId).ToList(),
                SelectedProductsIds = _visitProductService.GetProductsIdsByVisitId(id).ToList(),
                Treatment = visit.Treatment

            };

            ViewData["IsAdmin"] = _workContext.CurrentCustomer.IsAdmin();
            ViewData["IsDoctor"] = _workContext.CurrentCustomer.IsInCustomerRole("Doctor");
            ViewData["DoctorsAndNurses"] = SelectListHelper.GetDoctorsAndNursesList(_customerService);
            ViewData["Treatments"] = SelectListHelper.GetProductsList(_productService);
            return View("~/Plugins/Teed.Plugin.Medical/Views/Visit/Edit.cshtml", model);
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
        public IActionResult BranchListData()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            var branches = _branchService.GetAll().AsEnumerable();
            var elements = branches.Select(x => new BranchesListModel
            {
                Id = x.Id,
                Branch = $"{x.Name}"
            }).ToList();

            return Json(elements);
        }

        [HttpPost]
        public IActionResult DoctorListData()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            var doctors = SelectListHelper.GetDoctorsAndNursesList(_customerService);
            var elements = doctors.Select(x => new DoctorsListModel
            {
                Id = int.Parse(x.Value),
                Doctor = $"{x.Text}"
            }).ToList();

            return Json(elements);
        }

        [HttpPost]
        public IActionResult GetVisitProductsList(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            var products = _visitProductService.GetAll()
                .AsEnumerable()
                .Where(x => x.VisitId == id)
                .Select(x => new VisitItemModel
                {
                    Id = x.Id,
                    ItemName = _productService.GetProductById(x.ProductId).Name,
                    ProductId = x.ProductId
                }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = products,
                Total = products.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ProductListData()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            var products = _productService.SearchProducts().Select(x => new ProductsListModel
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
            return Json(products);
        }

        [HttpPost]
        public IActionResult AppointmentListData(int doctorId, int branchId, int patientId)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            if (doctorId == 0 || branchId == 0 || patientId == 0)
            {
                return Json("");
            }

            var appointments = _appointmentService.GetAll()
                .Where(x => x.BranchId == branchId && x.DoctorId == doctorId && x.PatientId == patientId)
                .AsEnumerable()
                .Select(x => new AppointmentListModel
                {
                    Id = x.Id,
                    Appointment = x.AppointmentDate.ToLocalTime().ToShortDateString() + " " + x.AppointmentDate.ToLocalTime().ToShortTimeString()
                }).ToList();
            return Json(appointments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddProductToVisit(int productId, int visitId)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Visit visit = _visitService.GetById(visitId);
            if (visit == null) return NotFound();

            VisitProduct visitProduct = new VisitProduct()
            {
                VisitId = visitId,
                ProductId = productId
            };
            _visitProductService.Insert(visitProduct);

            _visitService.VisitUpdated(visit);

            return Json(new { Result = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(VisitModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            if (!_workContext.CurrentCustomer.IsAdmin() &&
                !_workContext.CurrentCustomer.IsInCustomerRole("Receptionist"))
            {
                model.DoctorId = _workContext.CurrentCustomer.Id;
            }

            if (model.DoctorId == 0)
            {
                ViewData["SelectedUser"] = model.PatientId;
                ViewData["IsAdmin"] = _workContext.CurrentCustomer.IsAdmin();
                ModelState.AddModelError("", "Debes seleccionar el doctor.");
                return View("~/Plugins/Teed.Plugin.Medical/Views/Visit/Create.cshtml", model);
            }
            else if (model.PatientId == 0)
            {
                ViewData["SelectedUser"] = model.PatientId;
                ViewData["IsAdmin"] = _workContext.CurrentCustomer.IsAdmin();
                ModelState.AddModelError("", "Debes seleccionar un paciente.");
                return View("~/Plugins/Teed.Plugin.Medical/Views/Visit/Create.cshtml", model);
            }
            else if (model.BranchId == 0)
            {
                ViewData["SelectedUser"] = model.PatientId;
                ViewData["IsAdmin"] = _workContext.CurrentCustomer.IsAdmin();
                ModelState.AddModelError("", "Debes seleccionar la sucursal.");
                return View("~/Plugins/Teed.Plugin.Medical/Views/Visit/Create.cshtml", model);
            }
            //else if (model.AppointmentId == 0)
            //{
            //    ViewData["SelectedUser"] = model.PatientId;
            //    ViewData["IsAdmin"] = _workContext.CurrentCustomer.IsAdmin();
            //    ModelState.AddModelError("", "Debes seleccionar la cita a la que pertenece.");
            //    return View("~/Plugins/Teed.Plugin.Medical/Views/Visit/Create.cshtml", model);
            //}
            //else if (_visitService.GetAll().Where(x => x.AppointmentId == model.AppointmentId).Any())
            //{
            //    ViewData["SelectedUser"] = model.PatientId;
            //    ViewData["IsAdmin"] = _workContext.CurrentCustomer.IsAdmin();
            //    ModelState.AddModelError("", "Ya existe una consulta asociada a la cita seleccionada.");
            //    return View("~/Plugins/Teed.Plugin.Medical/Views/Visit/Create.cshtml", model);
            //}

            Visit visit = new Visit()
            {
                BranchId = model.BranchId,
                Comment = model.Comment,
                CurrentCondition = model.CurrentCondition,
                Diagnosis = model.Diagnosis,
                DoctorId = _workContext.CurrentCustomer.IsAdmin() || _workContext.CurrentCustomer.IsInCustomerRole("Receptionist") ? model.DoctorId : _workContext.CurrentCustomer.Id,
                Evolution = model.Evolution,
                ImportantRecord = model.ImportantRecord,
                LastUpdate = DateTime.UtcNow,
                PatientId = model.PatientId,
                PreviousTx = model.PreviousTx,
                Prognostic = model.Prognostic,
                Price = model.Price,
                VisitDate = DateTime.UtcNow,
                AppointmentId = model.AppointmentId,
                Treatment = model.Treatment
            };
            _visitService.Insert(visit);

            if (model.AppointmentId != 0)
            {
                Appointment appointment = _appointmentService.GetById(model.AppointmentId);
                appointment.Note = appointment.Note + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Se creó una consulta para esta cita por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + ".";
                appointment.Note = appointment.Note + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Cita marcada como completada por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + ".";
                _appointmentService.SetAppointmentAsCompleted(appointment);
            }

            foreach (var item in model.SelectedUsersIds)
            {
                if (item != model.DoctorId)
                {
                    _visitExtraUsersService.Insert(new VisitExtraUsers()
                    {
                        VisitId = visit.Id,
                        CustomerId = item
                    });
                }
            }

            //foreach (var item in model.SelectedProductsIds)
            //{
            //    _visitProductService.Insert(new VisitProduct()
            //    {
            //        VisitId = visit.Id,
            //        ProductId = item
            //    });
            //}

            //Prescription prescription = new Prescription()
            //{
            //    Comment = model.Comments,
            //    PatientId = model.PatientId,
            //    DoctorId = _workContext.CurrentCustomer.Id
            //};
            //_prescriptionService.Insert(prescription);

            //var prescriptionProducts = _prescriptionItemService.GetAllByPrescriptionId2(model.PatientId).ToList();
            //foreach(PrescriptionItem item in prescriptionProducts)
            //{
            //    item.PrescriptionId = prescription.Id;
            //    _prescriptionItemService.Update(item);
            //}

            return RedirectToAction("Edit", new { id = visit.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(VisitModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();



            if (model.DoctorId == 0)
            {
                ViewData["SelectedUser"] = model.PatientId;
                ViewData["IsAdmin"] = _workContext.CurrentCustomer.IsAdmin();
                ModelState.AddModelError("", "Debes seleccionar el doctor.");
                return View("~/Plugins/Teed.Plugin.Medical/Views/Visit/Edit.cshtml", model);
            }
            else if (model.PatientId == 0)
            {
                ViewData["SelectedUser"] = model.PatientId;
                ViewData["IsAdmin"] = _workContext.CurrentCustomer.IsAdmin();
                ModelState.AddModelError("", "Debes seleccionar un paciente.");
                return View("~/Plugins/Teed.Plugin.Medical/Views/Visit/Edit.cshtml", model);
            }
            else if (model.BranchId == 0)
            {
                ViewData["SelectedUser"] = model.PatientId;
                ViewData["IsAdmin"] = _workContext.CurrentCustomer.IsAdmin();
                ModelState.AddModelError("", "Debes seleccionar la sucursal.");
                return View("~/Plugins/Teed.Plugin.Medical/Views/Visit/Edit.cshtml", model);
            }
            else if (model.AppointmentId == 0)
            {
                ViewData["SelectedUser"] = model.PatientId;
                ViewData["IsAdmin"] = _workContext.CurrentCustomer.IsAdmin();
                ModelState.AddModelError("", "Debes seleccionar la cita a la que pertenece.");
                return View("~/Plugins/Teed.Plugin.Medical/Views/Visit/Edit.cshtml", model);
            }

            Visit visit = _visitService.GetById(model.Id);
            if (visit == null) return NotFound();

            /*if (_visitService.GetAll().Where(x => x.AppointmentId == model.AppointmentId).Any())
            {
                ViewData["SelectedUser"] = model.PatientId;
                ViewData["IsAdmin"] = _workContext.CurrentCustomer.IsAdmin();
                ModelState.AddModelError("", "Ya existe una consulta asociada a la cita seleccionada.");
                return View("~/Plugins/Teed.Plugin.Medical/Views/Visit/Edit.cshtml", model);
            }*/

            visit.BranchId = model.BranchId == 0 ? visit.BranchId : model.BranchId;
            visit.Comment = model.Comment;
            visit.CurrentCondition = model.CurrentCondition;
            visit.Diagnosis = model.Diagnosis;
            visit.DoctorId = model.DoctorId == 0 ? visit.DoctorId : model.DoctorId;
            visit.Evolution = model.Evolution;
            visit.ImportantRecord = model.ImportantRecord;
            visit.LastUpdate = DateTime.UtcNow;
            visit.PatientId = model.PatientId == 0 ? visit.PatientId : model.PatientId;
            visit.PreviousTx = model.PreviousTx;
            visit.Price = model.Price;
            visit.Prognostic = model.Prognostic;
            visit.AppointmentId = model.AppointmentId;

            _visitService.Update(visit);
            foreach (var extraUserId in model.SelectedUsersIds)
            {
                if (extraUserId != model.DoctorUserId &&
                    _visitExtraUsersService.GetAll().Where(x => x.VisitId == visit.Id && x.CustomerId == extraUserId).FirstOrDefault() == null)
                {
                    _visitExtraUsersService.Insert(new VisitExtraUsers()
                    {
                        VisitId = visit.Id,
                        CustomerId = extraUserId
                    });
                }
            }

            //foreach (var productId in model.SelectedProductsIds)
            //{
            //    if (_visitProductService.GetByProductAndVisitId(productId, model.Id).FirstOrDefault() == null)
            //    {
            //        _visitProductService.Insert(new VisitProduct()
            //        {
            //            VisitId = model.Id,
            //            ProductId = productId
            //        });
            //    }
            //}
            return RedirectToAction("Edit", new { id = visit.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddProductToPrescription(int productId, string itemName, string dosage, int prescriptionId)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            PrescriptionItem prescriptionItem = new PrescriptionItem()
            {
                ItemName = itemName,
                Dosage = dosage,
                PrescriptionId = prescriptionId,
                ProductId = productId
            };
            _prescriptionItemService.Insert(prescriptionItem);

            return Json(new { Result = true });
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
    }
}
