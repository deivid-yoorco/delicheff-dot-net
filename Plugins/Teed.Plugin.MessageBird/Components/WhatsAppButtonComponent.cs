using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.MessageBird;
using Teed.Plugin.MessageBird.Models;
using Teed.Plugin.MessageBird.Security;

namespace Teed.Plugin.MessageBird.Components
{
    [ViewComponent(Name = "WhatsAppButton")]
    public class WhatsAppButtonComponent : NopViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        private readonly IShippingService _shippingService;

        public WhatsAppButtonComponent(IWorkContext workContext,
            IStoreService storeService, ISettingService settingService,
            IPermissionService permissionService, ICustomerService customerService)
        {
            _workContext = workContext;
            _storeService = storeService;
            _settingService = settingService;
            _permissionService = permissionService;
            _customerService = customerService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var model = new WhatsAppSendModel
            {
                IsAllowed = true
            };

            if (!_permissionService.Authorize(TeedMessageBirdPermissionProvider.MessageBirdAdmin) &&
                !_permissionService.Authorize(TeedMessageBirdPermissionProvider.MessageBird))
                model.IsAllowed = false;

            var customer = _customerService.GetCustomerById((int)additionalData);
            if (customer == null)
                model.IsAllowed = false;
            else
            {
                var storeId = _workContext.CurrentCustomer.GetAttribute<int>(SystemCustomerAttributeNames.AdminAreaStoreScopeConfiguration);
                var store = _storeService.GetStoreById(storeId);
                storeId = store != null ? store.Id : 0;
                var messageBirdSettings = _settingService.LoadSetting<MessageBirdSettings>(storeId);

                model.CustomerId = customer.Id;
                model.Settings = messageBirdSettings;

                var addresses = customer.Addresses.ToList();
                if (addresses.Any())
                {
                    var customerPhoneSelectList = addresses.Where(x => !string.IsNullOrEmpty(x.PhoneNumber))
                        .Select(x => x.PhoneNumber).Distinct()
                        .Select(x => new SelectListItem
                        {
                            Text = x.Replace(" ", ""),
                            Value = x.Replace(" ", "")
                        }).ToList();

                    model.Phones.AddRange(customerPhoneSelectList);

                    var customerNameSelectList = addresses.Where(x => !string.IsNullOrEmpty(x.FirstName))
                        .Select(x => new SelectListItem
                        {
                            Text = x.FirstName + " " + x.LastName,
                            Value = x.FirstName
                        }).GroupBy(x => x.Text).Select(x => x.FirstOrDefault()).ToList();
                    model.Names.AddRange(customerNameSelectList);
                }
            }

            return View("~/Plugins/Teed.Plugin.MessageBird/Views/WhatsAppButton.cshtml", model);
        }
    }
}
