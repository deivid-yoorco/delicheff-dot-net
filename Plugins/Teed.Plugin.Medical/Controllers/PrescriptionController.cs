using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Helpers;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Linq;
using Teed.Plugin.Medical.Domain;
using Teed.Plugin.Medical.Domain.Prescription;
using Teed.Plugin.Medical.Models;
using Teed.Plugin.Medical.Models.Patients;
using Teed.Plugin.Medical.Models.Prescription;
using Teed.Plugin.Medical.Security;
using Teed.Plugin.Medical.Services;

namespace Teed.Plugin.Medical.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class PrescriptionController : BasePluginController
    {
        PatientService _patientService;
        PrescriptionService _prescriptionService;
        PrescriptionItemService _prescriptionItemService;
        private readonly IWorkContext _workContext;
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;

        public PrescriptionController(PatientService patientService,
            PrescriptionService prescriptionService,
            IWorkContext workContext,
            IPermissionService permissionService,
            IProductService productService,
            PrescriptionItemService prescriptionItemService)
        {
            _patientService = patientService;
            _workContext = workContext;
            _permissionService = permissionService;
            _prescriptionService = prescriptionService;
            _prescriptionItemService = prescriptionItemService;
            _productService = productService;
        }

        public IActionResult Create(int id = 0)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            ViewData["Patients"] = _workContext.CurrentCustomer.IsAdmin() || _workContext.CurrentCustomer.IsInCustomerRole("Receptionist") ? _patientService.GetAll() : _patientService.GetAllByDoctorId(_workContext.CurrentCustomer.Id);
            ViewData["SelectedUser"] = id;
            return View("~/Plugins/Teed.Plugin.Medical/Views/Prescription/Create.cshtml");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Medical/Views/Prescription/List.cshtml");
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Prescription prescription = _prescriptionService.GetById(id);
            if (prescription == null) return NotFound();

            PrescriptionsModel model = new PrescriptionsModel()
            {
                Id = prescription.Id,
                Comments = prescription.Comment,
                PatientId = prescription.PatientId,
                ItemsCount = _prescriptionItemService.GetElementsCount(prescription.Id)
            };
            GetPatientsList(model);
            ViewData["Products"] = _productService.SearchProducts().ToList();
            return View("~/Plugins/Teed.Plugin.Medical/Views/Prescription/Edit.cshtml", model);
        }

        public IActionResult Details(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Prescription prescription = _prescriptionService.GetById(id);
            if (prescription == null) return NotFound();

            Patient patient = _patientService.GetById(prescription.PatientId);
            if (patient == null) return NotFound();

            PrescriptionsDetailsModel model = new PrescriptionsDetailsModel()
            {
                Id = prescription.Id,
                Comment = prescription.Comment,
                CreationDate = prescription.CreatedOnUtc.ToLocalTime().ToShortDateString(),
                CreationTime = prescription.CreatedOnUtc.ToLocalTime().ToLongTimeString(),
                PatientFullName = patient.FirstName + " " + patient.LastName,
                PatientId = patient.Id,
                Products = _prescriptionItemService.GetAllByPrescriptionId(prescription.Id).Select(x => new ProductsInPrescription
                {
                    Dosage = x.Dosage,
                    Name = x.ProductId > 0 ? _productService.GetProductById(x.ProductId.GetValueOrDefault()).Name : x.ItemName,
                }).ToList()
            };
            return View("~/Plugins/Teed.Plugin.Medical/Views/Prescription/Details.cshtml", model);
        }

        [HttpPost]
        public IActionResult ListData()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            var prescriptions = _workContext.CurrentCustomer.IsAdmin() || _workContext.CurrentCustomer.IsInCustomerRole("Receptionist") ? _prescriptionService.ListAsNoTracking() : _prescriptionService.ListByDoctorId(_workContext.CurrentCustomer.Id);
            var prescriptionList = prescriptions
                .Select(x => new PrescriptionListModel
                {
                    Id = x.Id,
                    CreationDate = x.CreatedOnUtc.ToLocalTime().ToShortDateString(),
                    CreationTime = x.CreatedOnUtc.ToLocalTime().ToLongTimeString(),
                    Patient = _patientService.GetById(x.PatientId)
                }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = prescriptionList,
                Total = prescriptionList.Count
            };
            return Json(gridModel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PrescriptionsModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            if (model.PatientId == 0)
            {
                ModelState.AddModelError("", "Debes seleccionar un paciente.");
                return View("~/Plugins/Teed.Plugin.Medical/Views/Prescription/Create.cshtml", model);
            }

            Prescription prescription = new Prescription()
            {
                Comment = model.Comments,
                PatientId = model.PatientId,
                DoctorId = _workContext.CurrentCustomer.Id
            };
            _prescriptionService.Insert(prescription);

            return RedirectToAction("Edit", new { id = prescription.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(PrescriptionsModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            if (!ModelState.IsValid) return View(model);

            Prescription prescription = _prescriptionService.GetById(model.Id);
            prescription.Comment = model.Comments;
            prescription.PatientId = model.PatientId;
            _prescriptionService.Update(prescription);

            return RedirectToAction("Edit", new { id = model.Id });
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
        public IActionResult ProductListData()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            var products = _productService.SearchProducts().Select(x => new ProductsListModel
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
            products.Add(new ProductsListModel { Id = 0, Name = "Otro" });
            return Json(products);
        }

        [HttpPost]
        public IActionResult UpdatePrescriptionProduct(PrescriptionItemModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            PrescriptionItem prescriptionItem = _prescriptionItemService.GetById(model.Id);
            if (prescriptionItem == null) return NotFound();

            prescriptionItem.Dosage = model.Dosage;
            _prescriptionItemService.Update(prescriptionItem);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult DeletePrescriptionProduct(PrescriptionItemModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            PrescriptionItem prescriptionItem = _prescriptionItemService.GetById(model.Id);
            if (prescriptionItem == null) return NotFound();

            _prescriptionItemService.Delete(prescriptionItem);

            return new NullJsonResult();
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

        private void GetPatientsList(PrescriptionsModel model)
        {
            model.Patients.Add(new SelectListItem { Text = "Selecciona el paciente....", Value = "0" });
            foreach (var c in _workContext.CurrentCustomer.IsAdmin() || _workContext.CurrentCustomer.IsInCustomerRole("Receptionist") ? _patientService.GetAll() : _patientService.GetAllByDoctorId(_workContext.CurrentCustomer.Id))
            {
                model.Patients.Add(new SelectListItem
                {
                    Text = $"{c.FirstName} {c.LastName}",
                    Value = c.Id.ToString(),
                    Selected = c.Id == model.PatientId
                });
            }
        }
    }
}