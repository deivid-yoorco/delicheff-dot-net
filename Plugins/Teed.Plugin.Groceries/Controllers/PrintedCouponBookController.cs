using iText.Forms.Xfdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.PrintedCouponBooks;
using Teed.Plugin.Groceries.Models.PrintedCouponBook;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;
using Teed.Plugin.Manager.Models.Groceries;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class PrintedCouponBookController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly ShippingAreaService _shippingAreaService;
        private readonly PrintedCouponBooksService _printedCouponBooksService;
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ProductMainManufacturerService _productMainManufacturerService;

        public PrintedCouponBookController(IPermissionService permissionService,
            ShippingAreaService shippingAreaService,
            IWorkContext workContext,
            ICustomerService customerService,
            IOrderService orderService,
            PrintedCouponBooksService printedCouponBooksService,
            IProductService productService,
            IPictureService pictureService,
            IManufacturerService manufacturerService,
            ProductMainManufacturerService productMainManufacturerService)
        {
            _permissionService = permissionService;
            _shippingAreaService = shippingAreaService;
            _workContext = workContext;
            _customerService = customerService;
            _orderService = orderService;
            _printedCouponBooksService = printedCouponBooksService;
            _productService = productService;
            _pictureService = pictureService;
            _manufacturerService = manufacturerService;
            _productMainManufacturerService = productMainManufacturerService;
        }

        public PrintedCouponBookModel PrepareModel(PrintedCouponBookModel model)
        {
            model.AvailableZipCodes = string.Join(",", _shippingAreaService.GetAll().Select(x => x.PostalCode.Replace(" ", ""))).Split(',')
                .Distinct()
                .Select(x => new SelectListItem { Value = x, Text = x }).ToList();
            return model;
        }

        [AuthorizeAdmin]
        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.PrintedCouponBook))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/PrintedCouponBook/List.cshtml");
        }

        [AuthorizeAdmin]
        public IActionResult Create()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.PrintedCouponBook))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/PrintedCouponBook/Create.cshtml", PrepareModel(new PrintedCouponBookModel()));
        }

        [AuthorizeAdmin]
        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.PrintedCouponBook))
                return AccessDeniedView();

            PrintedCouponBook printedCouponBook = _printedCouponBooksService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (printedCouponBook == null) return NotFound();

            var orderIdsNotCancelled = OrderUtils.GetFilteredOrders(_orderService)
                .Select(x => x.Id).ToList();
            var couponBookItemsUsed = _orderService.GetAllOrderItemsQuery()
                .Where(x => printedCouponBook.ConnectedProductId == x.ProductId)
                .Select(x => x.OrderId)
                .ToList();
            var inventoryAvailable = printedCouponBook.Inventory - couponBookItemsUsed.Intersect(orderIdsNotCancelled).Count();
            PrintedCouponBookModel model = new PrintedCouponBookModel()
            {
                Id = printedCouponBook.Id,
                Name = printedCouponBook.Name,
                Active = printedCouponBook.Active,
                BookTypeId = printedCouponBook.BookTypeId,
                BookTypeValue = printedCouponBook.BookTypeValue,
                StartDate = printedCouponBook.StartDate,
                EndDate = printedCouponBook.EndDate,
                ReferencePictureId = printedCouponBook.ReferencePictureId,
                Inventory = printedCouponBook.Inventory,
                InventoryAvailable = inventoryAvailable,
                ConnectedProductId = printedCouponBook.ConnectedProductId,
                Log = printedCouponBook.Log,
            };

            if (!string.IsNullOrEmpty(model.BookTypeValue))
            {
                if (model.BookTypeId == (int)PrintedCouponBookType.Subtotal)
                    model.Subtotal = decimal.Parse(model.BookTypeValue);
                else if (model.BookTypeId == (int)PrintedCouponBookType.Client)
                    model.SelectedCustomerIds = model.BookTypeValue.Split(',').Select(x => int.Parse(x)).ToList();
            }

            return View("~/Plugins/Teed.Plugin.Groceries/Views/PrintedCouponBook/Edit.cshtml", PrepareModel(model));
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult Create(PrintedCouponBookModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.PrintedCouponBook))
                return AccessDeniedView();

            if (!ModelState.IsValid) return BadRequest();

            PrintedCouponBook printedCouponBook = new PrintedCouponBook()
            {
                Active = model.Active,
                BookTypeId = model.BookTypeId,
                Inventory = model.Inventory,
                BookTypeValue = model.BookTypeValue,
                Name = model.Name,
                ReferencePictureId = model.ReferencePictureId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Email}) creó una nueva cuponera impresa."
            };
            if (model.BookTypeId == (int)PrintedCouponBookType.Subtotal)
                printedCouponBook.BookTypeValue = model.Subtotal.ToString();
            else if (model.BookTypeId == (int)PrintedCouponBookType.Client)
                printedCouponBook.BookTypeValue = string.Join(",", model.SelectedCustomerIds);

            _printedCouponBooksService.Insert(printedCouponBook);

            try
            {
                var utcNow = DateTime.UtcNow;
                var product = new Product
                {
                    Name = model.Name,
                    Published = false,
                    Sku = $"PCB-{printedCouponBook.Id}",
                    PublishedDateUtc = utcNow,
                    CreatedOnUtc = utcNow,
                    UpdatedOnUtc = utcNow,
                };
                _productService.InsertProduct(product);
                printedCouponBook.ConnectedProductId = product.Id;
                _printedCouponBooksService.Update(printedCouponBook);

                _pictureService.SetSeoFilename(model.ReferencePictureId, _pictureService.GetPictureSeName(product.Name));
                _productService.InsertProductPicture(new ProductPicture
                {
                    PictureId = model.ReferencePictureId,
                    ProductId = product.Id,
                    DisplayOrder = 0,
                });

                var displayOrder = 1;
                var manufacturer = _manufacturerService.GetAllManufacturers()
                    .Where(x => x.Id == 0 || x.Name == "Cuponeras impresas").FirstOrDefault();
                _manufacturerService.InsertProductManufacturer(new ProductManufacturer()
                {
                    ProductId = product.Id,
                    ManufacturerId = manufacturer.Id,
                    DisplayOrder = displayOrder
                });
                var log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - " +
                    $"El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) asignó el fabricante principal " +
                    $"\"{manufacturer?.Name}\" ({manufacturer.Id}).\n";
                var currentProductMainManufacturer = new Domain.Product.ProductMainManufacturer()
                {
                    ManufacturerId = manufacturer.Id,
                    ProductId = product.Id,
                    Log = log
                };
                _productMainManufacturerService.Insert(currentProductMainManufacturer);

                _productService.UpdateProduct(product);
            }
            catch (Exception e)
            {
                _printedCouponBooksService.Delete(printedCouponBook);
                ModelState.AddModelError("", "Error al guardar producto conectado con cuponera: " + e.Message);
                return View("~/Plugins/Teed.Plugin.Groceries/Views/PrintedCouponBook/Create.cshtml", PrepareModel(model));
            }

            return RedirectToAction("List");
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult Edit(PrintedCouponBookModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.PrintedCouponBook))
                return AccessDeniedView();

            if (!ModelState.IsValid) return BadRequest();

            PrintedCouponBook printedCouponBook = _printedCouponBooksService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (printedCouponBook == null) return NotFound();

            if (model.BookTypeId == (int)PrintedCouponBookType.Subtotal)
                model.BookTypeValue = model.Subtotal.ToString();
            else if (model.BookTypeId == (int)PrintedCouponBookType.Client)
                model.BookTypeValue = string.Join(",", model.SelectedCustomerIds);

            var updateProductName = false;
            var updateProductPicture = false;
            var newLog = string.Empty;
            if (model.Name != printedCouponBook.Name)
            {
                updateProductName = true;
                newLog += $".\nCambió el nombre de {printedCouponBook.Name} a {model.Name}";
            }

            if (model.Active != printedCouponBook.Active)
            {
                newLog += $".\nCambió si está activa la cuponera de {(printedCouponBook.Active ? "Activa" : "No activa")} a {(model.Active ? "Activa" : "No activa")}";
            }

            if (model.BookTypeId != printedCouponBook.BookTypeId)
            {
                newLog += $".\nCambió el el tipo de cuponera de {((PrintedCouponBookType)printedCouponBook.BookTypeId).GetDisplayName()} a {((PrintedCouponBookType)model.BookTypeId).GetDisplayName()}";
            }

            if (model.BookTypeValue != printedCouponBook.BookTypeValue)
            {
                newLog += $".\nCambió el valor del tipo de la cuponera de \"{printedCouponBook.BookTypeValue}\" a \"{model.BookTypeValue}\"";
            }

            if (model.Inventory != printedCouponBook.Inventory)
            {
                newLog += $".\nCambió el inventario de {printedCouponBook.Inventory} a {model.Inventory}";
            }

            if (model.ReferencePictureId != printedCouponBook.ReferencePictureId)
            {
                updateProductPicture = true;
                newLog += $".\nCambió la imágen de {printedCouponBook.ReferencePictureId} a {model.ReferencePictureId}";
            }

            if (model.StartDate != printedCouponBook.StartDate)
            {
                newLog += $".\nCambió la fecha de inicio de {printedCouponBook.StartDate:dd/MM/yyyy hh:mm tt} a {model.StartDate:dd/MM/yyyy hh:mm tt}";
            }

            if (model.EndDate != printedCouponBook.EndDate)
            {
                newLog += $".\nCambió la fecha de final de {printedCouponBook.EndDate:dd/MM/yyyy hh:mm tt} a {model.EndDate:dd/MM/yyyy hh:mm tt}";
            }

            printedCouponBook.Name = model.Name;
            printedCouponBook.Active = model.Active;
            printedCouponBook.BookTypeId = model.BookTypeId;
            printedCouponBook.BookTypeValue = model.BookTypeValue;
            printedCouponBook.Inventory = model.Inventory;
            printedCouponBook.ReferencePictureId = model.ReferencePictureId;
            printedCouponBook.StartDate = model.StartDate;
            printedCouponBook.EndDate = model.EndDate;

            printedCouponBook.Log += "\n\n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Email}) editó la cuponera{newLog}";

            _printedCouponBooksService.Update(printedCouponBook);

            if (updateProductName || updateProductPicture)
            {
                var product = _productService.GetProductById(printedCouponBook.Id);
                if (updateProductName)
                    product.Name = printedCouponBook.Name;
                if (updateProductPicture)
                {
                    foreach (var productPicture in product.ProductPictures)
                    {
                        var pictureId = productPicture.PictureId;
                        _productService.DeleteProductPicture(productPicture);
                        var picture = _pictureService.GetPictureById(pictureId);
                        if (picture != null)
                            _pictureService.DeletePicture(picture);
                    }
                    _pictureService.SetSeoFilename(model.ReferencePictureId, _pictureService.GetPictureSeName(product.Name));
                    _productService.InsertProductPicture(new ProductPicture
                    {
                        PictureId = model.ReferencePictureId,
                        ProductId = product.Id,
                        DisplayOrder = 0,
                    });
                }

                _productService.UpdateProduct(product);
            }

            return RedirectToAction("List");
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.PrintedCouponBook))
                return AccessDeniedView();

            var query = _printedCouponBooksService.GetAll().Where(x => !x.Deleted);
            var queryList = query.OrderBy(x => x.Name);
            var pagedList = new PagedList<PrintedCouponBook>(queryList, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    x.Name,
                    ReferencePicture = x.ReferencePictureId > 0 ? _pictureService.GetPictureUrl(x.ReferencePictureId) : "#",
                    x.Active,
                }).ToList(),
                Total = query.Count()
            };

            return Json(gridModel);
        }
    }
}
