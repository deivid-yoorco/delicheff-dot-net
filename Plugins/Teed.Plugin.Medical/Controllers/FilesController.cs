using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Patients;
using Teed.Plugin.Medical.Models.File;
using Teed.Plugin.Medical.Models.Patients;
using Teed.Plugin.Medical.Services;

namespace Teed.Plugin.Medical.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class FilesController : BasePluginController
    {
        private readonly PatientService _patientService;
        private readonly PatientFileService _patientFileService;
        private readonly IWorkContext _workContext;

        public FilesController(PatientService patientService,
            PatientFileService patientFileService,
            IWorkContext workContext)
        {
            _patientService = patientService;
            _workContext = workContext;
            _patientFileService = patientFileService;
        }

        public IActionResult Create(int pid)
        {
            ViewData["Patient"] = _patientService.GetById(pid);
            return View("~/Plugins/Teed.Plugin.Medical/Views/File/Create.cshtml");
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(FileModel model, bool continueEditing)
        {
            string baseDirectoryName = "patients-media";
            string directoryPath = $"./wwwroot/{baseDirectoryName}/{model.PatientId}";
            Directory.CreateDirectory(directoryPath);

            string rawPath = $"{directoryPath}/{model.File.FileName}";
            string newFileName = DateTime.Now.ToString($"{model.PatientId}-ddMMyyyy-HHmmss") + Path.GetExtension(rawPath);
            string fullPath = $"{directoryPath}/{newFileName}";
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }

            PatientFile patientFile = new PatientFile()
            {
                Extension = Path.GetExtension(rawPath),
                FileType = model.File.ContentType,
                Size = model.File.Length,
                PatientId = model.PatientId,
                FileUrl = $"/{baseDirectoryName}/{model.PatientId}/{newFileName}",
                UploadedDateUtc = DateTime.UtcNow,
                UploadedByUserId = _workContext.CurrentCustomer.Id
            };
            _patientFileService.Insert(patientFile);

            if (continueEditing)
            {
                return RedirectToAction("Create", new { pid = model.PatientId });
            }
            return RedirectToAction("Details", "Patient", new { id = model.PatientId });
        }
    }
}
