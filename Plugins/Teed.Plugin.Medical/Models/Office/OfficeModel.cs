using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Models.Office
{
    public class OfficeModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Este campo es requerido.")]
        public string Name { get; set; }

        public int BranchId { get; set; }
    }
}