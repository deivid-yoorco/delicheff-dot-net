using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Nop.Web.Models.ShoppingCart.ShoppingCartModel;

namespace Teed.Plugin.ShoppingCartUrlGenerator.Models
{
    public class ShoppingCartUrlPublicViewModel
    {
        public int ShoppingCartUrlId { get; set; }
        public string Body { get; set; }
        public List<ShoppingCartItemModel> Products { get; set; }
        public List<SelectedProduct> SelectedProducts { get; set; }
    }

    public class SelectedProduct
    {
        public bool Selected { get; set; }
        public int ProductId { get; set; }
        public int SelectedQuantity { get; set; }
    }
}
