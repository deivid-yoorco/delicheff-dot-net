using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;
using Teed.Plugin.Payments.Replacement.Models;

namespace Teed.Plugin.Payments.Replacement.Components
{
    [ViewComponent(Name = "PaymentReplacement")]
    public class PaymentReplacementViewComponent : NopViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        public PaymentReplacementViewComponent(IWorkContext workContext,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            this._workContext = workContext;
            this._settingService = settingService;
            this._storeContext = storeContext;
        }

        public IViewComponentResult Invoke()
        {
            var ReplacementPaymentSettings = _settingService.LoadSetting<ReplacementPaymentSettings>(_storeContext.CurrentStore.Id);

            var model = new PaymentInfoModel
            {
                DescriptionText = ReplacementPaymentSettings.GetLocalizedSetting(x => x.DescriptionText, _workContext.WorkingLanguage.Id, 0)
            };

            return View("~/Plugins/Payments.Replacement/Views/PaymentInfo.cshtml", model);
        }
    }
}
