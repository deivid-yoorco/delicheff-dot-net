using Nop.Web.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Web.Models.Catalog;
using Nop.Core;

namespace Teed.Plugin.FavoriteProducts.Models
{
    public class FavoriteProductsModel : BaseNopEntityModel
    {
        public FavoriteProductsModel()
        {
            ListProducts = new List<ProductOverviewModel>();
            PagingFilteringContext = new CatalogPagingFilteringModel();
        }


        public List<ProductOverviewModel> ListProducts { get; set; }
        public FavoriteProductsSettings configModel { get; set; }
        public CatalogPagingFilteringModel PagingFilteringContext { get; set; }
        public string  pager { get; set; }
    }
}