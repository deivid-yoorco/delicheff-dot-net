using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Models.Prescription
{
    public class PrescriptionsDetailsModel : BaseNopModel
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientFullName { get; set; }
        public string Comment { get; set; }
        public string CreationDate { get; set; }
        public string CreationTime { get; set; }
        public List<ProductsInPrescription> Products { get; set; }
    }

    public class ProductsInPrescription
    {
        public string Name { get; set; }
        public string Dosage { get; set; }
    }
}