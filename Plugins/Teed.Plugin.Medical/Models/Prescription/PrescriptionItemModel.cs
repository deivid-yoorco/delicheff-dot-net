using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Models.Prescription
{
    public class PrescriptionItemModel : BaseNopModel
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public string ItemName { get; set; }
        public string Dosage { get; set; }
        public int PrescriptionId { get; set; }
    }
}