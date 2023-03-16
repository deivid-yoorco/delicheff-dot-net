using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.OrderOptionButtons
{
    public class OrderOptionButtonsModel
    {
        public int OrderId { get; set; }
        public string Date { get; set; }
        public int FranchiseId { get; set; }
        public int VehicleId { get; set; }
    }
}
