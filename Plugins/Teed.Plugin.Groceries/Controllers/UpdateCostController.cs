using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.Product;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;

namespace Teed.Plugin.Groceries.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class UpdateCostController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IProductLogService _productLogService;
        private readonly IWorkContext _workContext;
        private readonly OrderReportStatusService _orderReportStatusService;
        private readonly OrderReportService _orderReportService;
        private readonly ProductPricePendingUpdateService _productPricePendingUpdateService;

        public UpdateCostController(IPermissionService permissionService,
            OrderReportStatusService orderReportStatusService,
            OrderReportService orderReportService,
            IProductService productService,
            ICustomerService customerService,
            IProductLogService productLogService,
            IWorkContext workContext,
            ProductPricePendingUpdateService productPricePendingUpdateService)
        {
            _permissionService = permissionService;
            _orderReportStatusService = orderReportStatusService;
            _orderReportService = orderReportService;
            _productService = productService;
            _customerService = customerService;
            _productLogService = productLogService;
            _workContext = workContext;
            _productPricePendingUpdateService = productPricePendingUpdateService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UpdateCost))
                return AccessDeniedView();

            var controlDate30Days = DateTime.Now.AddDays(-30).Date;
            var products = _productService.GetAllProductsQuery().Where(x => !x.Deleted && x.Published).ToList();
            var productIds = products.Select(x => x.Id).ToList();

            var reportStatus = _orderReportStatusService.GetAll().Where(x => x.StatusTypeId == 2)
                .GroupBy(x => DbFunctions.AddMilliseconds(x.ShippingDate, x.BuyerId))
                .Select(x => x.Key)
                .ToList();
            List<IGrouping<int, Domain.OrderReports.OrderReport>> reportGroup = _orderReportService.GetAll()
                .Where(x => reportStatus.Contains(DbFunctions.AddMilliseconds(x.OrderShippingDate, x.OriginalBuyerId)) && x.Invoice != null)
                .ToList()
                .Where(x => productIds.Contains(x.ProductId))
                .GroupBy(x => x.ProductId)
                .Where(x => x.Key > 0)
                .ToList();
            var reportedProductIds = reportGroup.Select(x => x.Key).ToList();

            var buyerIds = reportGroup.SelectMany(x => x).GroupBy(x => x.OriginalBuyerId).Select(x => x.Key).ToList();
            var customers = _customerService.GetAllCustomersQuery().Where(x => buyerIds.Contains(x.Id)).ToList();

            var notReportedProducts = products.Where(x => !reportedProductIds.Contains(x.Id)).Select(x => new ReportGroupData()
            {
                AverageCost = "0",
                CurrentCost = x.ProductCost.ToString("C"),
                LastReportedCost = new List<string>() { "S/I" },
                MedianCost = "0",
                ProductId = x.Id,
                ProductName = x.Name,
                SortValue = 0,
                CurrentCostValue = x.ProductCost,
                LastReportedCostValue = 0,
                Published = x.Published
            }).ToList();

            var less30DaysReports = new List<ReportGroupData>();
            var more30DaysReports = new List<ReportGroupData>();

            foreach (var group in reportGroup)
            {
                var orderedData = group.OrderByDescending(x => x.ReportedDateUtc.Date).ToList();
                var product = products.Where(x => x.Id == group.Key).FirstOrDefault();
                if (product == null) continue;

                var productId = group.Key;
                var averageCost = Math.Round(orderedData.Take(3).Select(x => x.UpdatedUnitCost).DefaultIfEmpty().Average(), 2).ToString("C");
                var currentCost = product.ProductCost.ToString("C");
                var currentCostValue = product.ProductCost;
                var lastReportedCost = orderedData.Take(10).Select(x =>
                {
                    string url = $"{x.ReportedDateUtc.ToLocalTime():dd-MM-yyyy} - {customers.Where(y => y.Id == x.OriginalBuyerId).FirstOrDefault().GetFullName()} - {x.UpdatedUnitCost:C}";
                    if (x.SentToSupermarket.HasValue && x.SentToSupermarket.Value)
                    {
                        url = $"/Admin/OrderDeliveryReports/OrderSuperReportDetails?date={x.OrderShippingDate:dd-MM-yyyy}";
                    }
                    return url;
                }).ToList();
                var medianCost = Math.Round(GetMedian(orderedData.Select(x => x.UpdatedUnitCost).ToList()), 2).ToString("C");
                var productName = product.Name;
                var lastReportedCostValue = orderedData.Select(x => x.UpdatedUnitCost).FirstOrDefault();
                var sortValue = product.ProductCost == 0 ? 1 : (product.ProductCost - orderedData.Select(x => x.UpdatedUnitCost).FirstOrDefault() / product.ProductCost);

                if (orderedData.FirstOrDefault().ReportedDateUtc >= controlDate30Days)
                {
                    less30DaysReports.Add(new ReportGroupData()
                    {
                        ProductId = productId,
                        AverageCost = averageCost,
                        CurrentCost = currentCost,
                        CurrentCostValue = currentCostValue,
                        LastReportedCost = lastReportedCost,
                        MedianCost = medianCost,
                        ProductName = productName,
                        LastReportedCostValue = lastReportedCostValue,
                        SortValue = sortValue,
                        Published = product.Published
                    });
                }
                else
                {
                    more30DaysReports.Add(new ReportGroupData()
                    {
                        ProductId = productId,
                        AverageCost = averageCost,
                        CurrentCost = currentCost,
                        CurrentCostValue = currentCostValue,
                        LastReportedCost = lastReportedCost,
                        MedianCost = medianCost,
                        ProductName = productName,
                        LastReportedCostValue = lastReportedCostValue,
                        SortValue = sortValue,
                        Published = product.Published
                    });
                }
            }

            var model = new ReportGroupModel()
            {
                Less30DaysReports = less30DaysReports
                .Where(x => Math.Round(x.CurrentCostValue, 2) != Math.Round(x.LastReportedCostValue, 2) || x.CurrentCostValue == 0)
                .OrderByDescending(x => x.SortValue)
                .ToList(),
                More30DaysReports = more30DaysReports
                .Where(x => Math.Round(x.CurrentCostValue, 2) != Math.Round(x.LastReportedCostValue, 2) || x.CurrentCostValue == 0)
                .OrderByDescending(x => x.SortValue)
                .ToList(),
                NoReportedProducts = notReportedProducts.OrderBy(x => x.CurrentCostValue).ToList()
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/UpdateCost/Index.cshtml", model);
        }

        [HttpPost]
        public IActionResult UpdateProductCost(UpdateCostModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UpdateCost))
                return AccessDeniedView();

            var product = _productService.GetProductById(model.ProductId);
            if (product == null) return NotFound();
            if (Math.Round(product.ProductCost, 2) == Math.Round(model.NewCost, 2)) return NoContent();
            string log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) actualizó el costo del producto de {product.ProductCost} a {model.NewCost}";
            product.ProductCost = model.NewCost;
            _productService.UpdateProduct(product);

            _productLogService.InsertProductLog(new ProductLog()
            {
                CreatedOnUtc = DateTime.UtcNow,
                ProductId = product.Id,
                UserId = _workContext.CurrentCustomer.Id,
                Message = log
            });

            var productPricePendingUpdate = _productPricePendingUpdateService.GetAll().Where(x => x.ProductId == product.Id).Any();
            if (!productPricePendingUpdate)
                _productPricePendingUpdateService.Insert(new ProductPricePendingUpdate() { ProductId = product.Id });

            return Ok();
        }

        private decimal GetMedian(List<decimal> pricesList)
        {
            if (pricesList.Count == 0) return 0;
            var count = (decimal)(pricesList.Count() - 1);
            var index = (int)Math.Ceiling(count / 2);
            return pricesList.OrderBy(x => x).ElementAt(index);
        }
    }

    public class ReportGroupModel
    {
        public List<ReportGroupData> Less30DaysReports { get; set; }
        public List<ReportGroupData> More30DaysReports { get; set; }
        public List<ReportGroupData> NoReportedProducts { get; set; }
    }

    public class ReportGroupData
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public List<string> LastReportedCost { get; set; }
        public string AverageCost { get; set; }
        public string MedianCost { get; set; }
        public string CurrentCost { get; set; }
        public decimal CurrentCostValue { get; set; }
        public decimal SortValue { get; set; }
        public decimal LastReportedCostValue { get; set; }
        public bool Published { get; set; }
    }

    public class UpdateCostModel
    {
        public int ProductId { get; set; }
        public decimal NewCost { get; set; }
    }
}
