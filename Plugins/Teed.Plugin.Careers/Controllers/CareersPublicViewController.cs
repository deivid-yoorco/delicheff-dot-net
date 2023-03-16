using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Web.Controllers;
using System.IO;
using Teed.Plugin.Careers.Domain;
using Teed.Plugin.Careers.Models;
using Teed.Plugin.Careers.Services;

namespace Teed.Plugin.Careers.Controllers
{
    public class CareersPublicViewController : BasePublicController
    {
        private readonly IPermissionService _permissionService;
        private readonly CareerPostulationService _careersService;
        private readonly CareersSettings _careersSettings;
        private readonly ISettingService _careerSettingConfiguration;

        public CareersPublicViewController(IPermissionService permissionService,
            CareerPostulationService careersService, CareersSettings careersSettings, ISettingService settingService)
        {
            _permissionService = permissionService;
            _careersService = careersService;
            _careersSettings = careersSettings;
            _careerSettingConfiguration = settingService;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Nominated")]
        public IActionResult Nominated()
        {
            return View("~/Plugins/Careers/Views/Nominated.cshtml");
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("careers")]
        public IActionResult JobBoard()
        {
            if (!_careersSettings.Published) return NotFound();
            var model = new CareersPostulationModel()
            {
                Body = _careersSettings.Body
            };
            return View("~/Plugins/Careers/Views/JobBoard.cshtml", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("CareersPublicView/JobBoard")]
        public IActionResult JobBoard(CareersPostulationModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Todos los campos son requeridos");
                return View("~/Plugins/Careers/Views/JobBoard.cshtml", model);
            }

            if (model.CVFile.Length == 0)
            {
                ModelState.AddModelError("", "Debes ingresar un archivo válido");
                return View("~/Plugins/Careers/Views/JobBoard.cshtml", model);
            }

            var stream = new MemoryStream();
            model.CVFile.CopyTo(stream);
            var bytes = stream.ToArray();

            var nominated = new CareerPostulations
            {
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                CVFile = bytes,
                Subject = model.Subject
            };
            _careersService.Insert(nominated);
            return Nominated();
        }
    }
}
