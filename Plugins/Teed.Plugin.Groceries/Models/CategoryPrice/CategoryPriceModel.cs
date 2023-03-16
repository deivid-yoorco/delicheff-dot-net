using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.CategoryPrice
{
    public class CategoryPriceModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Coefficient { get; set; }
        public int ParentCategoryId { get; set; }
        public decimal PercentageOfUtility { get; set; }
    }
}
