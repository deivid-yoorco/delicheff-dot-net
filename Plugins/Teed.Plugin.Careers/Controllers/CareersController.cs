using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System.Linq;
using Teed.Plugin.Careers.Domain;
using Teed.Plugin.Careers.Models;
using Teed.Plugin.Careers.Security;
using Teed.Plugin.Careers.Services;

namespace Teed.Plugin.Careers.Controllers
{
    [Area(AreaNames.Admin)]
    public class CareersController : BasePaymentController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly CareerPostulationService _careersService;
        private readonly CareersSettings _careersSettings;


        public CareersController(IPermissionService permissionService,
            CareerPostulationService careersService,
            CareersSettings careersSettings,
            IWorkContext workContext,
            ISettingService settingService)
        {
            _careersService = careersService;
            _careersSettings = careersSettings;
            _workContext = workContext;
            _settingService = settingService;
            _permissionService = permissionService;
        }

        #endregion

        [AuthorizeAdmin]
        public IActionResult Configure()
        {
            //whether user has the authority
            if (!_permissionService.Authorize(CareersPermissionProvider.Careers))
                return AccessDeniedView();

            //prepare model
            var model = new ConfigurationModel
            {
                Title = _careersSettings.Title,
                Body = _careersSettings.Body,
                ShowInFooter1 = _careersSettings.ShowInFooter1,
                ShowInFooter2 = _careersSettings.ShowInFooter2,
                ShowInFooter3 = _careersSettings.ShowInFooter3,
                ShowInMobileMenu = _careersSettings.ShowInMobileMenu,
                ShowInHeader = _careersSettings.ShowInHeader,
                Published = _careersSettings.Published
            };
            return View("~/Plugins/Careers/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            //whether user has the authority
            if (!_permissionService.Authorize(CareersPermissionProvider.Careers))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _careersSettings.Title = model.Title;
            _careersSettings.Body = model.Body;
            _careersSettings.ShowInFooter1 = model.ShowInFooter1;
            _careersSettings.ShowInFooter2 = model.ShowInFooter2;
            _careersSettings.ShowInFooter3 = model.ShowInFooter3;
            _careersSettings.ShowInMobileMenu = model.ShowInMobileMenu;
            _careersSettings.Published = model.Published;
            _careersSettings.ShowInHeader = model.ShowInHeader;
            _settingService.SaveSetting(_careersSettings);

            return Configure();
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(CareersPermissionProvider.Careers))
                return AccessDeniedView();

            return View("~/Plugins/Careers/Views/List.cshtml");
        }


        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(CareersPermissionProvider.Careers))
                return AccessDeniedView();

            var query = _careersService.GetAll();
            var pagedList = new PagedList<CareerPostulations>(query.OrderByDescending(m => m.CreatedOnUtc), command.Page - 1, command.PageSize);

            DataSourceResult gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    x.FullName,
                    x.Email,
                    x.PhoneNumber,
                    x.CreatedOnUtc,
                    x.CVFile,
                    x.Subject
                }),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        [HttpGet]
        public IActionResult DownloadCV(int id)
        {
            CareerPostulations career = _careersService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (career == null) return NotFound();

            return File(career.CVFile, "application/pdf");
        }
    }
}