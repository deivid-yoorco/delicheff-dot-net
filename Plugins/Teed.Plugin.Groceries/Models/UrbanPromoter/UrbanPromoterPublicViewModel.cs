using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Controllers;

namespace Teed.Plugin.Groceries.Models.UrbanPromoter
{
    public class UrbanPromoterPublicViewModel
    {
        public UrbanPromoterPublicViewModel()
        {
            DiscountsWithPromoter = new List<string>();
            OrdersWithPromoter = new List<OrdersWithPromoter>();
        }

        public virtual string CustomerName { get; set; }
        public virtual decimal FeesTotal { get; set; }
        public virtual decimal PaymentsTotal { get; set; }
        public virtual decimal PendingTotal { get; set; }

        public virtual string AccountBankName { get; set; }
        public virtual string AccountOwnerName { get; set; }
        public virtual string AccountAddress { get; set; }
        public virtual string AccountNumber { get; set; }
        public virtual string AccountCLABE { get; set; }
        public virtual bool CashPayment { get; set; }

        public virtual List<string> DiscountsWithPromoter { get; set; }
        public virtual List<OrdersWithPromoter> OrdersWithPromoter { get; set; }
    }

    public class OrdersWithPromoter
    {
        public virtual DateTime Date { get; set; }
        public virtual string Client { get; set; }
        public virtual string Status { get; set; }
        public virtual string UsedCoupon { get; set; }
        public virtual decimal Comission { get; set; }
    }
}
