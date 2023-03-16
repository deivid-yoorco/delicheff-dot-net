using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Teed.Plugin.Groceries.Domain.TipsWithCards;
using Teed.Plugin.Groceries.Services;

namespace Teed.Plugin.Groceries.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class TipsWithCardController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        private readonly TipsWithCardService _tipsWithCardService;
        private readonly IWorkContext _workContext;

        public TipsWithCardController(IPermissionService permissionService, ICustomerService customerService,
            TipsWithCardService tipsWithCardService, IWorkContext workContext)
        {
            _permissionService = permissionService;
            _customerService = customerService;
            _tipsWithCardService = tipsWithCardService;
            _workContext = workContext;
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/TipsWithCard/List.cshtml");
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var tipsWithCards = _tipsWithCardService.GetAll()
                .Where(x => x.Deleted == false && string.IsNullOrEmpty(x.Log))
                .OrderByDescending(x => x.CreatedOnUtc);
            var pagedList = GroupedPageList(tipsWithCards, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    Date = TimeZoneInfo.ConvertTimeFromUtc(x.CreatedOnUtc, TimeZoneInfo.Local).ToString("dd/MM/yyyy"),
                    OrderId = "#" + x.OrderId,
                    Amount = x.Amount.ToString("C"),
                    ReportedByUsersName = _customerService.GetCustomerById(x.ReportedByUserId).GetFullName()
                }).ToList(),
                Total = tipsWithCards.Count()
            };

            return Json(gridModel);
        }

        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            if (id > 0)
            {
                var tipsWithCard = _tipsWithCardService.GetAll().Where(x => x.Id == id).FirstOrDefault();

                tipsWithCard.Deleted = true;
                var currentCustomer = _workContext.CurrentCustomer;
                tipsWithCard.Log = 
                    $"{DateTime.UtcNow.ToString("dd/MM/yyyy")} - el usuario {currentCustomer.Username} ({currentCustomer.Id}) eliminó el reporte de propina con tarjeta";

                _tipsWithCardService.Update(tipsWithCard);
            }

            return View("~/Plugins/Teed.Plugin.Groceries/Views/TipsWithCard/List.cshtml");
        }

        private List<TipsWithCard> GroupedPageList(IQueryable<TipsWithCard> source, int pageIndex, int pageSize)
        {
            List<TipsWithCard> filteredList = new List<TipsWithCard>();
            filteredList.AddRange(source.Skip(pageIndex * pageSize).Take(pageSize).ToList());
            return filteredList;
        }
    }
}
