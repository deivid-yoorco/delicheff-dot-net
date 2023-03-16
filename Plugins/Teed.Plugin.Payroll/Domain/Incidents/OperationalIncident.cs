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
    public class OperationalIncident : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int OrderId { get; set; }
        public virtual DateTime? OrderDeliveryDate { get; set; }
        public virtual int OrderDeliveryRouteId { get; set; }
        public virtual int OrderDeliveryFranchiseId { get; set; }
        public virtual int OrderDeliveryRescuedRouteId { get; set; }
        public virtual int OrderDeliveryRescuedFranchiseId { get; set; }

        public virtual DateTime ReportDate { get; set; }
        public virtual int ResponsibleArea1 { get; set; }
        public virtual int ResponsibleArea2 { get; set; }
        public virtual int ResponsibleCustomerId1 { get; set; }
        public virtual int ResponsibleCustomerId2 { get; set; }

        public virtual decimal IncidentAmount { get; set; }
        public virtual string IncidentDetails { get; set; }
        public virtual bool Authorized { get; set; }
        public virtual DateTime? AppliedInBiweek { get; set; }
        public virtual int SolutionTypeId { get; set; }
        public virtual decimal SolutionAmount { get; set; }
        public virtual string Comments { get; set; }

        public virtual int OperationalIncidentTypeId { get; set; }

        public virtual string Log { get; set; }
    }
}
