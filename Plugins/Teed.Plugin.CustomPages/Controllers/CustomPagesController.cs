using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.CustomPages.Domain.CustomPages;
using Teed.Plugin.CustomPages.Models.CustomPages;
using Teed.Plugin.CustomPages.Security;
using Teed.Plugin.CustomPages.Services;

namespace Teed.Plugin.CustomPages.Controllers
{
    [Area(AreaNames.Admin)]
    public class CustomPagesController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IUrlRecordService _urlRecordService;
        private readonly CustomPagesService _customPagesService;
        private readonly BannersService _bannersService;
        private readonly BoxesService _boxesService;
        private readonly CarouselService _carouselService;
        private readonly CategoryDropdownService _categoryDropdownService;
        private readonly CollageService _collageService;
        private readonly ParallaxService _parallaxService;
        private readonly PopUpService _popUpService;
        private readonly SliderService _sliderService;
        private readonly TagsService _tagsService;
        private readonly TopThreeService _topThreeService;
        private readonly CustomPageProductService _customPageProductService;

        public CustomPagesController(IPermissionService permissionService, IWorkContext workContext,
            CustomPagesService customPagesService, IUrlRecordService urlRecordService, IProductService productService,
            BannersService bannersService,
            BoxesService boxesService,
            CarouselService carouselService,
            CategoryDropdownService categoryDropdownService,
            CollageService collageService,
            ParallaxService parallaxService,
            PopUpService popUpService,
            SliderService sliderService,
            TagsService tagsService,
            TopThreeService topThreeService,
            CustomPageProductService customPageProductService)
        {
            _permissionService = permissionService;
            _customPagesService = customPagesService;
            _workContext = workContext;
            _urlRecordService = urlRecordService;
            _bannersService = bannersService;
            _boxesService = boxesService;
            _carouselService = carouselService;
            _categoryDropdownService = categoryDropdownService;
            _collageService = collageService;
            _parallaxService = parallaxService;
            _popUpService = popUpService;
            _sliderService = sliderService;
            _tagsService = tagsService;
            _topThreeService = topThreeService;
            _customPageProductService = customPageProductService;
            _productService = productService;
        }

        public IActionResult List()
        {
            return View("~/Plugins/Teed.Plugin.CustomPages/Views/CustomPages/List.cshtml");
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(CustomPagesPermissionProvider.CustomPages))
                return AccessDeniedView();

            var customPages = _customPagesService.GetAllPaged(command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = customPages.OrderBy(x => x.DisplayOrder),
                Total = customPages.TotalCount
            };

            return Json(gridModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!_permissionService.Authorize(CustomPagesPermissionProvider.CustomPages))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.CustomPages/Views/CustomPages/Create.cshtml");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(CustomPagesPermissionProvider.CustomPages))
                return AccessDeniedView();

            CustomPage customPage = _customPagesService.GetById(id);
            if (customPage == null) return NotFound();

            EditViewModel model = new EditViewModel()
            {
                Id = customPage.Id,
                Name = customPage.Name,
                PrimaryColor = customPage.PrimaryColor,
                Published = customPage.Published,
                ProductSectionTitle = customPage.ProductSectionTitle,
                HideNavBar = customPage.HideNavBar,
                TabColor = customPage.TabColor,
                DisplayOrder = customPage.DisplayOrder
            };

            GetBannerData(model);
            GetBoxData(model);
            GetCarouselData(model);
            GetCategoryDropdownData(model);
            GetCollageData(model);
            GetParallaxData(model);
            GetPopUpData(model);
            GetSliderData(model);
            GetTagsData(model);
            GetTopThreeData(model);
            GetProductsData(model);

