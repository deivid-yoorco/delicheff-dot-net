using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.UrbanPromoters;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.ScheduleTasks
{
    public class UrbanPromoterFeesInsertScheduleTask : IScheduleTask
    {
        private readonly IDiscountService _discountService;
        private readonly IOrderService _orderService;
        private readonly UrbanPromoterService _urbanPromoterService;
        private readonly UrbanPromoterFeeService _urbanPromoterFeeService;
        private readonly UrbanPromoterCouponService _urbanPromoterCouponService;
        private readonly UrbanPromoterPaymentService _urbanPromoterPaymentService;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;

        public UrbanPromoterFeesInsertScheduleTask(IOrderService orderService, IDiscountService discountService,
            UrbanPromoterService urbanPromoterService, UrbanPromoterFeeService urbanPromoterFeeService,
            UrbanPromoterCouponService urbanPromoterCouponService, UrbanPromoterPaymentService urbanPromoterPaymentService,
            ISettingService settingService, ILogger logger)
        {
            _discountService = discountService;
            _orderService = orderService;
            _urbanPromoterService = urbanPromoterService;
            _urbanPromoterFeeService = urbanPromoterFeeService;
            _urbanPromoterCouponService = urbanPromoterCouponService;
            _urbanPromoterPaymentService = urbanPromoterPaymentService;
            _settingService = settingService;
            _logger = logger;
        }

        public void Execute()
        {
            //Check if its time to run
            if (TaskRunUtils.ShouldRunTask(_settingService, "UrbanPromoterFeesInsertScheduleTask"))
            {
                _logger.Information($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Running UrbanPromoterFeesInsertScheduleTask.");

                try
                {
                    var promoters = _urbanPromoterService.GetAll().Where(x => x.IsActive).ToList();
                    foreach (var promoter in promoters)
                    {
                        var feeOrderIds = promoter.GetUrbanPromoterFees().Select(x => x.RelatedOrderId).ToList();
                        var discountIds = promoter.GetUrbanPromoterCoupons().Select(x => x.DiscountId).ToList();
                        var discountUsage = _discountService.GetAllDiscountUsageHistory()
                            .Where(x => discountIds.Contains(x.DiscountId) && !feeOrderIds.Contains(x.OrderId))
                            .ToList();

                        var orderIdsForFee = discountUsage.Select(x => x.OrderId).ToList();
                        var ordersForFee = OrderUtils.GetFilteredOrders(_orderService)
                            .Where(x => x.ShippingStatusId == 40 && orderIdsForFee.Contains(x.Id) && !feeOrderIds.Contains(x.Id))
                            .ToList();
                        foreach (var orderForFee in ordersForFee)
                        {
                            var currentDiscount = discountUsage.Where(x => orderForFee.Id == x.OrderId).FirstOrDefault();
                            var promoterCoupon = promoter.GetUrbanPromoterCoupons()
                                .Where(x => x.DiscountId == currentDiscount.DiscountId)
                                .FirstOrDefault();
                            if (promoterCoupon != null)
                            {
                                var fee = new UrbanPromoterFee
                                {
                                    FeeAmount = promoterCoupon.Fee,
                                    FeeGenerationDateUtc = DateTime.UtcNow,
                                    RelatedOrderId = orderForFee.Id,
                                    UrbanPromoterId = promoter.Id
                                };
                                _urbanPromoterFeeService.Insert(fee);

                                promoter.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - Se generó comisión de forma automática por la orden #{orderForFee.Id} con cupón con nombre \"{currentDiscount.Discount.Name}\" y monto de {promoterCoupon.Fee.ToString("C")}.\n";
                                _urbanPromoterService.Update(promoter);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Error while running UrbanPromoterFeesInsertScheduleTask:\n\"{e.Message}\".", e);
                }

                _logger.Information($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Finished running UrbanPromoterFeesInsertScheduleTask.");
            }
        }
    }
}
