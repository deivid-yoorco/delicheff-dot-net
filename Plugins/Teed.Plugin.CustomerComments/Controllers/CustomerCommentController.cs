using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Services.Orders;
using Teed.Plugin.CustomerComments.Services;
using Teed.Plugin.CustomerComments.Security;
using Teed.Plugin.CustomerComments.Models;
using Teed.Plugin.CustomerComments.Domain.CustomerComment;

namespace Teed.Plugin.CustomerComments.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class CustomerCommentController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        private readonly CustomerCommentService _customerCommentService;
        private readonly IWorkContext _workContext;

        public CustomerCommentController(IPermissionService permissionService, IWorkContext workContext,
            ICustomerService customerService, CustomerCommentService customerCommentService)
        {
            _permissionService = permissionService;
            _customerService = customerService;
            _customerCommentService = customerCommentService;
            _workContext = workContext;
        }

        [HttpPost]
        public IActionResult CommentsData(int customerId = 0)
        {
            if (!_permissionService.Authorize(TeedCustomerCommentsPermissionProvider.CustomerComments))
                return AccessDeniedView();

            var customerComments = _customerCommentService.GetAllByCustomerId(customerId)
                .OrderByDescending(x => x.CreatedOnUtc);
            var adminIdsThatComment = customerComments.Select(x => x.CreatedByCustomerId).ToList();
            var admins = _customerService.GetAllCustomersQuery()
                .Where(x => adminIdsThatComment.Contains(x.Id))
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = customerComments
                .Select(x => new
                {
                    x.Id,
                    CreatedByCustomer = admins.Where(y => y.Id == x.CreatedByCustomerId).FirstOrDefault()?.GetFullName(),
                    x.Message,
                    CreatedOn = x.CreatedOnUtc.ToLocalTime().ToString("dd/MM/yyyy hh:mm:ss tt")
                }).ToList(),
                Total = customerComments.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult Create(CustomerCommentModel model)
        {
            if (!_permissionService.Authorize(TeedCustomerCommentsPermissionProvider.CustomerComments))
                return AccessDeniedView();

            if (!string.IsNullOrEmpty(model.Message))
            {
                var comment = new CustomerComment
                {
                    CreatedByCustomerId = _workContext.CurrentCustomer.Id,
                    CustomerId = model.CustomerId,
                    Message = model.Message,
                    Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) insertó un nuevo comentario del cliente ({model.CustomerId}): \"{model.Message}\".\n"
                };
                _customerCommentService.Insert(comment);
                return Ok();
            }
            else
                return BadRequest();
        }

        [HttpPost]
        public IActionResult Delete(CustomerCommentModel model)
        {
            if (!_permissionService.Authorize(TeedCustomerCommentsPermissionProvider.CustomerComments))
                return AccessDeniedView();

            var comment = _customerCommentService.GetById(model.Id);
            if (comment == null)
                return Ok();

            comment.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) eliminó el comentario del cliente ({comment.CustomerId}): \"{comment.Message}\".\n";
            _customerCommentService.Delete(comment);
            return Ok();
        }
    }
}