using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using Teed.Plugin.ShoppingCartUrlGenerator.Domain;
using Teed.Plugin.ShoppingCartUrlGenerator.Models;
using Teed.Plugin.ShoppingCartUrlGenerator.Security;
using Teed.Plugin.ShoppingCartUrlGenerator.Services;

namespace Teed.Plugin.ShoppingCartUrlGenerator.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class ShoppingCartUrlController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ShoppingCartUrlService _shoppingCartUrlService;
        private readonly ShoppingCartUrlProductService _shoppingCartUrlProductService;

        public ShoppingCartUrlController(IPermissionService permissionService, ShoppingCartUrlService shoppingCartUrlService,
            IProductService productService, IShoppingCartService shoppingCartService, IWorkContext workContext,
            ShoppingCartUrlProductService shoppingCartUrlProductService, IStoreContext storeContext)
        {
            _permissionService = permissionService;
            _shoppingCartUrlService = shoppingCartUrlService;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _shoppingCartUrlProductService = shoppingCartUrlProductService;
            _storeContext = storeContext;
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedShoppingCartUrlPermissionProvider.ShoppingCartUrl))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.ShoppingCartUrlGenerator/Views/ShoppingCartUrl/Index.cshtml");
        }

        public IActionResult WidgetGenerator()
        {
            if (!_permissionService.Authorize(TeedShoppingCartUrlPermissionProvider.ShoppingCartUrl))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.ShoppingCartUrlGenerator/Views/ShoppingCartUrl/WidgetGenerator.cshtml");
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(TeedShoppingCartUrlPermissionProvider.ShoppingCartUrl))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.ShoppingCartUrlGenerator/Views/ShoppingCartUrl/Create.cshtml");
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedShoppingCartUrlPermissionProvider.ShoppingCartUrl))
                return AccessDeniedView();

            var shoppingCartUrl = _shoppingCartUrlService.GetById(id);
            if (shoppingCartUrl == null) return NotFound();

            var productIds = shoppingCartUrl.ShoppingCartUrlProducts.Select(x => x.ProductId).ToList();
            var products = _productService.GetAllProductsQuery().Where(x => productIds.Contains(x.Id) && !x.Deleted).ToList();

            ShoppingCartUrlModel model = new ShoppingCartUrlModel()
            {
                Id = shoppingCartUrl.Id,
                IsActive = shoppingCartUrl.IsActive,
                Code = shoppingCartUrl.Code,
                Body = shoppingCartUrl.Body,
                SelectedProductsData = shoppingCartUrl.ShoppingCartUrlProducts.Where(y => !y.Deleted).Select(x => new CreateEditShoppingCartUrlData()
                {
                    ProductId = x.ProductId,
                    ProductProperty = x.SelectedPropertyOption,
                    ProductQty = x.Quantity,
                    ProductUnit = x.BuyingBySecondary ? 1 : 0,
                    ProductName = products.Where(y => y.Id == x.ProductId).Select(y => y.Name).FirstOrDefault(),
                    ProductGroceryQuantity = GetGroceryQuantity(products.Where(y => y.Id == x.ProductId).FirstOrDefault(), x.BuyingBySecondary, x.Quantity)
                }).ToList()
            };

            return View("~/Plugins/Teed.Plugin.ShoppingCartUrlGenerator/Views/ShoppingCartUrl/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult CreateShoppingCartUrl(CreateEditShoppingCartUrlModel model)
        {
            if (!_permissionService.Authorize(TeedShoppingCartUrlPermissionProvider.ShoppingCartUrl))
                return AccessDeniedView();

            if (string.IsNullOrEmpty(model.UrlCode)) return BadRequest();

            var codeExists = _shoppingCartUrlService.GetAll().Where(x => x.Code.ToLower() == model.UrlCode.Trim().ToLower()).Any();
            if (codeExists) return BadRequest("Ya existe un carrito con el código seleccionado. Por favor utiliza otro código.");

            if (model.Products.GroupBy(x => x.ProductId).Where(x => x.Count() > 1).Any())
                return BadRequest("Uno de los productos está repetido. No puedes agregar dos veces el mismo producto en una sola URL.");

            ShoppingCartUrl shoppingCartUrl = new ShoppingCartUrl()
            {
                IsActive = model.IsActive,
                Code = model.UrlCode.Trim(),
                Body = model.Body
            };
            _shoppingCartUrlService.Insert(shoppingCartUrl);

            foreach (var product in model.Products)
            {
                _shoppingCartUrlProductService.Insert(new ShoppingCartUrlProduct()
                {
                    ProductId = product.ProductId,
                    SelectedPropertyOption = product.ProductProperty,
                    Quantity = product.ProductQty,
                    BuyingBySecondary = product.ProductUnit == 1,
                    ShoppingCartUrlId = shoppingCartUrl.Id
                });
            }

            return Ok(shoppingCartUrl.Id);
        }

        [HttpPost]
        public IActionResult EditShoppingCartUrl(CreateEditShoppingCartUrlModel model)
        {
            if (!_permissionService.Authorize(TeedShoppingCartUrlPermissionProvider.ShoppingCartUrl))
                return AccessDeniedView();

            ShoppingCartUrl shoppingCartUrl = _shoppingCartUrlService.GetById(model.Id);
            if (shoppingCartUrl == null) return NotFound();

            if (shoppingCartUrl.Code.ToLower().Trim() != model.UrlCode.ToLower().Trim())
            {
                var codeExists = _shoppingCartUrlService.GetAll().Where(x => x.Code.ToLower() == model.UrlCode.Trim().ToLower()).Any();
                if (codeExists) return BadRequest("Ya existe un carrito con el código seleccionado. Por favor utiliza otro código.");
            }

            if (model.Products != null && model.Products.GroupBy(x => x.ProductId).Where(x => x.Count() > 1).Any())
                return BadRequest("Uno de los productos está repetido. No puedes agregar dos veces el mismo producto en una sola URL.");

            shoppingCartUrl.Code = model.UrlCode.Trim();
            shoppingCartUrl.IsActive = model.IsActive;
            shoppingCartUrl.Body = model.Body;

            _shoppingCartUrlService.Update(shoppingCartUrl);

            if (model.Products != null && model.Products.Count > 0)
            {
                foreach (var product in model.Products)
                {
                    _shoppingCartUrlProductService.Insert(new ShoppingCartUrlProduct()
                    {
                        ProductId = product.ProductId,
                        SelectedPropertyOption = product.ProductProperty,
                        Quantity = product.ProductQty,
                        BuyingBySecondary = product.ProductUnit == 1,
                        ShoppingCartUrlId = shoppingCartUrl.Id
                    });
                }
            }

            return Ok(shoppingCartUrl.Id);
        }

        [HttpGet]
        public IActionResult Delete(int elementId)
        {
            if (!_permissionService.Authorize(TeedShoppingCartUrlPermissionProvider.ShoppingCartUrl))
                return AccessDeniedView();

            ShoppingCartUrl shoppingCartUrl = _shoppingCartUrlService.GetById(elementId);
            if (shoppingCartUrl == null) return NotFound();
            _shoppingCartUrlService.Delete(shoppingCartUrl);

            return NoContent();
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedShoppingCartUrlPermissionProvider.ShoppingCartUrl))
                return AccessDeniedView();

            var query = _shoppingCartUrlService.GetAll();
            var pagedList = new PagedList<ShoppingCartUrl>(query.OrderByDescending(m => m.CreatedOnUtc), command.Page - 1, command.PageSize);

            DataSourceResult gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    CreationDate = x.CreatedOnUtc.ToLocalTime().ToString("dd-MM-yyyy"),
                    Url = $"{_storeContext.CurrentStore.SecureUrl}sc/{x.Code}",
                    IsActive = x.IsActive ? "Si" : "No",
                    ProductsCount = x.ShoppingCartUrlProducts.Where(y => !y.Deleted).Count()
                }),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        [HttpGet]
        public IActionResult ProductListData()
        {
            if (!_permissionService.Authorize(TeedShoppingCartUrlPermissionProvider.ShoppingCartUrl))
                return AccessDeniedView();

            var value = Request.Query["filter[filters][0][value]"];
            int.TryParse(value, out int intValue);
            if (string.IsNullOrWhiteSpace(value)) return Json(string.Empty);

            var products = _productService.GetAllProductsQuery()
                .Where(x => x.Published)
                .Where(x => x.Name.Contains(value) || x.Sku.Contains(value) || x.Id == intValue)
                .Select(x => new ProductData()
                {
                    Id = x.Id,
                    Product = x.Name
                }).ToList();

            return Json(products);
        }

        [HttpGet]
        public IActionResult SearchListData()
        {
            if (!_permissionService.Authorize(TeedShoppingCartUrlPermissionProvider.ShoppingCartUrl))
                return AccessDeniedView();

            var value = Request.Query["filter[filters][0][value]"];
            if (string.IsNullOrWhiteSpace(value)) return Json(string.Empty);
            string valueParsed = value.ToString().ToLower().Trim();

            var urls = _shoppingCartUrlService.GetAll()
                .Where(x => x.Code.ToLower().Contains(valueParsed))
                .Select(x => new
                {
                    x.Id,
                    ShoppingCartUrl = x.Code + " (" + x.ShoppingCartUrlProducts.Where(y => !y.Deleted).Count() + " productos)"
                }).ToList();

            return Json(urls);
        }

        [HttpGet]
        public IActionResult GetWidgetProducts(int id)
        {
            if (!_permissionService.Authorize(TeedShoppingCartUrlPermissionProvider.ShoppingCartUrl))
                return AccessDeniedView();

            ShoppingCartUrl shoppingCartUrl = _shoppingCartUrlService.GetById(id);
            if (shoppingCartUrl == null) return NotFound();
            var urlProducts = shoppingCartUrl.ShoppingCartUrlProducts.Where(x => !x.Deleted).ToList();
            var urlProductIds = urlProducts.Select(x => x.ProductId).ToList();
            var products = _productService.GetAllProductsQuery().Where(x => urlProductIds.Contains(x.Id)).ToList();
            var result = urlProducts.Select(x => new
            {
                x.SelectedPropertyOption,
                Quantity = GetGroceryQuantity(products.Where(y => y.Id == x.ProductId).FirstOrDefault(), x.BuyingBySecondary, x.Quantity),
                Product = products.Where(y => y.Id == x.ProductId).Select(y => new
                {
                    y.Id,
                    y.Name,
                    y.Published
                }).FirstOrDefault()
            }).ToList();

            return Ok(result);
        }

        [HttpGet]
        public IActionResult GetProductData(int id)
        {
            if (!_permissionService.Authorize(TeedShoppingCartUrlPermissionProvider.ShoppingCartUrl))
                return AccessDeniedView();

            Product product = _productService.GetProductById(id);
            if (product == null) return NotFound();
            return Ok(new { product.EquivalenceCoefficient, product.WeightInterval, product.PropertiesOptions });
        }

        //[HttpGet]
        //[AllowAnonymous]
        //[Route("sc/{urlCode}")]
        //public IActionResult AddProductsToCart(string urlCode)
        //{
        //    ShoppingCartUrl shoppingCartUrl = _shoppingCartUrlService.GetAll().Where(x => x.IsActive && x.Code.ToUpper() == urlCode.ToUpper()).FirstOrDefault();
        //    if (shoppingCartUrl == null) return NotFound();

        //    //var idsList = shoppingCartUrl.ProductIds.Trim().Split(',').Select(x => int.Parse(x)).ToList();

        //    //foreach (var productId in idsList)
        //    //{
        //    //    Product product = _productService.GetProductById(productId);
        //    //    if (product == null) continue;

        //    //    _shoppingCartService.AddToCart(_workContext.CurrentCustomer, product,
        //    //        ShoppingCartType.ShoppingCart, 0, quantity: 1, buyingBySecondary: true, selectedPropertyOption: "");
        //    //}
        //    //AddToCartUrlProducts

        //    return View("~/Themes/TeedMaterialV2/Views/ShoppingCart/ShoppingCartUrl.cshtml", urlCode);
        //}

        [HttpGet]
        public IActionResult DeleteProduct(int cartUrlId, int productId)
        {
            if (!_permissionService.Authorize(TeedShoppingCartUrlPermissionProvider.ShoppingCartUrl))
                return AccessDeniedView();

            var element = _shoppingCartUrlProductService.GetAll().Where(x => x.ProductId == productId && x.ShoppingCartUrlId == cartUrlId).FirstOrDefault();
            if (element == null) return NotFound();
            _shoppingCartUrlProductService.Delete(element);

            return NoContent();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult GetVariableData(string variable)
        {
            if (string.IsNullOrWhiteSpace(variable)) return NotFound();
            var variablesArray = variable.Split(';');
            var shoppingCartVariable = variablesArray.Where(x => x.Contains("sc")).FirstOrDefault();
            if (string.IsNullOrEmpty(shoppingCartVariable)) return NotFound();
            var shoppingCartProductVariables = variablesArray.Where(x => !x.Contains("sc")).ToList();

            int.TryParse(shoppingCartVariable.Split(':').LastOrDefault(), out int shoppingCartId);
            ShoppingCartUrl shoppingCartUrl = _shoppingCartUrlService.GetById(shoppingCartId);
            if (shoppingCartUrl == null) return NotFound();
            if (!shoppingCartUrl.IsActive) return NoContent();

            var urlProducts = shoppingCartUrl.ShoppingCartUrlProducts.Where(x => !x.Deleted).ToList();
            var urlProductIds = urlProducts.Select(x => x.ProductId).ToList();
            var products = _productService.GetAllProductsQuery().Where(x => urlProductIds.Contains(x.Id)).ToList();

            var result = new List<object>();
            foreach (var item in shoppingCartProductVariables)
            {
                if (string.IsNullOrEmpty(item)) continue;
                var variableArray = item.Split(':');
                int.TryParse(variableArray.FirstOrDefault(), out int productId);
                result.Add(new
                {
                    Id = productId,
                    Text = string.IsNullOrWhiteSpace(variableArray.LastOrDefault()) ? products.Where(x => x.Id == productId).FirstOrDefault()?.Name : variableArray.LastOrDefault()
                });
            }

            return Ok(new { ShoppingCartUrlCode = shoppingCartUrl.Code, Products = result });
        }

        private string GetGroceryQuantity(Product product, bool buyingBySecondary, int quantity)
        {
            if (product == null) return "";
            string unit = "pz";
            decimal weight = quantity;
            if (product.EquivalenceCoefficient > 0 && buyingBySecondary)
            {
                weight = (quantity * 1000) / product.EquivalenceCoefficient;
                unit = "gr";
            }
            else if (product.WeightInterval > 0)
            {
                weight = quantity * product.WeightInterval;
                unit = "gr";
            }

            if (weight >= 1000)
            {
                weight = (weight / 1000);
                unit = "kg";
            }

            return Math.Round(weight, 2) + " " + unit;
        }
    }
}
