using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Areas.Admin.Models.Home;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Helpers;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Services.Catalog;
using Nop.Core.Domain.Catalog;
using System.Collections.Generic;
using Nop.Web.Utils;
using Nop.Services.Media;
using Nop.Core.Data;
using Nop.Services.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using System;
using Nop.Web.Framework.Kendoui;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;

namespace Nop.Web.Areas.Admin.Controllers
{
    public class DashController : BaseAdminController
    {
        #region Fields

        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;

        private readonly IProductService _productService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IPictureService _pictureService;
        private readonly IRepository<RelatedProduct> _relatedProductRepository;
        private readonly IRepository<CrossSellProduct> _crossSellProductRepository;
        private readonly IOrderService _orderService;
        private readonly IPriceLogService _priceLogService;
        private readonly IStockLogService _stockLogService;

        #endregion

        #region Ctor

        public DashController(AdminAreaSettings adminAreaSettings,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IWorkContext workContext,
            IProductService productService,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IPictureService pictureService,
            IRepository<RelatedProduct> relatedProductRepository,
            IRepository<CrossSellProduct> crossSellProductRepository,
            IOrderService orderService,
            IPriceLogService priceLogService,
            IStockLogService stockLogService)
        {
            this._adminAreaSettings = adminAreaSettings;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._settingService = settingService;
            this._workContext = workContext;
            this._productService = productService;
            this._productAttributeParser = productAttributeParser;
            this._productAttributeService = productAttributeService;
            this._pictureService = pictureService;
            this._relatedProductRepository = relatedProductRepository;
            this._crossSellProductRepository = crossSellProductRepository;
            this._orderService = orderService;
            this._priceLogService = priceLogService;
            this._stockLogService = stockLogService;
        }
        
        #endregion
        
        #region Methods

        [HttpPost]
        public IActionResult PriceChanges(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return Unauthorized();

            var priceLogQuery = _priceLogService.GetAllQuery().OrderByDescending(x => x.CreatedOnUtc);
            if (_workContext.CurrentCustomer.IsInCustomerRole("Vendors"))
            {
                var vendorProductIds = _productService.SearchProducts(vendorId: _workContext.CurrentVendor.Id, showHidden: true).Select(x => x.Id);
                priceLogQuery = priceLogQuery.Where(x => vendorProductIds.Contains(x.ProductId)).OrderByDescending(x => x.CreatedOnUtc);
            }

            var pagedList = new PagedList<PriceLog>(priceLogQuery, command.Page - 1, command.PageSize);

            var data = pagedList.Select(x => new PriceLogModel()
            {
                CreatedOnUtc = x.CreatedOnUtc.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt"),
                NewPrice = x.NewPrice,
                OldPrice = x.OldPrice,
                Product = x.Product,
                ProductId = x.ProductId,
                SKU = x.SKU,
                User = x.User
            }).OrderByDescending(x => x.CreatedOnUtc);

            var gridModel = new DataSourceResult
            {
                Data = data,
                Total = priceLogQuery.Count()
            };

            return Json(gridModel);
        }

        protected virtual DataSourceResult GetPriceLogPages(int pageIndex, int pageSize, IQueryable<PriceLogModel> query)
        {
            var data = new PagedList<PriceLogModel>(query, pageIndex, pageSize);

            var gridModel = new DataSourceResult
            {
                Data = data,
                Total = data.TotalCount
            };

            return gridModel;
        }

