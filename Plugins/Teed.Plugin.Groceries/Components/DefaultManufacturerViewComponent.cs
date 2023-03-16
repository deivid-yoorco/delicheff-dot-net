using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Components
{
    [ViewComponent(Name = "MainManufacturer")]
    public class DefaultManufacturerViewComponent : NopViewComponent
    {
        private readonly IProductService _productService;
        private readonly ProductMainManufacturerService _productMainManufacturerService;

        public DefaultManufacturerViewComponent(IProductService productService,
            ProductMainManufacturerService productMainManufacturerService)
        {
            _productService = productService;
            _productMainManufacturerService = productMainManufacturerService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var productModel = (ProductModel)additionalData;
            var product = _productService.GetProductById(productModel != null ? productModel.Id : 0);
            string defaultManufacturer = string.Empty;

            if (product != null)
            {
                var mainManufacturer = _productMainManufacturerService.GetAll().Where(x => x.ProductId == product.Id).FirstOrDefault();
                defaultManufacturer = ProductUtils.GetMainManufacturer(product.ProductManufacturers, mainManufacturer)?.Name;
            }

            var model = new DefaultManufacturerComponentModel()
            {
                DefaultManufacturer = defaultManufacturer,
                ProductModel = productModel,
                SelectedMainManufacturerId = product == null ? 0 : _productMainManufacturerService.GetAll().Where(x => x.ProductId == product.Id).Select(x => x.ManufacturerId).FirstOrDefault()
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/DefaultManufacturer/Default.cshtml", model);
        }
    }

    public class DefaultManufacturerComponentModel
    {
        public string DefaultManufacturer { get; set; }
        public ProductModel ProductModel { get; set; }
        public int? SelectedMainManufacturerId { get; set; }
    }
}
