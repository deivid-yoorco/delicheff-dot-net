using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.CancelOrderButtonModel
{
    public class CancelOrderButtonModel
    {
        public int OrderId { get; set; }
        public bool CancelButtonEnable { get; set; }
        public int StatusOrder { get; set; }
    }
}
