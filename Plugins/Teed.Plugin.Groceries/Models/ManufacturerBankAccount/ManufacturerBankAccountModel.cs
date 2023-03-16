using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.ManufacturerBankAccount
{
    public class ManufacturerBankAccountModel
    {
        public int Id { get; set; }
        public int ManufacturerId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountOwner { get; set; }
        public string BankName { get; set; }
    }
}
