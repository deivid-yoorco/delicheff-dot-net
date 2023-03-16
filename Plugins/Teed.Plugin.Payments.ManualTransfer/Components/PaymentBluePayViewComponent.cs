using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;
using Teed.Plugin.Payments.ManualTransfer.Models;

namespace Teed.Plugin.Payments.ManualTransfer.Components
{
    [ViewComponent(Name = "PaymentManualTransfer")]
    public class PaymentManualTransferViewComponent : NopViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        public PaymentManualTransferViewComponent(IWorkContext workContext,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            this._workContext = workContext;
            this._settingService = settingService;
            this._storeContext = storeContext;
        }

        public IViewComponentResult Invoke()
        {
            var ManualTransferPaymentSettings = _settingService.LoadSetting<ManualTransferPaymentSettings>(_storeContext.CurrentStore.Id);

            var model = new PaymentInfoModel
            {
                DescriptionText = ManualTransferPaymentSettings.GetLocalizedSetting(x => x.DescriptionText, _workContext.WorkingLanguage.Id, 0)
            };

            return View("~/Plugins/Payments.ManualTransfer/Views/PaymentInfo.cshtml", model);
        }
    }
}
