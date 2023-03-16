using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.Manufacturer;
using Teed.Plugin.Groceries.Models.ManufacturerBankAccount;
using Teed.Plugin.Groceries.Services;

namespace Teed.Plugin.Groceries.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class ManufacturerBankAccountController : BasePluginController
    {
        private readonly ManufacturerBankAccountService _manufacturerBankAccountService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;

        public ManufacturerBankAccountController(ManufacturerBankAccountService manufacturerBankAccountService,
            IPermissionService permissionService,
            IWorkContext workContext)
        {
            _manufacturerBankAccountService = manufacturerBankAccountService;
            _permissionService = permissionService;
            _workContext = workContext;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(ManufacturerBankAccountModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return Unauthorized();

            var manufacturerBankAccount = _manufacturerBankAccountService.GetAll()
                .Where(x => x.ManufacturerId == model.ManufacturerId)
                .FirstOrDefault();

            if (manufacturerBankAccount == null)
            {
                _manufacturerBankAccountService.Insert(new ManufacturerBankAccount()
                {
                    AccountNumber = model.AccountNumber,
                    AccountOwner = model.AccountOwner,
                    BankName = model.BankName,
                    ManufacturerId = model.ManufacturerId,
                    Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) agregó la cuenta bancaria del fabricante.\n"
                });
            }
            else
            {
                var log = GetLog(model, manufacturerBankAccount);
                manufacturerBankAccount.BankName = model.BankName;
                manufacturerBankAccount.AccountNumber = model.AccountNumber;
                manufacturerBankAccount.AccountOwner = model.AccountOwner;
                manufacturerBankAccount.Log += log;
                _manufacturerBankAccountService.Update(manufacturerBankAccount);
            }

            return NoContent();
        }

        private string GetLog(ManufacturerBankAccountModel newValue, ManufacturerBankAccount previousValue)
        {
            var log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) modificó la cuenta bancaria del fabricante. ";
            
            if (newValue.AccountNumber != previousValue.AccountNumber)
            {
                log += $"Cambió el número de cuenta de {previousValue.AccountNumber} a {newValue.AccountNumber}. ";
            }

            if (newValue.AccountOwner != previousValue.AccountOwner)
            {
                log += $"Cambió el titular de la cuenta de {previousValue.AccountOwner} a {newValue.AccountOwner}. ";
            }

            if (newValue.BankName != previousValue.BankName)
            {
                log += $"Cambió el nombre del banco de {previousValue.BankName} a {newValue.BankName}. ";
            }

            return log + "\n";
        }
    }
}
