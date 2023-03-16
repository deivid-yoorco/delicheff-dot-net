using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.OrderReports;
using Teed.Plugin.Groceries.Models.OperationalErrors;
using Teed.Plugin.Groceries.Models.Warnings;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class WarningsController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;
        private readonly IDbContext _dbContext;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly OrderReportService _orderReportService;
        private readonly CostsIncreaseWarningService _costsIncreaseWarningService;
        private readonly CostsDecreaseWarningService _costsDecreaseWarningService;
        private readonly DateTime ReportChangeDate = new DateTime(2022, 07, 26).Date;

        public WarningsController(IPermissionService permissionService, IProductService productService, IDbContext dbContext, OrderReportService orderReportService,
            ICustomerService customerService, IWorkContext workContext, CostsIncreaseWarningService costsIncreaseWarningService,
            CostsDecreaseWarningService costsDecreaseWarningService)
        {
            _permissionService = permissionService;
            _productService = productService;
            _dbContext = dbContext;
            _orderReportService = orderReportService;
            _customerService = customerService;
            _workContext = workContext;
            _costsIncreaseWarningService = costsIncreaseWarningService;
            _costsDecreaseWarningService = costsDecreaseWarningService;
        }

        public IActionResult Index()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.Warnings))
                return AccessDeniedView();

            var products = _productService.GetAllProductsQuery().Where(x => !x.Deleted).ToList();

            string productsWithoutChildCategoryQuery = System.IO.File.ReadAllText("Plugins/Teed.Plugin.Groceries/src/files/sql/products-without-child-categories.sql");
            List<ProductQueryResult> productsWithoutChildCategory = _dbContext.SqlQuery<ProductQueryResult>(productsWithoutChildCategoryQuery).ToList();

            string productsWithoutParentCategoryQuery = System.IO.File.ReadAllText("Plugins/Teed.Plugin.Groceries/src/files/sql/products-without-parent-categories.sql");
            List<ProductQueryResult> productsWithoutParentCategory = _dbContext.SqlQuery<ProductQueryResult>(productsWithoutParentCategoryQuery).ToList();

            string productsWithoutEarningsQuery = System.IO.File.ReadAllText("Plugins/Teed.Plugin.Groceries/src/files/sql/products-without-earnings.sql");
            List<ProductQueryResult> productsWithoutEarnings = _dbContext.SqlQuery<ProductQueryResult>(productsWithoutEarningsQuery).ToList();

            string productsWithMoreThanOneChildCategoryQuery = System.IO.File.ReadAllText("Plugins/Teed.Plugin.Groceries/src/files/sql/products-with-morethanone-child-categories.sql");
            List<ProductQueryResult> productsWithMoreThanOneChildCategory = _dbContext.SqlQuery<ProductQueryResult>(productsWithMoreThanOneChildCategoryQuery).ToList();

            string productsWithMoreThanOneParentCategoryQuery = System.IO.File.ReadAllText("Plugins/Teed.Plugin.Groceries/src/files/sql/products-with-morethanone-parent-categories.sql");
            List<ProductQueryResult> productsWithMoreThanOneParentCategory = _dbContext.SqlQuery<ProductQueryResult>(productsWithMoreThanOneParentCategoryQuery).ToList();

            string productsWithoutImageQuery = System.IO.File.ReadAllText("Plugins/Teed.Plugin.Groceries/src/files/sql/products-without-picture.sql");
            List<ProductQueryResult> productsWithoutImage = _dbContext.SqlQuery<ProductQueryResult>(productsWithoutImageQuery).ToList();

            string productsWithLowMarginQuery = System.IO.File.ReadAllText("Plugins/Teed.Plugin.Groceries/src/files/sql/products-with-low-margin.sql");
            List<ProductQueryResult> productsWithLowMargin = _dbContext.SqlQuery<ProductQueryResult>(productsWithLowMarginQuery).ToList();

            string productsWithLowMargin13Query = System.IO.File.ReadAllText("Plugins/Teed.Plugin.Groceries/src/files/sql/products-with-low-margin-13.sql");
            List<ProductQueryResult> productsWithLowMargin13 = _dbContext.SqlQuery<ProductQueryResult>(productsWithLowMargin13Query).ToList();

            string productsSkuRepeatedQuery = System.IO.File.ReadAllText("Plugins/Teed.Plugin.Groceries/src/files/sql/products_sku_repeated.sql");
            List<ProductQueryResult> productsSkuRepeatedList = _dbContext.SqlQuery<ProductQueryResult>(productsSkuRepeatedQuery).ToList();

            string productsWithoutManufacturerQuery = System.IO.File.ReadAllText("Plugins/Teed.Plugin.Groceries/src/files/sql/products-without-manufacturer.sql");
            List<ProductWithoutManufacturer> productsWithoutManufacturerList = _dbContext.SqlQuery<ProductWithoutManufacturer>(productsWithoutManufacturerQuery).ToList();

            var wrongCategoryProducts = new List<ProductQueryResult>();

            Models.Warnings.IndexViewModel model = new Models.Warnings.IndexViewModel()
            {
                ProductsWithoutChildCategory = productsWithoutChildCategory,
                ProductsWithoutParentCategory = productsWithoutParentCategory,
                ProductsWithoutEarnings = productsWithoutEarnings,
                ProductsWithWrongChildCategory = wrongCategoryProducts,
                ProductsWithMoreThanOneChildCategory = productsWithMoreThanOneChildCategory,
                ProductsWithMoreThanOneParentCategory = productsWithMoreThanOneParentCategory,
                ProductsWithoutImage = productsWithoutImage,
                ProductsWithLowMargin = productsWithLowMargin,
                productsSkuRepeatedList = productsSkuRepeatedList,
                ProductsWithLowMargin13 = productsWithLowMargin13,
                ProductsWithoutManufacturer = productsWithoutManufacturerList
            };

            var warnings = _costsIncreaseWarningService.GetAll()
                .GroupBy(x => new { x.ProductId, CreatedOnUtc = DbFunctions.TruncateTime(x.CreatedOnUtc) })
                .Select(x => x.OrderByDescending(y => y.Attended).ThenBy(y => y.CreatedOnUtc).FirstOrDefault())
                .Where(x => !x.Attended).ToList();
            var productIds = warnings.Select(x => x.ProductId).ToList();
            var productsOfWarnings = _productService.GetAllProductsQuery()
                .Where(x => productIds.Contains(x.Id)).ToList();
            model.CostsIncreaseWarnings = warnings.Select(x => new CostsIncreaseWarningModel
            {
                Attended = x.Attended,
                ProductId = x.ProductId,
                ProductName = productsOfWarnings.Where(y => y.Id == x.ProductId).FirstOrDefault()?.Name,
                CreatedOn = x.CreatedOnUtc.ToLocalTime().Date
            }).ToList();

            var decreaseWarnings = _costsDecreaseWarningService.GetAll()
                .GroupBy(x => new { x.ProductId, CreatedOnUtc = DbFunctions.TruncateTime(x.CreatedOnUtc) })
                .Select(x => x.OrderByDescending(y => y.Attended).ThenBy(y => y.CreatedOnUtc).FirstOrDefault())
                .Where(x => !x.Attended).ToList();
            productIds = decreaseWarnings.Select(x => x.ProductId).ToList();
            productsOfWarnings = _productService.GetAllProductsQuery()
                .Where(x => productIds.Contains(x.Id)).ToList();
            model.CostsDecreaseWarnings = decreaseWarnings.Select(x => new CostsDecreaseWarningModel
            {
                Attended = x.Attended,
                ProductId = x.ProductId,
                ProductName = productsOfWarnings.Where(y => y.Id == x.ProductId).FirstOrDefault().Name,
                CreatedOn = x.CreatedOnUtc.ToLocalTime().Date
            }).ToList();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Warnings/Index.cshtml", model);
        }

        public IActionResult OperationalErrors()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OperationalErrors))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/OperationalErrors/Index.cshtml");
        }

        public IActionResult GetOperationalErrorsData(string initDate = null, string endDate = null)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.OperationalErrors))
                return AccessDeniedView();

            var controlInitDate = DateTime.Now.AddDays(-30).Date;
            var controlEndDate = DateTime.Now.Date;
            var model = new Models.OperationalErrors.IndexViewModel();

            string orderReportWithNoManufacturer = System.IO.File.ReadAllText("Plugins/Teed.Plugin.Groceries/src/files/sql/order-reports-with-no-manufacturer.sql");
            List<ReportNoManufacturerQueryResult> reportNoManufacturerQueryResult = _dbContext.SqlQuery<ReportNoManufacturerQueryResult>(orderReportWithNoManufacturer, new SqlParameter[]
            {
                new SqlParameter("@initDate", controlInitDate.ToString("yyyyMMdd")),
                new SqlParameter("@endDate", controlEndDate.ToString("yyyyMMdd"))
            }).ToList();

            var productsWithoutManufacturer = reportNoManufacturerQueryResult
                .Select(x => new DataResult()
                {
                    DateTime = x.CreatedOnUtc.ToLocalTime().ToString("dd-MM-yyyy hh:mm:ss tt"),
                    ErrorMadeByCustomer = x.ReportedBy,
                    ErrorDescription = $"El comprador {x.ReportedBy} reportó en su app que compró el producto '{x.ProductName}' pero no reportó con qué fabricante fue comprado."
                }).ToList();

            model.BuyerErrors = productsWithoutManufacturer;

            var gridModel = new DataSourceResult
            {
                Data = model.BuyerErrors,
                Total = productsWithoutManufacturer.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult CostIncreaseDates()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.Warnings))
                return AccessDeniedView();

            var warnings = _costsIncreaseWarningService.GetAll()
                .Select(x => new { x.CreatedOnUtc, x.OriginalOrderShippingDate, x.Attended, x.ProductId })
                .ToList()
                .OrderByDescending(x => x.CreatedOnUtc)
                .GroupBy(x => GetCorrectReportDate(x.CreatedOnUtc, x.OriginalOrderShippingDate));

            //var warnings = _costsIncreaseWarningService.GetAll()
            //    .Select(x => new { x.CreatedOnUtc, x.Attended, x.ProductId })
            //    .ToList()
            //    .OrderByDescending(x => x.CreatedOnUtc)
            //    .GroupBy(x => x.CreatedOnUtc.ToLocalTime().Date);

            var gridModel = new DataSourceResult
            {
                Data = warnings.Select(x =>
                {
                    var filtered = x.GroupBy(y => y.ProductId).Select(y => y.FirstOrDefault()).ToList();
                    return new
                    {
                        DateUnformatted = x.Key.Date,
                        Date = x.Key.ToString("dd-MM-yyyy"),
                        AttendedCount = filtered.Where(y => y.Attended).Count(),
                        UnattendedCount = filtered.Where(y => !y.Attended).Count()
                    };
                }).OrderByDescending(x => x.DateUnformatted).ToList(),
                Total = warnings.Count()
            };

            return Json(gridModel);
        }

        public IActionResult CostIncreaseWarnings(string date)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.Warnings))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Warnings/CostIncreaseWarnings.cshtml", date);
        }

        [HttpPost]
        public IActionResult CostIncreaseWarnings(string date, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.Warnings))
                return AccessDeniedView();

            var parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;
            var warnings = _costsIncreaseWarningService.GetAll()
                .Select(x => new
                {
                    x.Id,
                    x.CreatedOnUtc,
                    x.OldCost,
                    x.NewCost,
                    x.ProductId,
                    x.OldReportedCostDate,
                    x.Comment,
                    x.Attended,
                    x.OriginalOrderShippingDate
                })
                .ToList()
                .Where(x => (GetCorrectReportDate(x.CreatedOnUtc, x.OriginalOrderShippingDate)) == parsedDate)
                .GroupBy(x => x.ProductId)
                .Select(x => x.OrderByDescending(y => y.CreatedOnUtc).FirstOrDefault())
                .ToList();
            var productIds = warnings.Select(x => x.ProductId).Distinct().ToList();
            var products = _productService.GetAllProductsQuery()
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Price
                })
                .Where(x => productIds.Contains(x.Id))
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = warnings.Select(x =>
                {
                    var product = products.Where(y => y.Id == x.ProductId).FirstOrDefault();
                    return new
                    {
                        x.Id,
                        x.ProductId,
                        ProductName = product?.Name,
                        OldReportedCostDate = x.OldReportedCostDate.ToString("dd-MM-yyyy"),
                        OriginalOrderShippingDate = x.OriginalOrderShippingDate != null ? x.OriginalOrderShippingDate.Value.ToString("dd-MM-yyyy") : "---",
                        OldCost = x.OldCost.ToString("C"),
                        NewCost = x.NewCost.ToString("C"),
                        Variation = (x.NewCost - x.OldCost).ToString("C").Replace("(", "-").Replace(")", ""),
                        PercentageVariation = x.OldCost == 0 ? "0%" : Math.Round(((x.NewCost - x.OldCost) / x.OldCost) * 100, 2) + "%",
                        PercentageVariationValue = x.OldCost == 0 ? 0 : Math.Round(((x.NewCost - x.OldCost) / x.OldCost) * 100, 2),
                        ProductPrice = products.Where(y => y.Id == x.ProductId).FirstOrDefault().Price.ToString("C"),
                        GrossMargin = product.Price == 0 ? "0%" : Math.Round((1 - (x.NewCost / product.Price)) * 100, 2) + "%",
                        GrossMarginValue = product.Price == 0 ? 0 : Math.Round((1 - (x.NewCost / product.Price)) * 100, 2),
                        Comment = x.Comment ?? "",
                        x.Attended
                    };
                }
                ).OrderByDescending(x => x.PercentageVariationValue).ToList(),
                Total = warnings.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult CostIncreaseUpdate(CostIncreaseUpdateModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.Warnings))
                return AccessDeniedView();

            var costsWarning = _costsIncreaseWarningService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (costsWarning != null)
            {
                costsWarning.Comment = model.Comment;
                costsWarning.Attended = model.Attended;
                costsWarning.Log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + $" - El usuario {_workContext.CurrentCustomer.GetFullName()} ({_workContext.CurrentCustomer.Id}) editó la advertencia de costo.\n";
                _costsIncreaseWarningService.Update(costsWarning);
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult CostDecreaseDates()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.Warnings))
                return AccessDeniedView();

            var warnings = _costsDecreaseWarningService.GetAll()
                .Select(x => new { x.CreatedOnUtc, x.OriginalOrderShippingDate, x.Attended, x.ProductId })
                .ToList()
                .OrderByDescending(x => x.CreatedOnUtc)
                .GroupBy(x => GetCorrectReportDate(x.CreatedOnUtc, x.OriginalOrderShippingDate));

            //var warnings = _costsDecreaseWarningService.GetAll()
            //    .Select(x => new { x.CreatedOnUtc, x.Attended, x.ProductId })
            //    .ToList()
            //    .OrderByDescending(x => x.CreatedOnUtc)
            //    .GroupBy(x => x.CreatedOnUtc.ToLocalTime().Date);

            var gridModel = new DataSourceResult
            {
                Data = warnings.Select(x =>
                {
                    var filtered = x.GroupBy(y => y.ProductId).Select(y => y.FirstOrDefault()).ToList();
                    return new
                    {
                        DateUnformatted = x.Key.Date,
                        Date = x.Key.ToString("dd-MM-yyyy"),
                        AttendedCount = filtered.Where(y => y.Attended).Count(),
                        UnattendedCount = filtered.Where(y => !y.Attended).Count()
                    };
                }).OrderByDescending(x => x.DateUnformatted).ToList(),
                Total = warnings.Count()
            };

            return Json(gridModel);
        }

        public IActionResult CostDecreaseWarnings(string date)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.Warnings))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Warnings/CostDecreaseWarnings.cshtml", date);
        }

        [HttpPost]
        public IActionResult CostDecreaseWarnings(string date, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.Warnings))
                return AccessDeniedView();

            var parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;
            var warnings = _costsDecreaseWarningService.GetAll()
                .Select(x => new
                {
                    x.Id,
                    x.CreatedOnUtc,
                    x.OldCost,
                    x.NewCost,
                    x.ProductId,
                    x.OldReportedCostDate,
                    x.Comment,
                    x.Attended,
                    x.OriginalOrderShippingDate
                })
                .ToList()
                //.Where(x => x.CreatedOnUtc.ToLocalTime().Date == parsedDate)
                .Where(x => (GetCorrectReportDate(x.CreatedOnUtc, x.OriginalOrderShippingDate)) == parsedDate)
                .GroupBy(x => x.ProductId)
                .Select(x => x.OrderByDescending(y => y.CreatedOnUtc).FirstOrDefault())
                .ToList();
            var productIds = warnings.Select(x => x.ProductId).Distinct().ToList();
            var products = _productService.GetAllProductsQuery()
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Price
                })
                .Where(x => productIds.Contains(x.Id))
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = warnings.Select(x =>
                {
                    var product = products.Where(y => y.Id == x.ProductId).FirstOrDefault();
                    return new
                    {
                        x.Id,
                        x.ProductId,
                        ProductName = product?.Name,
                        OldReportedCostDate = x.OldReportedCostDate.ToString("dd-MM-yyyy"),
                        OriginalOrderShippingDate = x.OriginalOrderShippingDate != null ? x.OriginalOrderShippingDate.Value.ToString("dd-MM-yyyy") : "---",
                        OldCost = x.OldCost.ToString("C"),
                        NewCost = x.NewCost.ToString("C"),
                        Variation = (x.NewCost - x.OldCost).ToString("C").Replace("(", "-").Replace(")", ""),
                        PercentageVariation = x.OldCost == 0 ? "0%" : Math.Round(((x.NewCost - x.OldCost) / x.OldCost) * 100, 2) + "%",
                        PercentageVariationValue = x.OldCost == 0 ? 0 : Math.Round(((x.NewCost - x.OldCost) / x.OldCost) * 100, 2),
                        ProductPrice = products.Where(y => y.Id == x.ProductId).FirstOrDefault().Price.ToString("C"),
                        GrossMargin = product.Price == 0 ? "0%" : Math.Round((1 - (x.NewCost / product.Price)) * 100, 2) + "%",
                        GrossMarginValue = product.Price == 0 ? 0 : Math.Round((1 - (x.NewCost / product.Price)) * 100, 2),
                        Comment = x.Comment ?? "",
                        x.Attended
                    };
                }
                ).OrderByDescending(x => x.PercentageVariationValue).ToList(),
                Total = warnings.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult CostDecreaseUpdate(CostDecreaseUpdateModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.Warnings))
                return AccessDeniedView();

            var costsWarning = _costsDecreaseWarningService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (costsWarning != null)
            {
                costsWarning.Comment = model.Comment;
                costsWarning.Attended = model.Attended;
                costsWarning.Log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + $" - El usuario {_workContext.CurrentCustomer.GetFullName()} ({_workContext.CurrentCustomer.Id}) editó la advertencia de costo.\n";
                _costsDecreaseWarningService.Update(costsWarning);
            }

            return Ok();
        }

        private DateTime GetCorrectReportDate(DateTime CreatedOnUtc, DateTime? OriginalOrderShippingDate)
        {
            var date = OriginalOrderShippingDate != null ? OriginalOrderShippingDate.Value.Date : CreatedOnUtc.ToLocalTime().Date;
            return date;
        }
    }

    public class CostIncreaseUpdateModel
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public bool Attended { get; set; }
    }

    public class CostDecreaseUpdateModel
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public bool Attended { get; set; }
    }

    public class ProductQueryResult
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Calculate { get; set; }
        public string ProductSku { get; set; }
    }


    public class ReportNoManufacturerQueryResult
    {
        public string ProductName { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime OrderShippingDate { get; set; }
        public string ReportedBy { get; set; }
    }

    public class ProductWithoutManufacturer
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }
}
