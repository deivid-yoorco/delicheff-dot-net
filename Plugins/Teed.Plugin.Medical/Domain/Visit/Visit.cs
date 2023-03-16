using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Domain.Visit
{
    public class Visit : BaseEntity
    {
        public virtual Guid GuidId { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int PatientId { get; set; }
        public virtual int DoctorId { get; set; }
        public virtual int BranchId { get; set; }
        public virtual int AppointmentId { get; set; }
        public virtual DateTime VisitDate { get; set; }
        public virtual DateTime LastUpdate { get; set; }
        public virtual string CurrentCondition { get; set; }
        public virtual string Evolution { get; set; }
        public virtual string PreviousTx { get; set; }
        public virtual string ImportantRecord { get; set; }
        public virtual string Diagnosis { get; set; }
        public virtual string Prognostic { get; set; }
        public virtual string Comment { get; set; }
        public virtual decimal Price { get; set; }
        public virtual string Treatment { get; set; }
    }
}