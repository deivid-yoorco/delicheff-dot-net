using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Domain.Notifications;
using Teed.Plugin.Api.Dtos.Categories;
using Teed.Plugin.Api.Dtos.Home;
using Teed.Plugin.Api.Dtos.Onboarding;
using Teed.Plugin.Api.Dtos.Products;
using Teed.Plugin.Api.Dtos.TaggableBox;
using Teed.Plugin.Api.Helper;
using Teed.Plugin.Api.Services;

namespace Teed.Plugin.Api.Controllers
{
    public class HomeController : ApiBaseController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ICategoryService _categoryService;
        private readonly IOrderService _orderService;
        private readonly IProductTagService _productTagService;
        private readonly OnboardingService _onboardingService;
        private readonly TaggableBoxService _taggableBoxService;

        #endregion

        #region Ctor

        public HomeController(IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            IPictureService pictureService,
            IPriceCalculationService priceCalculationService,
            IProductService productService,
            ICategoryService categoryService,
            IOrderService orderService,
            ICustomerService customerService,
            IProductTagService productTagService,
            OnboardingService onboardingService,
            TaggableBoxService taggableBoxService)
        {
            _workContext = workContext;
            _storeService = storeService;
            _settingService = settingService;
            _pictureService = pictureService;
            _productService = productService;
            _customerService = customerService;
            _priceCalculationService = priceCalculationService;
            _categoryService = categoryService;
            _orderService = orderService;
            _onboardingService = onboardingService;
            _taggableBoxService = taggableBoxService;
            _productTagService = productTagService;
        }

        #endregion

        #region Methods

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult GetSliderPictures1()
        {
            var homePageSettings = _settingService.LoadSetting<TeedApiPluginSettings>();

            List<GetSliderPicturesFullDto> dto = new List<GetSliderPicturesFullDto>();
            if (homePageSettings.BannerPicture1Id != 0)
            {
                dto.Add(new GetSliderPicturesFullDto()
                {
                    SliderPictureUrl = _pictureService.GetPictureUrl(homePageSettings.BannerPicture1Id),
                    ActionTypeId = homePageSettings.BannerPicture1TypeId,
                    AdditionalData = GetActionTypeObject((ActionType)homePageSettings.BannerPicture1TypeId, homePageSettings.BannerPicture1AdditionalData),
                });
            }
            if (homePageSettings.BannerPicture2Id != 0)
            {
                dto.Add(new GetSliderPicturesFullDto()
                {
                    SliderPictureUrl = _pictureService.GetPictureUrl(homePageSettings.BannerPicture2Id),
                    ActionTypeId = homePageSettings.BannerPicture2TypeId,
                    AdditionalData = GetActionTypeObject((ActionType)homePageSettings.BannerPicture2TypeId, homePageSettings.BannerPicture2AdditionalData),
                });
            }
            if (homePageSettings.BannerPicture3Id != 0)
            {
                dto.Add(new GetSliderPicturesFullDto()
                {
                    SliderPictureUrl = _pictureService.GetPictureUrl(homePageSettings.BannerPicture3Id),
                    ActionTypeId = homePageSettings.BannerPicture3TypeId,
                    AdditionalData = GetActionTypeObject((ActionType)homePageSettings.BannerPicture3TypeId, homePageSettings.BannerPicture3AdditionalData),
                });
            }
            if (homePageSettings.BannerPicture4Id != 0)
            {
                dto.Add(new GetSliderPicturesFullDto()
                {
                    SliderPictureUrl = _pictureService.GetPictureUrl(homePageSettings.BannerPicture4Id),
                    ActionTypeId = homePageSettings.BannerPicture4TypeId,
                    AdditionalData = GetActionTypeObject((ActionType)homePageSettings.BannerPicture4TypeId, homePageSettings.BannerPicture4AdditionalData),
                });
            }
            if (homePageSettings.BannerPicture5Id != 0)
            {
                dto.Add(new GetSliderPicturesFullDto()
                {
                    SliderPictureUrl = _pictureService.GetPictureUrl(homePageSettings.BannerPicture5Id),
                    ActionTypeId = homePageSettings.BannerPicture5TypeId,
                    AdditionalData = GetActionTypeObject((ActionType)homePageSettings.BannerPicture5TypeId, homePageSettings.BannerPicture5AdditionalData),
                });
            }

            return Ok(dto);
        }

