using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.HeatMap
{
    public class HeatMapModel
    {
        public List<HeatMapData> Data { get; set; }
    }

    public class HeatMapData
    {
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string PostalCode { get; set; }
        public decimal Weight { get; set; }
    }
}
