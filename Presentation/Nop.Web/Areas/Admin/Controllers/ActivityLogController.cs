using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using Nop.Web.Areas.Admin.Extensions;
using Nop.Web.Areas.Admin.Models.Logging;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Localization;
using Nop.Core;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class ActivityLogController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;

        #endregion Fields

        #region Ctor

        public ActivityLogController(ICustomerActivityService customerActivityService,
            IDateTimeHelper dateTimeHelper, ILocalizationService localizationService,
            IPermissionService permissionService, IWorkContext workContext)
        {
            this._customerActivityService = customerActivityService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._workContext = workContext;
        }

        #endregion

        #region Activity log types

        public virtual IActionResult ListTypes()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            var model = _customerActivityService
                .GetAllActivityTypes()
                .Select(x => x.ToModel())
                .ToList();
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult SaveTypes(IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            //activity log
            _customerActivityService.InsertActivity("EditActivityLogTypes", _localizationService.GetResource("ActivityLog.EditActivityLogTypes"));

            var formKey = "checkbox_activity_types";
            var checkedActivityTypes = !StringValues.IsNullOrEmpty(form[formKey]) ?
                form[formKey].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x)).ToList() :
                new List<int>();

            var activityTypes = _customerActivityService.GetAllActivityTypes();
            foreach (var activityType in activityTypes)
            {
                activityType.Enabled = checkedActivityTypes.Contains(activityType.Id);
                //_customerActivityService.UpdateActivityType(activityType);
            }

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.ActivityLog.ActivityLogType.Updated"));
            return RedirectToAction("ListTypes");
        }

        public void InsertOrUpdateResource(string systemKeyword, string textValue)
        {
            if (!string.IsNullOrEmpty(systemKeyword) && !string.IsNullOrEmpty(textValue))
            {
                var languageId = _workContext.WorkingLanguage.Id;
                var res = _localizationService.GetLocaleStringResourceByName($"ActivityLog.{systemKeyword}", languageId, false);
                if (res == null)
                {
                    var resource = new LocaleStringResource { LanguageId = languageId };
                    resource.ResourceName = $"ActivityLog.{systemKeyword}";
                    resource.ResourceValue = textValue;
                    _localizationService.InsertLocaleStringResource(resource);
                }
                else
                {
                    res.ResourceValue = textValue;
                    _localizationService.UpdateLocaleStringResource(res);
                }
            }
        }

        public virtual IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            return View(new ActivityLogTypeModel());
        }

        [HttpPost]
        public virtual IActionResult Create(ActivityLogTypeModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            if (!string.IsNullOrEmpty(model.SystemKeyword) && !string.IsNullOrEmpty(model.Name))
            {
                model.SystemKeyword = model.SystemKeyword.Trim();
                model.Name = model.Name.Trim();
                var activityLogType = new ActivityLogType
                {
                    Name = model.Name,
                    SystemKeyword = model.SystemKeyword,
                    Enabled = model.Enabled,
                };
                _customerActivityService.InsertActivityType(activityLogType);

                InsertOrUpdateResource(model.SystemKeyword, model.TextValue);
                return RedirectToAction("Edit", new { id = activityLogType.Id });
            }
            return View(model);
        }

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            var activityLogType = _customerActivityService.GetActivityTypeById(id);
            if (activityLogType == null)
                return RedirectToAction("ListTypes");

            var model = new ActivityLogTypeModel
            {
                Id = activityLogType.Id,
                Name = activityLogType.Name,
                SystemKeyword = activityLogType.SystemKeyword,
                Enabled = activityLogType.Enabled,
            };
            var res = _localizationService.GetLocaleStringResourceByName($"ActivityLog.{activityLogType.SystemKeyword}", _workContext.WorkingLanguage.Id, false);
            if (res != null)
                model.TextValue = res.ResourceValue;
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Edit(ActivityLogTypeModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            var activityLogType = _customerActivityService.GetActivityTypeById(model.Id);
            if (activityLogType == null)
                return RedirectToAction("ListTypes");

            if (!string.IsNullOrEmpty(model.SystemKeyword) && !string.IsNullOrEmpty(model.Name))
            {
                activityLogType.Name = model.Name.Trim();
                activityLogType.SystemKeyword = model.SystemKeyword.Trim();
                activityLogType.Enabled = model.Enabled;
                _customerActivityService.UpdateActivityType(activityLogType);

                InsertOrUpdateResource(model.SystemKeyword, model.TextValue);
                return RedirectToAction("Edit", new { id = activityLogType.Id });
            }
            return View(model);
        }

        #endregion

        #region Activity log

        public virtual IActionResult ListLogs()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            var activityLogSearchModel = new ActivityLogSearchModel();
            activityLogSearchModel.ActivityLogType.Add(new SelectListItem
            {
                Value = "0",
                Text = "All"
            });

            foreach (var at in _customerActivityService.GetAllActivityTypes())
            {
                activityLogSearchModel.ActivityLogType.Add(new SelectListItem
                {
                    Value = at.Id.ToString(),
                    Text = at.Name
                });
            }
            return View(activityLogSearchModel);
        }

        [HttpPost]
        public virtual IActionResult ListLogs(DataSourceRequest command, ActivityLogSearchModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedKendoGridJson();

            var startDateValue = (model.CreatedOnFrom == null) ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

            var endDateValue = (model.CreatedOnTo == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var activityLog = _customerActivityService.GetAllActivities(startDateValue, endDateValue, null, model.ActivityLogTypeId, command.Page - 1, command.PageSize, model.IpAddress);
            var gridModel = new DataSourceResult
            {
                Data = activityLog.Select(x =>
                {
                    var m = x.ToModel();
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    return m;

                }),
                Total = activityLog.TotalCount
            };
            return Json(gridModel);
        }

        public virtual IActionResult AcivityLogDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            var activityLog = _customerActivityService.GetActivityById(id);
            if (activityLog == null)
            {
                throw new ArgumentException("No activity log found with the specified id");
            }
            //_customerActivityService.DeleteActivity(activityLog);

            //activity log
            _customerActivityService.InsertActivity("DeleteActivityLog", _localizationService.GetResource("ActivityLog.DeleteActivityLog"));

            return new NullJsonResult();
        }

        public virtual IActionResult ClearAll()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            //_customerActivityService.ClearAllActivities();

            //activity log
            _customerActivityService.InsertActivity("DeleteActivityLog", _localizationService.GetResource("ActivityLog.DeleteActivityLog"));

            return RedirectToAction("ListLogs");
        }

        #endregion
    }
}