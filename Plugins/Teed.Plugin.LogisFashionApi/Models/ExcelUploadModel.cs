using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Logisfashion.Models
{
    public class ExcelUploadModel
    {
        public IFormFile ExcelFile { get; set; }

        public bool IsAssortment { get; set; }

        [Required]
        public string OrderNumber { get; set; }
    }
}
