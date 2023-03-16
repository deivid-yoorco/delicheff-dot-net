using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teed.Plugin.Manager.Domain.PurchaseOrders;
using Teed.Plugin.Manager.Models.PurchaseOrder;
using Teed.Plugin.Manager.Services;
using Teed.Plugin.Medical.Security;

namespace Teed.Plugin.Manager.Controllers
{
    [Area(AreaNames.Admin)]
    public class PurchaseOrderController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IProductService _productService;
        private readonly IShippingService _shippingService;
        private readonly PurchaseOrderService _purchaseOrderService;
        private readonly PurchaseOrderProductService _purchaseOrderProductService;

        public PurchaseOrderController(IPermissionService permissionService,
            ICustomerService customerService,
            IWorkContext workContext,
            IShippingService shippingService,
            IProductService productService,
            PurchaseOrderService purchaseOrderService,
            PurchaseOrderProductService purchaseOrderProductService)
        {
            _permissionService = permissionService;
            _purchaseOrderService = purchaseOrderService;
            _workContext = workContext;
            _productService = productService;
            _purchaseOrderProductService = purchaseOrderProductService;
            _customerService = customerService;
            _shippingService = shippingService;
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedPurchaseOrders))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Manager/Views/PurchaseOrder/List.cshtml");
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedPurchaseOrders))
                return AccessDeniedView();

            ViewData["Branches"] = _shippingService.GetAllWarehouses().ToList();
            return View("~/Plugins/Teed.Plugin.Manager/Views/PurchaseOrder/Create.cshtml");
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedPurchaseOrders))
                return AccessDeniedView();

            PurchaseOrder purchaseOrder = _purchaseOrderService.GetById(id);
            if (purchaseOrder == null) return NotFound();

            EditPurchaseOrderModel model = new EditPurchaseOrderModel()
            {
                Comment = purchaseOrder.Comment,
                Log = purchaseOrder.Log,
                Id = purchaseOrder.Id,
                PurchaseOrderStatus = purchaseOrder.PurchaseOrderStatus,
                PartialPaymentValue = purchaseOrder.PartialPaymentValue,
                PaymentStatus = purchaseOrder.PaymentStatus
            };

            return View("~/Plugins/Teed.Plugin.Manager/Views/PurchaseOrder/Edit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int branchId, string comment, string productsJson)
        {
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedPurchaseOrders))
                return AccessDeniedView();

            PurchaseOrder purchaseOrder = new PurchaseOrder()
            {
                BranchId = branchId,
                Comment = comment,
                PurchaseOrderStatus = PurchaseOrderStatus.Pending,
                PaymentStatus = PurchaseOrderPaymentStatus.Pending,
                RequestedById = _workContext.CurrentCustomer.Id,
                RequestedDateUtc = DateTime.UtcNow,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - ODC creada por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})."
            };
            _purchaseOrderService.Insert(purchaseOrder);

            var products = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CreatePurchaseOrderProductModel>>(productsJson);
            foreach (var product in products)
            {
                var productWarehouse = _productService.GetProductById(product.Id)?.ProductWarehouseInventory.Where(x => x.WarehouseId == branchId).FirstOrDefault();

                PurchaseOrderProduct purchaseOrderProduct = new PurchaseOrderProduct()
                {
                    PurchaseOrderId = purchaseOrder.Id,
                    ProductId = product.Id,
                    RequestedUnits = product.Units,
                    CurrentInventory = productWarehouse == null ? 0 : productWarehouse.StockQuantity,
                    CustomProductName = product.ProductName
                };
                _purchaseOrderProductService.Insert(purchaseOrderProduct);
            };

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int purchaseOrderId, int branchId, string comment, int purchaseOrderStatus, int purchaseOrderPaymentStatus, decimal partialPaymentValue, string productsJson)
        {
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedPurchaseOrders))
                return AccessDeniedView();

            PurchaseOrder purchaseOrder = _purchaseOrderService.GetById(purchaseOrderId);
            if (purchaseOrder == null) return NotFound();

            purchaseOrder.BranchId = branchId;
            purchaseOrder.Comment = comment;
            purchaseOrder.PurchaseOrderStatus = (PurchaseOrderStatus)purchaseOrderStatus;
            purchaseOrder.PaymentStatus = (PurchaseOrderPaymentStatus)purchaseOrderPaymentStatus;
            purchaseOrder.PartialPaymentValue = partialPaymentValue;

            var currentCustomer = _workContext.CurrentCustomer;
            switch ((PurchaseOrderStatus)purchaseOrderStatus)
            {
                case PurchaseOrderStatus.Pending:
                    purchaseOrder.Log = purchaseOrder.Log + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - PurchaseOrder marcada como \"Pendiente\" por " + currentCustomer.GetFullName() + $" ({currentCustomer.Id})" + ".";
                    break;
                case PurchaseOrderStatus.Reviewing:
                    purchaseOrder.Log = purchaseOrder.Log + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - PurchaseOrder marcada como \"En revisión\" por " + currentCustomer.GetFullName() + $" ({currentCustomer.Id})" + ".";
                    break;
                case PurchaseOrderStatus.Approved:
                    purchaseOrder.Log = purchaseOrder.Log + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - PurchaseOrder marcada como \"Aprobada\" por " + currentCustomer.GetFullName() + $" ({currentCustomer.Id})" + ".";
                    break;
                case PurchaseOrderStatus.Requested:
                    purchaseOrder.Log = purchaseOrder.Log + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - PurchaseOrder marcada como \"Solicitada al Proveedor\" por " + currentCustomer.GetFullName() + $" ({currentCustomer.Id})" + ".";
                    break;
                case PurchaseOrderStatus.Delivered:
                    purchaseOrder.Log = purchaseOrder.Log + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - PurchaseOrder marcada como \"Entregado\" por " + currentCustomer.GetFullName() + $" ({currentCustomer.Id})" + ".";
                    break;
            }
            _purchaseOrderService.Update(purchaseOrder);

            var newProducts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CreatePurchaseOrderProductModel>>(productsJson);
            var existingProducts = _purchaseOrderProductService.GetAll().Where(x => x.PurchaseOrderId == purchaseOrderId).Select(x => new CreatePurchaseOrderProductModel() { Id = x.ProductId, Units = x.RequestedUnits }).ToList();
            foreach (var product in existingProducts)
            {
                if (!newProducts.Select(x => x.Id).Contains(product.Id))
                {
                    PurchaseOrderProduct purchaseOrderProductObject = _purchaseOrderProductService.GetAll().Where(x => x.PurchaseOrderId == purchaseOrder.Id && x.ProductId == product.Id).FirstOrDefault();
                    _purchaseOrderProductService.Delete(purchaseOrderProductObject);
                }
            }
            foreach (var newProduct in newProducts)
            {
                var existingProduct = _purchaseOrderProductService.GetAll().Where(x => x.ProductId == newProduct.Id && x.PurchaseOrderId == purchaseOrder.Id).FirstOrDefault();
                var productWarehouse = _productService.GetProductById(newProduct.Id)?.ProductWarehouseInventory.Where(x => x.WarehouseId == branchId).FirstOrDefault();
                if (existingProduct == null)
                {
                    PurchaseOrderProduct purchaseOrderProduct = new PurchaseOrderProduct()
                    {
                        PurchaseOrderId = purchaseOrder.Id,
                        ProductId = newProduct.Id,
                        RequestedUnits = newProduct.Units,
                        CurrentInventory = productWarehouse == null ? 0 : productWarehouse.StockQuantity
                    };
                    _purchaseOrderProductService.Insert(purchaseOrderProduct);
                }
                else
                {
                    if (existingProduct.RequestedUnits != newProduct.Units) existingProduct.RequestedUnits = newProduct.Units;
                    if (purchaseOrder.BranchId != branchId) existingProduct.RequestedUnits = productWarehouse == null ? 0 : productWarehouse.StockQuantity;
                    _purchaseOrderProductService.Update(existingProduct);
                }
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult ProductListData()
        {
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedPurchaseOrders))
                return AccessDeniedView();

            var products = _productService.SearchProducts().Select(x => new ProductsListModel
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
            return Json(products);
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command, int branchId = 0, string filterDate = null)
        {
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedPurchaseOrders))
                return AccessDeniedView();

            var count = _purchaseOrderService.GetAll();
            var purchaseOrderList = _purchaseOrderService.ListAsNoTracking(command.Page - 1, command.PageSize, branchId, filterDate).AsEnumerable();

            if (purchaseOrderList == null) return NotFound();

            if (branchId != 0)
            {
                count = count.Where(x => x.BranchId == branchId);
            }

            var purchaseOrders = purchaseOrderList.ToList().OrderByDescending(x => x.RequestedDateUtc).Select(x => new PurchaseOrderListModel()
            {
                Id = x.Id,
                PurchaseOrderStatus = EnumHelper.GetDisplayName(x.PurchaseOrderStatus),
                RequestedDate = x.RequestedDateUtc.ToLocalTime().ToString("dd MMMM yyyy - hh:mm tt", new CultureInfo("es-MX")),
                Comments = x.Comment,
                RequestedBy = _customerService.GetCustomerById(x.RequestedById)?.GetFullName(),
            }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = purchaseOrders,
                Total = count.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult RequestedPurchaseOrder(int id)
        {
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedPurchaseOrders))
                return AccessDeniedView();

            PurchaseOrder purchaseOrder = _purchaseOrderService.GetById(id);
            if (purchaseOrder == null) return NotFound();

            purchaseOrder.PurchaseOrderStatus = PurchaseOrderStatus.Requested;
            purchaseOrder.Log = purchaseOrder.Log + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - ODC marcada como \"Solicitada al proveedor\" por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}).";
            _purchaseOrderService.Update(purchaseOrder);

            return Ok();
        }

        [HttpPost]
        public IActionResult ApprovedPurchaseOrder(int id)
        {
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedPurchaseOrders))
                return AccessDeniedView();

            PurchaseOrder purchaseOrder = _purchaseOrderService.GetById(id);
            if (purchaseOrder == null) return NotFound();

            purchaseOrder.PurchaseOrderStatus = PurchaseOrderStatus.Approved;
            purchaseOrder.Log = purchaseOrder.Log + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - ODC marcada como \"Aprobada\" por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}).";
            _purchaseOrderService.Update(purchaseOrder);

            return Ok();
        }

        [HttpPost]
        public IActionResult DeliveredPurchaseOrder(int id)
        {
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedPurchaseOrders))
                return AccessDeniedView();

            PurchaseOrder purchaseOrder = _purchaseOrderService.GetById(id);
            if (purchaseOrder == null) return NotFound();

            purchaseOrder.PurchaseOrderStatus = PurchaseOrderStatus.Delivered;
            purchaseOrder.Log = purchaseOrder.Log + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - ODC marcada como \"Entregada\" por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}).";

            List<PurchaseOrderProduct> products = _purchaseOrderProductService.GetAll().ToList();
            foreach (var purchaseOrderProduct in products)
            {
                Product product = _productService.GetProductById(purchaseOrderProduct.ProductId);
                if (product != null)
                {
                    ProductWarehouseInventory productWarehouseInventory = product.ProductWarehouseInventory.Where(x => x.WarehouseId == purchaseOrder.BranchId).FirstOrDefault();
                    if (productWarehouseInventory != null)
                    {
                        int previousStockQuantityIn = productWarehouseInventory.StockQuantity;
                        productWarehouseInventory.StockQuantity = productWarehouseInventory.StockQuantity + purchaseOrderProduct.RequestedUnits;
                        _productService.UpdateProduct(product);
                        _productService.AddStockQuantityHistoryEntry(product, productWarehouseInventory.StockQuantity - previousStockQuantityIn, productWarehouseInventory.StockQuantity,
                            productWarehouseInventory.WarehouseId, "Se agregaron cantidades al producto desde ODC.");
                    }
                }
            }

            _purchaseOrderService.Update(purchaseOrder);

            return Ok();
        }

        [HttpPost]
        public IActionResult ReviewingPurchaseOrder(int id)
        {
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedPurchaseOrders))
                return AccessDeniedView();

            PurchaseOrder purchaseOrder = _purchaseOrderService.GetById(id);
            if (purchaseOrder == null) return NotFound();

            purchaseOrder.PurchaseOrderStatus = PurchaseOrderStatus.Reviewing;
            purchaseOrder.Log = purchaseOrder.Log + " \n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - ODC marcada como \"En revisión\" por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}).";
            _purchaseOrderService.Update(purchaseOrder);

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetSelectedProducts(int id)
        {
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedPurchaseOrders))
                return AccessDeniedView();

            var purchaseOrderProduct = _purchaseOrderProductService.GetAll()
                .AsEnumerable()
                .Where(x => x.PurchaseOrderId == id)
                .Select(x => new EditPurchaseOrderProductModel()
                {
                    Id = x.ProductId,
                    Units = x.RequestedUnits,
                    ProductName = GetProductName(x.ProductId, x.CustomProductName)
                }).ToList();

            return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(purchaseOrderProduct));
        }

        private string GetProductName(int productId, string defaultName)
        {
            var product = _productService.GetProductById(productId);
            return product == null ? defaultName : product.Name;
        }
    }

    public class ProductsListModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
