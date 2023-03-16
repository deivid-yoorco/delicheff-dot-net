using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Controllers;

namespace Teed.Plugin.Groceries.Models.OperationalErrors
{
    public class IndexViewModel
    {
        public List<DataResult> BuyerErrors { get; set; }
    }

    public class DataResult
    {
        public string DateTime { get; set; }
        public string ErrorMadeByCustomer { get; set; }
        public string ErrorDescription { get; set; }
    }
}
