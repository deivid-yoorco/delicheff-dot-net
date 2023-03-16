using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Services.Helpers
{
    public static class DiscountHelper
    {
        public static List<DiscountAppliedTemp> GetListOfCouponsBeingUsed(List<DiscountForCaching> appliedDiscounts)
        {
            var doneDiscounts = new List<DiscountAppliedTemp>();
            if (appliedDiscounts != null)
                doneDiscounts.AddRange(appliedDiscounts.Where(x => x.RequiresCouponCode).Select(x => new DiscountAppliedTemp
                {
                    DiscountId = x.Id,
                    AmountDiscounted = x.DiscountedAmount
                }).ToList());
            return doneDiscounts;
        }

        public static List<SpecialPromotionItem> GetSpecialPromotionItems(Customer customer, IList<ShoppingCartItem> cart,
            IDiscountService _discountService, ISpecialDiscountTakeXPayYService _specialDiscountTakeXPayYService,
            IPriceCalculationService _priceCalculationService)
        {
            var appliedSpecialPromotionItems = new List<SpecialPromotionItem>();
            var usingCoupons = customer.ParseAppliedDiscountCouponCodes()
                .Select(x => x.ToLower()).ToList();
            var discounts = _discountService.GetAllDiscounts()
                .Where(x => x.RequiresCouponCode && !string.IsNullOrEmpty(x.CouponCode)
                && usingCoupons.Contains(x.CouponCode?.ToLower())).ToList();

            foreach (var discount in discounts)
            {
                var specialPromotionItems = new List<SpecialPromotionItem>();
                var specialPromotionDiscount = _specialDiscountTakeXPayYService.GetSpecialDiscountByDiscountId(discount.Id);
                if ((specialPromotionDiscount?.IsAcitve ?? false) &&
                    specialPromotionDiscount?.EntityTypeId > 0 && specialPromotionDiscount?.EntityId > 0 &&
                    specialPromotionDiscount?.TakeAmount > 0 && specialPromotionDiscount?.PayAmount > 0)
                {
                    var entityId = specialPromotionDiscount.EntityId;
                    var toGive = specialPromotionDiscount.TakeAmount;
                    var toPay = specialPromotionDiscount.PayAmount;
                    var items = cart.OrderBy(x => x.Id).ToList();
                    var itemsOfId = new List<ShoppingCartItem>();

                    if (specialPromotionDiscount.EntityTypeId == (int)TakeXPayYEntityType.Product)
                        itemsOfId = items.Where(x => x.ProductId == entityId).ToList();
                    else if (specialPromotionDiscount.EntityTypeId == (int)TakeXPayYEntityType.Category)
                    {
                        var productCategories = cart.Select(x => new
                        {
                            ProductId = x.Product.Id,
                            ProductCategoryIds = x.Product.ProductCategories.Select(y => y.CategoryId).ToList(),
                        }).ToList();
                        var productIdsWithCategory = productCategories
                            .Where(x => x.ProductCategoryIds.Contains(specialPromotionDiscount.EntityId))
                            .Select(x => x.ProductId)
                            .Distinct()
                            .ToList();
                        foreach (var productIdWithCategory in productIdsWithCategory)
                        {
                            var tempItems = items.Where(x => x.ProductId == productIdWithCategory).ToList();
                            if (tempItems.Any())
                                itemsOfId.AddRange(tempItems);
                        }
                    }

                    foreach (var productOfId in itemsOfId)
                    {
                        for (int i = 0; i < productOfId.Quantity; i++)
                        {
                            specialPromotionItems.Add(new SpecialPromotionItem
                            {
                                ProductId = productOfId.ProductId,
                                IdCount = $"{productOfId.ProductId}-{i}",
                                Name = productOfId.Product.Name,
                                Price = _priceCalculationService.GetUnitPrice(productOfId),
                                AttributesXml = productOfId.AttributesXml ?? ""
                            });
                        }
                    }

                    if (specialPromotionItems.Any())
                    {
                        var amountToGive = (int)decimal.Floor(specialPromotionItems.Count() / toGive);
                        var amountToPay = toGive - toPay;
                        for (int i = 0; i < amountToGive; i++)
                        {

                            for (int e = 0; e < amountToPay; e++)
                            {
                                var itemsThatCanApply = specialPromotionItems.OrderBy(x => x.Price).ToList();
                                foreach (var item in itemsThatCanApply)
                                {
                                    var appliedItemsOfProduct = appliedSpecialPromotionItems.Where(x => x.ProductId == item.ProductId &&
                                        x.AttributesXml == item.AttributesXml).ToList();
                                    if (!appliedItemsOfProduct.Where(x => x.IdCount == item.IdCount).Any())
                                    {
                                        item.AppliedId = discount.Id;
                                        appliedSpecialPromotionItems.Add(item);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return appliedSpecialPromotionItems.Any() ? appliedSpecialPromotionItems : null;
        }
    }
}
