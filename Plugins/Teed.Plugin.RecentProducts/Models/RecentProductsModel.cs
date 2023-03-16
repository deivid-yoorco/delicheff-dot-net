using Nop.Web.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Web.Models.Catalog;
using Nop.Core;

namespace Teed.Plugin.RecentProducts.Models
{
    public class RecentProductsModel : BaseNopEntityModel
    {
        public RecentProductsModel()
        {
            //ListProducts = new List<ProductOverviewModel>();
            PagingFilteringContext = new CatalogPagingFilteringModel();
        }


        public List<CategoryProducsGroup> CategoryProducsGroups { get; set; }
        //public List<ProductOverviewModel> ListProducts { get; set; }
        public RecentProductsSettings configModel { get; set; }
        public CatalogPagingFilteringModel PagingFilteringContext { get; set; }
        public string  pager { get; set; }
    }

    public class CategoryProducsGroup : BaseNopEntityModel
    {
        public CategoryProducsGroup()
        {
            ListProducts = new List<ProductOverviewModel>();
        }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<ProductOverviewModel> ListProducts { get; set; }
    }

}