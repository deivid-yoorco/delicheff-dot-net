using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Tasks;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Services.Tasks;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System.Linq;
using Teed.Plugin.Emails.Helpers;
using Teed.Plugin.Emails.Models;

namespace Teed.Plugin.Emails.Controllers
{
    public class EmailsPluginController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IScheduleTaskService _scheduleTaskService;

        public EmailsPluginController(IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            ILocalizationService localizationService,
            IScheduleTaskService scheduleTaskService)
        {
            this._settingService = settingService;
            this._storeService = storeService;
            this._workContext = workContext;
            this._localizationService = localizationService;
            _scheduleTaskService = scheduleTaskService;
        }

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var emailSettings = _settingService.LoadSetting<TeedEmailsPluginSettings>(storeScope);
            var model = new ConfigurationModel()
            {
                ActiveStoreScopeConfiguration = storeScope,

                Address = emailSettings.Address,
                Aviso = emailSettings.Aviso,
                StoreColor = emailSettings.StoreColor,
                HeaderEmailPicture1Id = emailSettings.HeaderEmailPicture1Id,
                HeaderEmailPicture2Id = emailSettings.HeaderEmailPicture2Id,
                HeaderEmailPicture3Id = emailSettings.HeaderEmailPicture3Id,
                HeaderEmailPicture4Id = emailSettings.HeaderEmailPicture4Id,
                HeaderEmailPicture5Id = emailSettings.HeaderEmailPicture5Id,
                ShippingEmailPicture1Id = emailSettings.ShippingEmailPicture1Id,
                ShippingEmailPicture2Id = emailSettings.ShippingEmailPicture2Id,
                ShippingEmailPicture3Id = emailSettings.ShippingEmailPicture3Id,
                BtnTextWelcome = emailSettings.BtnTextWelcome,
                SubTitleWelcome = emailSettings.SubTitleWelcome,
                BtnTextPassword = emailSettings.BtnTextPassword,
                SubTitlePassword = emailSettings.SubTitlePassword,
                BtnTextValidate = emailSettings.BtnTextValidate,
                SubTitleValidate = emailSettings.SubTitleValidate,
                BtnTextStore = emailSettings.BtnTextStore,
                SubTitleStore = emailSettings.SubTitleStore,
                BtnTextPayment = emailSettings.BtnTextPayment,
                SubTitlePayment = emailSettings.SubTitlePayment,
                BtnTextPaypal = emailSettings.BtnTextPaypal,
                SubTitlePaypal = emailSettings.SubTitlePaypal,
                BtnTextOrderConfirm = emailSettings.BtnTextOrderConfirm,
                SubTitleOrderConfirm = emailSettings.SubTitleOrderConfirm,
                BtnTextOrderSent = emailSettings.BtnTextOrderSent,
                SubTitleOrderSent = emailSettings.SubTitleOrderSent,
                BtnTextOrderDelivered = emailSettings.BtnTextOrderDelivered,
                SubTitleOrderDelivered = emailSettings.SubTitleOrderDelivered,
                AbandonShoppingCartEmailIsActive = emailSettings.AbandonShoppingCartEmailIsActive,
                HoursToSendAbandonShoppingCartEmail = emailSettings.HoursToSendAbandonShoppingCartEmail,
                ReorderEmailIsActive = emailSettings.ReorderEmailIsActive,
                DaysToSendReorderEmail = emailSettings.DaysToSendReorderEmail,
                ReorderOnlyCompletedOrders = emailSettings.ReorderOnlyCompletedOrders
            };
            return View("~/Plugins/Teed.Plugin.Emails/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var emailSettings = _settingService.LoadSetting<TeedEmailsPluginSettings>(storeScope);

            emailSettings.Address = model.Address;
            emailSettings.Aviso = model.Aviso;
            emailSettings.StoreColor = model.StoreColor;
            emailSettings.HeaderEmailPicture1Id = model.HeaderEmailPicture1Id;
            emailSettings.HeaderEmailPicture2Id = model.HeaderEmailPicture2Id;
            emailSettings.HeaderEmailPicture3Id = model.HeaderEmailPicture3Id;
            emailSettings.HeaderEmailPicture4Id = model.HeaderEmailPicture4Id;
            emailSettings.HeaderEmailPicture5Id = model.HeaderEmailPicture5Id;
            emailSettings.ShippingEmailPicture1Id = model.ShippingEmailPicture1Id;
            emailSettings.ShippingEmailPicture2Id = model.ShippingEmailPicture2Id;
            emailSettings.ShippingEmailPicture3Id = model.ShippingEmailPicture3Id;
            emailSettings.BtnTextWelcome = model.BtnTextWelcome;
            emailSettings.SubTitleWelcome = model.SubTitleWelcome;
            emailSettings.BtnTextPassword = model.BtnTextPassword;
            emailSettings.SubTitlePassword = model.SubTitlePassword;
            emailSettings.BtnTextValidate = model.BtnTextValidate;
            emailSettings.SubTitleValidate = model.SubTitleValidate;
            emailSettings.BtnTextStore = model.BtnTextStore;
            emailSettings.SubTitleStore = model.SubTitleStore;
            emailSettings.BtnTextPayment = model.BtnTextPayment;
            emailSettings.SubTitlePayment = model.SubTitlePayment;
            emailSettings.BtnTextPaypal = model.BtnTextPaypal;
            emailSettings.SubTitlePaypal = model.SubTitlePaypal;
            emailSettings.BtnTextOrderConfirm = model.BtnTextOrderConfirm;
            emailSettings.SubTitleOrderConfirm = model.SubTitleOrderConfirm;
            emailSettings.BtnTextOrderSent = model.BtnTextOrderSent;
            emailSettings.SubTitleOrderSent = model.SubTitleOrderSent;
            emailSettings.BtnTextOrderDelivered = model.BtnTextOrderDelivered;
            emailSettings.SubTitleOrderDelivered = model.SubTitleOrderDelivered;
            emailSettings.HoursToSendAbandonShoppingCartEmail = model.HoursToSendAbandonShoppingCartEmail;
            emailSettings.AbandonShoppingCartEmailIsActive = model.AbandonShoppingCartEmailIsActive;
            emailSettings.ReorderEmailIsActive = model.ReorderEmailIsActive;
            emailSettings.DaysToSendReorderEmail = model.DaysToSendReorderEmail;
            emailSettings.ReorderOnlyCompletedOrders = model.ReorderOnlyCompletedOrders;

            ScheduleTask abandonedShoppingCartScheduleTask = _scheduleTaskService.GetTaskByType(EmailHelper.ABANDONED_SHOPPING_CART_SCHEDULETASK_NAME);
            if (abandonedShoppingCartScheduleTask == null && model.HoursToSendAbandonShoppingCartEmail > 0)
            {
                _scheduleTaskService.InsertTask(new ScheduleTask()
                {
                    Enabled = model.AbandonShoppingCartEmailIsActive,
                    Name = "Abandoned shopping cart email",
                    Seconds = 3600, // 1 hour
                    StopOnError = false,
                    Type = EmailHelper.ABANDONED_SHOPPING_CART_SCHEDULETASK_NAME
                });
            }
            else if (abandonedShoppingCartScheduleTask != null)
            {
                abandonedShoppingCartScheduleTask.Enabled = model.AbandonShoppingCartEmailIsActive;
                _scheduleTaskService.UpdateTask(abandonedShoppingCartScheduleTask);
            }

            ScheduleTask reorderScheduleTask = _scheduleTaskService.GetTaskByType(EmailHelper.REORDER_SCHEDULETASK_NAME);
            if (reorderScheduleTask == null && model.DaysToSendReorderEmail > 0)
            {
                _scheduleTaskService.InsertTask(new ScheduleTask()
                {
                    Enabled = model.ReorderEmailIsActive,
                    Name = "Reorder email",
                    Seconds = 86400, // 24 hours
                    StopOnError = false,
                    Type = EmailHelper.REORDER_SCHEDULETASK_NAME
                });
            }
            else if (reorderScheduleTask != null)
            {
                reorderScheduleTask.Enabled = model.ReorderEmailIsActive;
                _scheduleTaskService.UpdateTask(reorderScheduleTask);
            }

            _settingService.SaveSetting(emailSettings);
            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return View("~/Plugins/Teed.Plugin.Emails/Views/Configure.cshtml", model);
        }

