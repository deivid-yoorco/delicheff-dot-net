using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Manager.Models.PurchaseOrder
{
    public class CreatePurchaseOrderProductModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public int Units { get; set; }
    }
}
