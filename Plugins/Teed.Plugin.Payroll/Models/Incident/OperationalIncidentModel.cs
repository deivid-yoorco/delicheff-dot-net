using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Controllers;
using Teed.Plugin.Payroll.Domain.Incidents;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;

namespace Teed.Plugin.Payroll.Models.Incident
{
    public class OperationalIncidentListModel
    {
        public OperationalIncidentListModel()
        {
            ResponsibleAreaTypes = new List<SelectListItem>();
            SolutionTypes = new List<SelectListItem>();
            FranchiseInfos = new List<FranchiseInfo>();
            RouteInfos = new List<RouteInfo>();
            Biweeks = new List<SelectListItem>();
            Employees = new List<SelectListItem>();
        }

        public OperationalIncidentType OperationalIncidentType { get; set; }
        public virtual IList<SelectListItem> ResponsibleAreaTypes { get; set; }
        public virtual IList<SelectListItem> SolutionTypes { get; set; }
        public List<FranchiseInfo> FranchiseInfos { get; set; }
        public List<RouteInfo> RouteInfos { get; set; }
        public virtual IList<SelectListItem> Biweeks { get; set; }
        public virtual IList<SelectListItem> Employees { get; set; }
    }

    public class OperationalIncidentModel
    {
        public virtual int Id { get; set; }
        public virtual int OrderId { get; set; }
        public virtual string OrderDeliveryDate { get; set; }
        public virtual int OrderDeliveryRouteId { get; set; }
        public virtual int OrderDeliveryFranchiseId { get; set; }
        public virtual int OrderDeliveryRescuedRouteId { get; set; }
        public virtual int OrderDeliveryRescuedFranchiseId { get; set; }

        public virtual string ReportDate { get; set; }
        public virtual int ResponsibleArea1 { get; set; }
        public virtual int ResponsibleArea2 { get; set; }
        public virtual int ResponsibleCustomerId1 { get; set; }
        public virtual int ResponsibleCustomerId2 { get; set; }

        public virtual decimal IncidentAmount { get; set; }
        public virtual string IncidentDetails { get; set; }
        public virtual bool Authorized { get; set; }
        public virtual string AppliedInBiweek { get; set; }
        public virtual int SolutionTypeId { get; set; }
        public virtual decimal SolutionAmount { get; set; }
        public virtual string Comments { get; set; }

        public virtual string Log { get; set; }
    }
}
