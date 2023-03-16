using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.ShippingRoutes
{
    public class OrderOptimizationRequest : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public int OrderId { get; set; }
        public string OriginalSelectedTime { get; set; }
        public int SelectedOptimizationTypeId { get; set; }
        public string TimeOption1 { get; set; }
        public string TimeOption2 { get; set; }
        public string TimeOption3 { get; set; }
        public string FinalTimeSelected { get; set; }
        public int CurrentStatusId { get; set; }
        public string Comments { get; set; }
        public string ManagerComment { get; set; }
    }
}