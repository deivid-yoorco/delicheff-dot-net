using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Controllers;

namespace Teed.Plugin.Groceries.Models.Warnings
{
    public class IndexViewModel
    {
        public IndexViewModel()
        {
            CostsIncreaseWarnings = new List<CostsIncreaseWarningModel>();
            CostsDecreaseWarnings = new List<CostsDecreaseWarningModel>();
        }

        public List<ProductQueryResult> ProductsWithoutParentCategory { get; set; }
        public List<ProductQueryResult> ProductsWithoutChildCategory { get; set; }
        public List<ProductQueryResult> ProductsWithoutEarnings { get; set; }
        public List<ProductQueryResult> ProductsWithWrongChildCategory { get; set; }
        public List<ProductQueryResult> ProductsWithMoreThanOneParentCategory { get; set; }
        public List<ProductQueryResult> ProductsWithMoreThanOneChildCategory { get; set; }
        public List<ProductQueryResult> ProductsWithoutImage { get; set; }
        public List<ProductQueryResult> ProductsWithLowMargin { get; set; }
        public List<ProductQueryResult> ProductsWithLowMargin13 { get; set; }
        public List<ProductQueryResult> productsSkuRepeatedList { get; set; }

        public List<ProductWithoutManufacturer> ProductsWithoutManufacturer { get; set; }

        public List<CostsIncreaseWarningModel> CostsIncreaseWarnings { get; set; }
        public List<CostsDecreaseWarningModel> CostsDecreaseWarnings { get; set; }
    }
}
