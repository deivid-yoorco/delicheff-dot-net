using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.UrbanPromoters;
using Teed.Plugin.Groceries.Models.UrbanPromoter;
using Teed.Plugin.Groceries.Services;

namespace Teed.Plugin.Groceries.Controllers
{
    public class UrbanPromoterPublicViewController : BasePublicController
    {
        private readonly UrbanPromoterService _urbanPromoterService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;

        public UrbanPromoterPublicViewController(UrbanPromoterService urbanPromoterService,
            IWorkContext workContext, IOrderService orderService,
            ICustomerService customerService, IDiscountService discountService)
        {
            _urbanPromoterService = urbanPromoterService;
            _workContext = workContext;
            _orderService = orderService;
            _customerService = customerService;
            _discountService = discountService;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("promotoresurbanos")]
        public IActionResult UrbanPromoterPublicView()
        {
            var currentCustomer = _workContext.CurrentCustomer;
            if (currentCustomer.IsRegistered())
            {
                var promoter = _urbanPromoterService.GetByCustomerId(currentCustomer.Id);
                if (promoter != null)
                {
                    var fees = promoter.GetUrbanPromoterFees();
                    var payments = promoter.GetUrbanPromoterPayment();
                    var coupons = promoter.GetUrbanPromoterCoupons();
                    var discountIds = coupons.Select(x => x.DiscountId).ToList();
                    var discounts = _discountService.GetAllDiscounts()
                        .Where(x => discountIds.Contains(x.Id))
                        .ToList();
                    var orderIds = fees.Select(x => x.RelatedOrderId).ToList();
                    var orders = _orderService.GetAllOrdersQuery()
                        .Where(x => orderIds.Contains(x.Id)).ToList();
                    var customerIds = orders.Select(x => x.CustomerId).ToList();
                    var customers = _customerService.GetAllCustomersQuery()
                        .Where(x => customerIds.Contains(x.Id))
                        .ToList();

                    var model = new UrbanPromoterPublicViewModel
                    {
                        CustomerName = currentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                        AccountAddress = promoter.AccountAddress,
                        AccountBankName = promoter.AccountBankName,
                        AccountCLABE = promoter.AccountCLABE,
                        AccountNumber = promoter.AccountNumber,
                        AccountOwnerName = promoter.AccountOwnerName,
                        CashPayment = promoter.CashPayment,

                        FeesTotal = fees.Select(x => x.FeeAmount).DefaultIfEmpty().Sum(),
                        PaymentsTotal = payments.Select(x => x.PaymentAmount).DefaultIfEmpty().Sum(),
                        PendingTotal = Math.Round(fees.Select(x => x.FeeAmount).DefaultIfEmpty().Sum() - payments.Select(x => x.PaymentAmount).DefaultIfEmpty().Sum(), 2),
                        OrdersWithPromoter = orders.Select(x => new OrdersWithPromoter
                        {
                            Client = customers.Where(y => y.Id == x.CustomerId).FirstOrDefault().GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                            Comission = fees.Where(y => y.RelatedOrderId == x.Id).FirstOrDefault().FeeAmount,
                            Date = x.SelectedShippingDate.Value,
                            Status = x.OrderStatusId == 10 ? "Pendiente" : x.OrderStatusId == 20 ? "Procesado" : x.OrderStatusId == 30 ? "Completado" : x.OrderStatusId == 40 ? "Cancelado" : "No enviado",
                            UsedCoupon = GetUsedCoupon(x, coupons)
                        }).ToList(),
                        DiscountsWithPromoter = discounts.Select(x => $"{x.CouponCode}").ToList()
                    };
                    return View("~/Plugins/Teed.Plugin.Groceries/Views/UrbanPromoter/PublicView.cshtml", model);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        private string GetUsedCoupon(Order order, List<UrbanPromoterCoupon> coupons)
        {
            var couponIds = coupons.Select(x => x.DiscountId).ToList();
            var discountIdsOfOrder = order.DiscountUsageHistory.Select(x => x.DiscountId).ToList();
            var discountId = couponIds.Intersect(discountIdsOfOrder).FirstOrDefault();
            var currentDiscount = order.DiscountUsageHistory.Where(x => x.DiscountId == discountId).FirstOrDefault().Discount;
            return $"{currentDiscount.CouponCode}";
        }
    }
}
