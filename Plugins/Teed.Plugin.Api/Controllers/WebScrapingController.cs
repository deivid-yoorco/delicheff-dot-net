using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Domain.Groceries;
using Teed.Plugin.Api.Dtos.Groceries;
using Teed.Plugin.Api.Services;

namespace Teed.Plugin.Api.Controllers
{
    public class WebScrapingController : ApiBaseController
    {
        private readonly WebScrapingUnitService _webScrapingUnitService;
        private readonly WebScrapingHistoryService _webScrapingHistoryService;

        public WebScrapingController(WebScrapingUnitService webScrapingUnitService, WebScrapingHistoryService webScrapingHistoryService)
        {
            _webScrapingUnitService = webScrapingUnitService;
            _webScrapingHistoryService = webScrapingHistoryService;
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ProcessData(Dictionary<string, string> body)
        {
            var dto = new ProcessDataDto()
            {
                Category1 = body["Category1"],
                Category2 = body["Category2"],
                Category3 = body["Category3"],
                PictureUrl = body["PictureUrl"],
                Price = body["Price"],
                ProductName = body["ProductName"],
                ProductUrl = body["ProductUrl"],
                Store = body["Store"]
            };

            var product = _webScrapingUnitService.GetAll().Where(x => x.ProductName.ToLower() == dto.ProductName.ToLower() && x.Store.ToLower() == dto.Store.ToLower()).FirstOrDefault();
            if (product == null)
            {
                product = new WebScrapingUnit()
                {
                    Category1 = dto.Category1,
                    Category2 = dto.Category2,
                    Category3 = dto.Category3,
                    ImageUrl = dto.PictureUrl,
                    ProductName = dto.ProductName,
                    ProductUrl = dto.ProductUrl,
                    Store = dto.Store
                };
                _webScrapingUnitService.Insert(product);
            }

            var wsHistory = new WebScrapingHistory()
            {
                Price = decimal.Parse(dto.Price),
                WebScrapingProductId = product.Id
            };

            _webScrapingHistoryService.Insert(wsHistory);

            return NoContent();
        }

        [HttpGet]
        [AllowAnonymous]
        public List<decimal> GetWebScrapingHistory(string date, int walmartProductId, int laComerProductId, int chedrauiProductId, int superamaProductId)
        {
            DateTime dateParsed = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var wsHistory = _webScrapingHistoryService.GetAll().Where(x => DbFunctions.TruncateTime(x.CreatedOnUtc) >= DbFunctions.TruncateTime(dateParsed));

            List<decimal> prices = new List<decimal>();
            if (walmartProductId > 0)
            {
                var storePriceHistory = wsHistory.Where(x => x.WebScrapingProductId == walmartProductId);
                if (storePriceHistory.Count() > 0)
                {
                    prices.Add(storePriceHistory.Average(x => x.Price));
                }
            }

            if (laComerProductId > 0)
            {
                var storePriceHistory = wsHistory.Where(x => x.WebScrapingProductId == laComerProductId);
                if (storePriceHistory.Count() > 0)
                {
                    prices.Add(storePriceHistory.Average(x => x.Price));
                }
            }

            if (chedrauiProductId > 0)
            {
                var storePriceHistory = wsHistory.Where(x => x.WebScrapingProductId == chedrauiProductId);
                if (storePriceHistory.Count() > 0)
                {
                    prices.Add(storePriceHistory.Average(x => x.Price));
                }
            }

            if (superamaProductId > 0)
            {
                var storePriceHistory = wsHistory.Where(x => x.WebScrapingProductId == superamaProductId);
                if (storePriceHistory.Count() > 0)
                {
                    prices.Add(storePriceHistory.Average(x => x.Price));
                }
            }

            return prices;
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetProductsList(string store)
        {
            var products = _webScrapingUnitService.GetAll().Where(x => x.Store.ToUpper() == store.ToUpper());
            var elements = products.Select(x => new
            {
                Id = x.Id,
                Product = x.ProductName
            }).ToList();

            return Json(elements);
        }

        [HttpGet]
        [AllowAnonymous]
        public decimal GetProdutPriceByDate(DateTime date, int wsUnitId)
        {
            return _webScrapingHistoryService.GetAll().Where(x => x.CreatedOnUtc == date &&
                 x.WebScrapingProductId == wsUnitId).Select(x => x.Price).DefaultIfEmpty().FirstOrDefault();
        }
    }
}
