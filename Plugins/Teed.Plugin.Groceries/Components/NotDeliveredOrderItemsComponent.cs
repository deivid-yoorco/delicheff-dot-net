using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Models.Order;
using Teed.Plugin.Groceries.Services;

namespace Teed.Plugin.Groceries.Components
{
    [ViewComponent(Name = "NotDeliveredOrderItems")]
    public class NotDeliveredOrderItemsComponent : NopViewComponent
    {
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly NotDeliveredOrderItemService _notDeliveredOrderItemService;
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IOrderService _orderService;

        public NotDeliveredOrderItemsComponent(ICustomerService customerService, IWorkContext workContext,
            NotDeliveredOrderItemService notDeliveredOrderItemService, IProductService productService,
            IPictureService pictureService, IProductAttributeParser productAttributeParser,
            IOrderService orderService)
        {
            _customerService = customerService;
            _workContext = workContext;
            _notDeliveredOrderItemService = notDeliveredOrderItemService;
            _productService = productService;
            _pictureService = pictureService;
            _productAttributeParser = productAttributeParser;
            _orderService = orderService;
        }

        public IViewComponentResult Invoke(string widgetZone, int additionalData)
        {
            var notDeliveredOrderItems = _notDeliveredOrderItemService.GetAll()
                .Where(x => x.OrderId == additionalData).ToList();

            var model = new NotDeliveredOrderItemsModel();
            model.OrderId = additionalData;
            model.Items = new List<Item>();
            foreach (var notDeliveredOrderItem in notDeliveredOrderItems)
            {
                var product = _productService.GetProductById(notDeliveredOrderItem.ProductId);
                var order = _orderService.GetOrderById(notDeliveredOrderItem.OrderId);
                var item = new Item
                {
                    Name = product.Name,
                    Sku = product.Sku,
                    Price = notDeliveredOrderItem.UnitPriceInclTax.ToString("C") + " con IVA",
                    Quantity = notDeliveredOrderItem.Quantity,
                    Discount = notDeliveredOrderItem.DiscountAmountInclTax.ToString("C") + " con IVA",
                    Total = notDeliveredOrderItem.PriceInclTax.ToString("C") + " con IVA",
                    NotDeliveredReason = notDeliveredOrderItem.NotDeliveredReason,
                    EquivalenceCoefficient = product.EquivalenceCoefficient,
                    BuyingBySecondary = notDeliveredOrderItem.BuyingBySecondary,
                    WeightInterval = product.WeightInterval,
                    SelectedPropertyOption = notDeliveredOrderItem.SelectedPropertyOption
                };

                var productPicture = product.ProductPictures.FirstOrDefault();
                if (productPicture != null)
                    item.PictureThumbnailUrl = _pictureService.GetPictureUrl(productPicture.PictureId, 75);

                model.Items.Add(item);
            }

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Order/NotDeliveredOrderItems.cshtml", model);
        }
    }
}
