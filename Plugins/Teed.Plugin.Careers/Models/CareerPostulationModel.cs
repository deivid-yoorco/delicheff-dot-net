using Nop.Web.Framework.Mvc.Models;
using Nop.Core;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Teed.Plugin.Careers.Models
{
    public class CareersPostulationModel : BaseNopModel
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public IFormFile CVFile { get; set; }
        public string Subject { get; set; }
        public bool Deleted { get; set; }

        public string Body { get; set; }
    }
}