        public IActionResult StockChanges(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return Unauthorized();

            var stockLogQuery = _stockLogService.GetAllQuery();
            if (_workContext.CurrentCustomer.IsInCustomerRole("Vendors"))
            {
                var vendorProductIds = _productService.SearchProducts(vendorId: _workContext.CurrentVendor.Id, showHidden: true).Select(x => x.Id);
                stockLogQuery = stockLogQuery.Where(x => vendorProductIds.Contains(x.ProductId));
            }

            var pagedList = new PagedList<StockLog>(stockLogQuery.OrderByDescending(x => x.CreatedOnUtc), command.Page - 1, command.PageSize);

            var data = pagedList.Select(x => new StockLogModel()
            {
                ChangeType = EnumHelper.GetDisplayName(x.ChangeType),
                CreatedOnUtc = x.CreatedOnUtc.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt"),
                NewStock = x.NewStock,
                OldStock = x.OldStock,
                Product = x.Product,
                ProductId = x.ProductId,
                SKU = x.SKU,
                User = x.User
            }).OrderByDescending(x => x.CreatedOnUtc).AsQueryable();

            var gridModel = new DataSourceResult
            {
                Data = data,
                Total = stockLogQuery.Count()
            };

            return Json(gridModel);
        }

        protected virtual DataSourceResult GetStockLogPages(int pageIndex, int pageSize, IQueryable<StockLogModel> query)
        {
            var data = new PagedList<StockLogModel>(query, pageIndex, pageSize);

            var gridModel = new DataSourceResult
            {
                Data = data,
                Total = data.TotalCount
            };

            return gridModel;
        }

        public IActionResult ProductsWithStockUnpublished()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return Unauthorized();
            
            var unpublishProducts = _productService.GetAllProductsQuery().Where(x => !x.Published);
            if(_workContext.CurrentCustomer.IsInCustomerRole("Vendors"))
            {
                int vendorId = _workContext.CurrentVendor.Id;
                unpublishProducts = unpublishProducts.Where(x => x.VendorId == vendorId);
            }

            var products = new List<ProductDashModel>();
            foreach (var product in unpublishProducts.Take(20).ToList())
            {
                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                {
                    if(product.StockQuantity > 0)
                    {
                        products.Add(new ProductDashModel
                        {
                            Id = product.Id,
                            Name = product.Name,
                            StockQuantity = product.StockQuantity,
                            Image = product.ProductPictures.Count > 0 ? _pictureService.GetPictureUrl(product.ProductPictures.FirstOrDefault().Picture) : "https://" + HttpContext.Request.Host + "/images/thumbs/default-image_100.png"
                        });
                    }
                }
                else if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
                {
                    int stock = 0;
                    foreach (var attribute in product.ProductAttributeMappings)
                    {
                        var attr = attribute.ProductAttributeValues.FirstOrDefault();
                        foreach (var value in attr.ProductAttributeMapping.ProductAttributeValues)
                        {
                            if (!string.IsNullOrWhiteSpace(value.Id.ToString()))
                            {
                                stock += ProductUtils.GetProductStock(product, value.Id.ToString(), _productAttributeParser, _productAttributeService);
                            }
                        }
                    }
                    if (stock > 0)
                    {
                        products.Add(new ProductDashModel
                        {
                            Id = product.Id,
                            Name = product.Name,
                            StockQuantity = stock,
                            Image = product.ProductPictures.Count > 0 ? _pictureService.GetPictureUrl(product.ProductPictures.FirstOrDefault().Picture) : "https://" + HttpContext.Request.Host + "/images/thumbs/default-image_100.png"
                        });
                    }
                }
            }

            return Json(products);
        }

        public IActionResult ProductsPublishedWithoutStock()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return Unauthorized();

            var publishProducts = _productService.GetPublishedProducts();
            if (_workContext.CurrentCustomer.IsInCustomerRole("Vendors"))
            {
                publishProducts = publishProducts.Where(x => x.VendorId == _workContext.CurrentVendor.Id).ToList();
            }

