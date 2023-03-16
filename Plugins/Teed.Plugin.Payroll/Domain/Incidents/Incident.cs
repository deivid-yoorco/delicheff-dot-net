using Nop.Core;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;

namespace Teed.Plugin.Payroll.Domain.Incidents
{
    public class Incident : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual IncidentType Type { get; set; }
        public virtual DateTime? Date { get; set; }
        public virtual bool Justified { get; set; }
        public virtual decimal? Amount { get; set; }
        public virtual string Reason { get; set; }
        public virtual int? Term { get; set; }
        public virtual decimal TermPaid { get; set; }
        public virtual DateTime? AppliedDate { get; set; }

        public virtual int? FileSize { get; set; }
        public virtual string FileExtension { get; set; }
        public virtual string FileMimeType { get; set; }
        public virtual byte[] File { get; set; }
        public virtual string FileTitle { get; set; }
        public virtual string FileDescription { get; set; }

        public virtual int PayrollEmployeeId { get; set; }
        public virtual PayrollEmployee PayrollEmployee { get; set; }

        public virtual string Log { get; set; }
    }
}
