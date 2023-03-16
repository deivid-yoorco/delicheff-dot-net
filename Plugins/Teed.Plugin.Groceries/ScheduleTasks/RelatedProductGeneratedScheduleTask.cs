using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Services.Tasks;
using Nop.Web.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Teed.Plugin.Groceries.Services;

namespace Teed.Plugin.Groceries.ScheduleTasks
{
    public class RelatedProductGeneratedScheduleTask : IScheduleTask
    {
        private readonly RelatedProductGeneratedService _relatedProductService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        public RelatedProductGeneratedScheduleTask(IProductService productService,
            RelatedProductGeneratedService relatedProductService,
            IOrderService orderService)
        {
            _relatedProductService = relatedProductService;
            _productService = productService;
            _orderService = orderService;
        }

        public void Execute()
        {
            int controlId = _relatedProductService.GetAll()
                .Select(x => x.UpdateControlId)
                .OrderByDescending(x => x)
                .FirstOrDefault();

            if (controlId == 0) controlId = 1;

            List<int> processedProductIds = _relatedProductService.GetAll()
                .Where(x => x.UpdateControlId == controlId)
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();
            var orderItems = OrderUtils.GetFilteredOrders(_orderService)
                .SelectMany(x => x.OrderItems);
            var soldProductIds = orderItems
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();
            var products = _productService.GetAllProductsQuery()
                .Where(x => x.Published)
                .Select(x => new { x.Id })
                .ToList();
            List<int> neverSoldProductIds = products
                .Where(x => !soldProductIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToList();

            var filteredProducts = products
                .Where(x => !neverSoldProductIds.Contains(x.Id) && !processedProductIds.Contains(x.Id))
                .ToList();

            if (filteredProducts.Count == 0)
            {
                filteredProducts = products.Where(x => !neverSoldProductIds.Contains(x.Id)).ToList();
                controlId++;
            }

            filteredProducts = filteredProducts.Take(200).ToList();
            var filteredProductIds = filteredProducts.Select(x => x.Id).ToList();
            var allOrders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.OrderItems.Where(y => filteredProductIds.Contains(y.ProductId)).Any())
                .Select(x => new
                {
                    OrderId = x.Id,
                    OrderItems = x.OrderItems.Select(y => new { y.ProductId })
                })
                .ToList();

            foreach (var product in filteredProducts)
            {
                var filteredItems = allOrders
                    .Where(x => x.OrderItems.Where(y => y.ProductId == product.Id).Any())
                    .SelectMany(x => x.OrderItems)
                    .Where(x => x.ProductId != product.Id)
                    .GroupBy(x => x.ProductId)
                    .Select(x => new { x.Key, Count = x.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(20)
                    .ToList();
                foreach (var item in filteredItems)
                {
                    _relatedProductService.Insert(new Domain.Product.RelatedProductGenerated()
                    {
                        ProductId = product.Id,
                        RelatedProductId = item.Key,
                        UpdateControlId = controlId,
                        OrderRelationCount = item.Count
                    });
                }
            }
        }
    }
}
