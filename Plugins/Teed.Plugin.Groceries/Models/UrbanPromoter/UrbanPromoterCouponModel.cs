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
    public class UrbanPromoterCouponModel
    {
        public virtual int Id { get; set; }
        public virtual int UrbanPromoterId { get; set; }
        public virtual int DiscountId { get; set; }
        public virtual decimal Fee { get; set; }
    }
}
