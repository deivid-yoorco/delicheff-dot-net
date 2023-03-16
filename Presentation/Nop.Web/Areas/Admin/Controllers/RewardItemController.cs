using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.RewardItem;
using Nop.Web.Framework.Kendoui;
using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Services.Events;
using Nop.Core.Domain.Logging;
using Nop.Web.Framework.Mvc;
using Nop.Core.Plugins;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class RewardItemController : BaseAdminController
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IRewardItemService _rewardItemService;
        private readonly IProductService _productService;
        private readonly RewardItemSettings _rewardItemSettings;
        private readonly IWorkContext _workContext;
        private readonly ICategoryService _categoryService;
        private readonly ILocalizationService _localizationService;

        #endregion Fields


        public RewardItemController(ISettingService settingService, IPermissionService permissionService,
            IRewardItemService rewardItemService, IProductService productService,
            RewardItemSettings rewardItemSettings, IWorkContext workContext,
            ICategoryService categoryService, ILocalizationService localizationService)
        {
            this._settingService = settingService;
            this._permissionService = permissionService;
            this._rewardItemService = rewardItemService;
            this._productService = productService;
            this._rewardItemSettings = rewardItemSettings;
            this._workContext = workContext;
            this._categoryService = categoryService;
            this._localizationService = localizationService;
        }

        [HttpGet]
        public virtual IActionResult RewardItemView()
        {
            var model = new RewardItemSettingViewModel
            {
                MinimumAmount = _rewardItemSettings.MinimumAmount
            };

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Edit(RewardItemSettingViewModel model)
        {
            _rewardItemSettings.MinimumAmount = model.MinimumAmount;
            _settingService.SaveSetting(_rewardItemSettings);
            return RedirectToAction("RewardItemView");
        }

        [HttpPost]
        public virtual IActionResult UpdateRewardItem(RewardItemsModel model)
        {
            var rewardItemUpdate = _rewardItemService.GetRewardItemById(model.Id);
            rewardItemUpdate.IsActive = model.IsActive;
            rewardItemUpdate.Log += $"{ DateTime.Now:dd-MM-yyyy hh:mm:ss tt} -El usuario { _workContext.CurrentCustomer.Email} ({ _workContext.CurrentCustomer.Id}) desactivo el producto .\n";
            _rewardItemService.UpdateRewardItem(rewardItemUpdate);
            return new NullJsonResult();
        }

        [HttpPost]
        public virtual IActionResult DeleteRewardItem(RewardItemsModel model)
        {
            var rewardItemUpdate = _rewardItemService.GetRewardItemById(model.Id);
            rewardItemUpdate.Log += $"{ DateTime.Now:dd-MM-yyyy hh:mm:ss tt} -El usuario { _workContext.CurrentCustomer.Email} ({ _workContext.CurrentCustomer.Id}) eliminó el producto .\n";
            _rewardItemService.DeleteRewardItem(rewardItemUpdate);
            return new NullJsonResult();
        }

        [HttpGet]
        public virtual IActionResult AddProductsRewardItem()
        {
            var model = new RewardItemsModel();

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult AddProductsRewardItem(RewardItemsModel model)
        {
            var product = _productService.GetProductById(model.ProductId);
            if (product == null)
                return RedirectToAction("RewardItemView");
            RewardItem rewardItem = new RewardItem()
            {
                ProductId = product.Id,
                BuyingBySecondary = model.BuyingBySecondary,
                Quantity = model.Quantity,
                SelectedPropertyOption = model.SelectedPropertyOption,
                IsActive = false,
                Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) agregó el producto \"{product.Name}\".\n"
            };
            _rewardItemService.InsertRewardItem(rewardItem);
            return RedirectToAction("RewardItemView");
        }

        [HttpPost]
        public virtual IActionResult LoadRewardsItems(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedKendoGridJson();

            var rewardsItems = _rewardItemService.GetAllRewardItemQuery().ToList();
            var productIds = rewardsItems.Select(z => z.ProductId).Distinct().ToList();
            var products = _productService.GetAllProductsQuery().Where(x => productIds.Contains(x.Id) && x.Published).Select(x => new
            {
                x.Id,
                x.Name,
                x.Price,
                x.ProductCost
            }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = products.Select(x => new
                {
                    Name = x.Name,
                    Price = x.Price,
                    ProductCost = x.ProductCost,
                    Quantity = rewardsItems.Where(y => y.ProductId == x.Id).FirstOrDefault().Quantity,
                    BuyingBySecondary = rewardsItems.Where(y => y.ProductId == x.Id).FirstOrDefault().BuyingBySecondary,
                    SelectedPropertyOption = rewardsItems.Where(y => y.ProductId == x.Id).FirstOrDefault().SelectedPropertyOption,
                    IsActive = rewardsItems.Where(y => y.ProductId == x.Id).Select(y => y.IsActive).FirstOrDefault(),
                    Id = rewardsItems.Where(y => y.ProductId == x.Id).Select(y => y.Id).FirstOrDefault()
                })
                ,
                Total = products.Count()
            };

            return Json(gridModel);
        }

        protected virtual List<int> GetChildCategoryIds(int parentCategoryId)
        {
            var categoriesIds = new List<int>();
            var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId, true);
            foreach (var category in categories)
            {
                categoriesIds.Add(category.Id);
                categoriesIds.AddRange(GetChildCategoryIds(category.Id));
            }
            return categoriesIds;
        }

        [HttpGet]
        public IActionResult GetProductFiltering(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                text = text.Trim().ToLower();
                var rewardsItems = _rewardItemService.GetAllRewardItemQuery().ToList();
                var RewardItemIds = rewardsItems.Select(x => x.ProductId).ToList();
                var products = _productService.GetAllProductsQuery()
                    .Where(x => !RewardItemIds.Contains(x.Id) && (x.Name.ToLower().Contains(text))).ToList()
                    .Select(x => new { Name = $"{x.Name} ({x.Price})", Id = x.Id.ToString() }).ToList();
                var productsFilter = products.Select(x => new
                {
                    id = x.Id,
                    name = x.Name
                }).ToList();

                return Json(productsFilter);
            }
            return Ok();
        }
    }

}