        // DEPRACTED 19/05/2022
        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult GetSliderPictures()
        {
            var homePageSettings = _settingService.LoadSetting<TeedApiPluginSettings>();

            List<GetSliderPicturesDto> dto = new List<GetSliderPicturesDto>();
            if (homePageSettings.BannerPicture1Id != 0)
            {
                dto.Add(new GetSliderPicturesDto()
                {
                    SliderPictureUrl = _pictureService.GetPictureUrl(homePageSettings.BannerPicture1Id)
                });
            }
            if (homePageSettings.BannerPicture2Id != 0)
            {
                dto.Add(new GetSliderPicturesDto()
                {
                    SliderPictureUrl = _pictureService.GetPictureUrl(homePageSettings.BannerPicture2Id)
                });
            }
            if (homePageSettings.BannerPicture3Id != 0)
            {
                dto.Add(new GetSliderPicturesDto()
                {
                    SliderPictureUrl = _pictureService.GetPictureUrl(homePageSettings.BannerPicture3Id)
                });
            }
            if (homePageSettings.BannerPicture4Id != 0)
            {
                dto.Add(new GetSliderPicturesDto()
                {
                    SliderPictureUrl = _pictureService.GetPictureUrl(homePageSettings.BannerPicture4Id)
                });
            }
            if (homePageSettings.BannerPicture5Id != 0)
            {
                dto.Add(new GetSliderPicturesDto()
                {
                    SliderPictureUrl = _pictureService.GetPictureUrl(homePageSettings.BannerPicture5Id)
                });
            }

            return Ok(dto);
        }

