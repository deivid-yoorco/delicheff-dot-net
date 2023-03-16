using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Dtos
{
    public class TipWithCardDto
    {
    }

    public class ReportTipWithCardDto
    {
        public int OrderId { get; set; }
        public decimal ReportedAmount { get; set; }
    }
}
