using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.Manufacturer
{
    public class ManufacturerBankAccount : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public string Log { get; set; }

        public int ManufacturerId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountOwner { get; set; }
        public string BankName { get; set; }
    }
}
