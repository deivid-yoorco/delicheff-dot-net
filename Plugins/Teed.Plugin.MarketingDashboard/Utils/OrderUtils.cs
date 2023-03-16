using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.MarketingDashboard.Utils
{
    public static class OrderUtils
    {
        public static readonly List<DateTime> DisabledDates = new List<DateTime>()
            {
                new DateTime(2020, 11, 16),
                new DateTime(2020, 12, 25),
                new DateTime(2021, 1, 1),
                new DateTime(2021, 2, 1),
                new DateTime(2021, 3, 15),
                new DateTime(2021, 5, 1),
                new DateTime(2021, 9, 16),
                new DateTime(2021, 11, 15),
                new DateTime(2021, 12, 25),
                new DateTime(2021, 4, 2),
            };

        public static string GetPaymentOptionName(string paymentMethodSystemName)
        {
            return paymentMethodSystemName.Contains("Cash") ? "Efectivo" :
                    paymentMethodSystemName.Contains("Replacement") ? "Reposición" :
                    paymentMethodSystemName.Contains("MercadoPagoQr") ? "QR Mercadopago" :
                    "Tarjeta";
        }

        public static IQueryable<Order> GetFilteredOrders(IOrderService orderService)
        {
            return orderService.GetAllOrdersQuery().Where(x => x.OrderStatusId != 40 &&
                !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10));
        }

        public static IQueryable<IGrouping<object, Order>> GetPedidosGroup(IQueryable<Order> orders)
        {
            return orders.GroupBy(x => new { x.CustomerId, x.SelectedShippingDate.Value, x.SelectedShippingTime, x.ShippingAddress.Address1 });
        }

        public static IEnumerable<IGrouping<dynamic, Order>> GetPedidosGroupByList(List<Order> orders, int versionCode = 0)
        {
            if (versionCode == 0)
            {
                return orders.GroupBy(x => new
                {
                    x.CustomerId,
                    SelectedShippingDate = x.SelectedShippingDate.Value,
                    x.SelectedShippingTime,
                    Address1 = x.ShippingAddress.Address1
                });
            }
            else
            {
                return orders.GroupBy(x => new
                {
                    x.CustomerId,
                    CreationDate = x.CreatedOnUtc.ToLocalTime().Date,
                    Address1 = x.ShippingAddress?.Address1
                });
            }
        }

        public static IQueryable<Order> GetPedidosOnly(IQueryable<Order> orders)
        {
            return GetPedidosGroup(orders).Select(x => x.FirstOrDefault());
        }

        public static ParsedShippingTimeModel ParseSelectedShippingTime(string selectedShippingTime)
        {
            switch (selectedShippingTime)
            {
                case "1:00 PM - 3:00 PM":
                    return new ParsedShippingTimeModel()
                    {
                        InitTime = new TimeSpan(13, 0, 0),
                        EndTime = new TimeSpan(15, 0, 0)
                    };
                case "3:00 PM - 5:00 PM":
                    return new ParsedShippingTimeModel()
                    {
                        InitTime = new TimeSpan(15, 0, 0),
                        EndTime = new TimeSpan(17, 0, 0)
                    };
                case "5:00 PM - 7:00 PM":
                    return new ParsedShippingTimeModel()
                    {
                        InitTime = new TimeSpan(17, 0, 0),
                        EndTime = new TimeSpan(19, 0, 0)
                    };
                case "7:00 PM - 9:00 PM":
                    return new ParsedShippingTimeModel()
                    {
                        InitTime = new TimeSpan(19, 0, 0),
                        EndTime = new TimeSpan(21, 0, 0)
                    };
                default:
                    return null;
            }
        }

        public static Order Clone(Order originalOrder)
        {
            var order = new Order()
            {
                Id = originalOrder.Id,
                SelectedShippingDate = originalOrder.SelectedShippingDate,
                SelectedShippingTime = originalOrder.SelectedShippingTime,
                CustomOrderNumber = originalOrder.CustomOrderNumber,
                ShippingAddress = originalOrder.ShippingAddress,
                AffiliateId = originalOrder.AffiliateId,
                AllowStoringCreditCardNumber = originalOrder.AllowStoringCreditCardNumber,
                AuthorizationTransactionCode = originalOrder.AuthorizationTransactionCode,
                AuthorizationTransactionId = originalOrder.AuthorizationTransactionId,
                AuthorizationTransactionResult = originalOrder.AuthorizationTransactionResult,
                BillingAddress = originalOrder.BillingAddress,
                BillingAddressId = originalOrder.BillingAddressId,
                CheckoutAttributeDescription = originalOrder.CheckoutAttributeDescription,
                CheckoutAttributesXml = originalOrder.CheckoutAttributesXml,
                PaymentMethodAdditionalFeeExclTax = originalOrder.PaymentMethodAdditionalFeeExclTax,
                PaymentMethodAdditionalFeeInclTax = originalOrder.PaymentMethodAdditionalFeeInclTax,
                PickupAddress = originalOrder.PickupAddress,
                PickupAddressId = originalOrder.PickupAddressId,
                RefundedAmount = originalOrder.RefundedAmount,
                ShippingAddressId = originalOrder.ShippingAddressId,
                BoxesDistribution = originalOrder.BoxesDistribution,
                CaptureTransactionId = originalOrder.CaptureTransactionId,
                CaptureTransactionResult = originalOrder.CaptureTransactionResult,
                CardCvv2 = originalOrder.CardCvv2,
                CardExpirationMonth = originalOrder.CardExpirationMonth,
                CardExpirationYear = originalOrder.CardExpirationYear,
                CardName = originalOrder.CardName,
                CardNumber = originalOrder.CardNumber,
                CardType = originalOrder.CardType,
                CreatedOnUtc = originalOrder.CreatedOnUtc,
                CurrencyRate = originalOrder.CurrencyRate,
                Customer = originalOrder.Customer,
                CustomerCurrencyCode = originalOrder.CustomerCurrencyCode,
                CustomerId = originalOrder.CustomerId,
                CustomerIp = originalOrder.CustomerIp,
                CustomerLanguageId = originalOrder.CustomerLanguageId,
                RouteId = originalOrder.RouteId,
                RouteDisplayOrder = originalOrder.RouteDisplayOrder,
                CustomerTaxDisplayType = originalOrder.CustomerTaxDisplayType,
                CustomerTaxDisplayTypeId = originalOrder.CustomerTaxDisplayTypeId,
                RescuedByRouteId = originalOrder.RescuedByRouteId,
                RescueRouteDisplayOrder = originalOrder.RescueRouteDisplayOrder,
                CustomValuesXml = originalOrder.CustomValuesXml,
                Deleted = originalOrder.Deleted,
                MaskedCreditCardNumber = originalOrder.MaskedCreditCardNumber,
                MessageGift = originalOrder.MessageGift,
                OrderDiscount = originalOrder.OrderDiscount,
                OrderGuid = originalOrder.OrderGuid,
                OrderShippingExclTax = originalOrder.OrderShippingExclTax,
                OrderShippingInclTax = originalOrder.OrderShippingInclTax,
                OrderStatus = originalOrder.OrderStatus,
                OrderStatusId = originalOrder.OrderStatusId,
                OrderSubTotalDiscountExclTax = originalOrder.OrderSubTotalDiscountExclTax,
                OrderSubTotalDiscountInclTax = originalOrder.OrderSubTotalDiscountInclTax,
                OrderSubtotalExclTax = originalOrder.OrderSubtotalExclTax,
                OrderSubtotalInclTax = originalOrder.OrderSubtotalInclTax,
                OrderTax = originalOrder.OrderTax,
                OrderTotal = originalOrder.OrderTotal,
                OriginalOrderTotal = originalOrder.OriginalOrderTotal,
                PaidDateUtc = originalOrder.PaidDateUtc,
                PaymentMethodSystemName = originalOrder.PaymentMethodSystemName,
                PaymentParameter = originalOrder.PaymentParameter,
                PaymentStatus = originalOrder.PaymentStatus,
                PaymentStatusId = originalOrder.PaymentStatusId,
                PickUpInStore = originalOrder.PickUpInStore,
                PreviousPointTransferDistance = originalOrder.PreviousPointTransferDistance,
                PreviousPointTransferTime = originalOrder.PreviousPointTransferTime,
                RedeemedRewardPointsEntry = originalOrder.RedeemedRewardPointsEntry,
                RewardPointsHistoryEntryId = originalOrder.RewardPointsHistoryEntryId,
                ShippingMethod = originalOrder.ShippingMethod,
                ShippingRateComputationMethodSystemName = originalOrder.ShippingRateComputationMethodSystemName,
                ShippingStatus = originalOrder.ShippingStatus,
                ShippingStatusId = originalOrder.ShippingStatusId,
                SignsGift = originalOrder.SignsGift,
                StoreId = originalOrder.StoreId,
                SubscriptionTransactionId = originalOrder.SubscriptionTransactionId,
                TaxRates = originalOrder.TaxRates,
                VatNumber = originalOrder.VatNumber,
                ZoneId = originalOrder.ZoneId
            };

            foreach (var item in originalOrder.OrderItems)
                order.OrderItems.Add(item);

            foreach (var item in originalOrder.Shipments)
                order.Shipments.Add(item);

            return order;
        }
    }

    public class ParsedShippingTimeModel
    {
        public TimeSpan InitTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
