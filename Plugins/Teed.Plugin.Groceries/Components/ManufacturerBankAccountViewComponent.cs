using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Models.ManufacturerBankAccount;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Components
{
    [ViewComponent(Name = "ManufacturerBankAccount")]
    public class ManufacturerBankAccountViewComponent : NopViewComponent
    {
        private readonly ManufacturerBankAccountService _manufacturerBankAccountService;

        public ManufacturerBankAccountViewComponent(ManufacturerBankAccountService manufacturerBankAccountService)
        {
            _manufacturerBankAccountService = manufacturerBankAccountService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var manufacturerId = (int)additionalData;
            var manufacturerBankAccount = _manufacturerBankAccountService
                .GetAll().Where(x => x.ManufacturerId == manufacturerId)
                .FirstOrDefault();
            var model = new ManufacturerBankAccountModel();
            model.ManufacturerId = manufacturerId;
            if (manufacturerBankAccount != null)
            {
                model.AccountNumber = manufacturerBankAccount.AccountNumber;
                model.AccountOwner = manufacturerBankAccount.AccountOwner;
                model.BankName = manufacturerBankAccount.BankName;
                model.Id = manufacturerBankAccount.Id;
            }
            
            return View("~/Plugins/Teed.Plugin.Groceries/Views/Shared/Components/ManufacturerBankAccount/Default.cshtml", model);
        }
    }
}
