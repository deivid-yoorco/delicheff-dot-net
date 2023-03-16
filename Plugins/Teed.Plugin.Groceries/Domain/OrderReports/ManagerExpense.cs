﻿using Nop.Core;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.OrderReports
{
    public class ManagerExpense : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int UserId { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual string Concept { get; set; }
        public virtual DateTime ShippingDate { get; set; }
        public virtual string Log { get; set; }
    }
}
