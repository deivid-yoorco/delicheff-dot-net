using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Shipping;

namespace Nop.Services.Helpers
{
    /// <summary>
    /// User agent helper
    /// </summary>
    public partial class ProductHelper : IProductHelper
    {
        #region Fields

        private readonly IShippingService _shippingService;
        private readonly IProductService _productService;
        private readonly IPriceLogService _priceLogService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public ProductHelper(IShippingService shippingService,
            IProductService productService,
            IPriceLogService priceLogService,
            IWorkContext workContext,
            IOrderService orderService)
        {
            this._shippingService = shippingService;
            this._productService = productService;
            this._priceLogService = priceLogService;
            this._workContext = workContext;
            this._orderService = orderService;
        }

        #endregion

        #region Product attributes

        public virtual bool AnyProductHasSpecificationOptionId(List<Product> products, int specificationOptionId)
        {
            var theyDo = products.SelectMany(x => x.ProductSpecificationAttributes)
                .Select(x => x.SpecificationAttributeOptionId)
                .Where(x => x == specificationOptionId)
                .Any();
            return theyDo;
        }

        #endregion
    }
}