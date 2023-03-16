using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Dtos
{
    public class ShippingDateDto
    {
        public DateTime Date { get; set; }
        public string ShippingTime { get; set; }
        public bool IsActive { get; set; }
        public bool Disabled { get; set; }
    }
}
