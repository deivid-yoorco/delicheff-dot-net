﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.Products;

namespace Teed.Plugin.Api.Dtos.Cart
{
    public class GetLoggedUserCartDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ShortDescription { get; set; }

        public decimal Price { get; set; }

        public string FullDescription { get; set; }

        public int Quantity { get; set; }

        public decimal Weight { get; set; }

        public int WarehouseId { get; set; }

        public int ProductStockQuantity { get; set; }

        public List<string> Pictures { get; set; }

        public List<ProductCategory> ProductCategories { get; set; }

        public List<ProductAttribute> ProductAttributes { get; set; }

        public List<SelectedAttribute> SelectedAttributes { get; set; }
    }

    public class SelectedAttribute
    {
        public int AttributeId { get; set; }

        public string AttributeName { get; set; }

        public int AttributeValueId { get; set; }

        public string AttributeValueName { get; set; }
    }
}
