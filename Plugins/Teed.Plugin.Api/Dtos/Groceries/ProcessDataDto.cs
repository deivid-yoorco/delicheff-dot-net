using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Groceries
{
    public class ProcessDataDto
    {
        public string ProductName { get; set; }
        public string Price { get; set; }
        public string Category1 { get; set; }
        public string Category2 { get; set; }
        public string Category3 { get; set; }
        public string PictureUrl { get; set; }
        public string ProductUrl { get; set; }
        public string Store { get; set; }
    }
}
