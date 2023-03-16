using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Web.Factories;
using Nop.Web.Models.CustomPages;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nop.Web.Controllers
{
    public class CustomPageController : BasePublicController
    {
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;

        public CustomPageController(IProductModelFactory productModelFactory, IProductService productService)
        {
            _productModelFactory = productModelFactory;
            _productService = productService;
        }

        public async Task<IActionResult> CustomPageDetails(int customPageId)
        {
            CustomPagesModel page = await GetPageData(customPageId);
            if (page == null || !page.Published) return NotFound();
            var products = _productService.SearchProducts().Where(x => page.SelectedProductIds.Contains(x.Id));
            page.Products = _productModelFactory.PrepareProductOverviewModels(products, true, true).ToList();
            return View(page);
        }

        private async Task<CustomPagesModel> GetPageData(int customPageId)
        {
            using (HttpClient client = new HttpClient())
            {
                var url = (Request.IsHttps ? "https" : "http") + $"://{Request.Host}/Admin/CustomPages/GetById/{customPageId}";
                var result = await client.GetAsync(url);
                if (result.IsSuccessStatusCode)
                {
                    var json = await result.Content.ReadAsStringAsync();
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<CustomPagesModel>(json);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
