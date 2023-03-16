using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Services.Payments;
using MercadoPagoSDK = MercadoPago;
using System.Linq;
using Teed.Plugin.Payments.MercadoPago.Data;
using Teed.Plugin.Payments.MercadoPago.Services;
using Teed.Plugin.Payments.MercadoPago.Domain;
using System;

namespace Teed.Plugin.Payments.MercadoPago
{
    public class MercadoPagoPlugin : BasePlugin, IPaymentMethod
    {
        #region Fields
        
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PluginObjectContext _context;
        private readonly MercadoPagoTransactionService _mercadoPagoTransactionService;

        #endregion

        #region Ctor

        public MercadoPagoPlugin(ISettingService settingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IWebHelper webHelper,
            IHttpContextAccessor httpContextAccessor,
            PluginObjectContext context,
            MercadoPagoTransactionService mercadoPagoTransactionService)
        {
            _orderTotalCalculationService = orderTotalCalculationService;
            _webHelper = webHelper;
            _settingService = settingService;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _mercadoPagoTransactionService = mercadoPagoTransactionService;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentMercadoPago/Configure";
        }

        public override void Install()
        {
            _settingService.SaveSetting(new MercadoPagoPaymentSettings
            {
                UseSandbox = true,
                BankTransferAllowed = true,
                CashAllowed = true,
                CreditCardAllowed = true,
                DebitCardAllowed = true,
                PrepaidCardAllowed = true
            });
            _context.Install();
            base.Install();
        }

        public override void Uninstall()
        {
            _settingService.DeleteSetting<MercadoPagoPaymentSettings>();
            _context.Uninstall();
            base.Uninstall();
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            return false;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            return new CapturePaymentResult { Errors = new[] { "Capture method not supported" } };
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return 0;
            //return this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
            //    _comproPagoPaymentSettings.AdditionalFee, _comproPagoPaymentSettings.AdditionalFeePercentage);
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            return new ProcessPaymentRequest();
        }

        /// <summary>
        /// Gets a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <param name="viewComponentName">View component name</param>
        public void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "PaymentMercadoPago";
        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            MercadoPagoPaymentSettings settings = _settingService.LoadSetting<MercadoPagoPaymentSettings>();

            MercadoPagoSDK.SDK.CleanConfiguration();
            MercadoPagoSDK.SDK.AccessToken = settings.UseSandbox ? settings.SandboxAccessToken : settings.AccessToken;

            MercadoPagoSDK.Resources.Preference preference = new MercadoPagoSDK.Resources.Preference();

            preference.Items.Add(new MercadoPagoSDK.DataStructures.Preference.Item
            {
                Title = "Orden #" + postProcessPaymentRequest.Order.Id,
                Quantity = 1,
                CurrencyId = MercadoPagoSDK.Common.CurrencyId.MXN,
                UnitPrice = postProcessPaymentRequest.Order.OrderTotal
            });

            preference.AutoReturn = MercadoPagoSDK.Common.AutoReturnType.all;
            preference.BackUrls = new MercadoPagoSDK.DataStructures.Preference.BackUrls()
            {
                Success = $"{_webHelper.GetStoreLocation()}PaymentMercadoPago/PaymentHandler",
                Failure = $"{_webHelper.GetStoreLocation()}PaymentMercadoPago/PaymentHandler",
                Pending = $"{_webHelper.GetStoreLocation()}PaymentMercadoPago/PaymentHandler"
            };

            preference.BinaryMode = settings.RequireInmediatePayment;
            preference.NotificationUrl = _webHelper.GetStoreLocation().Contains("localhost") ? string.Empty : $"{_webHelper.GetStoreLocation()}PaymentMercadoPago/Notifications"; // WEBHOOK URL

            preference.PaymentMethods = new MercadoPagoSDK.DataStructures.Preference.PaymentMethods()
            {
                ExcludedPaymentTypes = new List<MercadoPagoSDK.DataStructures.Preference.PaymentType>()
            };

            if (!settings.CashAllowed)
            {
                preference.PaymentMethods.Value.ExcludedPaymentTypes.Add(new MercadoPagoSDK.DataStructures.Preference.PaymentType()
                {
                    Id = "ticket"
                });

                preference.PaymentMethods.Value.ExcludedPaymentTypes.Add(new MercadoPagoSDK.DataStructures.Preference.PaymentType()
                {
                    Id = "atm"
                });
            }

            if (!settings.CreditCardAllowed)
            {
                preference.PaymentMethods.Value.ExcludedPaymentTypes.Add(new MercadoPagoSDK.DataStructures.Preference.PaymentType()
                {
                    Id = "credit_card"
                });
            }

            if (!settings.DebitCardAllowed)
            {
                preference.PaymentMethods.Value.ExcludedPaymentTypes.Add(new MercadoPagoSDK.DataStructures.Preference.PaymentType()
                {
                    Id = "debit_card"
                });
            }

            if (!settings.PrepaidCardAllowed)
            {
                preference.PaymentMethods.Value.ExcludedPaymentTypes.Add(new MercadoPagoSDK.DataStructures.Preference.PaymentType()
                {
                    Id = "prepaid_card"
                });
            }

            if (!settings.BankTransferAllowed)
            {
                preference.PaymentMethods.Value.ExcludedPaymentTypes.Add(new MercadoPagoSDK.DataStructures.Preference.PaymentType()
                {
                    Id = "bank_transfer"
                });
            }

            preference.ExternalReference = postProcessPaymentRequest.Order.Id.ToString();

            preference.Expires = true;

            preference.ExpirationDateTo = DateTime.Now.AddDays(2);

            preference.Save();

            _mercadoPagoTransactionService.Insert(new MercadoPagoTransaction()
            {
                MercadoPagoTransactionId = preference.Id,
                OrderId = postProcessPaymentRequest.Order.Id
            });

            _httpContextAccessor.HttpContext.Response.Redirect(settings.UseSandbox ? preference.SandboxInitPoint : preference.InitPoint);
        }

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult();
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            return new RefundPaymentResult { Errors = new[] { "Refund method not supported" } };
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            return new List<string>();
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            return new VoidPaymentResult { Errors = new[] { "Void method not supported" } };
        }

        #endregion

        #region Properties

        public bool SupportCapture
        {
            get { return false; }
        }

        public bool SupportPartiallyRefund
        {
            get { return false; }
        }

        public bool SupportRefund
        {
            get { return false; }
        }

        public bool SupportVoid
        {
            get { return false; }
        }

        public RecurringPaymentType RecurringPaymentType
        {
            get { return RecurringPaymentType.NotSupported; }
        }

        public PaymentMethodType PaymentMethodType
        {
            get { return PaymentMethodType.Redirection; }
        }

        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        public string PaymentMethodDescription
        {
            get { return "Paga con MercadoPago. Serás redirigido para poder culminar la transacción."; }
        }

        #endregion
    }
}
