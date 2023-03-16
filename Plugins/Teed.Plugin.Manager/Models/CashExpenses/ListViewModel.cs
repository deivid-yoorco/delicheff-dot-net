using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Manager.Models.CashExpenses
{
    public class ListViewModel
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public DateTime DateObject { get; set; }
        public CustomerModel User { get; set; }
        public string Concept { get; set; }
        public decimal Charge { get; set; }
        public decimal Deposit { get; set; }
        public int AttachmentsCount { get; set; }
        public List<string> Attachments { get; set; }
        public decimal Balance { get; set; }
        public int OrderId { get; set; }
        public string Type { get; set; }
    }
}
