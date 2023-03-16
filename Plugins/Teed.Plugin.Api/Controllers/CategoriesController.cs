using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.Categories;
using Teed.Plugin.Api.Dtos.Products;
using Teed.Plugin.Api.Helper;

namespace Teed.Plugin.Api.Controllers
{
    public class CategoriesController : ApiBaseController
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IPriceCalculationService _priceCalculationService;

        #endregion

        #region Ctor

        public CategoriesController(
            ICategoryService categoryService,
            IPictureService pictureService,
            IProductService productService,
            IPriceCalculationService priceCalculationService,
            ICustomerService customerService)
        {
            _categoryService = categoryService;
            _pictureService = pictureService;
            _productService = productService;
            _customerService = customerService;
            _priceCalculationService = priceCalculationService;
        }

        #endregion

        #region Methods

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult GetAll()
        {
            var categories = _categoryService.GetAllCategories().Select(x => new GetAllCategoriesDto()
            {
                CategoryId = x.Id,
                CategoryName = x.Name,
                ImageUrl = _pictureService.GetPictureUrl(x.PictureId),
                IsParent = x.ParentCategoryId == 0,
                ProductsCount = _productService.GetNumberOfProductsInCategory(new List<int>() { x.Id })
            }).ToList();

            return Ok(categories);
        }

        #region DEPRECATED

        //[HttpGet]
        //[AllowAnonymous]
        //public virtual IActionResult GetProductsInCategory(int categoryId)
        //{
        //    int order = 0;
        //    var products = _productService.SearchProducts(categoryIds: new List<int>() { categoryId })
        //        .Where(x => x.Published && x.StockQuantity > 0)
        //        .OrderByDescending(x => x.ShowOnHomePage)
        //        .ThenBy(x => x.DisplayOrder)
        //        .AsEnumerable()
        //        .Select(x => new GetProductsInCategoryDto()
        //        {
        //            Id = x.Id,
        //            ProductName = x.Name,
        //            ImageUrl = x.ProductPictures.FirstOrDefault() == null ? _pictureService.GetDefaultPictureUrl() : _pictureService.GetPictureUrl(x.ProductPictures.FirstOrDefault().PictureId),
        //            Price = x.Price,
        //            CreationDate = x.CreatedOnUtc,
        //            InitialOrder = order++,
        //            ProductAttributes = x.ProductSpecificationAttributes
        //                .Select(y => new ProductAttribute()
        //                {
        //                    SpecificationAttributeId = y.SpecificationAttributeOption.SpecificationAttributeId,
        //                    SpecificationAttributeName = y.SpecificationAttributeOption.SpecificationAttribute.Name,
        //                    SpecificationAttributeOptionId = y.SpecificationAttributeOptionId,
        //                    SpecificationAttributeOptionName = y.SpecificationAttributeOption.Name
        //                }).ToList()
        //        }).ToList();

        //    return Ok(products);
        //}

        #endregion

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult GetProductsInCategory(int categoryId, int page, int elementsPerPage, int sortBy)
        {
            Customer customer = null;
            if (!string.IsNullOrWhiteSpace(UserId))
                customer = _customerService.GetCustomerById(int.Parse(UserId));

            var products = _productService.SearchProducts(categoryIds: new List<int>() { categoryId }, overridePublished: true, 
                pageIndex: page, pageSize: elementsPerPage, orderBy: (Nop.Core.Domain.Catalog.ProductSortingEnum)sortBy)
                .Where(x => x.StockQuantity > 0 && !x.GiftProductEnable)
                .ToList();

            var dto = products.Select(x => new ProductDto()
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
                IsInWishlist = customer!= null && customer.ShoppingCartItems.Where(y => y.ProductId == x.Id && y.ShoppingCartType == ShoppingCartType.Wishlist).Any(),
                Stock = x.ManageInventoryMethod == ManageInventoryMethod.DontManageStock ? 1000 : x.StockQuantity
            }).ToList();

            return Ok(dto);
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult GetChildCategories(int parentCategoryId)
        {
            var childCategories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId);
            var dto = childCategories.Select(x => new CategoryDto()
            {
                Id = x.Id,
                Name = x.Name,
                PictureUrl = x.PictureId == 0 ? _pictureService.GetDefaultPictureUrl() : _pictureService.GetPictureUrl(x.PictureId)
            }).ToList();

            return Ok(dto);
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult GetCategoryTree()
        {
            var dto = _categoryService.GetAllCategories()
                .Select(x => new CategoryTreeDto() { 
                    Id = x.Id,
                    Name = x.Name,
                    ParentId = x.ParentCategoryId
                }).ToList();

            return Ok(dto);
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult GetParentCategories()
        {
            var dto = _categoryService.GetAllCategories()
                .Where(y => y.ParentCategoryId == 0 && y.Published)
                .OrderBy(y => y.DisplayOrder)
                .Select(x => new ParentCategoriesDto()
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToList();

            return Ok(dto);
        }

        #endregion
    }
}
