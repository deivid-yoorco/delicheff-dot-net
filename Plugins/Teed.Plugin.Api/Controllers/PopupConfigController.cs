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
using Teed.Plugin.Api.Domain.Popups;
using Teed.Plugin.Api.Dtos.Address;
using Teed.Plugin.Api.Dtos.Customer;
using Teed.Plugin.Api.Models;
using Teed.Plugin.Api.Services;
using Teed.Plugin.Api.Security;

namespace Teed.Plugin.Api.Controllers
{
    [Area(AreaNames.Admin)]
    public class PopupConfigController : BasePluginController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IPermissionService _permissionService;
        private readonly PopupService _popupService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public PopupConfigController(ICustomerService customerService,
            IPermissionService permissionService,
            PopupService popupService,
            IWorkContext workContext)
        {
            _customerService = customerService;
            _permissionService = permissionService;
            _popupService = popupService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.PopupConfig))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Api/Views/PopupConfig/List.cshtml");
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.PopupConfig))
                return AccessDeniedView();

            var queryList = _popupService.GetAll().ToList();
            var pagedList = new PagedList<Popup>(queryList, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Mondays,
                    x.Tuesdays,
                    x.Wednesdays,
                    x.Thursdays,
                    x.Fridays,
                    x.Saturdays,
                    x.Sundays,
                    x.FirstTimeOnly,
                    x.Active,
                    ViewableDeadlineDate = CreateDateString(x.ViewableDeadlineDate)
                }).ToList(),
                Total = queryList.Count()
            };

            return Json(gridModel);
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.PopupConfig))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Api/Views/PopupConfig/Create.cshtml", new PopupModel());
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(PopupModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.PopupConfig))
                return AccessDeniedView();

            if (string.IsNullOrEmpty(model.Name))
                return View("~/Plugins/Teed.Plugin.Api/Views/PopupConfig/Create.cshtml", model);

            var parseDate = DateTime.MinValue;
            if (!string.IsNullOrEmpty(model.ViewableDeadlineDateString))
                parseDate = DateTime.ParseExact(model.ViewableDeadlineDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;

            var popup = new Popup
            {
                Id = model.Id,
                Name = model.Name,
                Mondays = model.Mondays,
                Tuesdays = model.Tuesdays,
                Wednesdays = model.Wednesdays,
                Thursdays = model.Thursdays,
                Fridays = model.Fridays,
                Saturdays = model.Saturdays,
                Sundays = model.Sundays,
                FirstTimeOnly = model.FirstTimeOnly,
                ImageId = model.ImageId > 0 ? model.ImageId : null,
                ImageForDesktopId = model.ImageForDesktopId > 0 ? model.ImageForDesktopId : null,
                Active = model.Active,
                Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) creó el nuevo Popup con nombre {model.Name}\n"
            };
            if (!string.IsNullOrEmpty(model.ViewableDeadlineDateString))
                popup.ViewableDeadlineDate = parseDate;
            else
                popup.ViewableDeadlineDate = null;

            _popupService.Insert(popup);

            if (continueEditing)
            {
                return RedirectToAction("Edit", new { id = popup.Id });
            }
            return RedirectToAction("List");
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.PopupConfig))
                return AccessDeniedView();

            var popup = _popupService.GetById(id);
            if (popup == null)
                return RedirectToAction("List");

            var model = new PopupModel
            {
                Id = popup.Id,
                Name = popup.Name,
                Mondays = popup.Mondays,
                Tuesdays = popup.Tuesdays,
                Wednesdays = popup.Wednesdays,
                Thursdays = popup.Thursdays,
                Fridays = popup.Fridays,
                Saturdays = popup.Saturdays,
                Sundays = popup.Sundays,
                FirstTimeOnly = popup.FirstTimeOnly,
                ImageId = popup.ImageId,
                ImageForDesktopId = popup.ImageForDesktopId,
                Active = popup.Active,
                ViewableDeadlineDateString =
                    popup.ViewableDeadlineDate != null ? (popup.ViewableDeadlineDate ?? DateTime.Now).ToString("dd-MM-yyyy") : null,
                Log = popup.Log
            };

            return View("~/Plugins/Teed.Plugin.Api/Views/PopupConfig/Edit.cshtml", model);
        }


        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(PopupModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.PopupConfig))
                return AccessDeniedView();

            if (string.IsNullOrEmpty(model.Name))
                return View("~/Plugins/Teed.Plugin.Api/Views/PopupConfig/Edit.cshtml", model);

            var popup = _popupService.GetById(model.Id);
            if (popup == null)
                return RedirectToAction("List");

            if (popup.Name != model.Name)
            {
                popup.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el nombre de {popup.Name} a {model.Name}\n";
                popup.Name = model.Name;
            }
            if (popup.Mondays != model.Mondays)
            {
                popup.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el valor de \"Todos los lunes\" de {popup.Mondays} a {model.Mondays}\n";
                popup.Mondays = model.Mondays;
            }
            if (popup.Tuesdays != model.Tuesdays)
            {
                popup.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el valor de \"Todos los martes\" de {popup.Tuesdays} a {model.Tuesdays}\n";
                popup.Tuesdays = model.Tuesdays;
            }
            if (popup.Wednesdays != model.Wednesdays)
            {
                popup.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el valor de \"Todos los miércoles\" de {popup.Wednesdays} a {model.Wednesdays}\n";
                popup.Wednesdays = model.Wednesdays;
            }
            if (popup.Thursdays != model.Thursdays)
            {
                popup.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el valor de \"Todos los jueves\" de {popup.Thursdays} a {model.Thursdays}\n";
                popup.Thursdays = model.Thursdays;
            }
            if (popup.Fridays != model.Fridays)
            {
                popup.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el valor de \"Todos los viernes\" de {popup.Fridays} a {model.Fridays}\n";
                popup.Fridays = model.Fridays;
            }
            if (popup.Saturdays != model.Saturdays)
            {
                popup.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el valor de \"Todos los sábados\" de {popup.Saturdays} a {model.Saturdays}\n";
                popup.Saturdays = model.Saturdays;
            }
            if (popup.Sundays != model.Sundays)
            {
                popup.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el valor de \"Todos los domingos\" de {popup.Sundays} a {model.Sundays}\n";
                popup.Sundays = model.Sundays;
            }
            if (popup.FirstTimeOnly != model.FirstTimeOnly)
            {
                popup.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el valor de \"Todos los Lunes\" de {popup.FirstTimeOnly} a {model.FirstTimeOnly}\n";
                popup.FirstTimeOnly = model.FirstTimeOnly;
            }
            if (popup.ImageId != model.ImageId)
            {
                popup.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió la imagen de {popup.ImageId} a {model.ImageId}\n";
                popup.ImageId = model.ImageId;
            }
            if (popup.ImageForDesktopId != model.ImageForDesktopId)
            {
                popup.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió la imagen de escritorio de {popup.ImageForDesktopId} a {model.ImageForDesktopId}\n";
                popup.ImageForDesktopId = model.ImageForDesktopId;
            }
            if (popup.Active != model.Active)
            {
                popup.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el valor de \"Activo\" de {popup.Active} a {model.Active}\n";
                popup.Active = model.Active;
            }
            var parseDate = DateTime.MinValue;
            if (!string.IsNullOrEmpty(model.ViewableDeadlineDateString))
                parseDate = DateTime.ParseExact(model.ViewableDeadlineDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;
            if (popup.ViewableDeadlineDate != parseDate)
            {
                var popupDate = CreateDateString(popup.ViewableDeadlineDate);
                var modelDate = model.ViewableDeadlineDateString;
                popup.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió la imagen de {popupDate} a {modelDate}\n";
                popup.ViewableDeadlineDate = parseDate;
            }

            _popupService.Update(popup);

            if (continueEditing)
            {
                return RedirectToAction("Edit", new { id = popup.Id });
            }
            return RedirectToAction("List");
        }

        public string CreateDateString(DateTime? date)
        {
            var final = "Sin fecha";
            if (date != null)
                final = (date ?? DateTime.Now).ToString("dd-MM-yyyy");
            return final;
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.PopupConfig))
                return AccessDeniedView();

            var popup = _popupService.GetById(id);
            if (popup == null)
                return RedirectToAction("List");

            popup.Deleted = true;
            _popupService.Update(popup);

            return RedirectToAction("List");
        }

        #endregion
    }
}
