using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;
using Teed.Plugin.Payments.CardOnDelivery.Models;

namespace Teed.Plugin.Payments.CardOnDelivery.Components
{
    [ViewComponent(Name = "PaymentCardOnDelivery")]
    public class PaymentCardOnDeliveryViewComponent : NopViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        public PaymentCardOnDeliveryViewComponent(IWorkContext workContext,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            this._workContext = workContext;
            this._settingService = settingService;
            this._storeContext = storeContext;
        }

        public IViewComponentResult Invoke()
        {
            var CardOnDeliveryPaymentSettings = _settingService.LoadSetting<CardOnDeliveryPaymentSettings>(_storeContext.CurrentStore.Id);

            var model = new PaymentInfoModel
            {
                DescriptionText = CardOnDeliveryPaymentSettings.GetLocalizedSetting(x => x.DescriptionText, _workContext.WorkingLanguage.Id, 0)
            };

            return View("~/Plugins/Payments.CardOnDelivery/Views/PaymentInfo.cshtml", model);
        }
    }
}