        [HttpGet]
        public IActionResult GetDataEmails()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var emailSettings = _settingService.LoadSetting<TeedEmailsPluginSettings>(storeScope);
            var model = new ConfigurationModel()
            {
                ActiveStoreScopeConfiguration = storeScope,

                Address = emailSettings.Address,
                Aviso = emailSettings.Aviso,
                StoreColor = emailSettings.StoreColor,
                HeaderEmailPicture1Id = emailSettings.HeaderEmailPicture1Id,
                HeaderEmailPicture2Id = emailSettings.HeaderEmailPicture2Id,
                HeaderEmailPicture3Id = emailSettings.HeaderEmailPicture3Id,
                HeaderEmailPicture4Id = emailSettings.HeaderEmailPicture4Id,
                HeaderEmailPicture5Id = emailSettings.HeaderEmailPicture5Id,
                ShippingEmailPicture1Id = emailSettings.ShippingEmailPicture1Id,
                ShippingEmailPicture2Id = emailSettings.ShippingEmailPicture2Id,
                ShippingEmailPicture3Id = emailSettings.ShippingEmailPicture3Id,
                BtnTextWelcome = emailSettings.BtnTextWelcome,
                SubTitleWelcome = emailSettings.SubTitleWelcome,
                BtnTextPassword = emailSettings.BtnTextPassword,
                SubTitlePassword = emailSettings.SubTitlePassword,
                BtnTextValidate = emailSettings.BtnTextValidate,
                SubTitleValidate = emailSettings.SubTitleValidate,
                BtnTextStore = emailSettings.BtnTextStore,
                SubTitleStore = emailSettings.SubTitleStore,
                BtnTextPayment = emailSettings.BtnTextPayment,
                SubTitlePayment = emailSettings.SubTitlePayment,
                BtnTextPaypal = emailSettings.BtnTextPaypal,
                SubTitlePaypal = emailSettings.SubTitlePaypal,
                BtnTextOrderConfirm = emailSettings.BtnTextOrderConfirm,
                SubTitleOrderConfirm = emailSettings.SubTitleOrderConfirm,
                BtnTextOrderSent = emailSettings.BtnTextOrderSent,
                SubTitleOrderSent = emailSettings.SubTitleOrderSent,
                BtnTextOrderDelivered = emailSettings.BtnTextOrderDelivered,
                SubTitleOrderDelivered = emailSettings.SubTitleOrderDelivered,
                ReorderEmailIsActive = emailSettings.ReorderEmailIsActive,
                ReorderOnlyCompletedOrders = emailSettings.ReorderOnlyCompletedOrders,
                DaysToSendReorderEmail = emailSettings.DaysToSendReorderEmail,
                AbandonShoppingCartEmailIsActive = emailSettings.AbandonShoppingCartEmailIsActive,
                HoursToSendAbandonShoppingCartEmail = emailSettings.HoursToSendAbandonShoppingCartEmail
            };
            return Ok(model);
        }

        #endregion
    }
}
