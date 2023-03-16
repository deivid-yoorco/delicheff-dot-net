using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Domain.Prescription
{
    public class PrescriptionItem : BaseEntity
    {
        public virtual Guid GuidId { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public int? ProductId { get; set; }
        public int PrescriptionId { get; set; }
        public string ItemName { get; set; }
        public string Dosage { get; set; }
    }
}
