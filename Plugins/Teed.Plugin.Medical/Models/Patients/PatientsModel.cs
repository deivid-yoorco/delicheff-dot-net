using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Medical.Models.Patients
{
    public class PatientsModel : BaseNopModel
    {
        public PatientsModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
        }

        public int Id { get; set; }

        
        public string Email { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es requerido.")]
        public string LastName { get; set; }
        
        public string Gender { get; set; }

        [Required(ErrorMessage = "Debes ingresar al menos un número telefónico")]
        public string Phone1 { get; set; }

        public string Phone2 { get; set; }

        public string Phone3 { get; set; }

        public int DateOfBirthDay { get; set; }
        
        public int DateOfBirthMonth { get; set; }
        
        public int DateOfBirthYear { get; set; }

        public DateTime? DateOfBirth
        {
            get
            {
                return DateOfBirthYear == 0 ||
                       DateOfBirthMonth == 0 ||
                       DateOfBirthDay == 0 ? null : new DateTime?(new DateTime(DateOfBirthYear, DateOfBirthMonth, DateOfBirthDay));
            }
        }

        
        public string StreetAddress { get; set; }

        public string StreetAddress2 { get; set; }

        
        public string City { get; set; }

        
        public int CountryId { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }

        
        public int StateProvinceId { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }

        
        public string ZipPostalCode { get; set; }

        public string PersonalPathologicalHistory { get; set; }

        public string PersonalNonPathologicalHistory { get; set; }

        public string CurrentConditions { get; set; }

        public string FamilyBackground { get; set; }

        public string Allergies { get; set; }

        public bool IsExistingUser { get; set; }

        public DateTime LastUpdateDate { get; set; }

        public int ReferdBy { get; set; }

        public string ReferedByExternal { get; set; }

        public string ReferedByUser { get; set; }

        public string Commentary { get; set; }

        public string Note { get; set; }

        public string UrlActive { get; set; }

        public bool ActiveUpdate { get; set; }

        public bool Acepted { get; set; }
    }
}