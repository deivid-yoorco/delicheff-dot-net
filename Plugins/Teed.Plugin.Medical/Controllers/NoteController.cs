using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Services;
using Teed.Plugin.Medical.Domain;
using Teed.Plugin.Medical.Domain.Note;
using Teed.Plugin.Medical.Models.Note;
using Teed.Plugin.Medical.Security;

namespace Teed.Plugin.Medical.Controllers
{


    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class NoteController : BasePluginController
    {
        PatientService _patientService;
        NoteService _noteService;
        PrescriptionItemService _prescriptionItemService;
        private readonly IWorkContext _workContext;
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;

        public NoteController(PatientService patientService,
            NoteService noteService,
            IWorkContext workContext,
            IPermissionService permissionService,
            IProductService productService,
            PrescriptionItemService prescriptionItemService)
        {
            _patientService = patientService;
            _workContext = workContext;
            _permissionService = permissionService;
            _noteService = noteService;
            _prescriptionItemService = prescriptionItemService;
            _productService = productService;
        }

        public IActionResult Create(int id = 0)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            ViewData["SelectedUser"] = id;
            ViewData["IsAdmin"] = _workContext.CurrentCustomer.IsAdmin();
            return View("~/Plugins/Teed.Plugin.Medical/Views/Note/Create.cshtml");
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Note note = _noteService.GetById(id);
            if (note == null) return NotFound();

            NotesModel model = new NotesModel()
            {
                Id = note.Id,
                Text = note.Text,
                PatientId = note.PatientId                
            };

            ViewData["SelectedUser"] = note.PatientId;
            return View("~/Plugins/Teed.Plugin.Medical/Views/Note/Edit.cshtml", model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(NotesModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            if (model.PatientId == 0)
            {
                ModelState.AddModelError("", "Debes seleccionar un paciente.");
                return View("~/Plugins/Teed.Plugin.Medical/Views/Note/Create.cshtml", model);
            }

            Note note = new Note()
            {
                Text = model.Text,
                PatientId = model.PatientId                                
            };
            _noteService.Insert(note);

            return RedirectToAction("Details", "Patient", new { id = model.PatientId} );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(NotesModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            if (!ModelState.IsValid) return View(model);

            Note prescription = _noteService.GetById(model.Id);
            prescription.Text = model.Text;
            prescription.PatientId = model.PatientId;
            _noteService.Update(prescription);

            return RedirectToAction("Edit", new { id = model.Id });
        }

        public IActionResult Details(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedMedical))
                return AccessDeniedView();

            Note note = _noteService.GetById(id);
            if (note == null) return NotFound();

            Patient patient = _patientService.GetById(note.PatientId);
            if (patient == null) return NotFound();

            NotesDetailsModel model = new NotesDetailsModel()
            {
                Id = note.Id,
                Text = note.Text,
                CreationDate = note.CreatedOnUtc.ToLocalTime().ToShortDateString(),
                CreationTime = note.CreatedOnUtc.ToLocalTime().ToLongTimeString(),
                PatientFullName = patient.FirstName + " " + patient.LastName,
                PatientId = patient.Id                
            };
            return View("~/Plugins/Teed.Plugin.Medical/Views/Note/Details.cshtml", model);
        }

        
    }
}
