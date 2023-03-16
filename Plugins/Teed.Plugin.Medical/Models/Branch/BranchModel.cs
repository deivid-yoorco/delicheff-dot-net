using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Models.Branch
{
    public class BranchModel
    {
        public BranchModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            SelectedUsersIds = new List<int>();
        }
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la sucursal es requerido.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Debes seleccionar al encargado de la sucursal.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido.")]
        public string Phone { get; set; }

        public string Phone2 { get; set; }

        [Required(ErrorMessage = "La dirección es requerida.")]
        public string StreetAddress { get; set; }

        public string StreetAddress2 { get; set; }

        [Required(ErrorMessage = "El municipio o delegación es requerido.")]
        public string City { get; set; }

        [Required(ErrorMessage = "Debes seleccionar el país.")]
        public int CountryId { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }

        [Required(ErrorMessage = "Debes seleccionar el estado.")]
        public int StateProvinceId { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }

         public IList<int> SelectedUsersIds { get; set; }

        [Required(ErrorMessage = "El código postal es requerido.")]
        public string ZipPostalCode { get; set; }

        public int? WeekOpenHour { get; set; }

        public int? WeekOpenMinute { get; set; }

        public int? WeekCloseHour { get; set; }

        public int? WeekCloseMinute { get; set; }

        public int? SaturdayOpenHour { get; set; }

        public int? SaturdayCloseHour { get; set; }

        public int? SaturdayOpenMinute { get; set; }

        public int? SaturdayCloseMinute { get; set; }

        public int? SundayOpenHour { get; set; }

        public int? SundayCloseHour { get; set; }

        public int? SundayOpenMinute { get; set; }

        public int? SundayCloseMinute { get; set; }

        public bool WorksOnSaturday { get; set; }
        public bool WorksOnSunday { get; set; }
        public string Algo { get; set; }
       
    }
}