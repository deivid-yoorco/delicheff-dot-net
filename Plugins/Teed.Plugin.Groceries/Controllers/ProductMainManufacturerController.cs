using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teed.Plugin.Groceries.Domain.OrderItemBuyers;
using Teed.Plugin.Groceries.Models.OrderItemBuyer;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class ProductMainManufacturerController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly OrderItemBuyerService _orderItemBuyerService;
        private readonly BuyerListDownloadService _buyerListDownloadService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICategoryService _categoryService;
        private readonly IProductLogService _productLogService;
        private readonly IWorkContext _workContext;
        private readonly SupermarketBuyerService _supermarketBuyerService;
        private readonly ProductMainManufacturerService _productMainManufacturerService;

        public ProductMainManufacturerController(IPermissionService permissionService, IOrderService orderService, ICustomerService customerService,
            OrderItemBuyerService orderItemBuyerService, IManufacturerService manufacturerService,
            ICategoryService categoryService, BuyerListDownloadService buyerListDownloadService,
            IWorkContext workContext, SupermarketBuyerService supermarketBuyerService, ProductMainManufacturerService productMainManufacturerService,
            IProductLogService productLogService)
        {
            _productLogService = productLogService;
            _permissionService = permissionService;
            _orderService = orderService;
            _customerService = customerService;
            _orderItemBuyerService = orderItemBuyerService;
            _manufacturerService = manufacturerService;
            _categoryService = categoryService;
            _buyerListDownloadService = buyerListDownloadService;
            _workContext = workContext;
            _supermarketBuyerService = supermarketBuyerService;
            _productMainManufacturerService = productMainManufacturerService;
        }

        [HttpPost]
        public IActionResult SaveMainManufacturer(int selectedMainManufacturerId, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (selectedMainManufacturerId == 0 || productId == 0) return NoContent();

            var currentProductMainManufacturer = _productMainManufacturerService.GetAll()
                .Where(x => x.ProductId == productId)
                .FirstOrDefault();
            string log = string.Empty;
            if (currentProductMainManufacturer == null)
            {
                var manufacturer = _manufacturerService.GetManufacturerById(selectedMainManufacturerId);
                log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) asignó el fabricante principal \"{manufacturer?.Name}\" ({selectedMainManufacturerId}).\n";
                currentProductMainManufacturer = new Domain.Product.ProductMainManufacturer()
                {
                    ManufacturerId = selectedMainManufacturerId,
                    ProductId = productId,
                    Log = log
                };
                _productMainManufacturerService.Insert(currentProductMainManufacturer);
                
            }
            else if (selectedMainManufacturerId != currentProductMainManufacturer.ManufacturerId)
            {
                var oldManufacturer = _manufacturerService.GetManufacturerById(currentProductMainManufacturer.ManufacturerId);
                var newManufacturer = _manufacturerService.GetManufacturerById(selectedMainManufacturerId);
                log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) modificó el fabricante principal de \"{oldManufacturer.Name}\" ({currentProductMainManufacturer.ManufacturerId}) a \"{newManufacturer.Name}\" ({selectedMainManufacturerId}).\n";
                currentProductMainManufacturer.ManufacturerId = selectedMainManufacturerId;
                currentProductMainManufacturer.Log += log;
                _productMainManufacturerService.Update(currentProductMainManufacturer);
            }

            var productLog = new ProductLog()
            {
                CreatedOnUtc = DateTime.UtcNow,
                UserId = _workContext.CurrentCustomer.Id,
                ProductId = productId,
                Message = log
            };
            _productLogService.InsertProductLog(productLog);

            return NoContent();
        }
    }
}