using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Models.File
{
    public class FileModel
    {
        public int PatientId { get; set; }
        public string Description { get; set; }

        [Required(ErrorMessage = "Debes seleccionar un archivo")]
        public IFormFile File { get; set; }
    }
}
