using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Models.Prescription
{
    public class PrescriptionsModel : BaseNopModel
    {
        public PrescriptionsModel()
        {
            Patients = new List<SelectListItem>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Debes seleccionar un paciente válido")]
        public int PatientId { get; set; }

        public string Comments { get; set; }
        public IList<SelectListItem> Patients { get; set; }

        public int? ProductId { get; set; }
        public string ItemName { get; set; }
        public string Dosage { get; set; }
        public int PrescriptionId { get; set; }

        public int ItemsCount { get; set; }
    }
}