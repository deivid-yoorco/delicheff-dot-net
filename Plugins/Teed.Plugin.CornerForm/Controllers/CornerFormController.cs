using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
using Teed.Plugin.CornerForm.Domain;
using Teed.Plugin.CornerForm.Models;
using Teed.Plugin.CornerForm.Security;
using Teed.Plugin.CornerForm.Services;

namespace Teed.Plugin.CornerForm.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class CornerFormController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ICustomerService _customerService;
        private readonly IPermissionService _permissionService;
        private readonly CornerFormResultService _cornerFormResultService;

        public CornerFormController(IWorkContext workContext, 
            IStoreService storeService, 
            ISettingService settingService, 
            CornerFormResultService cornerFormResultService,
            ICustomerService customerService,
            IPermissionService permissionService)
        {
            _workContext = workContext;
            _storeService = storeService;
            _settingService = settingService;
            _cornerFormResultService = cornerFormResultService;
            _customerService = customerService;
            _permissionService = permissionService;
        }

        public IActionResult Configure()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var pluginSettings = _settingService.LoadSetting<CornerFormSettings>(storeScope);
            var model = new ConfigurationModel()
            {
                Question = pluginSettings.Question,
                MinimizedText = pluginSettings.MinimizedText,
                ButtonColor = pluginSettings.ButtonColor,
                ButtonTextColor = pluginSettings.ButtonTextColor
            };
            return View("~/Plugins/Teed.Plugin.CornerForm/Views/CornerForm/Configure.cshtml", model);
        }

        [HttpPost]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(CornerFormPermissionProvider.CornerForm)) 
                return AccessDeniedView();

            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var pluginSettings = _settingService.LoadSetting<CornerFormSettings>(storeScope);

            pluginSettings.Question = model.Question;
            pluginSettings.MinimizedText = model.MinimizedText;
            pluginSettings.ButtonColor = model.ButtonColor.Contains("#") ? model.ButtonColor : "#" + model.ButtonColor;
            pluginSettings.ButtonTextColor = model.ButtonTextColor.Contains("#") ? model.ButtonTextColor : "#" + model.ButtonTextColor;

            _settingService.SaveSetting(pluginSettings);

            return View("~/Plugins/Teed.Plugin.CornerForm/Views/CornerForm/Configure.cshtml", model);
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(CornerFormPermissionProvider.CornerForm))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.CornerForm/Views/CornerForm/List.cshtml");
        }

        public IActionResult ExportAnswers()
        {
            if (!_permissionService.Authorize(CornerFormPermissionProvider.CornerForm))
                return AccessDeniedView();

            var cornerFormResults = _cornerFormResultService.GetAll().ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("Formulario");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Fecha";
                    worksheet.Cells[row, 2].Value = "Pregunta";
                    worksheet.Cells[row, 3].Value = "Respuesta";
                    worksheet.Cells[row, 4].Value = "Solicitado por";
                    foreach (var item in cornerFormResults)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = item.CreatedOnUtc.ToLocalTime();
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 2].Value = item.QuestionText;
                        worksheet.Cells[row, 3].Value = item.AnswerText;

                        string email = item.CustomerId == 0 ? string.Empty : _customerService.GetCustomerById(item.CustomerId)?.Email;
                        worksheet.Cells[row, 4].Value = email;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"questions_{DateTime.Now.ToString("ddMMyyyyhhmmss")}.xlsx");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("[controller]/[action]")]
        public IActionResult GetSettings()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var pluginSettings = _settingService.LoadSetting<CornerFormSettings>(storeScope);
            return Ok(pluginSettings);
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(CornerFormPermissionProvider.CornerForm))
                return AccessDeniedView();

            var query = _cornerFormResultService.GetAll();
            var queryList = query.OrderByDescending(m => m.CreatedOnUtc).ToList();
            var pagedList = new PagedList<CornerFormResult>(queryList, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    CreateDate = x.CreatedOnUtc.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt"),
                    x.QuestionText,
                    x.AnswerText
                }).ToList(),
                Total = query.Count()
            };

            return Json(gridModel);
        }
    }
}
