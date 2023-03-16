using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System.Net.Http;
using System.Threading.Tasks;
using Nop.Services.Orders;
using Nop.Core.Domain.Orders;
using System;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Services.Messages;
using Nop.Services.Events;
using Nop.Core.Domain.Localization;
using Nop.Services.Catalog;
using Newtonsoft.Json.Linq;
using System.Linq;
using Nop.Core.Domain.Catalog;
using Nop.Services.Customers;
using Nop.Services.Payments;
using Teed.Plugin.Payments.MercadoPago;
using Teed.Plugin.Payments.MercadoPago.Models;
using Microsoft.AspNetCore.Authorization;
using Teed.Plugin.Payments.MercadoPago.Services;
using Teed.Plugin.Payments.MercadoPago.Domain;

using MercadoPagoSDK = MercadoPago;
using MercadoPago.Resources;
using MercadoPago.DataStructures.Preference;
using MercadoPago.Common;
using Nop.Web.Factories;
using System.Collections.Generic;

namespace Teed.Plugin.Payments.MercadoPago.Controller
{
    public class PaymentMercadoPagoController : BasePaymentController
    {
        #region Fields

        private const string MERCADOPAGO_API_URL = "https://api.mercadopago.com";
        private const string MERCADOPAGO_PAYMENT = "/v1/payments/";

        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IRewardPointService _rewardPointService;
        private readonly IProductService _productService;
        private readonly IStockLogService _stockLogService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IPaymentService _paymentService;
        private readonly ICustomerService _customerService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly MercadoPagoTransactionService _mercadoPagoTransactionService;
        private readonly MercadoPagoApiLogService _mercadoPagoApiLogService;
        private readonly PaymentSettings _paymentSettings;
        private readonly MercadoPagoPaymentSettings _mercadoPagoPaymentSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly OrderSettings _orderSettings;

        #endregion

        #region Ctor

