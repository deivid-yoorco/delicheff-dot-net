using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Win32;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Media;
using Nop.Services.Orders;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.Products;
using Teed.Plugin.Api.Helper;

namespace Teed.Plugin.Api.Controllers
{
    public class ProductController : ApiBaseController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IPictureService _pictureService;
        private readonly ICustomerService _customerService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ISettingService _settingService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public ProductController(IProductService productService,
            IPictureService pictureService,
            IProductAttributeService productAttributeService,
            IPriceCalculationService priceCalculationService,
            ICustomerService customerService,
            ISettingService settingService,
            IHostingEnvironment hostingEnvironment,
            IOrderService orderService)
        {
            _productService = productService;
            _pictureService = pictureService;
            _productAttributeService = productAttributeService;
            _customerService = customerService;
            _priceCalculationService = priceCalculationService;
            _settingService = settingService;
            _hostingEnvironment = hostingEnvironment;
            _orderService = orderService;
        }

        #endregion

        #region Methods

        [HttpGet]
        public virtual IActionResult GetCustomerFavoriteProducts(int page, int elementsPerPage)
        {
            if (string.IsNullOrEmpty(UserId)) return NoContent();

            int customerId = int.Parse(UserId);

            var customerOrdersQuery = _orderService.GetAllOrdersQuery()
                .Where(x => x.CustomerId == customerId);

            if (customerOrdersQuery.Count() == 0) return NoContent();

            Customer customer = _customerService.GetCustomerById(customerId);

            var products = customerOrdersQuery
                .SelectMany(x => x.OrderItems)
                .GroupBy(x => x.Product)
                .Where(x => x.Key.Published)
                .OrderByDescending(x => x.Count())
                .Select(x => x.Key);

            var pagedList = new PagedList<Product>(products, page, elementsPerPage);
            var dto = pagedList.Select(x => new ProductDto()
            {
                Id = x.Id,
                Sku = x.Sku,
                Name = x.Name,
                PictureUrl = "/Product/ProductImage?id=" + x.Id,
                Price = x.Price,
                OldPrice = x.OldPrice > x.Price ? x.OldPrice : 0,
                EquivalenceCoefficient = x.EquivalenceCoefficient,
                WeightInterval = ProductHelper.GetWeightInterval(x),
                CurrentCartQuantity = customer.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(y => y.Quantity).FirstOrDefault(),
                BuyingBySecondary = customer.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(y => y.BuyingBySecondary).FirstOrDefault(),
                PropertyOptions = string.IsNullOrEmpty(x.PropertiesOptions) ? null : x.PropertiesOptions.Split(',').Select(y => y.ToUpper().First() + y.ToLower().Substring(1)).ToArray(),
                SelectedPropertyOption = customer.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(y => y.SelectedPropertyOption).FirstOrDefault(),
                Discount = _priceCalculationService.GetDiscountAmount(x, customer, x.Price),
                IsInWishlist = customer.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.Wishlist).Any(),
                Stock = x.ManageInventoryMethod == ManageInventoryMethod.DontManageStock ? 1000 : x.StockQuantity
            }).ToList();

