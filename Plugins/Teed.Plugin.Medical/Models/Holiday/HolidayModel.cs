using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Models.Holiday
{
    public class HolidayModel
    {
        public HolidayModel()
        {
            SelectedBranchesIds = new List<int>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Debes seleccionar la fecha.")]
        public DateTime HolidayDate { get; set; }

        [Required(ErrorMessage = "Debes ingresar el nombre del feriado.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Debes seleccionar por lo menos una sucursal.")]
        public IList<int> SelectedBranchesIds { get; set; }
    }
}