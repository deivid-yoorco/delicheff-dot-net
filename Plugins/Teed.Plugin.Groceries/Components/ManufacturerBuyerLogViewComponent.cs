using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Components
{
    [ViewComponent(Name = "ManufacturerBuyerLog")]
    public class ManufacturerBuyerLogViewComponent : NopViewComponent
    {
        private readonly ManufacturerBuyerService _manufacturerBuyerService;

        public ManufacturerBuyerLogViewComponent(ManufacturerBuyerService manufacturerBuyerService)
        {
            _manufacturerBuyerService = manufacturerBuyerService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var manufacturerId = (int)additionalData;
            var defaultManufacturerAssignLog = _manufacturerBuyerService.GetAll().Where(x => x.ManufacturerId == manufacturerId).FirstOrDefault()?.Log;
            
            return View("~/Plugins/Teed.Plugin.Groceries/Views/Shared/Components/ManufacturerBuyerLog/Default.cshtml", defaultManufacturerAssignLog);
        }
    }
}