            return Ok(dto);
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult GetProduct(int id)
        {
            Product product = _productService.GetProductById(id);
            if (product == null) return NotFound();

            Customer customer = null;
            if (!string.IsNullOrWhiteSpace(UserId))
                customer = _customerService.GetCustomerById(int.Parse(UserId));

            ProductDto productDto = new ProductDto()
            {
                Id = product.Id,
                Sku = product.Sku,
                Name = product.Name,
                PictureUrl = "/Product/ProductImage?id=" + product.Id,
                Price = product.Price,
                OldPrice = product.OldPrice > product.Price ? product.OldPrice : 0,
                EquivalenceCoefficient = product.EquivalenceCoefficient,
                WeightInterval = ProductHelper.GetWeightInterval(product),
                CurrentCartQuantity = customer != null ? customer.ShoppingCartItems.Where(y => y.ProductId == product.Id && y.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(y => y.Quantity).FirstOrDefault() : 0,
                BuyingBySecondary = customer != null && customer.ShoppingCartItems.Where(y => y.ProductId == product.Id && y.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(y => y.BuyingBySecondary).FirstOrDefault(),
                PropertyOptions = product.PropertiesOptions?.Split(',').Select(x => x.ToUpper().First() + x.ToLower().Substring(1)).ToArray(),
                SelectedPropertyOption = customer?.ShoppingCartItems.Where(y => y.ProductId == product.Id && y.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(y => y.SelectedPropertyOption).FirstOrDefault(),
                Discount = _priceCalculationService.GetDiscountAmount(product, customer ?? new Customer(), product.Price, out List<DiscountForCaching> appliedDiscounts),
                IsInWishlist = customer != null && customer.ShoppingCartItems.Where(y => y.ProductId == product.Id && y.ShoppingCartType == ShoppingCartType.Wishlist).Any(),
                Stock = product.ManageInventoryMethod == ManageInventoryMethod.DontManageStock ? 1000 : product.StockQuantity
            };

            return Ok(productDto);
        }

        [HttpGet]
        public virtual IActionResult GetProductQuantityList(int stockQty,
            bool buyingBySecondary,
            decimal equivalence,
            decimal weightInterval,
            int page = 0,
            int elementsPerPage = 0)
        {
            string unit = "pz";
            var list = new List<SelectListItem>();
            for (int i = (page * elementsPerPage) + 1; i <= (page * elementsPerPage) + elementsPerPage; i++)
            {
                if (i > stockQty) continue;
                decimal qty = i;
                if (equivalence > 0 && buyingBySecondary)
                {
                    qty = (i * 1000) / equivalence;
                    unit = "gr";
                }
                else if ((weightInterval > 0 && buyingBySecondary) || (weightInterval > 0 && equivalence == 0))
                {
                    qty = i * weightInterval;
                    unit = "gr";
                }

                if (qty >= 1000 && (buyingBySecondary || weightInterval > 0))
                {
                    qty /= 1000;
                    unit = "kg";
                }

                list.Add(new SelectListItem()
                {
                    Text = Math.Round(qty, 2).ToString() + " " + unit,
                    Value = i.ToString()
                });
            }


            return Ok(list);
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult ProductImage(int id)
        {
            var pictures = _pictureService.GetPicturesByProductId(id);
            byte[] bytes = pictures.FirstOrDefault()?.PictureBinary;
            string mimeType = pictures.FirstOrDefault()?.MimeType;

            if (pictures.Count == 0)
            {
                using (WebClient client = new WebClient())
                {
                    bytes = client.DownloadData(_pictureService.GetDefaultPictureUrl());
                    mimeType = "image/png";
                }
            }

            return File(bytes, mimeType);
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult GetFeaturedProducts()
        {
            var dto = _productService.SearchProducts().Where(x => x.ShowOnHomePage && !x.GiftProductEnable)
                .AsEnumerable()
                .Select(x => new GetFeaturedProductsDto()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Price = x.Price,
                    PictureUrl = "/Product/ProductImage?id=" + x.Id
                }).ToList();

            return Ok(dto);
        }

        //[HttpPost]
        //public virtual IActionResult UpdateProductImages(Dictionary<string, string> body)
        //{
        //    var sku = body["sku"];
        //    var url = body["url"];
        //    if (!string.IsNullOrWhiteSpace(sku) && !string.IsNullOrWhiteSpace(url))
        //    {
        //        try
        //        {
        //            Product product = _productService.GetProductBySku(sku);
        //            if (product == null) return NotFound($"Product with sku '{sku}' not found");
        //            if (product.ProductPictures.Count <= 0)
        //            {
        //                byte[] fileBinary;
        //                if (url.Contains("base64"))
        //                {
        //                    fileBinary = Convert.FromBase64String(url.Replace("data:image/jpeg;base64,", ""));
        //                }
        //                else
        //                {
        //                    using (var webClient = new WebClient())
        //                    {
        //                        //var urlParsed = url.Replace("/img_small/", "/img_large/").Replace("s.jpg", "L.jpg");
        //                        fileBinary = webClient.DownloadData(url);
        //                    }
        //                }

        //                var picture = _pictureService.InsertPicture(fileBinary, MimeTypes.ImageJpeg, null);
        //                _pictureService.UpdatePicture(picture.Id,
        //                    _pictureService.LoadPictureBinary(picture),
        //                    picture.MimeType,
        //                    picture.SeoFilename,
        //                    "",
        //                    "",
        //                    false,
        //                    false,
        //                    null,
        //                    null,
        //                    null,
        //                    null);

        //                _productService.InsertProductPicture(new ProductPicture
        //                {
        //                    PictureId = picture.Id,
        //                    ProductId = product.Id,
        //                    DisplayOrder = 0,
        //                });
        //            }
        //        }
        //        catch (Exception e)
        //        {

        //        }
        //    }

        //    return Ok();
        //}

        //[HttpGet]
        //[AllowAnonymous]
        //public virtual IActionResult UpdateProductLocalImages()
        //{
        //    List<string> notFountProducts = new List<string>();
        //    string path = "C:\\Users\\Ivan Salazar\\Desktop\\Pics";
        //    if (Directory.Exists(path))
        //    {
        //        var products = _productService.SearchProducts(showHidden: true);
        //        string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories).ToArray();
        //        int count = 0;
        //        foreach (var file in files)
        //        {
        //            count++;
        //            var sku = file.Split('\\').Last().Split('.').FirstOrDefault().Split('-').FirstOrDefault().Split('_').FirstOrDefault();

        //            var product = products.Where(x => x.Sku.Trim().ToLower() == sku.Trim().ToLower()).FirstOrDefault();
        //            if (product == null)
        //            {
        //                notFountProducts.Add(file);
        //                continue;
        //            }
        //            if (product.ProductPictures.Count > 0)
        //            {
        //                var productPictures = product.ProductPictures.ToList();
        //                foreach (var productPicture in productPictures)
        //                {
        //                    _productService.DeleteProductPicture(productPicture);
        //                }
        //            }

        //            var allSkuFiles = files.Where(x => x.Contains(sku)).ToList();
        //            foreach (var item in allSkuFiles)
        //            {
        //                byte[] fileBinary = System.IO.File.ReadAllBytes(item);
        //                var picture = _pictureService.InsertPicture(fileBinary, MimeTypes.ImageJpeg, null);
        //                _pictureService.UpdatePicture(picture.Id,
        //                    picture.PictureBinary,
        //                    picture.MimeType,
        //                    picture.SeoFilename,
        //                    "",
        //                    "",
        //                    false,
        //                    false,
        //                    null,
        //                    null,
        //                    null,
        //                    null);

        //                _productService.InsertProductPicture(new ProductPicture
        //                {
        //                    PictureId = picture.Id,
        //                    ProductId = product.Id,
        //                    DisplayOrder = 0,
        //                });
        //            }
        //        }
        //    }

        //    return Ok(string.Join("; ", notFountProducts));
        //}

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetProductImagesInZip(int jumpTo = 0, int cutAmount = 500)
        {
            string path = "C:/Users/Ivan Salazar/Desktop/Products.xlsx";
            FileInfo fileInfo = new FileInfo(path);
            var errorList = new List<CellErrorModel1>();

            using (var memoryStream = new MemoryStream())
            {
                using (var package = new ExcelPackage(fileInfo.OpenRead()))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet != null)
                    {
                        int init = 0;
                        List<string> headers = new List<string>();
                        int totalRows = worksheet.Dimension.End.Row;
                        int totalColumns = worksheet.Dimension.End.Column;
                        var range = worksheet.Cells[1, 1, 1, totalColumns];

                        try
                        {
                            var cells = worksheet.Cells.ToList();
                            var groups = GetCellGroups(cells, worksheet.Dimension.End.Row - 1);
                            if (groups == null) return BadRequest("Ocurrió un problema creando los grupos para la carga de datos");
                            var dataList = new List<CellObjectModel1>();
                            for (int g = 0; g < groups.Count; g++)
                            {
                                int currentColumn = 0;
                                var data = new CellObjectModel1();
                                data.ProductId = groups[g][currentColumn].Value != null && !string.IsNullOrWhiteSpace(groups[g][currentColumn].Value.ToString()) ? (int?)int.Parse(groups[g][currentColumn].Value.ToString()) : null;
                                currentColumn++;
                                dataList.Add(data);
                            }

                            var productIds = dataList.Where(x => x.ProductId > 0).Select(x => x.ProductId).ToList();
                            var products = _productService.GetAllProductsQuery()
                                .Where(x => productIds.Contains(x.Id) && !x.Deleted)
                                .OrderBy(x => x.Id)
                                .ToList();
                            var fullAmount = products.Count();
                            products = products
                            .Select((x, i) => new { Index = i, Value = x })
                            .GroupBy(x => x.Index / cutAmount)
                            .Select(x => x.Select(v => v.Value).ToList())
                            .ToList()[jumpTo];

                            using (var compressedFileStream = new MemoryStream())
                            {
                                //Create an archive and store the stream in memory.
                                using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Create, false))
                                {
                                    // Start compiling data
                                    int count = 0;
                                    foreach (var product in products)
                                    {
                                        count++;
                                        try
                                        {
                                            var image = _pictureService.GetPicturesByProductId(product.Id)
                                                .OrderByDescending(x => x.Id).FirstOrDefault();
                                            if (image != null)
                                            {
                                                //Create a zip entry for each attachment
                                                var name = $"{product.Sku}{GetDefaultExtension(image.MimeType)}";
                                                var zipEntry = zipArchive.CreateEntry(name);

                                                //Get the stream of the attachment
                                                using (var originalFileStream = new MemoryStream(image.PictureBinary))
                                                using (var zipEntryStream = zipEntry.Open())
                                                {
                                                    //Copy the attachment stream to the zip entry stream
                                                    originalFileStream.CopyTo(zipEntryStream);
                                                }
                                            }
                                            else
                                                errorList.Add(new CellErrorModel1
                                                {
                                                    CellObjectModel = null,
                                                    Error = "No images for product " + product.Name + " - " + product.Sku
                                                });
                                        }
                                        catch (Exception e)
                                        {
                                            errorList.Add(new CellErrorModel1
                                            {
                                                CellObjectModel = null,
                                                Error = e.Message
                                            });
                                        }
                                    }
                                }

                                var finalAmount = ((jumpTo + 1) * 1 * cutAmount);
                                if (finalAmount > fullAmount)
                                    finalAmount = fullAmount;
                                return new FileContentResult(compressedFileStream.ToArray(), "application/zip") { FileDownloadName = "Imagenes de productos de rango " + (jumpTo * 1 * cutAmount) + " a " + finalAmount + ".zip" };
                            }
                        }
                        catch (Exception e)
                        {
                            return BadRequest("Ocurrió un problema cargando los datos: " + e.Message);
                        }
                    }
                    else
                    {
                        return BadRequest("No fue posible cargar el excel");
                    }
                }
            }
        }

