using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Helpers
{
    public static class ProductHelper
    {
        public static decimal GetWeightInterval(Product product)
        {
            if (product.EquivalenceCoefficient > 0)
                return 1000 / product.EquivalenceCoefficient;
            else if (product.WeightInterval > 0)
                return product.WeightInterval;
            else
                return 0;
        }
    }
}
