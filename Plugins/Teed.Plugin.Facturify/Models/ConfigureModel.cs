using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Facturify.Domain;

namespace Teed.Plugin.Facturify.Models
{
    public class ConfigureModel
    {
        [Required(ErrorMessage = "Este campo es requerido")]
        public string BusinessName { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        public string RFC { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        public FiscalRegime Regime { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        public string AddressPostalCode { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        public string AddressStreet { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        public string AddressExteriorNumber { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        public string AddressInternalNumber { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        public string AddressSuburb { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        public string AddressMunicipality { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        public string AddressCity { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        public string AddressState { get; set; }
        
        public IFormFile CsdKeyFile { get; set; }
        
        public IFormFile CsdCerFile { get; set; }
        
        public string CsdKeyFilePassword { get; set; }
        
        public IFormFile FielKeyFile { get; set; }
        
        public IFormFile FielCerFile { get; set; }
        
        public string FielKeyFilePassword { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        public string ApiKey { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        public string ApiSecret { get; set; }

        public string FacturifyUuid { get; set; }

        public string CsdKeyFileBase64 { get; set; }

        public string CsdCerFileBase64 { get; set; }

        public string FielKeyFileBase64 { get; set; }

        public string FielCerFileBase64 { get; set; }

        public bool Sandbox { get; set; }

        public IFormFile Image { get; set; }

        public string DaysAllowed { get; set; }
        public int DaysAllowedInt { get; set; }

        public bool AllowBillGeneration { get; set; }
    }
}
