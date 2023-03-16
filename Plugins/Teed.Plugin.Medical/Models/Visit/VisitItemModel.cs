using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Models.Visit
{
    public class VisitItemModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ItemName { get; set; }
    }
}
