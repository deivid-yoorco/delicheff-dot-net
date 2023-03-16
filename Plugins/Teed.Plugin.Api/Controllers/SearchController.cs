using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Events;
using Nop.Services.Media;
using Nop.Web.Framework.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.Categories;
using Teed.Plugin.Api.Dtos.Products;
using Teed.Plugin.Api.Dtos.Search;
using Teed.Plugin.Api.Helper;

namespace Teed.Plugin.Api.Controllers
{
    public class SearchController : ApiBaseController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly ISearchTermService _searchTermService;
        private readonly IStoreContext _storeContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkContext _workContext;
        private readonly IPictureService _pictureService;
        private readonly ICategoryService _categoryService;
        private readonly ICustomerService _customerService;
        private readonly IPriceCalculationService _priceCalculationService;

        #endregion

        #region Ctor

        public SearchController(IProductService productService,
            ISearchTermService searchTermService,
            IStoreContext storeContext,
            IEventPublisher eventPublisher,
            ICategoryService categoryService,
            IWorkContext workContext,
            IPictureService pictureService,
            ICustomerService customerService,
            IPriceCalculationService priceCalculationService)
        {
            _productService = productService;
            _searchTermService = searchTermService;
            _storeContext = storeContext;
            _eventPublisher = eventPublisher;
            _workContext = workContext;
            _pictureService = pictureService;
            _categoryService = categoryService;
            _customerService = customerService;
            _priceCalculationService = priceCalculationService;
        }

        #endregion

        #region DEPRECATED

        //[HttpGet]
        //[AllowAnonymous]
        //public virtual IActionResult SearchProducts(string searchTerms)
        //{
        //    int order = 0;
        //    var searchResults = _productService.SearchProducts(keywords: searchTerms)
        //        .AsEnumerable()
        //        .Select(x => new SearchDto()
        //        {
        //            Id = x.Id,
        //            ProductName = x.Name,
        //            ImageUrl = x.ProductPictures.FirstOrDefault() == null ? _pictureService.GetDefaultPictureUrl() : _pictureService.GetPictureUrl(x.ProductPictures.FirstOrDefault().PictureId),
        //            Price = x.Price,
        //            CreationDate = x.CreatedOnUtc,
        //            InitialOrder = order++,
        //            ProductAttributes = x.ProductSpecificationAttributes
        //                .Select(y => new Dtos.Search.ProductAttribute()
        //                {
        //                    SpecificationAttributeId = y.SpecificationAttributeOption.SpecificationAttributeId,
        //                    SpecificationAttributeName = y.SpecificationAttributeOption.SpecificationAttribute.Name,
        //                    SpecificationAttributeOptionId = y.SpecificationAttributeOptionId,
        //                    SpecificationAttributeOptionName = y.SpecificationAttributeOption.Name
        //                }).ToList()
        //        }).ToList();

        //    if (!string.IsNullOrEmpty(searchTerms))
        //    {
        //        var searchTerm = _searchTermService.GetSearchTermByKeyword(searchTerms, _storeContext.CurrentStore.Id);
        //        if (searchTerm != null)
        //        {
        //            searchTerm.Count++;
        //            _searchTermService.UpdateSearchTerm(searchTerm);
        //        }
        //        else
        //        {
        //            searchTerm = new SearchTerm
        //            {
        //                Keyword = searchTerms,
        //                StoreId = _storeContext.CurrentStore.Id,
        //                Count = 1
        //            };
        //            _searchTermService.InsertSearchTerm(searchTerm);
        //        }
        //    }

        //    _eventPublisher.Publish(new ProductSearchEvent
        //    {
        //        SearchTerm = searchTerms,
        //        SearchInDescriptions = true,
        //        CategoryIds = new int[0],
        //        ManufacturerId = 0,
        //        WorkingLanguageId = _workContext.WorkingLanguage.Id,
        //        VendorId = 0
        //    });

        //    return Ok(searchResults);
        //}

        #endregion

        #region Methods

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult SearchProducts(string searchTerm)
        {
            var searchResults = _productService.SearchProducts(keywords: searchTerm,
                searchDescriptions: true, searchProductTags: true, searchSku: true)
                .Where(x => !x.GiftProductEnable)
                .AsEnumerable()
                .Select(x => new ProductDto()
                {
                    Id = x.Id,
                    Sku = x.Sku,
                    Name = x.Name,
                    PictureUrl = "/Product/ProductImage?id=" + x.Id
                }).Take(15).ToList();

            return Ok(searchResults);
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult SearchProductsWithPage(string searchTerm, int page, int elementsPerPage, int sortBy)
        {
            Customer customer = null;
            if (!string.IsNullOrWhiteSpace(UserId))
                customer = _customerService.GetCustomerById(int.Parse(UserId));

            var searchResults = _productService.SearchProducts(keywords: searchTerm, pageIndex: page, pageSize: elementsPerPage,
                searchDescriptions: true, searchProductTags: true, searchSku: true, orderBy: (Nop.Core.Domain.Catalog.ProductSortingEnum)sortBy)
                .Where(x => !x.GiftProductEnable)
                .AsEnumerable()
                .Select(x => new ProductDto()
                {
                    Id = x.Id,
                    Sku = x.Sku,
                    Name = x.Name,
                    OldPrice = x.OldPrice > x.Price ? x.OldPrice : 0,
                    Price = x.Price,
                    Discount = _priceCalculationService.GetDiscountAmount(x, customer ?? new Customer(), x.Price, out List<DiscountForCaching> appliedDiscounts),
                    PictureUrl = "/Product/ProductImage?id=" + x.Id,
                    EquivalenceCoefficient = x.EquivalenceCoefficient,
                    WeightInterval = ProductHelper.GetWeightInterval(x),
                    CurrentCartQuantity = customer != null ? customer.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(y => y.Quantity).FirstOrDefault() : 0,
                    BuyingBySecondary = customer != null && customer.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(y => y.BuyingBySecondary).FirstOrDefault(),
                    SelectedPropertyOption = customer?.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.ShoppingCart).Select(y => y.SelectedPropertyOption).FirstOrDefault(),
                    PropertyOptions = x.PropertiesOptions?.Split(',').Select(z => z.ToUpper().First() + z.ToLower().Substring(1)).ToArray(),
                    IsInWishlist = customer != null && customer.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.Wishlist).Any(),
                    Stock = x.ManageInventoryMethod == ManageInventoryMethod.DontManageStock ? 1000 : x.StockQuantity
                }).ToList();

            return Ok(searchResults);
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult SearchCategories(string searchTerm)
        {
            var searchResults = _categoryService.GetAllCategories(categoryName: searchTerm)
                .Select(x => new CategoryDto()
                {
                    Id = x.Id,
                    Name = x.Name,
                    ParentCategoryId = x.ParentCategoryId
                }).ToList();

            return Ok(searchResults);
        }

        #endregion
    }
}
