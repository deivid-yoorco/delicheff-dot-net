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
    public class UrbanPromoterModel
    {
        public virtual int Id { get; set; }
        public virtual int CustomerId { get; set; }
        public virtual string AccountBankName { get; set; }
        public virtual string AccountOwnerName { get; set; }
        public virtual string AccountAddress { get; set; }
        public virtual string AccountNumber { get; set; }
        public virtual string AccountCLABE { get; set; }
        public virtual bool CashPayment { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual string Log { get; set; }
    }

    public partial class AddPayment
    {
        public virtual DateTime Date { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual IFormFile File { get; set; }
        public virtual byte[] FileArray { get; set; }
        public virtual int UrbanPromoterId { get; set; }
    }
}
