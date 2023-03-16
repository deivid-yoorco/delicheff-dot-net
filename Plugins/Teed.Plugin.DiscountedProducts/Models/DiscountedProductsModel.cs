using Nop.Web.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Web.Models.Catalog;
using Nop.Core;

namespace Teed.Plugin.DiscountedProducts.Models
{
    public class DiscountedProductsModel : BaseNopEntityModel
    {
        public DiscountedProductsModel()
        {
            ListProducts = new List<ProductOverviewModel>();
            PagingFilteringContext = new CatalogPagingFilteringModel();
        }


        public List<ProductOverviewModel> ListProducts { get; set; }
        public DiscountedProductsSettings configModel { get; set; }
        public CatalogPagingFilteringModel PagingFilteringContext { get; set; }
        public string  pager { get; set; }
    }
}