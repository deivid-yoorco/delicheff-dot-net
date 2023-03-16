using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Security;
using Teed.Plugin.Payroll.Services;
using Teed.Plugin.Payroll.Models.MinimumWagesCatalog;
using Teed.Plugin.Payroll.Domain.MinimumWagesCatalogs;
using Nop.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Teed.Plugin.Payroll.Controllers
{
    [Area(AreaNames.Admin)]
    public class MinimumWagesCatalogController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly PayrollEmployeeService _payrollEmployeeService;
        private readonly PayrollEmployeeFileService _payrollEmployeeFileService;
        private readonly PayrollSalaryService _payrollSalaryService;
        private readonly JobCatalogService _jobCatalogService;
        private readonly SubordinateService _subordinateService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly BiweeklyPaymentService _biweeklyPaymentService;
        private readonly MinimumWagesCatalogService _minimumWagesCatalogService;

        public MinimumWagesCatalogController(IPermissionService permissionService, PayrollEmployeeService payrollEmployeeService,
            PayrollEmployeeFileService payrollEmployeeFileService, PayrollSalaryService payrollSalaryService,
            JobCatalogService jobCatalogService, SubordinateService subordinateService, ICustomerService customerService,
            IWorkContext workContext, BiweeklyPaymentService biweeklyPaymentService, MinimumWagesCatalogService minimumWagesCatalogService)
        {
            _permissionService = permissionService;
            _payrollEmployeeService = payrollEmployeeService;
            _payrollEmployeeFileService = payrollEmployeeFileService;
            _payrollSalaryService = payrollSalaryService;
            _jobCatalogService = jobCatalogService;
            _subordinateService = subordinateService;
            _customerService = customerService;
            _workContext = workContext;
            _biweeklyPaymentService = biweeklyPaymentService;
            _minimumWagesCatalogService = minimumWagesCatalogService;
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Payroll/Views/MinimumWagesCatalog/List.cshtml");
        }

        public IActionResult ListData()
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();

            var wages = _minimumWagesCatalogService.GetAll().ToList();

            var gridModel = new DataSourceResult
            {
                Data = wages.Select(x => new
                {
                    x.Id,
                    x.Uma,
                    Year = x.Year.ToString("yyyy"),
                    x.Amount,
                    UpdatedDate = x.UpdatedOnUtc.ToLocalTime().ToString("dd/MM/yyyy")
                }).ToList(),
                Total = wages.Count()
            };

            return Json(gridModel);
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            var model = new MinimumWagesCatalogModel();

            return View("~/Plugins/Teed.Plugin.Payroll/Views/MinimumWagesCatalog/Create.cshtml", model);
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            var wage = _minimumWagesCatalogService.GetById(id);
            if (wage == null)
                return RedirectToAction("List");

            var model = new MinimumWagesCatalogModel
            {
                Id = wage.Id,
                Amount = wage.Amount,
                Uma = wage.Uma,
                Year = wage.Year.Year
            };

            return View("~/Plugins/Teed.Plugin.Payroll/Views/MinimumWagesCatalog/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(MinimumWagesCatalogModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            var wage = new MinimumWagesCatalog
            {
                Id = model.Id,
                Amount = model.Amount,
                Uma = model.Uma,
                Year = new DateTime(model.Year, 1, 1)
            };

            _minimumWagesCatalogService.Insert(wage);

            if (continueEditing)
            {
                return RedirectToAction("Edit", new { id = wage.Id });
            }
            return RedirectToAction("List");
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(MinimumWagesCatalogModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            var wage = _minimumWagesCatalogService.GetById(model.Id);
            if (wage == null)
                return RedirectToAction("List");

            wage.Amount = model.Amount;
            wage.Uma = model.Uma;
            wage.Year = new DateTime(model.Year, 1, 1);

            _minimumWagesCatalogService.Update(wage);

            if (continueEditing)
            {
                return RedirectToAction("Edit", new { id = wage.Id });
            }
            return RedirectToAction("List");
        }
    }
}