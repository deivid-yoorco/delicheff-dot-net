using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain;
using Teed.Plugin.Medical.Models.Patients;
using Teed.Plugin.Medical.Security;
using Teed.Plugin.Medical.Services;

namespace Teed.Plugin.Medical.Controllers
{
    [Area(AreaNames.Admin)]
    public class UpdatePatientController : BasePluginController
    {
        private readonly PatientService _patientService;      
        private readonly ICountryService _countryService;       
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;


        public UpdatePatientController(PatientService patientService,
             IPermissionService permissionService,
            ICountryService countryService,
            ILocalizationService localizationService,
            IStateProvinceService stateProvinceService
            )
        {
            _patientService = patientService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _permissionService = permissionService;
            _localizationService = localizationService;

        }


        public IActionResult Update(int id)
        {
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
                Commentary = patient.Commentary,
                ActiveUpdate = patient.UpdatePageActive
            };

            if (!patient.UpdatePageActive)
            {
                ModelState.AddModelError("", "El enlace proporcionado no es válido");
                return View("~/Plugins/Teed.Plugin.Medical/Views/UpdatePatient/Update.cshtml", model);
            }

            PreparePatientModel(model);
            return View("~/Plugins/Teed.Plugin.Medical/Views/UpdatePatient/Update.cshtml", model);
        }

        [HttpPost]        
        public IActionResult Update(PatientsModel model)
        {
            Patient patient = _patientService.GetById(model.Id);
            if (model.Acepted)
            {
                
                if (patient == null) return BadRequest();

                patient.FirstName = model.FirstName;
                patient.LastName = model.LastName;
                patient.Email = model.Email;
                patient.DateOfBirth = new DateTime(model.DateOfBirthYear, model.DateOfBirthMonth, model.DateOfBirthDay);
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
                patient.UpdatePageActive = false;


                _patientService.Update(patient);

                return View("~/Plugins/Teed.Plugin.Medical/Views/UpdatePatient/DoneUpdate.cshtml", model);
            }
            else
            {
                
                model.ActiveUpdate = true;
                PreparePatientModel(model);
                ModelState.AddModelError("", "Debes aceptar el aviso de privacidad");
                return View("~/Plugins/Teed.Plugin.Medical/Views/UpdatePatient/Update.cshtml", model);
            }

           
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
    }
}
