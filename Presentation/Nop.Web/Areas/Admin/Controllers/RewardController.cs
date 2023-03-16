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
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Models.Rewards;
using Nop.Core.Domain.Rewards;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class RewardsController : BaseAdminController
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public RewardsController(ISettingService settingService, IPermissionService permissionService)
        {
            this._settingService = settingService;
            this._permissionService = permissionService;
        }

        #endregion

        #region Methods

        #region Configuration

        public async Task<IActionResult> Configuration()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageRewards))
                return AccessDeniedView();

            RewardSettings settings = _settingService.LoadSetting<RewardSettings>();
            var model = new RewardModel
            {
                Title = settings.Title,
                Description = settings.Description,
                Active = settings.Active,
                ImageId = settings.ImageId,
                LevelMonthsCount = settings.LevelMonthsCount,
                IsActive = settings.IsActive,
                OrderPoints = settings.OrderPoints,
                FacebookPoints = settings.FacebookPoints
            };
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Configuration(RewardModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageRewards))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                RewardSettings settings = _settingService.LoadSetting<RewardSettings>();
                settings.Active = model.Active;
                settings.Description = model.Description;
                settings.Title = model.Title;
                settings.ImageId = model.ImageId;
                settings.LevelMonthsCount = model.LevelMonthsCount;
                settings.IsActive = model.IsActive;
                settings.OrderPoints = model.OrderPoints;
                settings.FacebookPoints = model.FacebookPoints;

                _settingService.SaveSetting(settings);
                SuccessNotification("Se actualizó la configuración de Reward Level Points de forma correcta.");
                return View(model);
            }

            //If we got this far, something failed
            return View(model);
        }

        //public async Task<IActionResult> Challenges()
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageRewards))
        //        return AccessDeniedView();

        //    ChallengesSettings settings = _settingService.LoadSetting<ChallengesSettings>();
        //    var model = new ChallengeModel
        //    {
        //        AppLoginActive = settings.AppLoginActive,
        //        AppLoginAmount = settings.AppLoginAmount,
        //        CreateAccountActive = settings.CreateAccountActive,
        //        CreateAccountAmount = settings.CreateAccountAmount,
        //        EightWeeksMinimumBoughtActive = settings.EightWeeksMinimumBoughtActive,
        //        EightWeeksMinimumBoughtAmount = settings.EightWeeksMinimumBoughtAmount,
        //        EightWeeksMinimumBoughtRequired = settings.EightWeeksMinimumBoughtRequired,
        //        FacebookFollowActive = settings.FacebookFollowActive,
        //        FacebookFollowAmount = settings.FacebookFollowAmount,
        //        FacebookLinkActive = settings.FacebookLinkActive,
        //        FacebookLinkAmount = settings.FacebookLinkAmount,
        //        FourWeeksMinimumBoughtActive = settings.FourWeeksMinimumBoughtActive,
        //        FourWeeksMinimumBoughtAmount = settings.FourWeeksMinimumBoughtAmount,
        //        FourWeeksMinimumBoughtRequired = settings.FourWeeksMinimumBoughtRequired,
        //        InstagramFollowActive = settings.InstagramFollowActive,
        //        InstagramFollowAmount = settings.InstagramFollowAmount,
        //        ProfileFinishActive = settings.ProfileFinishActive,
        //        ProfileFinishAmount = settings.ProfileFinishAmount,
        //        TwelveWeeksMinimumBoughtActive = settings.TwelveWeeksMinimumBoughtActive,
        //        TwelveWeeksMinimumBoughtAmount = settings.TwelveWeeksMinimumBoughtAmount,
        //        TwelveWeeksMinimumBoughtRequired = settings.TwelveWeeksMinimumBoughtRequired
        //    };
        //    return View(model);
        //}

        //[HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        //public async Task<IActionResult> Challenges(ChallengeModel model)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageRewards))
        //        return AccessDeniedView();

        //    if (ModelState.IsValid)
        //    {
        //        ChallengesSettings settings = _settingService.LoadSetting<ChallengesSettings>();
        //        settings.AppLoginActive = model.AppLoginActive;
        //        settings.AppLoginAmount = model.AppLoginAmount;
        //        settings.CreateAccountActive = model.CreateAccountActive;
        //        settings.CreateAccountAmount = model.CreateAccountAmount;
        //        settings.EightWeeksMinimumBoughtActive = model.EightWeeksMinimumBoughtActive;
        //        settings.EightWeeksMinimumBoughtAmount = model.EightWeeksMinimumBoughtAmount;
        //        settings.EightWeeksMinimumBoughtRequired = model.EightWeeksMinimumBoughtRequired;
        //        settings.FacebookFollowActive = model.FacebookFollowActive;
        //        settings.FacebookFollowAmount = model.FacebookFollowAmount;
        //        settings.FacebookLinkActive = model.FacebookLinkActive;
        //        settings.FacebookLinkAmount = model.FacebookLinkAmount;
        //        settings.FourWeeksMinimumBoughtActive = model.FourWeeksMinimumBoughtActive;
        //        settings.FourWeeksMinimumBoughtAmount = model.FourWeeksMinimumBoughtAmount;
        //        settings.FourWeeksMinimumBoughtRequired = model.FourWeeksMinimumBoughtRequired;
        //        settings.InstagramFollowActive = model.InstagramFollowActive;
        //        settings.InstagramFollowAmount = model.InstagramFollowAmount;
        //        settings.ProfileFinishActive = model.ProfileFinishActive;
        //        settings.ProfileFinishAmount = model.ProfileFinishAmount;
        //        settings.TwelveWeeksMinimumBoughtActive = model.TwelveWeeksMinimumBoughtActive;
        //        settings.TwelveWeeksMinimumBoughtAmount = model.TwelveWeeksMinimumBoughtAmount;
        //        settings.TwelveWeeksMinimumBoughtRequired = model.TwelveWeeksMinimumBoughtRequired;

        //        _settingService.SaveSetting(settings);
        //        SuccessNotification("Se actualizó la configuración de Challenges de forma correcta.");
        //        return View(model);
        //    }

        //    //If we got this far, something failed
        //    return View(model);
        //}

        #endregion

        #endregion
    }
}