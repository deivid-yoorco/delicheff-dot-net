using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using Teed.Plugin.Groceries.Domain.Corcel;
using Teed.Plugin.Groceries.Domain.Product;
using Teed.Plugin.Groceries.Services;

namespace Teed.Plugin.Groceries.Utils
{
    public static class CorcelUtils
    {
        public static List<CorcelRuleType> CorcelAppliesToCustomer(Customer customer, CorcelProductService _corcelProductService,
            CorcelCustomerService _corcelCustomerService, IOrderService _orderService)
        {
            var listEnum = new List<CorcelRuleType>();
            var corcelProducts = _corcelProductService.GetAll().ToList();
            var corcelProductIds = corcelProducts.Select(x => x.ProductId).ToList();

            //if (!corcelCustomerIds.Contains(customer.Id))
            //    return null;

            var ordersOfCustomerQuery = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.CustomerId == customer.Id);
            var orderIdsOfCustomerQuery = ordersOfCustomerQuery.Select(x => x.Id).ToList();
            var orderIdsWithCorcelProducts = _orderService.GetAllOrderItemsQuery()
                .Where(x => orderIdsOfCustomerQuery.Contains(x.OrderId) && corcelProductIds.Contains(x.ProductId))
                .Select(x => x.OrderId).Distinct().ToList();
            var ordersWeek = ordersOfCustomerQuery
                .Where(x => orderIdsWithCorcelProducts.Contains(x.Id))
                .ToList();
            var ordersOfCustomerByMonth = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.CustomerId == customer.Id && orderIdsWithCorcelProducts.Contains(x.Id))
                .Select(x => new { x.Id, x.SelectedShippingDate, x.OrderItems, x.CustomerId, x.ShippingAddress })
                .ToList()
                .GroupBy(x => x.SelectedShippingDate.Value.ToString("MM/yyyy"))
                .ToList();

            var piecesCount = 0;
            var quantityCount = 0;
            foreach (var orders in ordersOfCustomerByMonth)
            {
                foreach (var innerOrder in orders)
                {
                    var productsWithCorcel = innerOrder.OrderItems.Where(x => corcelProductIds.Contains(x.ProductId)).ToList();

                    // Que compre productos de repostería y panadería de más de 1 kg (se toma de equivalencia manual).
                    if (!listEnum.Contains(CorcelRuleType.Rule1))
                    {
                        foreach (var item in productsWithCorcel)
                        {
                            var productForCompare = corcelProducts.Where(x => x.ProductId == item.ProductId).FirstOrDefault();
                            if (productForCompare != null)
                            {
                                if (productForCompare.Quantity < item.Quantity)
                                {
                                    listEnum.Add(CorcelRuleType.Rule1);
                                    break;
                                }
                            }
                        }
                    }

                    if (ordersOfCustomerByMonth.Count() > 1)
                    {
                        // Que en un mismo pedido compre 3 o más piezas de un mismo producto de Repostería y panadería, al menos 1 vez al mes.
                        if (!listEnum.Contains(CorcelRuleType.Rule2))
                        {
                            var itemsWithMoreThan3 = productsWithCorcel.Where(x => x.Quantity >= 3).ToList();
                            if (itemsWithMoreThan3.Any())
                            {
                                piecesCount++;
                            }
                        }

                        // Que en un mismo pedido compre 5 o más productos diferentes de la categoría de Repostería y panadería, al menos 1 vez al mes.
                        if (!listEnum.Contains(CorcelRuleType.Rule3))
                        {
                            if (productsWithCorcel.Count() >= 5)
                            {
                                quantityCount++;
                            }
                        }
                    }

                    // Que la dirección contenga la palabra "local", "panadería", "repostería", "cafetería".
                    if (!listEnum.Contains(CorcelRuleType.Rule4))
                    {
                        var address = $"{innerOrder.ShippingAddress.Address1}, {innerOrder.ShippingAddress.Address2}".ToLower().Trim();
                        if (address.Contains("local") || address.Contains("panadería") || address.Contains("repostería") || address.Contains("cafetería") ||
                            address.Contains("panaderia") || address.Contains("reposteria") || address.Contains("cafeteria"))
                            listEnum.Add(CorcelRuleType.Rule4);
                    }
                }

                if (!listEnum.Contains(CorcelRuleType.Rule2))
                    if (ordersOfCustomerByMonth.Count() <= piecesCount)
                    {
                        listEnum.Add(CorcelRuleType.Rule2);
                    }
                if (!listEnum.Contains(CorcelRuleType.Rule3))
                    if (ordersOfCustomerByMonth.Count() <= quantityCount)
                    {
                        listEnum.Add(CorcelRuleType.Rule3);
                    }
            }

            return listEnum;
        }
    }
}
