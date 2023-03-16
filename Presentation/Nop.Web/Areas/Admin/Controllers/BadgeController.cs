using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Extensions;
using Nop.Web.Areas.Admin.Helpers;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System.Net.Http;
using System.Threading.Tasks;
using Nop.Services.Rewards;
using Nop.Web.Areas.Admin.Models.Rewards;
using Nop.Core.Domain.Rewards;
using Nop.Services.Helpers;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class BadgeController : BaseAdminController
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IBadgeService _badgeService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;

        #endregion

        #region Ctor

        public BadgeController(ICategoryService categoryService,
            IProductTagService productTagService,
            IProductService productService,
            IPermissionService permissionService,
            IWorkContext workContext,
            IBadgeService badgeService,
            IStaticCacheManager cacheManager,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService)
        {
            this._categoryService = categoryService;
            this._productTagService = productTagService;
            this._productService = productService;
            this._permissionService = permissionService;
            this._workContext = workContext;
            this._badgeService = badgeService;
            this._cacheManager = cacheManager;
            this._localizationService = localizationService;
            this._customerActivityService = customerActivityService;
        }

        #endregion

        #region Utilities

        protected virtual void PrepareModel(BadgeModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            PrepareSelectLists(model);
        }

        protected virtual void PrepareSelectLists(BadgeModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.AvailableProducts = new List<SelectListItem>();
            model.AvailableSubcategories = new List<SelectListItem>();
            model.AvailableTags = new List<SelectListItem>();
            model.SelectedProductIds = new List<int>();
            model.SelectedSubcategoryIds = new List<int>();
            model.SelectedTagIds = new List<int>();

            var allSubcategories = _categoryService.GetAllCategories(showHidden: true)
                .Where(x => !x.Deleted).ToList();
            var allProducts = _productService.GetAllProductsQuery()
                .Where(x => !x.Deleted).ToList();
            var allTags = _productTagService.GetAllProductTags()
                .ToList();

            model.AvailableSubcategories = allSubcategories.Select(x => new SelectListItem
            {
                Text = x.GetFormattedBreadCrumb(_categoryService),
                Value = x.Id.ToString()
            }).ToList();
            model.AvailableProducts = allProducts.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();
            model.AvailableTags = allTags.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();
            model.ElementType = Enum.GetValues(typeof(ElementType)).Cast<ElementType>().Select(v => new SelectListItem
            {
                Text = v.GetDisplayName(),
                Value = ((int)v).ToString()
            }).ToList();
        }

        #endregion

        #region Methods

        #region List

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageRewards))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public virtual IActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageRewards))
                return AccessDeniedKendoGridJson();

            var badges = _badgeService.GetBadges().ToList();
            var gridModel = new DataSourceResult
            {
                Data = badges.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description
                }),
                Total = badges.Count()
            };
            return Json(gridModel);
        }

        #endregion

        #region Create / Edit / Delete

        public async Task<IActionResult> Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageRewards))
                return AccessDeniedView();

            var model = new BadgeModel();

            PrepareModel(model);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(BadgeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageRewards))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var badge = new Badge
                {
                    Name = model.Name,
                    Description = model.Description,
                    BronzeImageId = model.BronzeImageId,
                    BronzeAmount = model.BronzeAmount,
                    SilverImageId = model.SilverImageId,
                    SilverAmount = model.SilverAmount,
                    GoldImageId = model.GoldImageId,
                    GoldAmount = model.GoldAmount,
                    ElementTypeId = model.ElementTypeId
                };

                if (badge.ElementTypeId == 1)
                    badge.ElementIds =
                        string.Join(",", model.SelectedSubcategoryIds);
                else if (badge.ElementTypeId == 2)
                    badge.ElementIds =
                        string.Join(",", model.SelectedTagIds);
                else if (badge.ElementTypeId == 3)
                    badge.ElementIds =
                        string.Join(",", model.SelectedProductIds);

                _badgeService.InsertBadge(badge);
                badge.Log = LogHelper.CreatingLog(_workContext.CurrentCustomer.Email,
                    _workContext.CurrentCustomer.Id.ToString(), "insignia", badge.Name, badge.Id.ToString());
                _badgeService.UpdateBadge(badge);

                SuccessNotification(_localizationService.GetResource("Admin.Reward.Badges.Created"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = badge.Id });
                }
                return RedirectToAction("List");
            }
            PrepareModel(model);
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageRewards))
                return AccessDeniedView();

            var badge = _badgeService.GetBadgeById(id);
            if (badge == null)
                return RedirectToAction("List");

            var model = new BadgeModel
            {
                Id = badge.Id,
                Name = badge.Name,
                Description = badge.Description,
                BronzeImageId = badge.BronzeImageId,
                BronzeAmount = badge.BronzeAmount,
                SilverImageId = badge.SilverImageId,
                SilverAmount = badge.SilverAmount,
                GoldImageId = badge.GoldImageId,
                GoldAmount = badge.GoldAmount,
                ElementTypeId = badge.ElementTypeId,
                Log = badge.Log
            };

            PrepareModel(model);
            if (badge.ElementTypeId == 1)
                model.SelectedSubcategoryIds =
                    badge.ElementIds.Split(',')
                    .Select(x => int.Parse(x)).ToList();
            else if (badge.ElementTypeId == 2)
                model.SelectedTagIds =
                    badge.ElementIds.Split(',')
                    .Select(x => int.Parse(x)).ToList();
            else if (badge.ElementTypeId == 3)
                model.SelectedProductIds =
                    badge.ElementIds.Split(',')
                    .Select(x => int.Parse(x)).ToList();

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(BadgeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageRewards))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var badge = _badgeService.GetBadgeById(model.Id);
                if (badge == null)
                    return RedirectToAction("List");

                var oldIds = badge.ElementIds;
                if (badge.ElementTypeId == 1)
                {
                    badge.ElementIds =
                        string.Join(",", model.SelectedSubcategoryIds);
                }
                else if (badge.ElementTypeId == 2)
                {
                    badge.ElementIds =
                        string.Join(",", model.SelectedTagIds);
                }
                else if (badge.ElementTypeId == 3)
                {
                    badge.ElementIds =
                        string.Join(",", model.SelectedProductIds);
                }

                if (badge.Name != model.Name)
                    badge.Log += LogHelper.ChangedEntityLog(_workContext.CurrentCustomer.Email,
                        _workContext.CurrentCustomer.Id.ToString(), "Nombre", badge.Name, model.Name);
                
                if (badge.Description != model.Description)
                    badge.Log += LogHelper.ChangedEntityLog(_workContext.CurrentCustomer.Email,
                        _workContext.CurrentCustomer.Id.ToString(), "Descripción", badge.Description, model.Description);
                
                if (badge.BronzeImageId != model.BronzeImageId)
                    badge.Log += LogHelper.ChangedEntityLog(_workContext.CurrentCustomer.Email,
                        _workContext.CurrentCustomer.Id.ToString(), "Imagen Bronze", badge.BronzeImageId.ToString(), model.BronzeImageId.ToString());
                
                if (badge.BronzeAmount != model.BronzeAmount)
                    badge.Log += LogHelper.ChangedEntityLog(_workContext.CurrentCustomer.Email,
                        _workContext.CurrentCustomer.Id.ToString(), "Monto Bronze", badge.BronzeAmount.ToString(), model.BronzeAmount.ToString());
                
                if (badge.SilverImageId != model.SilverImageId)
                    badge.Log += LogHelper.ChangedEntityLog(_workContext.CurrentCustomer.Email,
                        _workContext.CurrentCustomer.Id.ToString(), "Imagen plata", badge.SilverImageId.ToString(), model.SilverImageId.ToString());
                
                if (badge.SilverAmount != model.SilverAmount)
                    badge.Log += LogHelper.ChangedEntityLog(_workContext.CurrentCustomer.Email,
                        _workContext.CurrentCustomer.Id.ToString(), "Monto plata", badge.SilverAmount.ToString(), model.SilverAmount.ToString());
                
                if (badge.GoldImageId != model.GoldImageId)
                    badge.Log += LogHelper.ChangedEntityLog(_workContext.CurrentCustomer.Email,
                        _workContext.CurrentCustomer.Id.ToString(), "Imagen oro", badge.GoldImageId.ToString(), model.GoldImageId.ToString());
                
                if (badge.GoldAmount != model.GoldAmount)
                    badge.Log += LogHelper.ChangedEntityLog(_workContext.CurrentCustomer.Email,
                        _workContext.CurrentCustomer.Id.ToString(), "Monto oro", badge.GoldAmount.ToString(), model.GoldAmount.ToString());
                
                if (badge.ElementTypeId != model.ElementTypeId)
                    badge.Log += LogHelper.ChangedEntityLog(_workContext.CurrentCustomer.Email,
                        _workContext.CurrentCustomer.Id.ToString(), "Tipo de elementos", ((ElementType)badge.ElementTypeId).GetDisplayName(), ((ElementType)model.ElementTypeId).GetDisplayName());
                
                if (badge.ElementIds != oldIds || (badge.ElementIds != oldIds && badge.ElementTypeId != model.ElementTypeId))
                    badge.Log += LogHelper.ChangedEntityLog(_workContext.CurrentCustomer.Email,
                        _workContext.CurrentCustomer.Id.ToString(), "Elementos", badge.ElementIds, oldIds);

                badge.Name = model.Name;
                badge.Description = model.Description;
                badge.BronzeImageId = model.BronzeImageId;
                badge.BronzeAmount = model.BronzeAmount;
                badge.SilverImageId = model.SilverImageId;
                badge.SilverAmount = model.SilverAmount;
                badge.GoldImageId = model.GoldImageId;
                badge.GoldAmount = model.GoldAmount;
                badge.ElementTypeId = model.ElementTypeId;

                _badgeService.UpdateBadge(badge);

                SuccessNotification(_localizationService.GetResource("Admin.Reward.Badges.Edited"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = badge.Id });
                }
                return RedirectToAction("List");
            }

            PrepareModel(model);
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var badge = _badgeService.GetBadgeById(id);
            if (badge == null)
                //No badge found with the specified id
                return RedirectToAction("List");

            badge.Log += LogHelper.DeletingLog(_workContext.CurrentCustomer.Email,
                _workContext.CurrentCustomer.Id.ToString(), "insignia", badge.Name, badge.Id.ToString());
            _badgeService.DeleteBadge(badge);

            //activity log
            _customerActivityService.InsertActivity("DeleteBadge", _localizationService.GetResource("ActivityLog.DeleteBadge"), badge.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Reward.Levels.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #endregion
    }
}