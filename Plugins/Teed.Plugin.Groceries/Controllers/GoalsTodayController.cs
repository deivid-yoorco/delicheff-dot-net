using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System.Linq;
using Teed.Plugin.Groceries;
using Teed.Plugin.Groceries.Models.MonitorHq;
using Teed.Plugin.Groceries.Security;

namespace Teed.Plugin.Groceries.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class GoalsTodayController : BasePluginController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly GoalsTodaySettings _goalsTodaySettings;


        public GoalsTodayController(IPermissionService permissionService,
            GoalsTodaySettings goalsTodaySettings,
            IWorkContext workContext,
            ISettingService settingService)
        {
            _goalsTodaySettings = goalsTodaySettings;
            _workContext = workContext;
            _settingService = settingService;
            _permissionService = permissionService;
        }

        #endregion

        [AuthorizeAdmin]
        public IActionResult GoalsTodayView()
        {
            //whether user has the authority
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.DashboardHqConfig))
                return AccessDeniedView();

            //prepare model
            var model = new GoalsTodayModel
            {
                GoalsMonday = _goalsTodaySettings.GoalsMonday,
                GoalsTuesday = _goalsTodaySettings.GoalsTuesday,
                GoalsWednesday = _goalsTodaySettings.GoalsWednesday,
                GoalsThursday = _goalsTodaySettings.GoalsThursday,
                GoalsFriday = _goalsTodaySettings.GoalsFriday,
                GoalsSaturday = _goalsTodaySettings.GoalsSaturday
            };
            return View("~/Plugins/Teed.Plugin.Groceries/Views/MarketingDashboardGoalsToday/GoalsToday.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult GoalsTodayView(GoalsTodayModel model)
        {
            //whether user has the authority
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.DashboardHqConfig))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return GoalsTodayView();

            //save settings
            _goalsTodaySettings.GoalsMonday = model.GoalsMonday;
            _goalsTodaySettings.GoalsTuesday = model.GoalsTuesday;
            _goalsTodaySettings.GoalsWednesday = model.GoalsWednesday;
            _goalsTodaySettings.GoalsThursday = model.GoalsThursday;
            _goalsTodaySettings.GoalsFriday = model.GoalsFriday;
            _goalsTodaySettings.GoalsSaturday = model.GoalsSaturday;
            _settingService.SaveSetting(_goalsTodaySettings);

            return GoalsTodayView();
        }
    }
}