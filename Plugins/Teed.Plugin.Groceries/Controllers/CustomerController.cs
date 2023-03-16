using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class CustomerController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IDbContext _dbContext;

        public CustomerController(IPermissionService permissionService,
            IWorkContext workContext,
            IOrderService orderService,
            ICustomerService customerService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IDbContext dbContext)
        {
            _permissionService = permissionService;
            _workContext = workContext;
            _orderService = orderService;
            _customerService = customerService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _dbContext = dbContext;
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult UpdateLimitedRoles(LimitedRolesModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return Unauthorized();

            var customer = _customerService.GetCustomerById(model.CustomerId);
            if (customer == null) return NotFound();

            var parsedSelectedRoleIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(model.SelectedRoleIds) ?? new List<int>();
            var parsedAllowedRoleIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(model.AllowedRoleIds) ?? new List<int>();

            var currentRoleIds = customer.CustomerRoles
                .Select(x => x.Id)
                .Where(x => parsedAllowedRoleIds.Contains(x))
                .ToList();
            var rolesToRemove = currentRoleIds.Where(x => !parsedSelectedRoleIds.Contains(x));
            var rolesToAdd = parsedSelectedRoleIds.Where(x => !currentRoleIds.Contains(x));
            var allRoles = _customerService.GetAllCustomerRoles();

            foreach (var roleId in rolesToRemove)
            {
                var role = allRoles.Where(x => x.Id == roleId).FirstOrDefault();
                customer.CustomerRoles.Remove(role);
            }

            foreach (var roleId in rolesToAdd)
            {
                var role = allRoles.Where(x => x.Id == roleId).FirstOrDefault();
                customer.CustomerRoles.Add(role);
            }

            _customerService.UpdateCustomer(customer);
            _customerActivityService.InsertActivity("EditCustomer", _localizationService.GetResource("ActivityLog.EditCustomer"), customer.Id);

            return NoContent();
        }

        [HttpGet]
        [AuthorizeAdmin]
        public IActionResult CustomerBirthdaysExport(int monthType = 1)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return Unauthorized();

            string customersDatesOfBirthInfo = System.IO.File.ReadAllText("Plugins/Teed.Plugin.Groceries/src/files/sql/generic-attributes-customer-dateOfBirth.sql");
            List<CustomerGenericResult> customersDatesOfBirths = _dbContext.SqlQuery<CustomerGenericResult>(customersDatesOfBirthInfo).ToList();

            var currentBirthdayCustomers = customersDatesOfBirths.Select(x => new
            {
                CustomerId = x.EntityId,
                DateOfBirth = !string.IsNullOrEmpty(x.Value) ? DateTime.ParseExact(x.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture).Date : (DateTime?)null,
            }).Where(x => x.DateOfBirth != null).ToList();

            var today = DateTime.Now.Date;
            var from = new DateTime(today.Year, today.Month, 1);
            if (monthType < 1)
                from = from.AddMonths(-1);
            else if (monthType > 1)
                from = from.AddMonths(1);
            var customerIds = currentBirthdayCustomers
                .Where(x => x.DateOfBirth.Value.Month == from.Month)
                .Select(x => x.CustomerId).ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => !x.Deleted && customerIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Email,
                    Address = x.ShippingAddress != null ? x.ShippingAddress : 
                    x.BillingAddress != null ? x.BillingAddress : 
                    x.Addresses.Any() ? x.Addresses.FirstOrDefault() : 
                    null })
                .OrderBy(x => x.Id)
                .ToList();

            if (customers.Any())
            {
                customerIds = customers.Select(x => x.Id).ToList();
                var customerEmails = customers.Select(x => x.Email).ToList();
                var customerPhonesInfo = System.IO.File.ReadAllText("Plugins/Teed.Plugin.Groceries/src/files/sql/generic-attributes-customer-phone.sql");
                List<CustomerGenericResult> customerPhones = _dbContext.SqlQuery<CustomerGenericResult>(customerPhonesInfo).ToList();
                var currentPhonesCustomers = customerPhones.Select(x => new
                {
                    CustomerId = x.EntityId,
                    Phone = x.Value,
                }).Where(x => customerIds.Contains(x.CustomerId) && !string.IsNullOrEmpty(x.Phone)).ToList();

                var customerFirstNamesInfo = System.IO.File.ReadAllText("Plugins/Teed.Plugin.Groceries/src/files/sql/generic-attributes-customer-firstName.sql");
                List<CustomerGenericResult> customerFirstNames = _dbContext.SqlQuery<CustomerGenericResult>(customerFirstNamesInfo).ToList();
                var currentFirstNamesCustomers = customerFirstNames.Select(x => new
                {
                    CustomerId = x.EntityId,
                    FirstName = x.Value,
                }).Where(x => customerIds.Contains(x.CustomerId)).ToList();

                var customerLastNamesInfo = System.IO.File.ReadAllText("Plugins/Teed.Plugin.Groceries/src/files/sql/generic-attributes-customer-lastName.sql");
                List<CustomerGenericResult> customerLastNames = _dbContext.SqlQuery<CustomerGenericResult>(customerLastNamesInfo).ToList();
                var currentLastNamesCustomers = customerLastNames.Select(x => new
                {
                    CustomerId = x.EntityId,
                    LastName = x.Value,
                }).Where(x => customerIds.Contains(x.CustomerId)).ToList();

                string newsLetterQuery = System.IO.File.ReadAllText("Plugins/Teed.Plugin.Groceries/src/files/sql/newsLetter.sql");
                var newsLetterQR = _dbContext.SqlQuery<NewsletterResults>(newsLetterQuery)
                    .Where(x => customerEmails.Contains(x.Email)).ToList();

                var customersInfo = customers.Select(x => new {
                    x.Id,
                    x.Email,
                    DateOfBirth  = currentBirthdayCustomers.Where(y => y.CustomerId == x.Id).FirstOrDefault().DateOfBirth.Value,
                    Phone = currentPhonesCustomers.Where(y => y.CustomerId == x.Id).FirstOrDefault() != null ? currentPhonesCustomers.Where(y => y.CustomerId == x.Id).FirstOrDefault().Phone :
                        x.Address != null ? x.Address.PhoneNumber : string.Empty,
                    FirstName = currentFirstNamesCustomers.Where(y => y.CustomerId == x.Id).FirstOrDefault()?.FirstName,
                    LastName = currentLastNamesCustomers.Where(y => y.CustomerId == x.Id).FirstOrDefault()?.LastName,
                    IsNewsActive = newsLetterQR.Where(y => y.Email == x.Email).FirstOrDefault()?.Active ?? false
                }).ToList();

                using (var stream = new MemoryStream())
                {
                    using (var xlPackage = new ExcelPackage(stream))
                    {
                        var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                        int row = 1;

                        worksheet.Cells[row, 1].Value = "Correo";
                        worksheet.Cells[row, 2].Value = "Nombre";
                        worksheet.Cells[row, 3].Value = "Teléfono";
                        worksheet.Cells[row, 4].Value = "Fecha de nacimiento";
                        worksheet.Cells[row, 5].Value = "Registrado al news";
                        row++;

                        foreach (var customerInfo in customersInfo)
                        {
                            worksheet.Cells[row, 1].Value = customerInfo.Email;
                            worksheet.Cells[row, 2].Value =
                                string.Join(" ", new List<string> { customerInfo.FirstName, customerInfo.LastName }.Where(x => !string.IsNullOrEmpty(x)).ToList());
                            worksheet.Cells[row, 3].Value = customerInfo.Phone;
                            worksheet.Cells[row, 4].Value = customerInfo.DateOfBirth;
                            worksheet.Cells[row, 4].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[row, 5].Value = customerInfo.IsNewsActive ? "Si" : "No";
                            row++;
                        }

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }

                        xlPackage.Save();
                    }

                    return File(stream.ToArray(), MimeTypes.TextXlsx, $"cel_clientes_cumpleaños_mes_" + from.ToString("MMM", new CultureInfo("es-MX")).Replace(".", "") + ".xlsx");
                }
            }
            return Ok();
        }
    }

    public class LimitedRolesModel
    {
        public string SelectedRoleIds { get; set; }
        public string AllowedRoleIds { get; set; }
        public int CustomerId { get; set; }
    }

    public class CustomerGenericResult
    {
        public int EntityId { get; set; }
        public string Value { get; set; }
    }
}