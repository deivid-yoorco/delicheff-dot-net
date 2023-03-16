using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Logisfashion.Models
{
    public class ConfigureModel
    {
        public bool Sandbox { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        public int ClientCode { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        public string ApiKey { get; set; }
    }
}