        public PaymentMercadoPagoController(IStoreService storeService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ISettingService settingService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IShoppingCartModelFactory shoppingCartModelFactory,
            IOrderService orderService,
            IWorkflowMessageService workflowMessageService,
            IEventPublisher eventPublisher,
            IRewardPointService rewardPointService,
            IProductService productService,
            IOrderProcessingService orderProcessingService,
            IPaymentService paymentService,
            ICustomerService customerService,
            OrderSettings orderSettings,
            PaymentSettings paymentSettings,
            LocalizationSettings localizationSettings,
            MercadoPagoPaymentSettings mercadoPagoPaymentSettings,
            MercadoPagoTransactionService mercadoPagoTransactionService,
            MercadoPagoApiLogService mercadoPagoApiLogService,
            IStockLogService stockLogService,
            IProductAttributeParser productAttributeParser)
        {
            _storeService = storeService;
            _workContext = workContext;
            _storeContext = storeContext;
            _settingService = settingService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _orderService = orderService;
            _workflowMessageService = workflowMessageService;
            _orderSettings = orderSettings;
            _eventPublisher = eventPublisher;
            _localizationSettings = localizationSettings;
            _rewardPointService = rewardPointService;
            _productService = productService;
            _mercadoPagoPaymentSettings = mercadoPagoPaymentSettings;
            _stockLogService = stockLogService;
            _productAttributeParser = productAttributeParser;
            _orderProcessingService = orderProcessingService;
            _paymentService = paymentService;
            _paymentSettings = paymentSettings;
            _mercadoPagoTransactionService = mercadoPagoTransactionService;
            _mercadoPagoApiLogService = mercadoPagoApiLogService;
            _customerService = customerService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var mercadoPagoPaymentSettings = _settingService.LoadSetting<MercadoPagoPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseSandbox = mercadoPagoPaymentSettings.UseSandbox,
                AdditionalFee = mercadoPagoPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = mercadoPagoPaymentSettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope,
                AccessToken = mercadoPagoPaymentSettings.AccessToken,
                PublicKey = mercadoPagoPaymentSettings.PublicKey,
                SandboxPublicKey = mercadoPagoPaymentSettings.SandboxPublicKey,
                SandboxAccessToken = mercadoPagoPaymentSettings.SandboxAccessToken,
                RequireInmediatePayment = mercadoPagoPaymentSettings.RequireInmediatePayment,
                BankTransferAllowed = mercadoPagoPaymentSettings.BankTransferAllowed,
                CashAllowed = mercadoPagoPaymentSettings.CashAllowed,
                CreditCardAllowed = mercadoPagoPaymentSettings.CreditCardAllowed,
                DebitCardAllowed = mercadoPagoPaymentSettings.DebitCardAllowed,
                PrepaidCardAllowed = mercadoPagoPaymentSettings.PrepaidCardAllowed,
            };
            if (storeScope > 0)
            {
                model.UseSandbox_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.UseSandbox, storeScope);
                model.AccessToken_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.AccessToken, storeScope);
                model.PublicKey_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.PublicKey, storeScope);
                model.SandboxAccessToken_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.SandboxAccessToken, storeScope);
                model.SandboxPublicKey_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.SandboxAccessToken, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.RequireInmediatePayment_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.RequireInmediatePayment, storeScope);

                model.BankTransferAllowed_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.BankTransferAllowed, storeScope);
                model.CashAllowed_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.CashAllowed, storeScope);
                model.CreditCardAllowed_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.CreditCardAllowed, storeScope);
                model.DebitCardAllowed_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.DebitCardAllowed, storeScope);
                model.PrepaidCardAllowed_OverrideForStore = _settingService.SettingExists(mercadoPagoPaymentSettings, x => x.PrepaidCardAllowed, storeScope);

            }

            return View("~/Plugins/Payments.MercadoPago/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var mercadoPagoPaymentSettings = _settingService.LoadSetting<MercadoPagoPaymentSettings>(storeScope);

            //save settings
            mercadoPagoPaymentSettings.UseSandbox = model.UseSandbox;
            mercadoPagoPaymentSettings.AccessToken = model.AccessToken;
            mercadoPagoPaymentSettings.PublicKey = model.PublicKey;
            mercadoPagoPaymentSettings.SandboxAccessToken = model.SandboxAccessToken;
            mercadoPagoPaymentSettings.SandboxPublicKey = model.SandboxPublicKey;
            mercadoPagoPaymentSettings.AdditionalFee = model.AdditionalFee;
            mercadoPagoPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            mercadoPagoPaymentSettings.RequireInmediatePayment = model.RequireInmediatePayment;

            mercadoPagoPaymentSettings.BankTransferAllowed = model.BankTransferAllowed;
            mercadoPagoPaymentSettings.CashAllowed = model.CashAllowed;
            mercadoPagoPaymentSettings.CreditCardAllowed = model.CreditCardAllowed;
            mercadoPagoPaymentSettings.DebitCardAllowed = model.DebitCardAllowed;
            mercadoPagoPaymentSettings.PrepaidCardAllowed = model.PrepaidCardAllowed;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(mercadoPagoPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(mercadoPagoPaymentSettings, x => x.PublicKey, model.PublicKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(mercadoPagoPaymentSettings, x => x.AccessToken, model.AccessToken_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(mercadoPagoPaymentSettings, x => x.SandboxPublicKey, model.SandboxPublicKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(mercadoPagoPaymentSettings, x => x.SandboxAccessToken, model.SandboxAccessToken_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(mercadoPagoPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(mercadoPagoPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(mercadoPagoPaymentSettings, x => x.RequireInmediatePayment, model.RequireInmediatePayment_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(mercadoPagoPaymentSettings, x => x.BankTransferAllowed, model.BankTransferAllowed_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(mercadoPagoPaymentSettings, x => x.CashAllowed, model.CashAllowed_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(mercadoPagoPaymentSettings, x => x.CreditCardAllowed, model.CreditCardAllowed_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(mercadoPagoPaymentSettings, x => x.DebitCardAllowed, model.DebitCardAllowed_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(mercadoPagoPaymentSettings, x => x.PrepaidCardAllowed, model.PrepaidCardAllowed_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [HttpGet]
        [AllowAnonymous]
        public Preference GetPreference()
        {
            var settings = _settingService.LoadSetting<MercadoPagoPaymentSettings>();

            // Agrega credenciales
            MercadoPagoSDK.SDK.CleanConfiguration();
            MercadoPagoSDK.SDK.AccessToken = settings.UseSandbox ? settings.SandboxAccessToken : settings.AccessToken;

            // Crea un objeto de preferencia
            Preference preference = new Preference();

            // Crea un ítem en la preferencia
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            foreach (var item in cart)
            {
                preference.Items.Add(
                  new Item()
                  {
                      Title = item.Product.Name,
                      Quantity = item.Quantity,
                      CurrencyId = CurrencyId.MXN,
                      UnitPrice = item.Product.Price * (item.Quantity == 0 ? 1 : item.Quantity)
                  }
                );
            }
            preference.Save();

            return preference;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Api/[controller]/[action]")]
        public IActionResult GetPreference(int userId)
        {
            var settings = _settingService.LoadSetting<MercadoPagoPaymentSettings>();

            var customer = _customerService.GetCustomerById(userId);

            MercadoPagoSDK.SDK.CleanConfiguration();
            MercadoPagoSDK.SDK.AccessToken = settings.UseSandbox ? settings.SandboxAccessToken : settings.AccessToken;

            Preference preference = new Preference();

            var cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var totals = _shoppingCartModelFactory.PrepareOrderTotalsModel(cart, false);

            preference.Items.Add(new Item()
            {
                Title = "Carrito de " + _storeContext.CurrentStore.Name,
                Quantity = 1,
                CurrencyId = MercadoPagoSDK.Common.CurrencyId.MXN,
                UnitPrice = totals.OrderTotalValue
            });

            preference.AutoReturn = MercadoPagoSDK.Common.AutoReturnType.all;
            preference.BackUrls = new MercadoPagoSDK.DataStructures.Preference.BackUrls()
            {
                Success = $"{_storeContext.CurrentStore.SecureUrl}PaymentMercadoPago/PaymentHandler",
                Failure = $"{_storeContext.CurrentStore.SecureUrl}PaymentMercadoPago/PaymentHandler",
                Pending = $"{_storeContext.CurrentStore.SecureUrl}PaymentMercadoPago/PaymentHandler"
            };

            preference.BinaryMode = settings.RequireInmediatePayment;
            preference.NotificationUrl = $"{_storeContext.CurrentStore.SecureUrl}PaymentMercadoPago/Notifications"; // WEBHOOK URL

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

            preference.ExternalReference = customer.Email;

            preference.Expires = true;

            preference.ExpirationDateTo = DateTime.Now.AddDays(2);

            preference.Save();

            var publicKey = settings.UseSandbox ? settings.SandboxPublicKey : settings.PublicKey;

            return Ok(new { preferenceId = preference.Id, publicKey });
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult PaymentHandler(string collection_id,
            string collection_status,
            string external_reference,
            string payment_type,
            string merchant_order_id,
            string preference_id,
            string site_id,
            string processing_mode,
            string merchant_account_id)
        {
            if (string.IsNullOrWhiteSpace(external_reference) || string.IsNullOrWhiteSpace(collection_status)) return BadRequest();
            int.TryParse(external_reference, out int orderId);

            Order order = _orderService.GetOrderById(orderId);
            if (order == null) return NotFound();

            AddOrderNote(order, Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                collection_id,
                collection_status,
                external_reference,
                payment_type,
                merchant_order_id,
                preference_id,
                site_id,
                processing_mode,
                merchant_account_id
            }));

            switch (collection_status)
            {
                case "approved":
                    MarkOrderAsPaid(order);
                    return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
                case "rejected":
                    MarkOrderAsCancelled(order, "Pago rechazado por MercadoPago");
                    return RedirectToAction("Failure", "Checkout", new { orderId = order.Id });
                case "pending":
                    return RedirectToAction("Pending", "Checkout", new { orderId = order.Id });
                default:
                    return RedirectToAction("Failure", "Checkout", new { orderId = order.Id });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Notifications(string topic, string id)
        {
            var settings = _settingService.LoadSetting<MercadoPagoPaymentSettings>();
            using (HttpClient client = new HttpClient())
            {
                var result = await client.GetAsync(MERCADOPAGO_API_URL + MERCADOPAGO_PAYMENT + id + $"?access_token={(settings.UseSandbox ? settings.SandboxAccessToken : settings.AccessToken)}");
                string json = await result.Content.ReadAsStringAsync();
                Order order = null;
                if (result.IsSuccessStatusCode)
                {
                    var paymentData = Newtonsoft.Json.JsonConvert.DeserializeObject<PaymentNotificationModel>(json);

                    int.TryParse(paymentData.external_reference, out int orderId);
                    order = _orderService.GetOrderById(orderId);
                    if (order == null)
                    {
                        _mercadoPagoApiLogService.Insert(new MercadoPagoApiLog()
                        {
                            Error = true,
                            ErrorMessage = "No se encontró la orden " + orderId,
                            NotificationJson = json,
                            OrderId = 0
                        });
                        return NotFound();
                    };

                    switch (paymentData.status)
                    {
                        case "pending":
                            AddOrderNote(order, "[MercadoPago] En espera del pago del cliente.");
                            break;
                        case "approved":
                            MarkOrderAsPaid(order);
                            break;
                        case "authorized":
                            AddOrderNote(order, "[MercadoPago] El pago fue autorizado pero no capturado todavía.");
                            break;
                        case "in_process":
                            AddOrderNote(order, "[MercadoPago] El pago está en revisión.");
                            break;
                        case "in_mediation":
                            AddOrderNote(order, "[MercadoPago] El usuario inició una disputa.");
                            break;
                        case "rejected":
                            AddOrderNote(order, "[MercadoPago] El pago fue rechazado. El usuario podría reintentar el pago.");
                            break;
                        case "refunded":
                            AddOrderNote(order, "[MercadoPago] El pago fue devuelto al usuario.");
                            break;
                        case "charged_back":
                            AddOrderNote(order, "[MercadoPago] Se ha realizado un contracargo en la tarjeta de crédito del comprador.");
                            break;
                        case "cancelled":
                            MarkOrderAsCancelled(order, "[MercadoPago] El pago fue cancelado por una de las partes o el pago expiró.");
                            break;
                        default:
                            break;
                    }
                }

                _mercadoPagoApiLogService.Insert(new MercadoPagoApiLog()
                {
                    Error = !result.IsSuccessStatusCode,
                    ErrorMessage = !result.IsSuccessStatusCode ? json : null,
                    NotificationJson = result.IsSuccessStatusCode ? json : null,
                    OrderId = order == null ? 0 : order.Id
                });
            }

            return Ok();
        }

        //[HttpGet]
        //[AllowAnonymous]
        //public IActionResult Test()
        //{
        //    MercadoPagoPaymentSettings settings = _settingService.LoadSetting<MercadoPagoPaymentSettings>();

        //    MercadoPagoSDK.SDK.CleanConfiguration();
        //    MercadoPagoSDK.SDK.AccessToken = settings.UseSandbox ? settings.SandboxAccessToken : settings.AccessToken;

        //    MercadoPagoSDK.Resources.Preference preference = new MercadoPagoSDK.Resources.Preference();

        //    var payment_methods = MercadoPagoSDK.SDK.Get("/v1/payment_methods");

        //    preference.Items.Add(new MercadoPagoSDK.DataStructures.Preference.Item()
        //    {
        //        Title = "Envío",
        //        Quantity = 1,
        //        CurrencyId = MercadoPagoSDK.Common.CurrencyId.MXN,
        //        UnitPrice = 100
        //    });

        //    preference.AutoReturn = MercadoPagoSDK.Common.AutoReturnType.all;
        //    preference.BackUrls = new MercadoPagoSDK.DataStructures.Preference.BackUrls()
        //    {
        //        Success = "http://localhost:8045/PaymentMercadoPago/PaymentHandler",
        //        Failure = "http://localhost:8045/PaymentMercadoPago/PaymentHandler",
        //        Pending = "http://localhost:8045/PaymentMercadoPago/PaymentHandler",
        //    };

        //    preference.NotificationUrl = "http://7623e493.ngrok.io/PaymentMercadoPago/Notifications";

        //    preference.PaymentMethods = new MercadoPagoSDK.DataStructures.Preference.PaymentMethods() {
        //        ExcludedPaymentTypes = new System.Collections.Generic.List<MercadoPagoSDK.DataStructures.Preference.PaymentType>()
        //    };

        //    if (!settings.CashAllowed)
        //    {
        //        preference.PaymentMethods.Value.ExcludedPaymentTypes.Add(new MercadoPagoSDK.DataStructures.Preference.PaymentType()
        //        {
        //            Id = "ticket"
        //        });

        //        preference.PaymentMethods.Value.ExcludedPaymentTypes.Add(new MercadoPagoSDK.DataStructures.Preference.PaymentType()
        //        {
        //            Id = "atm"
        //        });
        //    }

        //    if (!settings.CreditCardAllowed)
        //    {
        //        preference.PaymentMethods.Value.ExcludedPaymentTypes.Add(new MercadoPagoSDK.DataStructures.Preference.PaymentType()
        //        {
        //            Id = "credit_card"
        //        });
        //    }

        //    if (!settings.DebitCardAllowed)
        //    {
        //        preference.PaymentMethods.Value.ExcludedPaymentTypes.Add(new MercadoPagoSDK.DataStructures.Preference.PaymentType()
        //        {
        //            Id = "debit_card"
        //        });
        //    }

        //    if (!settings.PrepaidCardAllowed)
        //    {
        //        preference.PaymentMethods.Value.ExcludedPaymentTypes.Add(new MercadoPagoSDK.DataStructures.Preference.PaymentType()
        //        {
        //            Id = "prepaid_card"
        //        });
        //    }

        //    if (!settings.BankTransferAllowed)
        //    {
        //        preference.PaymentMethods.Value.ExcludedPaymentTypes.Add(new MercadoPagoSDK.DataStructures.Preference.PaymentType()
        //        {
        //            Id = "bank_transfer"
        //        });
        //    }

        //    preference.Save();

        //    return Redirect(preference.SandboxInitPoint);
        //}

        #endregion

        #region Private Methods

        public virtual void MarkOrderAsPaid(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            _orderProcessingService.MarkOrderAsPaid(order);

            //add a note
            AddOrderNote(order, "Pago registrado por MercadoPago");

            ProcessOrderPaid(order);
        }

        protected virtual void MarkOrderAsCancelled(Order order, string reason)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            SetOrderStatus(order, OrderStatus.Cancelled, true);
            AddOrderNote(order, "La orden fue cancelada de forma automática" + (string.IsNullOrWhiteSpace(reason) ? "." : $" ({reason})"));

            ReturnBackRedeemedRewardPoints(order);

            //Adjust inventory
            foreach (var orderItem in order.OrderItems)
            {
                var previousStock = orderItem.Product.ManageInventoryMethod == ManageInventoryMethod.ManageStock ? orderItem.Product.StockQuantity :
                   _productAttributeParser.FindProductAttributeCombination(orderItem.Product, orderItem.AttributesXml).StockQuantity;

                _productService.AdjustInventory(orderItem.Product, orderItem.Quantity, orderItem.AttributesXml,
                    string.Format(_localizationService.GetResource("Admin.StockQuantityHistory.Messages.CancelOrder"), order.Id));

                _stockLogService.InsertStockLog(new StockLog()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    NewStock = previousStock + orderItem.Quantity,
                    OldStock = previousStock,
                    Product = orderItem.Product.Name,
                    ProductId = orderItem.Product.Id,
                    SKU = orderItem.Product.Sku,
                    User = _workContext.CurrentCustomer.GetFullName(),
                    ChangeType = TypeChangeEnum.Cancel
                });

            }

            _eventPublisher.Publish(new OrderCancelledEvent(order));
        }

        protected virtual void ReturnBackRedeemedRewardPoints(Order order)
        {
            //were some points redeemed when placing an order?
            if (order.RedeemedRewardPointsEntry == null)
                return;

            //return back
            _rewardPointService.AddRewardPointsHistoryEntry(order.Customer, -order.RedeemedRewardPointsEntry.Points, order.StoreId,
                string.Format(_localizationService.GetResource("RewardPoints.Message.ReturnedForOrder"), order.CustomOrderNumber));
            _orderService.UpdateOrder(order);
        }

        protected virtual void ProcessOrderPaid(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //raise event
            _eventPublisher.Publish(new OrderPaidEvent(order));

            //order paid email notification
            if (order.OrderTotal != decimal.Zero)
            {
                //we should not send it for free ($0 total) orders?
                //remove this "if" statement if you want to send it in this case

                _workflowMessageService.SendOrderPaidCustomerNotification(order, order.CustomerLanguageId,
                    null, null);

                _workflowMessageService.SendOrderPaidStoreOwnerNotification(order, _localizationSettings.DefaultAdminLanguageId);
            }
        }

        protected virtual void AddOrderNote(Order order, string note)
        {
            order.OrderNotes.Add(new OrderNote
            {
                Note = note,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            _orderService.UpdateOrder(order);
        }

        public virtual void CheckOrderStatus(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (order.PaymentStatus == Nop.Core.Domain.Payments.PaymentStatus.Paid && !order.PaidDateUtc.HasValue)
            {
                //ensure that paid date is set
                order.PaidDateUtc = DateTime.UtcNow;
                _orderService.UpdateOrder(order);
            }

            switch (order.OrderStatus)
            {
                case OrderStatus.Pending:
                    if (order.PaymentStatus == Nop.Core.Domain.Payments.PaymentStatus.Authorized ||
                        order.PaymentStatus == Nop.Core.Domain.Payments.PaymentStatus.Paid)
                    {
                        SetOrderStatus(order, OrderStatus.Processing, false);
                    }

                    if (order.ShippingStatus == ShippingStatus.PartiallyShipped ||
                        order.ShippingStatus == ShippingStatus.Shipped ||
                        order.ShippingStatus == ShippingStatus.Delivered)
                    {
                        SetOrderStatus(order, OrderStatus.Processing, false);
                    }
                    break;
                //is order complete?
                case OrderStatus.Cancelled:
                case OrderStatus.Complete:
                    return;
            }

            if (order.PaymentStatus != Nop.Core.Domain.Payments.PaymentStatus.Paid)
                return;

            bool completed;

            if (order.ShippingStatus == ShippingStatus.ShippingNotRequired)
            {
                //shipping is not required
                completed = true;
            }
            else
            {
                //shipping is required
                if (_orderSettings.CompleteOrderWhenDelivered)
                {
                    completed = order.ShippingStatus == ShippingStatus.Delivered;
                }
                else
                {
                    completed = order.ShippingStatus == ShippingStatus.Shipped ||
                                order.ShippingStatus == ShippingStatus.Delivered;
                }
            }

            if (completed)
            {
                SetOrderStatus(order, OrderStatus.Complete, true);
            }
        }

        protected virtual void SetOrderStatus(Order order, OrderStatus os, bool notifyCustomer)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var prevOrderStatus = order.OrderStatus;
            if (prevOrderStatus == os)
                return;

            //set and save new order status
            order.OrderStatusId = (int)os;
            _orderService.UpdateOrder(order);

            //order notes, notifications
            AddOrderNote(order, $"Order status has been changed to {os.ToString()}");

            if (prevOrderStatus != OrderStatus.Complete &&
                os == OrderStatus.Complete
                && notifyCustomer)
            {
                //notification
                var orderCompletedCustomerNotificationQueuedEmailId = _workflowMessageService
                    .SendOrderCompletedCustomerNotification(order, order.CustomerLanguageId, null,
                    null);
                if (orderCompletedCustomerNotificationQueuedEmailId > 0)
                {
                    AddOrderNote(order, $"\"Order completed\" email (to customer) has been queued. Queued email identifier: {orderCompletedCustomerNotificationQueuedEmailId}.");
                }
            }

            if (prevOrderStatus != OrderStatus.Cancelled &&
                os == OrderStatus.Cancelled
                && notifyCustomer)
            {
                //notification
                var orderCancelledCustomerNotificationQueuedEmailId = _workflowMessageService.SendOrderCancelledCustomerNotification(order, order.CustomerLanguageId);
                if (orderCancelledCustomerNotificationQueuedEmailId > 0)
                {
                    AddOrderNote(order, $"\"Order cancelled\" email (to customer) has been queued. Queued email identifier: {orderCancelledCustomerNotificationQueuedEmailId}.");

                }
            }
        }

        #endregion
    }
}