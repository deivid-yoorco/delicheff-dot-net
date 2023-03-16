using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.Incidents;
using Teed.Plugin.Payroll.Domain.JobCatalogs;

namespace Teed.Plugin.Payroll.Domain.Assistances
{
    public class Assistance : BaseEntity
    {
        public virtual string BadgeNumber { get; set; }
        public virtual string Area { get; set; }
        public virtual DateTime CheckIn { get; set; }
        public virtual int Type { get; set; }
        public virtual string Name { get; set; }
    }
}
