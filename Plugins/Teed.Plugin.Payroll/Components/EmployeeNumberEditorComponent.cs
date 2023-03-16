using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;
using System.Linq;
using Nop.Services.Security;
using Teed.Plugin.Payroll.Services;
using Teed.Plugin.Payroll.Security;

namespace Teed.Plugin.Payroll.Components
{
    [ViewComponent(Name = "EmployeeNumberEditor")]
    public class EmployeeNumberEditorComponent : NopViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IPermissionService _permissionService;
        private readonly PayrollEmployeeService _payrollEmployeeService;

        public EmployeeNumberEditorComponent(ICustomerService customerService,
            IWorkContext workContext, IPermissionService permissionService,
            PayrollEmployeeService payrollEmployeeService)
        {
            _customerService = customerService;
            _workContext = workContext;
            _permissionService = permissionService;
            _payrollEmployeeService = payrollEmployeeService;
        }

        public IViewComponentResult Invoke(string widgetZone, int additionalData)
        {
            var customerId = (int)additionalData;
            int? employeeNumber = null;

            if (customerId > 0)
            {
                var existingPayroll = _payrollEmployeeService.GetAll(onlyEmployees: false)
                    .Where(x => x.CustomerId == customerId)
                    .OrderByDescending(x => x.CreatedOnUtc)
                    .FirstOrDefault();

                if (existingPayroll != null)
                    employeeNumber = existingPayroll.EmployeeNumber;
            }

            return View("~/Plugins/Teed.Plugin.Payroll/Views/Shared/Components/EmployeeNumberEditor/Default.cshtml",
                new EmployeeNumberEditorModel
                {
                    CustomerId = customerId,
                    EmployeeNumber = employeeNumber,
                    ShowEditor = customerId > 0 ? _permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee) : false
                });
        }
    }

    public class EmployeeNumberEditorModel
    {
        public int CustomerId { get; set; }
        public int? EmployeeNumber { get; set; }
        public bool ShowEditor { get; set; }
    }
}