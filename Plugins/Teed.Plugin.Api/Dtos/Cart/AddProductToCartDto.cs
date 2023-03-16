using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Cart
{
    public class UpdateShoppingCartDto
    {
        public int ProductId { get; set; }

        public bool BuyingBySecondary { get; set; }

        public string SelectedPropertyOption { get; set; }

        public int NewQuantity { get; set; }

        public int[] ProductAttributeIds { get; set; }
    }
}
