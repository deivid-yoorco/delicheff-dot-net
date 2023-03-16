using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Manager.Domain.Expenses;

namespace Teed.Plugin.Manager.Models.Expenses
{
    public class CreateViewModel
    {
        public CreateViewModel()
        {
            Files = new List<IFormFile>();
        }

        public virtual string SelectedDate { get; set; }
        public virtual int CreatedByUserId { get; set; }
        public virtual string Concept { get; set; }
        public virtual string Comments { get; set; }
        public virtual int CategoryId { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual decimal Tax { get; set; }
        public virtual decimal Total { get; set; }
        public virtual VoucherType VoucherType { get; set; }
        public virtual PaymentType PaymentType { get; set; }

        public IList<IFormFile> Files { get; set; }
    }
}
