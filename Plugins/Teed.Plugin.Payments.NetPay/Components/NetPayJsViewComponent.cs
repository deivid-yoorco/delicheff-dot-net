using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;

namespace Teed.Plugin.Payments.NetPay.Components
{
    [ViewComponent(Name = "NetPayJs")]
    public class NetPayJsViewComponent : NopViewComponent
    {
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        public NetPayJsViewComponent(ISettingService settingService, IStoreContext storeContext)
        {
            _settingService = settingService;
            _storeContext = storeContext;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var netPayPaymentSettings = _settingService.LoadSetting<NetPayPaymentSettings>(_storeContext.CurrentStore.Id);
            var useSandbox = netPayPaymentSettings.UseSandbox;
           return View("~/Plugins/Payments.NetPay/Views/NetPayJs.cshtml", useSandbox);
        }
    }
}