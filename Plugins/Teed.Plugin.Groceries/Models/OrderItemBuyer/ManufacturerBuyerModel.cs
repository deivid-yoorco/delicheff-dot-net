using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.OrderItemBuyers;

namespace Teed.Plugin.Groceries.Models.OrderItemBuyer
{
    public class ManufacturerBuyerModel
    {
        public ManufacturerBuyerModel()
        {
            Manufacturers = new List<Manufacturer>();
            Customers = new List<Customer>();
            ManufacturerBuyers = new List<ManufacturerBuyerInfo>();
            BuyersWithoutCards = new List<BuyersWithoutCard>();
            Log = new List<string>();
        }

        public List<Manufacturer> Manufacturers { get; set; }
        public List<Customer> Customers { get; set; }
        public List<ManufacturerBuyerInfo> ManufacturerBuyers { get; set; }
        public List<BuyersWithoutCard> BuyersWithoutCards { get; set; }
        public List<string> Log { get; set; }
    }

    public class ManufacturerBuyerAssignData
    {
        public int ManufacturerId { get; set; }
        public int CustomerId { get; set; }
    }

    public class ManufacturerBuyerInfo
    {
        public ManufacturerBuyer ManufacturerBuyer { get; set; }
        public bool? IsCorporateCard { get; set; }
    }

    public class BuyersWithoutCard
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }
}