            var products = new List<ProductDashModel>();
            foreach (var product in publishProducts.Take(20))
            {
                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                {
                    if (product.StockQuantity < 1)
                    {
                        products.Add(new ProductDashModel
                        {
                            Id = product.Id,
                            Name = product.Name,
                            StockQuantity = product.StockQuantity,
                            Image = product.ProductPictures.Count > 0 ? _pictureService.GetPictureUrl(product.ProductPictures.FirstOrDefault().Picture) : "https://" + HttpContext.Request.Host + "/images/thumbs/default-image_100.png"
                        });
                    }
                }
                else if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
                {
                    int stock = 0;
                    foreach (var attribute in product.ProductAttributeMappings)
                    {
                        var attr = attribute.ProductAttributeValues.FirstOrDefault();
                        foreach (var value in attr.ProductAttributeMapping.ProductAttributeValues)
                        {
                            if (!string.IsNullOrWhiteSpace(value.Id.ToString()))
                            {
                                stock += ProductUtils.GetProductStock(product, value.Id.ToString(), _productAttributeParser, _productAttributeService);
                            }
                        }
                    }
                    if (stock < 1)
                    {
                        products.Add(new ProductDashModel
                        {
                            Id = product.Id,
                            Name = product.Name,
                            StockQuantity = stock,
                            Image = product.ProductPictures.Count > 0 ? _pictureService.GetPictureUrl(product.ProductPictures.FirstOrDefault().Picture) : "https://" + HttpContext.Request.Host + "/images/thumbs/default-image_100.png"
                        });
                    }
                }
            }

