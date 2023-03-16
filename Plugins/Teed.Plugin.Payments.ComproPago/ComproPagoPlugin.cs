using CompropagoSdk;
using CompropagoSdk.Factory;
using CompropagoSdk.Factory.Models;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.ComproPago
{
    public class ComproPagoPlugin : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly ComproPagoPaymentSettings _comproPagoPaymentSettings;

        #endregion

        #region Ctor

        public ComproPagoPlugin(ILocalizationService localizationService,
            ISettingService settingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IWebHelper webHelper,
            ComproPagoPaymentSettings comproPagoPaymentSettings)
        {
            _localizationService = localizationService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _comproPagoPaymentSettings = comproPagoPaymentSettings;
            _webHelper = webHelper;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentComproPago/Configure";
        }

        public override void Install()
        {
            _settingService.SaveSetting(new ComproPagoPaymentSettings
            {
                UseSandbox = true
            });

            base.Install();
        }

        public override void Uninstall()
        {
            _settingService.DeleteSetting<ComproPagoPaymentSettings>();

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
            return this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
                _comproPagoPaymentSettings.AdditionalFee, _comproPagoPaymentSettings.AdditionalFeePercentage);
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
            viewComponentName = "PaymentComproPago";
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
            Client client = new Client(
                _comproPagoPaymentSettings.PublicKey,
                _comproPagoPaymentSettings.PrivateKey,
                _comproPagoPaymentSettings.UseSandbox
            );

            var email = postProcessPaymentRequest.Order.Customer.Email;
            if (string.IsNullOrWhiteSpace(email)) email = postProcessPaymentRequest.Order.ShippingAddress.Email;
            var firstName = postProcessPaymentRequest.Order.Customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
            if (string.IsNullOrWhiteSpace(firstName)) firstName = postProcessPaymentRequest.Order.ShippingAddress.FirstName;
            var lastName = postProcessPaymentRequest.Order.Customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
            if (string.IsNullOrWhiteSpace(lastName)) lastName = postProcessPaymentRequest.Order.ShippingAddress.LastName;

            var orderInfo = new Dictionary<string, string>
            {
                {"order_id", postProcessPaymentRequest.Order.Id.ToString()},
                {"order_name", "ORDEN #" + postProcessPaymentRequest.Order.Id.ToString()},
                {"order_price", postProcessPaymentRequest.Order.OrderTotal.ToString()},
                {"customer_name", firstName + " " + lastName},
                {"customer_email", email},
                {"payment_type", postProcessPaymentRequest.Order.PaymentParameter },
                {"currency", "MXN"},
                {"expiration_time", (DateTime.Now.AddDays(7) - new DateTime(1970, 1, 1)).TotalSeconds.ToString()}
            };

            PlaceOrderInfo order = Factory.PlaceOrderInfo(orderInfo);
            NewOrderInfo newOrder = client.Api.PlaceOrder(order);
            return;
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
            get { return PaymentMethodType.Standard; }
        }

        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        public string PaymentMethodDescription
        {
            get { return "Pago en efectivo. Recibirás las instrucciones por correo electrónico."; }
        }

        #endregion
    }
}