        private List<List<CellDataModel>> GetCellGroups(List<ExcelRangeBase> elements, int finalRow)
        {
            int i = 0;
            int g = 0;
            try
            {
                var list = new List<List<CellDataModel>>();
                var headerLetters = elements.Where(x => x.Start.Row == 1).Select(x => x.Address).Select(x => new String(x.Where(y => Char.IsLetter(y)).ToArray())).ToList();
                for (i = 0; i < finalRow; i++)
                {
                    var listData = new List<CellDataModel>();
                    for (g = 0; g < headerLetters.Count; g++)
                    {
                        var address = headerLetters[g] + (i + 2).ToString();
                        var element = elements.Where(x => x.Address == address).FirstOrDefault();
                        if (element == null || element.Value == null)
                        {
                            listData.Add(new CellDataModel() { Address = address, Value = null });
                        }
                        else
                        {
                            listData.Add(new CellDataModel() { Address = address, Value = element.Value.ToString() });
                        }
                    }
                    list.Add(listData);
                }

                return list;
            }
            catch (Exception w)
            {
                return null;
            }
        }

        private string GetDefaultExtension(string mimeType)
        {
            string result;
            RegistryKey key;
            object value;

            key = Registry.ClassesRoot.OpenSubKey(@"MIME\Database\Content Type\" + mimeType, false);
            value = key != null ? key.GetValue("Extension", null) : null;
            result = value != null ? value.ToString() : string.Empty;

            return result;
        }

        public class CellObjectModel1
        {
            public int? ProductId { get; set; }

        }

        public class CellErrorModel1
        {
            public CellObjectModel1 CellObjectModel { get; set; }
            public string Error { get; set; }
        }

        public class CellDataModel
        {
            public string Address { get; set; }
            public string Value { get; set; }
        }

        #endregion
    }
}