            return Json(products);
        }

        public IActionResult ProductsLessMargin()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return Unauthorized();

            var publishedProducts = _productService.GetPublishedProducts();
            if (_workContext.CurrentCustomer.IsInCustomerRole("Vendors"))
            {
                publishedProducts = publishedProducts.Where(x => x.VendorId == _workContext.CurrentVendor.Id).ToList();
            }

            var products = new List<ProductDashModel>();
            products = publishedProducts.Where(x => x.Price > 0).OrderBy(x => x.ProductCost > 0 ? ((x.Price - x.ProductCost) / x.ProductCost) : 1).Take(20).Select(x => new ProductDashModel()
            {
                Id = x.Id,
                Image = x.ProductPictures.Count > 0 ? _pictureService.GetPictureUrl(x.ProductPictures.FirstOrDefault().Picture) : "https://"  + HttpContext.Request.Host + "/images/thumbs/default-image_100.png",
                Name = x.Name,
                Cost = x.ProductCost,
                Price = x.Price
            }).ToList();

            return Json(products);
        }

        public IActionResult RelatedProducts()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return Unauthorized();

            var publishedProducts = _productService.GetPublishedProducts();
            if (_workContext.CurrentCustomer.IsInCustomerRole("Vendors"))
            {
                publishedProducts = publishedProducts.Where(x => x.VendorId == _workContext.CurrentVendor.Id).ToList();
            }

            var related = _relatedProductRepository.TableNoTracking;
            var products = new List<ProductDashModel>();
            foreach (var product in publishedProducts.Take(20))
            {
                var count = related.Where(x => x.ProductId1 == product.Id).Count();
                if(count < 2)
                {
                    products.Add(new ProductDashModel()
                    {
                        Id = product.Id,
                        Image = product.ProductPictures.Count > 0 ? _pictureService.GetPictureUrl(product.ProductPictures.FirstOrDefault().Picture) : "https://" + HttpContext.Request.Host + "/images/thumbs/default-image_100.png",
                        Name = product.Name
                    });
                }
            }

            return Json(products);
        }

        public IActionResult CrosssellProducts()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return Unauthorized();

            var publishedProducts = _productService.GetPublishedProducts();
            if (_workContext.CurrentCustomer.IsInCustomerRole("Vendors"))
            {
                publishedProducts = publishedProducts.Where(x => x.VendorId == _workContext.CurrentVendor.Id).ToList();
            }

            var cross = _crossSellProductRepository.TableNoTracking;
            var products = new List<ProductDashModel>();

            foreach (var product in publishedProducts.Take(20))
            {
                var count = cross.Where(x => x.ProductId1 == product.Id).Count();
                if (count < 2)
                {
                    products.Add(new ProductDashModel()
                    {
                        Id = product.Id,
                        Image = product.ProductPictures.Count > 0 ? _pictureService.GetPictureUrl(product.ProductPictures.FirstOrDefault().Picture) : "https://" + HttpContext.Request.Host + "/images/thumbs/default-image_100.png",
                        Name = product.Name
                    });
                }
            }

            return Json(products);
        }

        [HttpPost]
        public IActionResult OrdersPaidNotSent(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return Unauthorized();

            var ordersQuery = _orderService.GetAllOrdersQuery().Where(x => x.PaymentStatusId == 30 && x.ShippingStatusId == 20);
            if (_workContext.CurrentCustomer.IsInCustomerRole("Vendors"))
            {
                ordersQuery = ordersQuery.Where(o => o.OrderItems
                    .Any(orderItem => orderItem.Product.VendorId == _workContext.CurrentVendor.Id));
            }

            var pagedList = new PagedList<Order>(ordersQuery.OrderByDescending(x => x.CreatedOnUtc), command.Page - 1, command.PageSize);

            var data = pagedList.Select(x => new OrdersDashModel()
            {
                Customer = $"{x.ShippingAddress.FirstName} {x.ShippingAddress.LastName}",
                Email = x.ShippingAddress.Email,
                Id = x.Id,
                PaidDate = x.PaidDateUtc.Value.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt"),
                StatusPaid = "Pagado",
                StatusSent = "No enviado"
            }).AsQueryable();

            var gridModel = new DataSourceResult
            {
                Data = data,
                Total = ordersQuery.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult OrdersSent(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return Unauthorized();

            IPagedList<Order> allOrders;
            if (_workContext.CurrentCustomer.IsInCustomerRole("Vendors"))
            {
                allOrders = _orderService.SearchOrders(vendorId: _workContext.CurrentVendor.Id);
            }
            else
            {
                allOrders = _orderService.SearchOrders();
            }

            var orders = allOrders.Where(x => x.PaymentStatus == PaymentStatus.Paid && x.ShippingStatus == ShippingStatus.Shipped).Select(x => new OrdersDashModel()
            {
                Customer = $"{x.ShippingAddress.FirstName} {x.ShippingAddress.LastName}",
                Email = x.ShippingAddress.Email,
                Id = x.Id,
                PaidDate = x.PaidDateUtc.Value.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt"),
                StatusSent = "Enviado"
            }).AsQueryable();

            var gridModel = GetOrdersPages(command.Page - 1, command.PageSize, orders);

            return Json(gridModel);
        }

        protected virtual DataSourceResult GetOrdersPages(int pageIndex, int pageSize, IQueryable<OrdersDashModel> query)
        {
            var data = new PagedList<OrdersDashModel>(query, pageIndex, pageSize);

            var gridModel = new DataSourceResult
            {
                Data = data,
                Total = data.TotalCount
            };

            return gridModel;
        }

        public IActionResult ProductPublish(int Id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return Unauthorized();

            var product = _productService.GetProductById(Id);
            product.Published = true;
            product.VisibleIndividually = true;
            product.ShowOnHomePage = true;

            _productService.UpdateProduct(product);

            return Ok();
        }

        public IActionResult ProductUnpublish(int Id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return Unauthorized();

            var product = _productService.GetProductById(Id);
            product.Published = false;
            product.VisibleIndividually = false;
            product.ShowOnHomePage = false;

            _productService.UpdateProduct(product);

            return Ok();
        }

        #endregion
    }

    public class ProductDashModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StockQuantity { get; set; }
        public string Image { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
    }

    public class OrdersDashModel
    {
        public int Id { get; set; }
        public string StatusPaid { get; set; }
        public string StatusSent { get; set; }
        public string PaidDate { get; set; }
        public string SentDate { get; set; }
        public string Customer { get; set; }
        public string Email { get; set; }
    }

    public class PriceLogModel
    {
        public string CreatedOnUtc { get; set; }
        public string SKU { get; set; }
        public string Product { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public string User { get; set; }
        public int ProductId { get; set; }
    }

    public class StockLogModel
    {
        public string CreatedOnUtc { get; set; }
        public string SKU { get; set; }
        public string Product { get; set; }
        public int OldStock { get; set; }
        public int NewStock { get; set; }
        public string User { get; set; }
        public int ProductId { get; set; }
        public string ChangeType { get; set; }
    }
}