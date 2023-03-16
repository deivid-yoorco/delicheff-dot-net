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
    public partial class LevelsController : BaseAdminController
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IStaticCacheManager _cacheManager;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILevelService _levelService;
        private readonly ILevelHistoryService _levelHistoryService;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public LevelsController(ICategoryService categoryService,
            IProductTagService productTagService,
            IProductService productService,
            IPermissionService permissionService,
            IWorkContext workContext,
            IStaticCacheManager cacheManager,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            ILevelService levelService,
            ILevelHistoryService levelHistoryService,
            ICustomerService customerService)
        {
            this._categoryService = categoryService;
            this._productTagService = productTagService;
            this._productService = productService;
            this._permissionService = permissionService;
            this._workContext = workContext;
            this._cacheManager = cacheManager;
            this._localizationService = localizationService;
            this._customerActivityService = customerActivityService;
            this._levelService = levelService;
            this._customerService = customerService;
            this._levelHistoryService = levelHistoryService;
        }

        #endregion

        #region Utilities

        protected virtual void PrepareModel(LevelModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            PrepareSelectLists(model);
        }

        protected virtual void PrepareSelectLists(LevelModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.CustomerRoles = new List<SelectListItem>();

            var allRoles = _customerService.GetAllCustomerRoles(showHidden: true)
                .ToList();

            model.CustomerRoles = allRoles.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public virtual IActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedKendoGridJson();

            var levels = _levelService.GetLevels().ToList();
            var gridModel = new DataSourceResult
            {
                Data = levels.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description
                }),
                Total = levels.Count()
            };
            return Json(gridModel);
        }

        #endregion

        #region Create / Edit / Delete

        public async Task<IActionResult> Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var model = new LevelModel();

            PrepareModel(model);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(LevelModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var level = new Level
                {
                    Name = model.Name,
                    Description = model.Description,
                    RequiredAmount = model.RequiredAmount,
                    CustomerRoleId = model.CustomerRoleId
                };

                _levelService.InsertLevel(level);
                level.Log = LogHelper.CreatingLog(_workContext.CurrentCustomer.Email,
                    _workContext.CurrentCustomer.Id.ToString(), "nivel", level.Name, level.Id.ToString());
                _levelService.UpdateLevel(level);

                SuccessNotification(_localizationService.GetResource("Admin.Reward.Levels.Created"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = level.Id });
                }
                return RedirectToAction("List");
            }
            PrepareModel(model);
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var level = _levelService.GetLevelById(id);
            if (level == null)
                return RedirectToAction("List");

            var model = new LevelModel
            {
                Id = level.Id,
                Name = level.Name,
                Description = level.Description,
                RequiredAmount = level.RequiredAmount,
                CustomerRoleId = level.CustomerRoleId,
                Log = level.Log
            };

            PrepareModel(model);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(LevelModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var level = _levelService.GetLevelById(model.Id);
                if (level == null)
                    return RedirectToAction("List");

                if (level.Name != model.Name)
                    level.Log += LogHelper.ChangedEntityLog(_workContext.CurrentCustomer.Email,
                        _workContext.CurrentCustomer.Id.ToString(), "Nombre", level.Name, model.Name);

                if (level.Description != model.Description)
                    level.Log += LogHelper.ChangedEntityLog(_workContext.CurrentCustomer.Email,
                        _workContext.CurrentCustomer.Id.ToString(), "Descripción", level.Description, model.Description);

                if (level.RequiredAmount != model.RequiredAmount)
                    level.Log += LogHelper.ChangedEntityLog(_workContext.CurrentCustomer.Email,
                        _workContext.CurrentCustomer.Id.ToString(), "Monto necesario", level.RequiredAmount.ToString(), model.RequiredAmount.ToString());

                if (level.CustomerRoleId != model.CustomerRoleId)
                    level.Log += LogHelper.ChangedEntityLog(_workContext.CurrentCustomer.Email,
                        _workContext.CurrentCustomer.Id.ToString(), "Id de rol a asignar", level.CustomerRoleId.ToString(), model.CustomerRoleId.ToString());

                level.Name = model.Name;
                level.Description = model.Description;
                level.RequiredAmount = model.RequiredAmount;
                level.CustomerRoleId = model.CustomerRoleId;

                _levelService.UpdateLevel(level);

                SuccessNotification(_localizationService.GetResource("Admin.Reward.Levels.Edited"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = level.Id });
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

            var level = _levelService.GetLevelById(id);
            if (level == null)
                //No level found with the specified id
                return RedirectToAction("List");

            level.Log += LogHelper.DeletingLog(_workContext.CurrentCustomer.Email,
                _workContext.CurrentCustomer.Id.ToString(), "nivel", level.Name, level.Id.ToString());
            _levelService.DeleteLevel(level);

            //activity log
            _customerActivityService.InsertActivity("DeleteLevel", _localizationService.GetResource("ActivityLog.DeleteLevel"), level.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Reward.Levels.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #endregion
    }
}