            ViewData["AllProducts"] = _productService.SearchProducts().Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = x.Name
            }).ToList();

            return View("~/Plugins/Teed.Plugin.CustomPages/Views/CustomPages/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult Create(CreateViewModel model)
        {
            if (!_permissionService.Authorize(CustomPagesPermissionProvider.CustomPages))
                return AccessDeniedView();

            CustomPage customPage = new CustomPage()
            {
                Name = model.Name,
                PrimaryColor = null,
                Published = false,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + $" creó la página {model.Name}.",
            };
            _customPagesService.Insert(customPage);

            var seName = customPage.ValidateSeName(model.Name, model.Name, true);
            _urlRecordService.SaveSlug(customPage, seName, 0);

            return RedirectToAction("Edit", new { customPage.Id });
        }

        [HttpPost]
        public IActionResult Edit(EditViewModel model)
        {
            if (!_permissionService.Authorize(CustomPagesPermissionProvider.CustomPages))
                return AccessDeniedView();

            CustomPage customPage = _customPagesService.GetById(model.Id);
            if (customPage == null) return NotFound();

            bool updateSlug = customPage.Name.ToLower() != model.Name.ToLower();
            customPage.Name = model.Name;
            customPage.PrimaryColor = model.PrimaryColor.Replace(" ", "").Replace("#", "");
            customPage.Published = model.Published;
            customPage.ProductSectionTitle = model.ProductSectionTitle;
            customPage.HideNavBar = model.HideNavBar;
            customPage.TabColor = model.TabColor.Replace(" ", "").Replace("#", "");
            customPage.DisplayOrder = model.DisplayOrder;

            _customPagesService.Update(customPage);

            if (updateSlug)
            {
                var seName = customPage.ValidateSeName(model.Name, model.Name, true);
                _urlRecordService.SaveSlug(customPage, seName, 0);
            }

            CreateOrUpdateBanner(model);
            CreateOrUpdateBox(model);
            CreateOrUpdateCarousel(model);
            CreateOrUpdateCategoryDropdown(model);
            CreateOrUpdateCollage(model);
            CreateOrUpdateParallax(model);
            CreateOrUpdatePopUp(model);
            CreateOrUpdateSlider(model);
            CreateOrUpdateTags(model);
            CreateOrUpdateTopThree(model);
            CreateOrUpdateProducts(model);

            return RedirectToAction("List");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetById(int id)
        {
            CustomPage customPage = _customPagesService.GetById(id);
            if (customPage == null) return NotFound();

            EditViewModel model = new EditViewModel()
            {
                Id = customPage.Id,
                Name = customPage.Name,
                PrimaryColor = customPage.PrimaryColor,
                Published = customPage.Published,
                ProductSectionTitle = customPage.ProductSectionTitle,
                HideNavBar = customPage.HideNavBar,
                TabColor = customPage.TabColor,
                DisplayOrder = customPage.DisplayOrder
            };

            GetBannerData(model);
            GetBoxData(model);
            GetCarouselData(model);
            GetCategoryDropdownData(model);
            GetCollageData(model);
            GetParallaxData(model);
            GetPopUpData(model);
            GetSliderData(model);
            GetTagsData(model);
            GetTopThreeData(model);
            GetProductsData(model);

            return Ok(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetCustomPages()
        {
            var customPages = _customPagesService.GetAll()
                .Where(x => x.Published).OrderBy(x => x.DisplayOrder)
                .ToList()
                .Select(x => new CustomPageLink()
                {
                    PageName = x.Name,
                    Slug = x.GetSeName(),
                    TabColor = x.TabColor
                });

            return Ok(customPages);
        }

        private void GetBannerData(EditViewModel model)
        {
            Banners element = _bannersService.GetByCustomPageId(model.Id);
            if (element != null)
            {
                model.BannerLink5 = element.BannerLink5;
                model.BannerLink6 = element.BannerLink6;
                model.BannerPicture5Id = element.BannerPicture5Id;
                model.BannerPicture6Id = element.BannerPicture6Id;
                model.BannerSubTitle5 = element.BannerSubTitle5;
                model.BannerSubTitle6 = element.BannerSubTitle6;
                model.BannerSubTitleColor5 = element.BannerSubTitleColor5;
                model.BannerSubTitleColor6 = element.BannerSubTitleColor6;
                model.BannerTitle5 = element.BannerTitle5;
                model.BannerTitle6 = element.BannerTitle6;
                model.BannerTitleColor5 = element.BannerTitleColor5;
                model.BannerTitleColor6 = element.BannerTitleColor6;
            }
        }

        private void GetBoxData(EditViewModel model)
        {
            Boxes element = _boxesService.GetByCustomPageId(model.Id);
            if (element != null)
            {
                model.BoxLink15 = element.BoxLink15;
                model.BoxLink16 = element.BoxLink16;
                model.BoxLink17 = element.BoxLink17;
                model.BoxLink18 = element.BoxLink18;
                model.BoxLink40 = element.BoxLink40;
                model.BoxLink41 = element.BoxLink41;
                model.BoxPicture15Id = element.BoxPicture15Id;
                model.BoxPicture16Id = element.BoxPicture16Id;
                model.BoxPicture17Id = element.BoxPicture17Id;
                model.BoxPicture18Id = element.BoxPicture18Id;
                model.BoxPicture40Id = element.BoxPicture40Id;
                model.BoxPicture41Id = element.BoxPicture41Id;
            }
        }

        private void GetCarouselData(EditViewModel model)
        {
            Carousel element = _carouselService.GetByCustomPageId(model.Id);
            if (element != null)
            {
                model.CarouselLink10 = element.CarouselLink10;
                model.CarouselLink11 = element.CarouselLink11;
                model.CarouselLink12 = element.CarouselLink12;
                model.CarouselLink13 = element.CarouselLink13;
                model.CarouselLink14 = element.CarouselLink14;
                model.CarouselLink7 = element.CarouselLink7;
                model.CarouselLink8 = element.CarouselLink8;
                model.CarouselLink9 = element.CarouselLink9;
                model.CarouselPicture10Id = element.CarouselPicture10Id;
                model.CarouselPicture11Id = element.CarouselPicture11Id;
                model.CarouselPicture12Id = element.CarouselPicture12Id;
                model.CarouselPicture13Id = element.CarouselPicture13Id;
                model.CarouselPicture14Id = element.CarouselPicture14Id;
                model.CarouselPicture7Id = element.CarouselPicture7Id;
                model.CarouselPicture8Id = element.CarouselPicture8Id;
                model.CarouselPicture9Id = element.CarouselPicture9Id;
                model.CarouselText = element.CarouselText;
                model.CarouselTextColor = element.CarouselTextColor;
                model.CarouselArrowId = element.CarouselArrowId;
            }
        }

        private void GetCategoryDropdownData(EditViewModel model)
        {
            CategoryDropdown element = _categoryDropdownService.GetByCustomPageId(model.Id);
            if (element != null)
            {
                model.CatLink6 = element.CatLink6;
                model.CatLinkDropdown1 = element.CatLinkDropdown1;
                model.CatLinkDropdown10 = element.CatLinkDropdown10;
                model.CatLinkDropdown2 = element.CatLinkDropdown2;
                model.CatLinkDropdown3 = element.CatLinkDropdown3;
                model.CatLinkDropdown4 = element.CatLinkDropdown4;
                model.CatLinkDropdown5 = element.CatLinkDropdown5;
                model.CatLinkDropdown6 = element.CatLinkDropdown6;
                model.CatLinkDropdown7 = element.CatLinkDropdown7;
                model.CatLinkDropdown8 = element.CatLinkDropdown8;
                model.CatLinkDropdown9 = element.CatLinkDropdown9;
                model.CatPicture6Id = element.CatPicture6Id;
                model.CatText6 = element.CatText6;
                model.CatTextColor6 = element.CatTextColor6;
                model.CatTextDropdown1 = element.CatTextDropdown1;
                model.CatTextDropdown10 = element.CatTextDropdown10;
                model.CatTextDropdown2 = element.CatTextDropdown2;
                model.CatTextDropdown3 = element.CatTextDropdown3;
                model.CatTextDropdown4 = element.CatTextDropdown4;
                model.CatTextDropdown5 = element.CatTextDropdown5;
                model.CatTextDropdown6 = element.CatTextDropdown6;
                model.CatTextDropdown7 = element.CatTextDropdown7;
                model.CatTextDropdown8 = element.CatTextDropdown8;
                model.CatTextDropdown9 = element.CatTextDropdown9;
            }
        }

        private void GetCollageData(EditViewModel model)
        {
            Collage element = _collageService.GetByCustomPageId(model.Id);
            if (element != null)
            {
                model.CollageEnable = element.CollageEnable;
                model.CollageLink19 = element.CollageLink19;
                model.CollageLink20 = element.CollageLink20;
                model.CollageLink21 = element.CollageLink21;
                model.CollageLink22 = element.CollageLink22;
                model.CollageLink23 = element.CollageLink23;
                model.CollageLink24 = element.CollageLink24;
                model.CollageLink25 = element.CollageLink25;
                model.CollageLink26 = element.CollageLink26;
                model.CollageLink27 = element.CollageLink27;
                model.CollageLink28 = element.CollageLink28;
                model.CollageLink29 = element.CollageLink29;
                model.CollageLink30 = element.CollageLink30;
                model.CollageLink31 = element.CollageLink31;
                model.CollageLink32 = element.CollageLink32;
                model.CollageLink33 = element.CollageLink33;
                model.CollageLink34 = element.CollageLink34;
                model.CollageLink35 = element.CollageLink35;
                model.CollageLink36 = element.CollageLink36;
                model.CollageLink37 = element.CollageLink37;
                model.CollageLink38 = element.CollageLink38;
                model.CollagePicture19Id = element.CollagePicture19Id;
                model.CollagePicture20Id = element.CollagePicture20Id;
                model.CollagePicture21Id = element.CollagePicture21Id;
                model.CollagePicture22Id = element.CollagePicture22Id;
                model.CollagePicture23Id = element.CollagePicture23Id;
                model.CollagePicture24Id = element.CollagePicture24Id;
                model.CollagePicture25Id = element.CollagePicture25Id;
                model.CollagePicture26Id = element.CollagePicture26Id;
                model.CollagePicture27Id = element.CollagePicture27Id;
                model.CollagePicture28Id = element.CollagePicture28Id;
                model.CollagePicture29Id = element.CollagePicture29Id;
                model.CollagePicture30Id = element.CollagePicture30Id;
                model.CollagePicture31Id = element.CollagePicture31Id;
                model.CollagePicture32Id = element.CollagePicture32Id;
                model.CollagePicture33Id = element.CollagePicture33Id;
                model.CollagePicture34Id = element.CollagePicture34Id;
                model.CollagePicture35Id = element.CollagePicture35Id;
                model.CollagePicture36Id = element.CollagePicture36Id;
                model.CollagePicture37Id = element.CollagePicture37Id;
                model.CollagePicture38Id = element.CollagePicture38Id;
            }
        }

        private void GetParallaxData(EditViewModel model)
        {
            Parallax element = _parallaxService.GetByCustomPageId(model.Id);
            if (element != null)
            {
                model.ParallaxLink1 = element.ParallaxLink1;
                model.ParallaxLink2 = element.ParallaxLink2;
                model.ParallaxPicture1Id = element.ParallaxPicture1Id;
                model.ParallaxPicture2Id = element.ParallaxPicture2Id;
                model.ParallaxText1 = element.ParallaxText1;
                model.ParallaxText2 = element.ParallaxText2;
            }
        }

        private void GetPopUpData(EditViewModel model)
        {
            PopUp element = _popUpService.GetByCustomPageId(model.Id);
            if (element != null)
            {
                model.PopUpEnable = element.PopUpEnable;
                model.PopUpPicture39Id = element.PopUpPicture39Id;
                model.PopUpPicture39ResponsiveId = element.PopUpPicture39ResponsiveId;
            }
        }

        private void GetSliderData(EditViewModel model)
        {
            Slider element = _sliderService.GetByCustomPageId(model.Id);
            if (element != null)
            {
                model.SliderBannerLink1 = element.SliderBannerLink1;
                model.SliderBannerLink2 = element.SliderBannerLink2;
                model.SliderBannerLink3 = element.SliderBannerLink3;
                model.SliderBannerLink4 = element.SliderBannerLink4;
                model.SliderBannerPicture1Id = element.SliderBannerPicture1Id;
                model.SliderBannerPicture2Id = element.SliderBannerPicture2Id;
                model.SliderBannerPicture3Id = element.SliderBannerPicture3Id;
                model.SliderBannerPicture4Id = element.SliderBannerPicture4Id;
                model.SliderBannerText1 = element.SliderBannerText1;
                model.SliderBannerText2 = element.SliderBannerText2;
                model.SliderBannerText3 = element.SliderBannerText3;
                model.SliderBannerText4 = element.SliderBannerText4;
                model.SliderVideoId = element.SliderVideoId;
                model.SliderArrowId = element.SliderArrowId;
            }
        }

        private void GetTagsData(EditViewModel model)
        {
            Tags element = _tagsService.GetByCustomPageId(model.Id);
            if (element != null)
            {
                model.TagsEnable = element.TagsEnable;
                model.TagsQty = element.TagsQty;
            }
        }

        private void GetTopThreeData(EditViewModel model)
        {
            TopThree element = _topThreeService.GetByCustomPageId(model.Id);
            if (element != null)
            {
                model.Top3Link3 = element.Top3Link3;
                model.Top3Link4 = element.Top3Link4;
                model.Top3Link5 = element.Top3Link5;
                model.Top3Picture3Id = element.Top3Picture3Id;
                model.Top3Picture4Id = element.Top3Picture4Id;
                model.Top3Picture5Id = element.Top3Picture5Id;
                model.Top3Text3 = element.Top3Text3;
                model.Top3Text4 = element.Top3Text4;
                model.Top3Text5 = element.Top3Text5;
            }
        }

        private void GetProductsData(EditViewModel model)
        {
            model.SelectedProductIds = _customPageProductService.GetAllByCustomPageId(model.Id).Select(x => x.ProductId).ToList();
        }

        private void CreateOrUpdateBanner(EditViewModel model)
        {
            Banners banner = _bannersService.GetByCustomPageId(model.Id);
            if (banner == null)
            {
                _bannersService.Insert(new Banners()
                {
                    BannerLink5 = model.BannerLink5,
                    BannerLink6 = model.BannerLink6,
                    BannerPicture5Id = model.BannerPicture5Id,
                    BannerPicture6Id = model.BannerPicture6Id,
                    BannerSubTitle5 = model.BannerSubTitle5,
                    BannerSubTitle6 = model.BannerSubTitle6,
                    BannerSubTitleColor5 = model.BannerSubTitleColor5,
                    BannerSubTitleColor6 = model.BannerSubTitleColor6,
                    BannerTitle5 = model.BannerTitle5,
                    BannerTitle6 = model.BannerTitle6,
                    BannerTitleColor5 = model.BannerTitleColor5,
                    BannerTitleColor6 = model.BannerTitleColor6,
                    CustomPageId = model.Id
                });
            }
            else
            {
                banner.BannerLink5 = model.BannerLink5;
                banner.BannerLink6 = model.BannerLink6;
                banner.BannerPicture5Id = model.BannerPicture5Id;
                banner.BannerPicture6Id = model.BannerPicture6Id;
                banner.BannerSubTitle5 = model.BannerSubTitle5;
                banner.BannerSubTitle6 = model.BannerSubTitle6;
                banner.BannerSubTitleColor5 = model.BannerSubTitleColor5;
                banner.BannerSubTitleColor6 = model.BannerSubTitleColor6;
                banner.BannerTitle5 = model.BannerTitle5;
                banner.BannerTitle6 = model.BannerTitle6;
                banner.BannerTitleColor5 = model.BannerTitleColor5;
                banner.BannerTitleColor6 = model.BannerTitleColor6;
                banner.CustomPageId = model.Id;

                _bannersService.Update(banner);
            }
        }

        private void CreateOrUpdateBox(EditViewModel model)
        {
            Boxes box = _boxesService.GetByCustomPageId(model.Id);
            if (box == null)
            {
                _boxesService.Insert(new Boxes()
                {
                    BoxLink15 = model.BoxLink15,
                    BoxLink16 = model.BoxLink16,
                    BoxLink17 = model.BoxLink17,
                    BoxLink18 = model.BoxLink18,
                    BoxLink40 = model.BoxLink40,
                    BoxLink41 = model.BoxLink41,
                    BoxPicture15Id = model.BoxPicture15Id,
                    BoxPicture16Id = model.BoxPicture16Id,
                    BoxPicture17Id = model.BoxPicture17Id,
                    BoxPicture18Id = model.BoxPicture18Id,
                    BoxPicture40Id = model.BoxPicture40Id,
                    BoxPicture41Id = model.BoxPicture41Id,
                    CustomPageId = model.Id
                });
            }
            else
            {
                box.BoxLink15 = model.BoxLink15;
                box.BoxLink16 = model.BoxLink16;
                box.BoxLink17 = model.BoxLink17;
                box.BoxLink18 = model.BoxLink18;
                box.BoxLink40 = model.BoxLink40;
                box.BoxLink41 = model.BoxLink41;
                box.BoxPicture15Id = model.BoxPicture15Id;
                box.BoxPicture16Id = model.BoxPicture16Id;
                box.BoxPicture17Id = model.BoxPicture17Id;
                box.BoxPicture18Id = model.BoxPicture18Id;
                box.BoxPicture40Id = model.BoxPicture40Id;
                box.BoxPicture41Id = model.BoxPicture41Id;
                box.CustomPageId = model.Id;

                _boxesService.Update(box);
            }
        }

        private void CreateOrUpdateCarousel(EditViewModel model)
        {
            Carousel carousel = _carouselService.GetByCustomPageId(model.Id);
            if (carousel == null)
            {
                _carouselService.Insert(new Carousel()
                {
                    CarouselLink10 = model.CarouselLink10,
                    CarouselLink11 = model.CarouselLink11,
                    CarouselLink12 = model.CarouselLink12,
                    CarouselLink13 = model.CarouselLink13,
                    CarouselLink14 = model.CarouselLink14,
                    CarouselLink7 = model.CarouselLink7,
                    CarouselLink8 = model.CarouselLink8,
                    CarouselLink9 = model.CarouselLink9,
                    CarouselPicture10Id = model.CarouselPicture10Id,
                    CarouselPicture11Id = model.CarouselPicture11Id,
                    CarouselPicture12Id = model.CarouselPicture12Id,
                    CarouselPicture13Id = model.CarouselPicture13Id,
                    CarouselPicture14Id = model.CarouselPicture14Id,
                    CarouselPicture7Id = model.CarouselPicture7Id,
                    CarouselPicture8Id = model.CarouselPicture8Id,
                    CarouselPicture9Id = model.CarouselPicture9Id,
                    CarouselText = model.CarouselText,
                    CarouselTextColor = model.CarouselTextColor,
                    CustomPageId = model.Id,
                    CarouselArrowId = model.CarouselArrowId
                });
            }
            else
            {
                carousel.CarouselLink10 = model.CarouselLink10;
                carousel.CarouselLink11 = model.CarouselLink11;
                carousel.CarouselLink12 = model.CarouselLink12;
                carousel.CarouselLink13 = model.CarouselLink13;
                carousel.CarouselLink14 = model.CarouselLink14;
                carousel.CarouselLink7 = model.CarouselLink7;
                carousel.CarouselLink8 = model.CarouselLink8;
                carousel.CarouselLink9 = model.CarouselLink9;
                carousel.CarouselPicture10Id = model.CarouselPicture10Id;
                carousel.CarouselPicture11Id = model.CarouselPicture11Id;
                carousel.CarouselPicture12Id = model.CarouselPicture12Id;
                carousel.CarouselPicture13Id = model.CarouselPicture13Id;
                carousel.CarouselPicture14Id = model.CarouselPicture14Id;
                carousel.CarouselPicture7Id = model.CarouselPicture7Id;
                carousel.CarouselPicture8Id = model.CarouselPicture8Id;
                carousel.CarouselPicture9Id = model.CarouselPicture9Id;
                carousel.CarouselText = model.CarouselText;
                carousel.CarouselTextColor = model.CarouselTextColor;
                carousel.CustomPageId = model.Id;
                carousel.CarouselArrowId = model.CarouselArrowId;

                _carouselService.Update(carousel);
            }
        }

        private void CreateOrUpdateCategoryDropdown(EditViewModel model)
        {
            CategoryDropdown categoryDropdown = _categoryDropdownService.GetByCustomPageId(model.Id);
            if (categoryDropdown == null)
            {
                _categoryDropdownService.Insert(new CategoryDropdown()
                {
                    CatLink6 = model.CatLink6,
                    CatLinkDropdown1 = model.CatLinkDropdown1,
                    CatLinkDropdown10 = model.CatLinkDropdown10,
                    CatLinkDropdown2 = model.CatLinkDropdown2,
                    CatLinkDropdown3 = model.CatLinkDropdown3,
                    CatLinkDropdown4 = model.CatLinkDropdown4,
                    CatLinkDropdown5 = model.CatLinkDropdown5,
                    CatLinkDropdown6 = model.CatLinkDropdown6,
                    CatLinkDropdown7 = model.CatLinkDropdown7,
                    CatLinkDropdown8 = model.CatLinkDropdown8,
                    CatLinkDropdown9 = model.CatLinkDropdown9,
                    CatPicture6Id = model.CatPicture6Id,
                    CatText6 = model.CatText6,
                    CatTextColor6 = model.CatTextColor6,
                    CatTextDropdown1 = model.CatTextDropdown1,
                    CatTextDropdown10 = model.CatTextDropdown10,
                    CatTextDropdown2 = model.CatTextDropdown2,
                    CatTextDropdown3 = model.CatTextDropdown3,
                    CatTextDropdown4 = model.CatTextDropdown4,
                    CatTextDropdown5 = model.CatTextDropdown5,
                    CatTextDropdown6 = model.CatTextDropdown6,
                    CatTextDropdown7 = model.CatTextDropdown7,
                    CatTextDropdown8 = model.CatTextDropdown8,
                    CatTextDropdown9 = model.CatTextDropdown9,
                    CustomPageId = model.Id
                });
            }
            else
            {
                categoryDropdown.CatLink6 = model.CatLink6;
                categoryDropdown.CatLinkDropdown1 = model.CatLinkDropdown1;
                categoryDropdown.CatLinkDropdown10 = model.CatLinkDropdown10;
                categoryDropdown.CatLinkDropdown2 = model.CatLinkDropdown2;
                categoryDropdown.CatLinkDropdown3 = model.CatLinkDropdown3;
                categoryDropdown.CatLinkDropdown4 = model.CatLinkDropdown4;
                categoryDropdown.CatLinkDropdown5 = model.CatLinkDropdown5;
                categoryDropdown.CatLinkDropdown6 = model.CatLinkDropdown6;
                categoryDropdown.CatLinkDropdown7 = model.CatLinkDropdown7;
                categoryDropdown.CatLinkDropdown8 = model.CatLinkDropdown8;
                categoryDropdown.CatLinkDropdown9 = model.CatLinkDropdown9;
                categoryDropdown.CatPicture6Id = model.CatPicture6Id;
                categoryDropdown.CatText6 = model.CatText6;
                categoryDropdown.CatTextColor6 = model.CatTextColor6;
                categoryDropdown.CatTextDropdown1 = model.CatTextDropdown1;
                categoryDropdown.CatTextDropdown10 = model.CatTextDropdown10;
                categoryDropdown.CatTextDropdown2 = model.CatTextDropdown2;
                categoryDropdown.CatTextDropdown3 = model.CatTextDropdown3;
                categoryDropdown.CatTextDropdown4 = model.CatTextDropdown4;
                categoryDropdown.CatTextDropdown5 = model.CatTextDropdown5;
                categoryDropdown.CatTextDropdown6 = model.CatTextDropdown6;
                categoryDropdown.CatTextDropdown7 = model.CatTextDropdown7;
                categoryDropdown.CatTextDropdown8 = model.CatTextDropdown8;
                categoryDropdown.CatTextDropdown9 = model.CatTextDropdown9;
                categoryDropdown.CustomPageId = model.Id;

                _categoryDropdownService.Update(categoryDropdown);
            }
        }

        private void CreateOrUpdateCollage(EditViewModel model)
        {
            Collage collage = _collageService.GetByCustomPageId(model.Id);
            if (collage == null)
            {
                _collageService.Insert(new Collage()
                {
                    CollageEnable = model.CollageEnable,
                    CollageLink19 = model.CollageLink19,
                    CollageLink20 = model.CollageLink20,
                    CollageLink21 = model.CollageLink21,
                    CollageLink22 = model.CollageLink22,
                    CollageLink23 = model.CollageLink23,
                    CollageLink24 = model.CollageLink24,
                    CollageLink25 = model.CollageLink25,
                    CollageLink26 = model.CollageLink26,
                    CollageLink27 = model.CollageLink27,
                    CollageLink28 = model.CollageLink28,
                    CollageLink29 = model.CollageLink29,
                    CollageLink30 = model.CollageLink30,
                    CollageLink31 = model.CollageLink31,
                    CollageLink32 = model.CollageLink32,
                    CollageLink33 = model.CollageLink33,
                    CollageLink34 = model.CollageLink34,
                    CollageLink35 = model.CollageLink35,
                    CollageLink36 = model.CollageLink36,
                    CollageLink37 = model.CollageLink37,
                    CollageLink38 = model.CollageLink38,
                    CollagePicture19Id = model.CollagePicture19Id,
                    CollagePicture20Id = model.CollagePicture20Id,
                    CollagePicture21Id = model.CollagePicture21Id,
                    CollagePicture22Id = model.CollagePicture22Id,
                    CollagePicture23Id = model.CollagePicture23Id,
                    CollagePicture24Id = model.CollagePicture24Id,
                    CollagePicture25Id = model.CollagePicture25Id,
                    CollagePicture26Id = model.CollagePicture26Id,
                    CollagePicture27Id = model.CollagePicture27Id,
                    CollagePicture28Id = model.CollagePicture28Id,
                    CollagePicture29Id = model.CollagePicture29Id,
                    CollagePicture30Id = model.CollagePicture30Id,
                    CollagePicture31Id = model.CollagePicture31Id,
                    CollagePicture32Id = model.CollagePicture32Id,
                    CollagePicture33Id = model.CollagePicture33Id,
                    CollagePicture34Id = model.CollagePicture34Id,
                    CollagePicture35Id = model.CollagePicture35Id,
                    CollagePicture36Id = model.CollagePicture36Id,
                    CollagePicture37Id = model.CollagePicture37Id,
                    CollagePicture38Id = model.CollagePicture38Id,
                    CustomPageId = model.Id
                });
            }
            else
            {
                collage.CollageEnable = model.CollageEnable;
                collage.CollageLink19 = model.CollageLink19;
                collage.CollageLink20 = model.CollageLink20;
                collage.CollageLink21 = model.CollageLink21;
                collage.CollageLink22 = model.CollageLink22;
                collage.CollageLink23 = model.CollageLink23;
                collage.CollageLink24 = model.CollageLink24;
                collage.CollageLink25 = model.CollageLink25;
                collage.CollageLink26 = model.CollageLink26;
                collage.CollageLink27 = model.CollageLink27;
                collage.CollageLink28 = model.CollageLink28;
                collage.CollageLink29 = model.CollageLink29;
                collage.CollageLink30 = model.CollageLink30;
                collage.CollageLink31 = model.CollageLink31;
                collage.CollageLink32 = model.CollageLink32;
                collage.CollageLink33 = model.CollageLink33;
                collage.CollageLink34 = model.CollageLink34;
                collage.CollageLink35 = model.CollageLink35;
                collage.CollageLink36 = model.CollageLink36;
                collage.CollageLink37 = model.CollageLink37;
                collage.CollageLink38 = model.CollageLink38;
                collage.CollagePicture19Id = model.CollagePicture19Id;
                collage.CollagePicture20Id = model.CollagePicture20Id;
                collage.CollagePicture21Id = model.CollagePicture21Id;
                collage.CollagePicture22Id = model.CollagePicture22Id;
                collage.CollagePicture23Id = model.CollagePicture23Id;
                collage.CollagePicture24Id = model.CollagePicture24Id;
                collage.CollagePicture25Id = model.CollagePicture25Id;
                collage.CollagePicture26Id = model.CollagePicture26Id;
                collage.CollagePicture27Id = model.CollagePicture27Id;
                collage.CollagePicture28Id = model.CollagePicture28Id;
                collage.CollagePicture29Id = model.CollagePicture29Id;
                collage.CollagePicture30Id = model.CollagePicture30Id;
                collage.CollagePicture31Id = model.CollagePicture31Id;
                collage.CollagePicture32Id = model.CollagePicture32Id;
                collage.CollagePicture33Id = model.CollagePicture33Id;
                collage.CollagePicture34Id = model.CollagePicture34Id;
                collage.CollagePicture35Id = model.CollagePicture35Id;
                collage.CollagePicture36Id = model.CollagePicture36Id;
                collage.CollagePicture37Id = model.CollagePicture37Id;
                collage.CollagePicture38Id = model.CollagePicture38Id;
                collage.CustomPageId = model.Id;

                _collageService.Update(collage);
            }
        }

        private void CreateOrUpdateParallax(EditViewModel model)
        {
            Parallax parallax = _parallaxService.GetByCustomPageId(model.Id);
            if (parallax == null)
            {
                _parallaxService.Insert(new Parallax()
                {
                    ParallaxLink1 = model.ParallaxLink1,
                    ParallaxLink2 = model.ParallaxLink2,
                    ParallaxPicture1Id = model.ParallaxPicture1Id,
                    ParallaxPicture2Id = model.ParallaxPicture2Id,
                    ParallaxText1 = model.ParallaxText1,
                    ParallaxText2 = model.ParallaxText2,
                    CustomPageId = model.Id
                });
            }
            else
            {
                parallax.ParallaxLink1 = model.ParallaxLink1;
                parallax.ParallaxLink2 = model.ParallaxLink2;
                parallax.ParallaxPicture1Id = model.ParallaxPicture1Id;
                parallax.ParallaxPicture2Id = model.ParallaxPicture2Id;
                parallax.ParallaxText1 = model.ParallaxText1;
                parallax.ParallaxText2 = model.ParallaxText2;
                parallax.CustomPageId = model.Id;

                _parallaxService.Update(parallax);
            }
        }

        private void CreateOrUpdatePopUp(EditViewModel model)
        {
            PopUp popUp = _popUpService.GetByCustomPageId(model.Id);
            if (popUp == null)
            {
                _popUpService.Insert(new PopUp()
                {
                    PopUpEnable = model.PopUpEnable,
                    PopUpPicture39Id = model.PopUpPicture39Id,
                    PopUpPicture39ResponsiveId = model.PopUpPicture39ResponsiveId,
                    CustomPageId = model.Id
                });
            }
            else
            {
                popUp.PopUpEnable = model.PopUpEnable;
                popUp.PopUpPicture39Id = model.PopUpPicture39Id;
                popUp.PopUpPicture39ResponsiveId = model.PopUpPicture39ResponsiveId;
                popUp.CustomPageId = model.Id;

                _popUpService.Update(popUp);
            }
        }

        private void CreateOrUpdateSlider(EditViewModel model)
        {
            Slider slider = _sliderService.GetByCustomPageId(model.Id);
            if (slider == null)
            {
                _sliderService.Insert(new Slider()
                {
                    SliderBannerLink1 = model.SliderBannerLink1,
                    SliderBannerLink2 = model.SliderBannerLink2,
                    SliderBannerLink3 = model.SliderBannerLink3,
                    SliderBannerLink4 = model.SliderBannerLink4,
                    SliderBannerPicture1Id = model.SliderBannerPicture1Id,
                    SliderBannerPicture2Id = model.SliderBannerPicture2Id,
                    SliderBannerPicture3Id = model.SliderBannerPicture3Id,
                    SliderBannerPicture4Id = model.SliderBannerPicture4Id,
                    SliderBannerText1 = model.SliderBannerText1,
                    SliderBannerText2 = model.SliderBannerText2,
                    SliderBannerText3 = model.SliderBannerText3,
                    SliderBannerText4 = model.SliderBannerText4,
                    SliderVideoId = model.SliderVideoId,
                    CustomPageId = model.Id,
                    SliderArrowId = model.SliderArrowId
                });
            }
            else
            {
                slider.SliderBannerLink1 = model.SliderBannerLink1;
                slider.SliderBannerLink2 = model.SliderBannerLink2;
                slider.SliderBannerLink3 = model.SliderBannerLink3;
                slider.SliderBannerLink4 = model.SliderBannerLink4;
                slider.SliderBannerPicture1Id = model.SliderBannerPicture1Id;
                slider.SliderBannerPicture2Id = model.SliderBannerPicture2Id;
                slider.SliderBannerPicture3Id = model.SliderBannerPicture3Id;
                slider.SliderBannerPicture4Id = model.SliderBannerPicture4Id;
                slider.SliderBannerText1 = model.SliderBannerText1;
                slider.SliderBannerText2 = model.SliderBannerText2;
                slider.SliderBannerText3 = model.SliderBannerText3;
                slider.SliderBannerText4 = model.SliderBannerText4;
                slider.SliderVideoId = model.SliderVideoId;
                slider.CustomPageId = model.Id;
                slider.SliderArrowId = model.SliderArrowId;

                _sliderService.Update(slider);
            }
        }

        private void CreateOrUpdateTags(EditViewModel model)
        {
            Tags tag = _tagsService.GetByCustomPageId(model.Id);
            if (tag == null)
            {
                _tagsService.Insert(new Tags()
                {
                    TagsEnable = model.TagsEnable,
                    TagsQty = model.TagsQty,
                    CustomPageId = model.Id
                });
            }
            else
            {
                tag.TagsEnable = model.TagsEnable;
                tag.TagsQty = model.TagsQty;
                tag.CustomPageId = model.Id;

                _tagsService.Update(tag);
            }
        }

        private void CreateOrUpdateTopThree(EditViewModel model)
        {
            TopThree topThree = _topThreeService.GetByCustomPageId(model.Id);
            if (topThree == null)
            {
                _topThreeService.Insert(new TopThree()
                {
                    Top3Link3 = model.Top3Link3,
                    Top3Link4 = model.Top3Link4,
                    Top3Link5 = model.Top3Link5,
                    Top3Picture3Id = model.Top3Picture3Id,
                    Top3Picture4Id = model.Top3Picture4Id,
                    Top3Picture5Id = model.Top3Picture5Id,
                    Top3Text3 = model.Top3Text3,
                    Top3Text4 = model.Top3Text4,
                    Top3Text5 = model.Top3Text5,
                    CustomPageId = model.Id
                });
            }
            else
            {
                topThree.Top3Link3 = model.Top3Link3;
                topThree.Top3Link4 = model.Top3Link4;
                topThree.Top3Link5 = model.Top3Link5;
                topThree.Top3Picture3Id = model.Top3Picture3Id;
                topThree.Top3Picture4Id = model.Top3Picture4Id;
                topThree.Top3Picture5Id = model.Top3Picture5Id;
                topThree.Top3Text3 = model.Top3Text3;
                topThree.Top3Text4 = model.Top3Text4;
                topThree.Top3Text5 = model.Top3Text5;
                topThree.CustomPageId = model.Id;

                _topThreeService.Update(topThree);
            }
        }

        private void CreateOrUpdateProducts(EditViewModel model)
        {
            List<int> existingProductsIds = _customPageProductService.GetAllByCustomPageId(model.Id).Select(x => x.ProductId).ToList();
            List<int> productsToDelete = existingProductsIds.Where(x => !model.SelectedProductIds.Contains(x)).ToList();
            foreach (var productId in productsToDelete)
            {
                CustomPageProduct customPageProduct = _customPageProductService.GetByCustomPageIdAndProductId(model.Id, productId);
                if (customPageProduct != null) _customPageProductService.Delete(customPageProduct);
            }

            foreach (var productId in model.SelectedProductIds)
            {
                if (_customPageProductService.GetByCustomPageIdAndProductId(model.Id, productId) == null)
                {
                    _customPageProductService.Insert(new CustomPageProduct()
                    {
                        ProductId = productId,
                        CustomPageId = model.Id
                    });
                }
            }
        }
    }
}