        protected virtual object GetActionTypeObject(ActionType type, string identifier)
        {
            switch (type)
            {
                case ActionType.OpenCategory:
                    var category = _categoryService.GetCategoryById(int.Parse(identifier));
                    if (category == null)
                        return null;
                    return new {
                        category.Id,
                        category.Name,
                        IsChild = category.ParentCategoryId > 0,
                    };
                case ActionType.OpenProduct:
                    var product = _productService.GetProductById(int.Parse(identifier));
                    if (product == null)
                        return null;
                    return new
                    {
                        product.Id,
                    };
                case ActionType.SearchTagOrKeyword:
                    if (string.IsNullOrEmpty(identifier))
                        return null;
                    return new
                    {
                        Keyword = identifier,
                    };
                case ActionType.OpenExternalLink:
                    if (string.IsNullOrEmpty(identifier))
                        return null;
                    return new
                    {
                        Link = identifier,
                    };
                default:
                    return null;
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetFeaturedCategories()
        {
            var homePageSettings = _settingService.LoadSetting<TeedApiPluginSettings>();
            if (homePageSettings.SelectedCategoriesIds.Count == 0 || !homePageSettings.IsActiveCategories) return NoContent();
            var categories = _categoryService.GetCategoriesByIds(homePageSettings.SelectedCategoriesIds.ToArray());

            var dto = categories.OrderBy(x => x.DisplayOrder).Select(x => new CategoryDto()
            {
                Id = x.Id,
                Name = x.Name,
                PictureUrl = x.PictureId == 0 ? _pictureService.GetDefaultPictureUrl() : _pictureService.GetPictureUrl(x.PictureId),
                ParentCategoryId = x.ParentCategoryId
            }).ToList();

            return Ok(dto);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetHomeProducts()
        {
            Customer customer = null;
            if (!string.IsNullOrWhiteSpace(UserId))
                customer = _customerService.GetCustomerById(int.Parse(UserId));

            var homePageSettings = _settingService.LoadSetting<TeedApiPluginSettings>();
            var dto = new GetHomeProductsDto()
            {
                HeaderText = homePageSettings.ProductsHeader,
                Products = _productService.GetProductsByIds(homePageSettings.SelectedProductsIds?.ToArray())
                .Where(x => x.Published)
                .Select(x => ProductHelper.GetProductDto(x, customer, _priceCalculationService))
                .ToList()
            };

            return Ok(dto);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetCategoryProducts()
        {
            Customer customer = null;
            if (!string.IsNullOrWhiteSpace(UserId))
                customer = _customerService.GetCustomerById(int.Parse(UserId));

            var categoryGroup = _productService.GetAllProductsQuery()
                .Where(x => x.Published)
                .GroupBy(x => x.ProductCategories.Where(y => y.Category.ParentCategoryId == 0 && y.Category.Published).Select(y => y.Category).FirstOrDefault())
                .ToList();

            Dictionary<int, int> customerBuyedProducts = null;
            if (customer != null)
            {
                var customerOrders = _orderService.GetAllOrdersQuery().Where(x => x.CustomerId == customer.Id).ToList();
                if (customerOrders.Count > 0)
                {
                    customerBuyedProducts = customerOrders.SelectMany(x => x.OrderItems).GroupBy(x => x.ProductId).ToDictionary(x => x.Key, x => x.Count());
                }
            }

            var dto = categoryGroup.Where(x => x.Key != null)
                .OrderBy(x => x.Key.DisplayOrder)
                .Select(x => new GetCategoryProductsDto()
                {
                    Category = new CategoryDto()
                    {
                        Id = x.Key.Id,
                        Name = x.Key.Name
                    },
                    Products = x.OrderByDescending(y => customerBuyedProducts == null ? y.DisplayOrder : GetCategoryProductOrder(customerBuyedProducts, y.Id))
                    .Take(5).Select(y => new ProductDto()
                    {
                        Id = y.Id,
                        Name = y.Name,
                        Sku = y.Sku,
                        OldPrice = y.OldPrice > y.Price ? y.OldPrice : 0,
                        PictureUrl = "/Product/ProductImage?id=" + y.Id,
                        Price = y.Price,
                        Discount = _priceCalculationService.GetDiscountAmount(y, customer ?? new Customer(), y.Price, out List<DiscountForCaching> appliedDiscounts),
                        EquivalenceCoefficient = y.EquivalenceCoefficient,
                        WeightInterval = ProductHelper.GetWeightInterval(y),
                        CurrentCartQuantity = customer != null ? customer.ShoppingCartItems.Where(z => z.ProductId == y.Id).Select(z => z.Quantity).FirstOrDefault() : 0,
                        BuyingBySecondary = customer != null && customer.ShoppingCartItems.Where(z => z.ProductId == y.Id).Select(z => z.BuyingBySecondary).FirstOrDefault(),
                        PropertyOptions = y.PropertiesOptions?.Split(',').Select(z => z.ToUpper().First() + z.ToLower().Substring(1)).ToArray(),
                        SelectedPropertyOption = customer?.ShoppingCartItems.Where(z => z.ProductId == y.Id).Select(z => z.SelectedPropertyOption).FirstOrDefault(),
                        Stock = y.ManageInventoryMethod == ManageInventoryMethod.DontManageStock ? 1000 : y.StockQuantity
                    }).ToList()
                }).ToList();

            return Ok(dto);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetSmartlookConfig()
        {
            var settings = _settingService.LoadSetting<TeedApiPluginSettings>();
            var dto = new SmartlookDto()
            {
                ProjectKey = settings.SmartlookProjectKey,
                IsActive = settings.SmartlookIsActive
            };

            return Ok(dto);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetActiveOnboardings()
        {
            var onboardings = _onboardingService.GetAll().Where(x => x.Active).Select(x => new GetActiveOnboardingsDto()
            {
                BackgroundColor = x.BackgroundColor,
                Subtitle = x.Subtitle,
                Title = x.Title,
                ImageId = x.ImageId ?? 0
            }).ToList();

            return Ok(onboardings);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetTaggableBoxes()
        {
            var boxes = _taggableBoxService.GetAll().ToList();

            var categoryIds = boxes.Where(x => x.Type == 2).Select(x => x.ElementId).ToArray();
            var categories = _categoryService.GetCategoriesByIds(categoryIds);

            var tagIds = boxes.Where(x => x.Type == 0).Select(x => x.ElementId).ToArray();
            var tags = _productTagService.GetAllProductTags();

            var dto = boxes.Select(x => new TaggableBoxDto()
            {
                ElementId = x.ElementId,
                PictureUrl = _pictureService.GetPictureUrl(x.PictureId),
                Position = x.Position,
                Type = x.Type,
                ElementName = x.Type == 2 ?
                categories.Where(y => y.Id == x.ElementId).Select(y => y.Name).FirstOrDefault() :
                x.Type == 0 ?
                tags.Where(y => y.Id == x.ElementId).Select(y => y.Name).FirstOrDefault() :
                null,
                IsChild = x.Type == 2 && categories.Where(y => y.Id == x.ElementId).FirstOrDefault()?.ParentCategoryId > 0
            }).ToList();

            return Ok(dto);
        }

        #endregion

        #region private methods

        private int GetCategoryProductOrder(Dictionary<int, int> customerBuyedProducts, int productId)
        {
            customerBuyedProducts.TryGetValue(productId, out int value);
            return value;
        }

        #endregion
    }
}
