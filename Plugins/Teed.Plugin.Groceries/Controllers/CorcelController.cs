using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Nop.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using OfficeOpenXml;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Models.Corcel;
using Teed.Plugin.Groceries.Domain.Corcel;

namespace Teed.Plugin.Payroll.Controllers
{
    [Area(AreaNames.Admin)]
    public class CorcelController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IProductService _productService;
        private readonly CorcelProductService _corcelProductService;

        public CorcelController(IPermissionService permissionService, IWorkContext workContext,
            IProductService productService, CorcelProductService corcelProductService)
        {
            _permissionService = permissionService;
            _workContext = workContext;
            _corcelProductService = corcelProductService;
            _productService = productService;
        }

        [Route("Admin/[controller]/Products")]
        public IActionResult ProductsIndex()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.CorcelProduct))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/CorcelProduct/Index.cshtml");
        }

        [HttpPost]
        public IActionResult CorcelProductList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.CorcelProduct))
                return AccessDeniedView();
            var corcelProducts = _corcelProductService.GetAll()
                .OrderBy(x => x.Id);
            var productIds = corcelProducts.Select(x => x.ProductId).ToList();
            var products = _productService.GetAllProductsQuery()
                .Where(x => productIds.Contains(x.Id)).ToList();

            var pagedList = new PagedList<CorcelProduct>(corcelProducts, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    x.ProductId,
                    ProductName = products.Where(y => y.Id == x.ProductId).FirstOrDefault()?.Name ?? "---",
                    x.Quantity,
                    AddedOnDate = x.CreatedOnUtc.ToLocalTime().ToString("dd/MM/yyyy"),
                }).OrderBy(x => x.ProductName).ToList(),
                Total = corcelProducts.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult CorcelProductAdd(CorcelProductModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.CorcelProduct))
                return AccessDeniedView();
            
            if (model.ProductId < 1 || model.Quantity < 1)
                return BadRequest("Información incompleta");

            try
            {
                var corcelProduct = _corcelProductService.GetAll()
                    .Where(x => x.ProductId == model.ProductId).FirstOrDefault();
                if (corcelProduct != null)
                {
                    if (corcelProduct.Quantity != model.Quantity)
                    {
                        corcelProduct.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) modificó " +
                            $"la cantidad del producto CORCEL de {corcelProduct.Quantity} a {model.Quantity}.\n";
                        corcelProduct.Quantity = model.Quantity;
                    }
                    _corcelProductService.Update(corcelProduct);
                }
                else
                {
                    corcelProduct = new CorcelProduct
                    {
                        ProductId = model.ProductId,
                        Quantity = model.Quantity,
                        Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) agregó un nuevo producto CORCEL.\n"
                    };

                    _corcelProductService.Insert(corcelProduct);
                }
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        public IActionResult CorcelProductUpdate(CorcelProductModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.CorcelProduct))
                return AccessDeniedView();

            if (model.ProductId < 1 || model.Quantity < 1)
                return BadRequest("Información incompleta");

            var corcelProduct = _corcelProductService.GetAll()
                .Where(x => x.Id == model.Id).FirstOrDefault();
            if (corcelProduct == null)
                return Ok();

            if (corcelProduct.Quantity != model.Quantity)
            {
                corcelProduct.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) modificó " +
                    $"la cantidad del producto CORCEL de {corcelProduct.Quantity} a {model.Quantity}.\n";
                corcelProduct.Quantity = model.Quantity;
            }
            _corcelProductService.Update(corcelProduct);
            return Ok();
        }

        [HttpPost]
        public IActionResult CorcelProductDelete(CorcelProductModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.CorcelProduct))
                return AccessDeniedView();
            var corcelProduct = _corcelProductService.GetAll()
                .Where(x => x.Id == model.Id).FirstOrDefault();
            if (corcelProduct == null)
                return Ok();

            corcelProduct.Deleted = true;
            _corcelProductService.Update(corcelProduct);
            return Ok();
        }

        [HttpPost]
        public IActionResult GetProductsFiltering(FilteringModel model)
        {
            if (!string.IsNullOrEmpty(model.Text) || model.ByIds.Any() || model.Texts.Any())
            {
                var products = _productService.GetAllProductsQuery()
                        .Where(x => !x.Deleted);
                if (model.ByIds.Any())
                {
                    var byProductIds = model.ByIds.Distinct().ToList();
                    var filtered = products
                        .Where(x => byProductIds.Contains(x.Id))
                        .ToList()
                        .Select(x => new
                        {
                            id = x.Id.ToString(),
                            name = $"{x.Name}",
                        })
                        .OrderBy(x => x.name)
                        .ToList();
                    return Json(filtered);
                }
                if (!string.IsNullOrEmpty(model.Text))
                {
                    var text = model.Text.Trim().ToLower();
                    var filtered = products
                        .Where(x => x.Name.ToLower().Contains(text))
                        .Select(x => new { x.Name, x.Id }).ToList();
                    var productsFilter = filtered.Select(x => new
                    {
                        id = x.Id,
                        name = x.Name
                    })
                        .OrderBy(x => x.name)
                        .ToList();

                    return Json(productsFilter);
                }
            }
            return Ok();
        }

        public class FilteringModel
        {
            public FilteringModel()
            {
                Texts = new List<string>();
                ByIds = new List<int>();
            }

            public string Text { get; set; }
            public IList<string> Texts { get; set; }
            public IList<int> ByIds { get; set; }

            public int TypeId { get; set; }
        }
    }
}