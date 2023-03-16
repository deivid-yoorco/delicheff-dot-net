using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.OrderDeliveryReports
{
    public class CreateOrderReportsViewModel
    {
        public virtual DateTime DeliveryDate { get; set; }
        public virtual IList<Groups> GroupsDistinc { get; set; }
        public virtual string JsonData { get; set; }

        public virtual string ExpenseJsonData { get; set; }
        public virtual decimal ExpenseAmount { get; set; }
        public virtual string ExpenseDescription { get; set; }
    }

    public class Groups
    {
        public virtual int GroupProductId { get; set; }
        public virtual string GroupProductName { get; set; }
        public virtual IList<ProductsInGroup> Products { get; set; }
    }

    public class ProductsInGroup
    {
        public virtual int OrderId { get; set; }
        public virtual int OrderItemId { get; set; }
        public virtual string OrderItemName { get; set; }
        public virtual decimal ItemCostKgPz { get; set; }

        public virtual decimal OrderItemCost { get; set; }
        public virtual string OrderItemSpecifications { get; set; }

        public virtual decimal OrderItemQuantity { get; set; }
        public virtual string OrderItemQuantitySpecific { get; set; }

        public virtual decimal RequestedQtyCost { get; set; }
        public virtual string ShoppingStoreId { get; set; }
        //public virtual decimal ItemKgPzStoreCost { get; set; }

        public virtual string OrderItemComments { get; set; }
    }
}