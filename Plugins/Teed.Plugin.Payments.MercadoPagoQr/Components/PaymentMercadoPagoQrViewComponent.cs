using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;
using Teed.Plugin.Payments.MercadoPagoQr.Models;

namespace Teed.Plugin.Payments.MercadoPagoQr.Components
{
    [ViewComponent(Name = "PaymentMercadoPagoQr")]
    public class PaymentMercadoPagoQrViewComponent : NopViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        public PaymentMercadoPagoQrViewComponent(IWorkContext workContext,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            this._workContext = workContext;
            this._settingService = settingService;
            this._storeContext = storeContext;
        }

        public IViewComponentResult Invoke()
        {
            var MercadoPagoQrPaymentSettings = _settingService.LoadSetting<MercadoPagoQrPaymentSettings>(_storeContext.CurrentStore.Id);

            var model = new PaymentInfoModel
            {
                DescriptionText = MercadoPagoQrPaymentSettings.GetLocalizedSetting(x => x.DescriptionText, _workContext.WorkingLanguage.Id, 0)
            };

            return View("~/Plugins/Payments.MercadoPagoQr/Views/PaymentInfo.cshtml", model);
        }
    }
}
