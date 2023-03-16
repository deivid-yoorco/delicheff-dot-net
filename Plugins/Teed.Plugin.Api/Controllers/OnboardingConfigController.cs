using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Domain.Onboardings;
using Teed.Plugin.Api.Dtos.Address;
using Teed.Plugin.Api.Dtos.Customer;
using Teed.Plugin.Api.Models;
using Teed.Plugin.Api.Services;
using Teed.Plugin.Api.Security;
using Teed.Plugin.Api.Dtos.Onboarding;

namespace Teed.Plugin.Api.Controllers
{
    [Area(AreaNames.Admin)]
    public class OnboardingConfigController : BasePluginController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IPermissionService _permissionService;
        private readonly OnboardingService _onboardingService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public OnboardingConfigController(ICustomerService customerService,
            IPermissionService permissionService,
            OnboardingService onboardingService,
            IWorkContext workContext)
        {
            _customerService = customerService;
            _permissionService = permissionService;
            _onboardingService = onboardingService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.OnboardingConfig))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Api/Views/OnboardingConfig/List.cshtml");
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.OnboardingConfig))
                return AccessDeniedView();

            var queryList = _onboardingService.GetAll().ToList();
            var pagedList = new PagedList<Onboarding>(queryList, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Title,
                    x.Subtitle,
                    x.Active,
                    x.BackgroundColor
                }).ToList(),
                Total = queryList.Count()
            };

            return Json(gridModel);
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.OnboardingConfig))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Api/Views/OnboardingConfig/Create.cshtml", new OnboardingModel());
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(OnboardingModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.OnboardingConfig))
                return AccessDeniedView();

            if (string.IsNullOrEmpty(model.Name))
                return View("~/Plugins/Teed.Plugin.Api/Views/OnboardingConfig/Create.cshtml", model);

            var onboarding = new Onboarding
            {
                Id = model.Id,
                Name = model.Name,
                Title = model.Title,
                Subtitle = model.Subtitle,
                BackgroundColor = !model.BackgroundColor.Contains("#") ? "#" + model.BackgroundColor : model.BackgroundColor,
                ImageId = model.ImageId > 0 ? model.ImageId : null,
                Active = model.Active,
                Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) creó el nuevo Onboarding con nombre {model.Name}\n"
            };

            _onboardingService.Insert(onboarding);

            if (continueEditing)
            {
                return RedirectToAction("Edit", new { id = onboarding.Id });
            }
            return RedirectToAction("List");
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.OnboardingConfig))
                return AccessDeniedView();

            var onboarding = _onboardingService.GetById(id);
            if (onboarding == null)
                return RedirectToAction("List");

            var model = new OnboardingModel
            {
                Id = onboarding.Id,
                Name = onboarding.Name,
                Title = onboarding.Title,
                Subtitle = onboarding.Subtitle,
                BackgroundColor = onboarding.BackgroundColor,
                ImageId = onboarding.ImageId,
                Active = onboarding.Active,
                Log = onboarding.Log
            };

            return View("~/Plugins/Teed.Plugin.Api/Views/OnboardingConfig/Edit.cshtml", model);
        }


        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(OnboardingModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.OnboardingConfig))
                return AccessDeniedView();

            if (string.IsNullOrEmpty(model.Name))
                return View("~/Plugins/Teed.Plugin.Api/Views/OnboardingConfig/Edit.cshtml", model);

            var onboarding = _onboardingService.GetById(model.Id);
            if (onboarding == null)
                return RedirectToAction("List");

            if (onboarding.Name != model.Name)
            {
                onboarding.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el nombre de {onboarding.Name} a {model.Name}\n";
                onboarding.Name = model.Name;
            }
            if (onboarding.Title != model.Title)
            {
                onboarding.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el título de {onboarding.Title} a {model.Title}\n";
                onboarding.Title = model.Title;
            }
            if (onboarding.Subtitle != model.Subtitle)
            {
                onboarding.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el subtítulo de {onboarding.Subtitle} a {model.Subtitle}\n";
                onboarding.Subtitle = model.Subtitle;
            }
            model.BackgroundColor = !model.BackgroundColor.Contains("#") ? "#" + model.BackgroundColor : model.BackgroundColor;
            if (onboarding.BackgroundColor != model.BackgroundColor)
            {
                onboarding.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el color de fondo de {onboarding.BackgroundColor} a {model.BackgroundColor}\n";
                onboarding.BackgroundColor = model.BackgroundColor;
            }
            if (onboarding.ImageId != model.ImageId)
            {
                onboarding.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió la imagen de {onboarding.ImageId} a {model.ImageId}\n";
                onboarding.ImageId = model.ImageId;
            }
            if (onboarding.Active != model.Active)
            {
                onboarding.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el valor de \"Activo\" de {onboarding.Active} a {model.Active}\n";
                onboarding.Active = model.Active;
            }

            _onboardingService.Update(onboarding);

            if (continueEditing)
            {
                return RedirectToAction("Edit", new { id = onboarding.Id });
            }
            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.OnboardingConfig))
                return AccessDeniedView();

            var onboarding = _onboardingService.GetById(id);
            if (onboarding == null)
                return RedirectToAction("List");

            onboarding.Deleted = true;
            _onboardingService.Update(onboarding);

            return RedirectToAction("List");
        }

        #endregion
    }
}
