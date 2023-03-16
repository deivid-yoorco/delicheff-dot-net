using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using Teed.Plugin.Groceries.Domain.Product;

namespace Teed.Plugin.Groceries.Utils
{
    public static class ProductUtils
    {
        /// <summary>
        /// Returns the quantity in pz or gr
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static decimal ParseProductQuantity(OrderItem item)
        {
            decimal result = item.Quantity;
            if (item.EquivalenceCoefficient > 0 && item.BuyingBySecondary)
            {
                result = (item.Quantity * 1000) / item.EquivalenceCoefficient;
            }
            else if (item.WeightInterval > 0)
            {
                result = item.Quantity * item.WeightInterval;
            }

            return Math.Round(result, 2);
        }

        /// <summary>
        /// Returns the quantity in pz or kg and the unite its in, as a tuple object
        /// </summary>
        /// <param name="item"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public static Tuple<decimal, string> ParseProductExactQuantity(OrderItem item, int quantity)
        {
            var unit = "pz";
            decimal result = quantity;

            if (item.EquivalenceCoefficient > 0)
            {
                result = Math.Round(quantity / item.EquivalenceCoefficient, 2);
            }
            else if (item.WeightInterval > 0)
            {
                result = Math.Round((quantity * item.WeightInterval) / 1000, 2);
            }
            else
                result = Math.Round(result, 0);

            if (item.EquivalenceCoefficient > 0 || item.WeightInterval > 0)
                unit = "kg";

            return new Tuple<decimal, string>(result, unit);
        }

        public static Manufacturer GetMainManufacturer(ICollection<ProductManufacturer> productManufacturers, ProductMainManufacturer productMainManufacturer)
        {
            if (productMainManufacturer != null && productMainManufacturer.ManufacturerId > 0)
            {
                var selectedManufacturer = productManufacturers.Where(x => x.ManufacturerId == productMainManufacturer.ManufacturerId).Select(x => x.Manufacturer).FirstOrDefault();
                if (selectedManufacturer != null) return selectedManufacturer;
            }

            return OrderManufacturers(productManufacturers)
                .Select(z => z.Manufacturer)
                .FirstOrDefault();
        }

        public static int GetMainManufacturerId(ICollection<ProductManufacturer> productManufacturers, ProductMainManufacturer productMainManufacturer)
        {
            if (productMainManufacturer != null && productMainManufacturer.ManufacturerId > 0)
            {
                var selectedManufacturer = productManufacturers.Where(x => x.ManufacturerId == productMainManufacturer.ManufacturerId).Select(x => x.Manufacturer).FirstOrDefault();
                if (selectedManufacturer != null) return selectedManufacturer.Id;
            }

            return OrderManufacturers(productManufacturers)
                .Select(z => z.ManufacturerId)
                .FirstOrDefault();
        }

        public static IOrderedEnumerable<ProductManufacturer> OrderManufacturers(ICollection<ProductManufacturer> productManufacturers, int mainManufacturerId = 0)
        {
            return productManufacturers
                .Where(z => !z.Manufacturer.Deleted)
                .OrderByDescending(z => z.ManufacturerId == mainManufacturerId)
                .ThenByDescending(z => z.Manufacturer.IsPaymentWhithTransfer)
                .ThenByDescending(z => z.Manufacturer.IsIncludeInReportByManufacturer)
                .ThenByDescending(z => z.Id)
                .ThenBy(z => z.Manufacturer.DisplayOrder);
        }
    }
}
