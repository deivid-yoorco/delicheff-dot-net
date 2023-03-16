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
using Teed.Plugin.Groceries.Helpers;
using Teed.Plugin.Groceries.Models.OrderItemBuyer;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class ManufacturerBuyerController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly OrderItemBuyerService _orderItemBuyerService;
        private readonly BuyerListDownloadService _buyerListDownloadService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICategoryService _categoryService;
        private readonly IWorkContext _workContext;
        private readonly SupermarketBuyerService _supermarketBuyerService;
        private readonly ManufacturerBuyerService _manufacturerBuyerService;
        private readonly CorporateCardService _corporateCardService;
        private readonly IStoreContext _storeContext;

        public ManufacturerBuyerController(IPermissionService permissionService, IOrderService orderService, ICustomerService customerService,
            OrderItemBuyerService orderItemBuyerService, IManufacturerService manufacturerService,
            ICategoryService categoryService, BuyerListDownloadService buyerListDownloadService,
            IWorkContext workContext, SupermarketBuyerService supermarketBuyerService, ManufacturerBuyerService manufacturerBuyerService,
            CorporateCardService corporateCardService, IStoreContext storeContext)
        {
            _permissionService = permissionService;
            _orderService = orderService;
            _customerService = customerService;
            _orderItemBuyerService = orderItemBuyerService;
            _manufacturerService = manufacturerService;
            _categoryService = categoryService;
            _buyerListDownloadService = buyerListDownloadService;
            _workContext = workContext;
            _supermarketBuyerService = supermarketBuyerService;
            _manufacturerBuyerService = manufacturerBuyerService;
            _corporateCardService = corporateCardService;
            _storeContext = storeContext;
        }

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ManufacturerBuyer))
                return AccessDeniedView();

            var role = _customerService.GetCustomerRoleBySystemName("buyer");
            var buyers = _customerService.GetAllCustomers(customerRoleIds: new int[] { role.Id }).ToList();
            var manufacturerBuyers = _manufacturerBuyerService.GetAll().ToList();
            var manufacturers = _manufacturerService.GetAllManufacturers()
                .OrderBy(x => manufacturerBuyers.Where(y => y.ManufacturerId == x.Id && y.BuyerId != 0).Any())
                .ThenBy(x => x.Name)
                .ToList();

            var manufacturerIds = manufacturers.Select(x => x.Id).ToList();
            var deletedManufacturers = manufacturerBuyers.Where(x => !manufacturerIds.Contains(x.ManufacturerId));

            var corporateCards = _corporateCardService.GetAll().ToList();
            var employeeIds = corporateCards.Select(x => x.EmployeeId).ToList();
            var employees = ExternalHelper.GetExternalPayrollEmployeesByIds(employeeIds, _storeContext);

            foreach (var item in deletedManufacturers)
                _manufacturerBuyerService.Delete(item);

            var model = new ManufacturerBuyerModel()
            {
                Customers = buyers.OrderBy(x => x.GetFullName()).ToList(),
                ManufacturerBuyers = manufacturerBuyers.Select(x => new ManufacturerBuyerInfo
                {
                    ManufacturerBuyer = x,
                    IsCorporateCard = CheckManufacturerType(x, manufacturers),
                }).ToList(),
                Manufacturers = manufacturers,
                Log = manufacturerBuyers.OrderByDescending(x => x.UpdatedOnUtc).Select(x =>
                {
                    string log = x.Log + $" Fabricante: {manufacturers.Where(y => y.Id == x.ManufacturerId).Select(y => y.Name).FirstOrDefault()}";
                    return log.Replace("\n", ".<br />");
                }).ToList(),
            };

            var buyersWithoutCards = new List<BuyersWithoutCard>();
            foreach (var buyer in model.ManufacturerBuyers.Where(x => x.IsCorporateCard != null && (x.IsCorporateCard ?? false)).ToList())
            {
                var cardNumber = string.Empty;
                var payrollOfUser = employees.Where(x => x.CustomerId == buyer.ManufacturerBuyer.BuyerId).FirstOrDefault();
                if (payrollOfUser != null)
                    cardNumber = corporateCards.Where(x => x.EmployeeId == payrollOfUser.Id).FirstOrDefault()?.CardNumber ?? string.Empty;
                if (string.IsNullOrEmpty(cardNumber) && !buyersWithoutCards.Any(x => x.Id == buyer.ManufacturerBuyer.Id))
                    buyersWithoutCards.Add(new BuyersWithoutCard
                    {
                        Id = buyer.ManufacturerBuyer.BuyerId,
                        Name = buyers.Where(x => x.Id == buyer.ManufacturerBuyer.BuyerId).FirstOrDefault() != null ?
                            buyers.Where(x => x.Id == buyer.ManufacturerBuyer.BuyerId).FirstOrDefault().GetFullName() :
                            "Sin especificar"
                    });
            }
            model.BuyersWithoutCards = buyersWithoutCards;

            return View("~/Plugins/Teed.Plugin.Groceries/Views/OrderItemBuyer/Configure.cshtml", model);
        }

        private bool? CheckManufacturerType(ManufacturerBuyer manufacturerBuyer, List<Manufacturer> manufacturers)
        {
            if (!(manufacturers.Where(y => y.Id == manufacturerBuyer.ManufacturerId).FirstOrDefault()?.IsPaymentWhithCorporateCard ?? false) &&
                !(manufacturers.Where(y => y.Id == manufacturerBuyer.ManufacturerId).FirstOrDefault()?.IsPaymentWhithTransfer ?? false))
                return null;
            else if ((manufacturers.Where(y => y.Id == manufacturerBuyer.ManufacturerId).FirstOrDefault()?.IsPaymentWhithCorporateCard ?? false) &&
                !(manufacturers.Where(y => y.Id == manufacturerBuyer.ManufacturerId).FirstOrDefault()?.IsPaymentWhithTransfer ?? false))
                return true;
            else if (!(manufacturers.Where(y => y.Id == manufacturerBuyer.ManufacturerId).FirstOrDefault()?.IsPaymentWhithCorporateCard ?? false) &&
                (manufacturers.Where(y => y.Id == manufacturerBuyer.ManufacturerId).FirstOrDefault()?.IsPaymentWhithTransfer ?? false))
                return false;
            else
                return null;
        }

        [HttpPost]
        public IActionResult Configure(List<ManufacturerBuyerAssignData> result)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ManufacturerBuyer))
                return AccessDeniedView();

            var currentData = _manufacturerBuyerService.GetAll().ToList();
            var customers = _customerService.GetAllCustomersQuery().Where(x => x.CustomerRoles.Count > 1).ToList();
            foreach (var item in result)
            {
                var existingElement = currentData.Where(x => x.ManufacturerId == item.ManufacturerId).FirstOrDefault();
                if (existingElement == null && item.CustomerId > 0)
                {
                    existingElement = new ManufacturerBuyer()
                    {
                        BuyerId = item.CustomerId,
                        ManufacturerId = item.ManufacturerId,
                        Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) creó la relación fabricante-comprador.\n"
                    };
                    _manufacturerBuyerService.Insert(existingElement);
                }
                else if (existingElement != null && (existingElement.ManufacturerId != item.ManufacturerId || existingElement.BuyerId != item.CustomerId))
                {
                    var currentCustomer = customers.Where(x => x.Id == existingElement.BuyerId).FirstOrDefault();
                    var newCustomer = customers.Where(x => x.Id == item.CustomerId).FirstOrDefault();
                    existingElement.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) modificó la relación fabricante-comprador el comprador pasó de {(currentCustomer == null ? "" : currentCustomer.GetFullName())} ({existingElement.BuyerId}) a {(newCustomer == null ? "" : newCustomer.GetFullName())} ({item.CustomerId}).\n";
                    existingElement.BuyerId = item.CustomerId;
                    existingElement.ManufacturerId = item.ManufacturerId;
                    _manufacturerBuyerService.Update(existingElement);
                }
            }

            return RedirectToAction("Configure");
        }
    }
}