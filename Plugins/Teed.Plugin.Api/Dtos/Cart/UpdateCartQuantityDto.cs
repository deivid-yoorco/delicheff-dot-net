using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Cart
{
    public class UpdateCartQuantityDto
    {
        public int ProductId { get; set; }

        public string CustomerId { get; set; }

        public int[] ProductAttributeIds { get; set; }
    }
}
