using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MessageBird;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Data;
using Nop.Services;
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Events;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Services.Tasks;
using Nop.Services.Tax;
using Nop.Web.Extensions;
using Nop.Web.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.Security.Captcha;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Customer;

namespace Nop.Web.Controllers
{
    public partial class CustomerController : BasePublicController
    {
        #region Fields

        private readonly IDiscountService _discountService;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly IAuthenticationService _authenticationService;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly TaxSettings _taxSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ITaxService _taxService;
        private readonly CustomerSettings _customerSettings;
        private readonly AddressSettings _addressSettings;
        private readonly ForumSettings _forumSettings;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IOrderService _orderService;
        private readonly IPictureService _pictureService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ISmsVerificationService _smsVerificationService;
        private readonly IDbContext _dbContext;

        private readonly ISettingService _settingService;
        private readonly IShippingService _shippingService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ShippingSettings _shippingSettings;

        private readonly MediaSettings _mediaSettings;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly StoreInformationSettings _storeInformationSettings;

        private readonly IStateProvinceService _stateProvinceService;

        #endregion

        #region Ctor

        public CustomerController(
            IDiscountService discountService,
            IAddressModelFactory addressModelFactory,
            ICustomerModelFactory customerModelFactory,
            IAuthenticationService authenticationService,
            DateTimeSettings dateTimeSettings,
            TaxSettings taxSettings,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerService customerService,
            ICustomerAttributeParser customerAttributeParser,
            ICustomerAttributeService customerAttributeService,
            IGenericAttributeService genericAttributeService,
            ICustomerRegistrationService customerRegistrationService,
            ITaxService taxService,
            CustomerSettings customerSettings,
            AddressSettings addressSettings,
            ForumSettings forumSettings,
            IAddressService addressService,
            ICountryService countryService,
            IOrderService orderService,
            ISettingService settingService,
            IPictureService pictureService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IShoppingCartService shoppingCartService,
            IExternalAuthenticationService externalAuthenticationService,
            IWebHelper webHelper,
            ICustomerActivityService customerActivityService,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            IEventPublisher eventPublisher,
            IShippingService shippingService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IOrderTotalCalculationService orderTotalCalculationService,
            ShippingSettings shippingSettings,
            MediaSettings mediaSettings,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            CaptchaSettings captchaSettings,
            StoreInformationSettings storeInformationSettings,
            IStateProvinceService stateProvinceService,
            ISmsVerificationService smsVerificationService,
            IDbContext dbContext
            )
        {
            this._discountService = discountService;
            this._addressModelFactory = addressModelFactory;
            this._customerModelFactory = customerModelFactory;
            this._authenticationService = authenticationService;
            this._dateTimeSettings = dateTimeSettings;
            this._taxSettings = taxSettings;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._customerService = customerService;
            this._customerAttributeParser = customerAttributeParser;
            this._customerAttributeService = customerAttributeService;
            this._genericAttributeService = genericAttributeService;
            this._customerRegistrationService = customerRegistrationService;
            this._taxService = taxService;
            this._customerSettings = customerSettings;
            this._addressSettings = addressSettings;
            this._forumSettings = forumSettings;
            this._addressService = addressService;
            this._countryService = countryService;
            this._orderService = orderService;
            this._pictureService = pictureService;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._shoppingCartService = shoppingCartService;
            this._externalAuthenticationService = externalAuthenticationService;
            this._webHelper = webHelper;
            this._customerActivityService = customerActivityService;
            this._addressAttributeParser = addressAttributeParser;
            this._addressAttributeService = addressAttributeService;
            this._eventPublisher = eventPublisher;
            this._shippingService = shippingService;
            this._currencyService = currencyService;
            this._priceFormatter = priceFormatter;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._shippingSettings = shippingSettings;
            this._mediaSettings = mediaSettings;
            this._workflowMessageService = workflowMessageService;
            this._localizationSettings = localizationSettings;
            this._captchaSettings = captchaSettings;
            this._storeInformationSettings = storeInformationSettings;
            this._settingService = settingService;
            this._stateProvinceService = stateProvinceService;
            this._smsVerificationService = smsVerificationService;
            this._dbContext = dbContext;
        }

        #endregion

        #region Utilities

        protected virtual string ParseCustomCustomerAttributes(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = "";
            var attributes = _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in attributes)
            {
                var controlId = $"customer_attribute_{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                )
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = _customerAttributeService.GetCustomerAttributeValues(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.FileUpload:
                    //not supported customer attributes
                    default:
                        break;
                }
            }

            return attributesXml;
        }

        #endregion

        #region Methods

        #region Login / logout

        [HttpsRequirement(SslRequirement.Yes)]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult Login(bool? checkoutAsGuest)
        {
            var model = _customerModelFactory.PrepareLoginModel(checkoutAsGuest);
            return View(model);
        }

        [HttpPost]
        [ValidateCaptcha]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult Login(LoginModel model, string returnUrl, bool captchaValid)
        {
            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            if (ModelState.IsValid)
            {
                if (_customerSettings.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }
                var loginResult = _customerRegistrationService.ValidateCustomer(_customerSettings.UsernamesEnabled ? model.Username : model.Email, model.Password);
                switch (loginResult)
                {
                    case CustomerLoginResults.Successful:
                        {
                            var customer = _customerSettings.UsernamesEnabled
                                ? _customerService.GetCustomerByUsername(model.Username)
                                : _customerService.GetCustomerByEmail(model.Email);

                            var productsInSesion = customer.ShoppingCartItems.ToList();

                            //migrate shopping cart
                            _shoppingCartService.MigrateShoppingCart(_workContext.CurrentCustomer, customer, true);

                            //sign in new customer
                            _authenticationService.SignIn(customer, model.RememberMe);

                            //raise event       
                            _eventPublisher.Publish(new CustomerLoggedinEvent(customer));

                            //activity log
                            _customerActivityService.InsertActivity(customer, "PublicStore.Login", _localizationService.GetResource("ActivityLog.PublicStore.Login"));

                            if (Nop.Services.TeedCommerceStores.CurrentStore == Nop.Services.TeedStores.CentralEnLinea)
                            {
                                if (productsInSesion.Count() > 0)
                                {
                                    var cartInSesionIds = productsInSesion.Select(x => x.Id).ToList();
                                    var array = cartInSesionIds.ToArray();
                                    return RedirectToAction("MergeCart", "ShoppingCart", new { cartInSesionIds = array });
                                }
                            }


                            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                                return RedirectToRoute("HomePage");

                            return Redirect(returnUrl);
                        }
                    case CustomerLoginResults.CustomerNotExist:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.CustomerNotExist"));
                        break;
                    case CustomerLoginResults.Deleted:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.Deleted"));
                        break;
                    case CustomerLoginResults.NotActive:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.NotActive"));
                        break;
                    case CustomerLoginResults.NotRegistered:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.NotRegistered"));
                        break;
                    case CustomerLoginResults.LockedOut:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.LockedOut"));
                        break;
                    case CustomerLoginResults.WrongPassword:
                    default:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials"));
                        break;
                }
            }

            //If we got this far, something failed, redisplay form
            model = _customerModelFactory.PrepareLoginModel(model.CheckoutAsGuest);
            return View(model);
        }

        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult Logout()
        {
            if (_workContext.OriginalCustomerIfImpersonated != null)
            {
                //activity log
                _customerActivityService.InsertActivity(_workContext.OriginalCustomerIfImpersonated,
                    "Impersonation.Finished",
                    _localizationService.GetResource("ActivityLog.Impersonation.Finished.StoreOwner"),
                    _workContext.CurrentCustomer.Email, _workContext.CurrentCustomer.Id);
                _customerActivityService.InsertActivity("Impersonation.Finished",
                    _localizationService.GetResource("ActivityLog.Impersonation.Finished.Customer"),
                    _workContext.OriginalCustomerIfImpersonated.Email, _workContext.OriginalCustomerIfImpersonated.Id);

                //logout impersonated customer
                _genericAttributeService.SaveAttribute<int?>(_workContext.OriginalCustomerIfImpersonated,
                    SystemCustomerAttributeNames.ImpersonatedCustomerId, null);

                //redirect back to customer details page (admin area)
                return this.RedirectToAction("Edit", "Customer",
                    new { id = _workContext.CurrentCustomer.Id, area = AreaNames.Admin });

            }

            //activity log
            _customerActivityService.InsertActivity("PublicStore.Logout", _localizationService.GetResource("ActivityLog.PublicStore.Logout"));

            //standard logout 
            _authenticationService.SignOut();

            //raise logged out event       
            _eventPublisher.Publish(new CustomerLoggedOutEvent(_workContext.CurrentCustomer));

            //EU Cookie
            if (_storeInformationSettings.DisplayEuCookieLawWarning)
            {
                //the cookie law message should not pop up immediately after logout.
                //otherwise, the user will have to click it again...
                //and thus next visitor will not click it... so violation for that cookie law..
                //the only good solution in this case is to store a temporary variable
                //indicating that the EU cookie popup window should not be displayed on the next page open (after logout redirection to homepage)
                //but it'll be displayed for further page loads
                TempData["nop.IgnoreEuCookieLawWarning"] = true;
            }

            return RedirectToRoute("HomePage");
        }

        #endregion

        #region Password recovery

        [HttpsRequirement(SslRequirement.Yes)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult PasswordRecovery()
        {
            var model = _customerModelFactory.PreparePasswordRecoveryModel();
            return View(model);
        }

        [HttpPost, ActionName("PasswordRecovery")]
        [PublicAntiForgery]
        [FormValueRequired("send-email")]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult PasswordRecoverySend(PasswordRecoveryModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = _customerService.GetCustomerByEmail(model.Email);
                if (customer != null && customer.Active && !customer.Deleted)
                {
                    //save token and current date
                    var passwordRecoveryToken = Guid.NewGuid();
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.PasswordRecoveryToken,
                        passwordRecoveryToken.ToString());
                    DateTime? generatedDateTime = DateTime.UtcNow;
                    _genericAttributeService.SaveAttribute(customer,
                        SystemCustomerAttributeNames.PasswordRecoveryTokenDateGenerated, generatedDateTime);

                    //send email
                    _workflowMessageService.SendCustomerPasswordRecoveryMessage(customer,
                        _workContext.WorkingLanguage.Id);

                    model.Result = _localizationService.GetResource("Account.PasswordRecovery.EmailHasBeenSent");
                }
                else
                {
                    model.Result = _localizationService.GetResource("Account.PasswordRecovery.EmailNotFound");
                }

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpsRequirement(SslRequirement.Yes)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult PasswordRecoveryConfirm(string token, string email)
        {
            var customer = _customerService.GetCustomerByEmail(email);
            if (customer == null)
                return RedirectToRoute("HomePage");

            if (string.IsNullOrEmpty(customer.GetAttribute<string>(SystemCustomerAttributeNames.PasswordRecoveryToken)))
            {
                return View(new PasswordRecoveryConfirmModel
                {
                    DisablePasswordChanging = true,
                    Result = _localizationService.GetResource("Account.PasswordRecovery.PasswordAlreadyHasBeenChanged")
                });
            }

            var model = _customerModelFactory.PreparePasswordRecoveryConfirmModel();

            //validate token
            if (!customer.IsPasswordRecoveryTokenValid(token))
            {
                model.DisablePasswordChanging = true;
                model.Result = _localizationService.GetResource("Account.PasswordRecovery.WrongToken");
            }

            //validate token expiration date
            if (customer.IsPasswordRecoveryLinkExpired(_customerSettings))
            {
                model.DisablePasswordChanging = true;
                model.Result = _localizationService.GetResource("Account.PasswordRecovery.LinkExpired");
            }

            return View(model);
        }

        [HttpPost, ActionName("PasswordRecoveryConfirm")]
        [PublicAntiForgery]
        [FormValueRequired("set-password")]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult PasswordRecoveryConfirmPOST(string token, string email, PasswordRecoveryConfirmModel model)
        {
            var customer = _customerService.GetCustomerByEmail(email);
            if (customer == null)
                return RedirectToRoute("HomePage");

            //validate token
            if (!customer.IsPasswordRecoveryTokenValid(token))
            {
                model.DisablePasswordChanging = true;
                model.Result = _localizationService.GetResource("Account.PasswordRecovery.WrongToken");
                return View(model);
            }

            //validate token expiration date
            if (customer.IsPasswordRecoveryLinkExpired(_customerSettings))
            {
                model.DisablePasswordChanging = true;
                model.Result = _localizationService.GetResource("Account.PasswordRecovery.LinkExpired");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var response = _customerRegistrationService.ChangePassword(new ChangePasswordRequest(email,
                    false, _customerSettings.DefaultPasswordFormat, model.NewPassword));
                if (response.Success)
                {
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.PasswordRecoveryToken, "");

                    model.DisablePasswordChanging = true;
                    model.Result = _localizationService.GetResource("Account.PasswordRecovery.PasswordHasBeenChanged");
                }
                else
                {
                    model.Result = response.Errors.FirstOrDefault();
                }

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion     

        #region Register

        [HttpsRequirement(SslRequirement.Yes)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult Register()
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

            var model = new RegisterModel();
            model = _customerModelFactory.PrepareRegisterModel(model, false, setDefaultValues: true);

            return View(model);
        }

        [HttpPost]
        [ValidateCaptcha]
        [ValidateHoneypot]
        [PublicAntiForgery]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult Register(RegisterModel model, string returnUrl, bool captchaValid)
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

            if (_workContext.CurrentCustomer.IsRegistered())
            {
                //Already registered customer. 
                _authenticationService.SignOut();

                //raise logged out event       
                _eventPublisher.Publish(new CustomerLoggedOutEvent(_workContext.CurrentCustomer));

                //Save a new record
                _workContext.CurrentCustomer = _customerService.InsertGuestCustomer();
            }
            var customer = _workContext.CurrentCustomer;
            customer.RegisteredInStoreId = _storeContext.CurrentStore.Id;

            //custom customer attributes
            var customerAttributesXml = ParseCustomCustomerAttributes(model.Form);
            var customerAttributeWarnings = _customerAttributeParser.GetAttributeWarnings(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            if (ModelState.IsValid)
            {
                if (_customerSettings.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }

                var isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
                var registrationRequest = new CustomerRegistrationRequest(customer,
                    model.Email,
                    _customerSettings.UsernamesEnabled ? model.Username : model.Email,
                    model.Password,
                    _customerSettings.DefaultPasswordFormat,
                    _storeContext.CurrentStore.Id,
                    isApproved);
                var registrationResult = _customerRegistrationService.RegisterCustomer(registrationRequest);
                if (registrationResult.Success)
                {
                    var continueCreation = true;
                    // SMS Verification
                    var smsSettings = _settingService.LoadSetting<SmsVerificationSettings>();
                    if (smsSettings.IsActive)
                    {
                        var smsVerification = _smsVerificationService.GetSmsVerificationByCustomerEmail(customer.Email);
                        if (smsVerification == null)
                        {
                            registrationResult.Errors.Add("Verificación por SMS no encontrada, necesaria para crear nuevo cliente.");
                            continueCreation = false;
                        }
                        else
                        {
                            smsVerification.CustomerId = customer.Id;
                            smsVerification.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El código se verificó con éxito por el nuevo usuario \"{string.Join(" ", new { model.FirstName, model.LastName })}\" con mail ingresado de registro \"{model.Email}\".\n";
                            _smsVerificationService.UpdateSmsVerification(smsVerification);
                        }
                    }

                    if (continueCreation)
                    {
                        //properties
                        if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                        {
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.TimeZoneId, model.TimeZoneId);
                        }
                        //VAT number
                        if (_taxSettings.EuVatEnabled)
                        {
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumber, model.VatNumber);

                            var vatNumberStatus = _taxService.GetVatNumberStatus(model.VatNumber, out string _, out string vatAddress);
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumberStatusId, (int)vatNumberStatus);
                            //send VAT number admin notification
                            if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                                _workflowMessageService.SendNewVatSubmittedStoreOwnerNotification(customer, model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                        }

                        //form fields
                        if (_customerSettings.GenderEnabled)
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, model.FirstName);
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, model.LastName);
                        if (_customerSettings.DateOfBirthEnabled)
                        {
                            var dateOfBirth = model.ParseDateOfBirth();
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, dateOfBirth);
                        }
                        if (_customerSettings.CompanyEnabled)
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Company, model.Company);
                        if (_customerSettings.StreetAddressEnabled)
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress, model.StreetAddress);
                        if (_customerSettings.StreetAddress2Enabled)
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress2, model.StreetAddress2);
                        if (_customerSettings.ZipPostalCodeEnabled)
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZipPostalCode, model.ZipPostalCode);
                        if (_customerSettings.CityEnabled)
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.City, model.City);
                        if (_customerSettings.CountryEnabled)
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CountryId, model.CountryId);
                        if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StateProvinceId,
                                model.StateProvinceId);
                        if (_customerSettings.PhoneEnabled)
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, model.Phone);
                        if (_customerSettings.FaxEnabled)
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Fax, model.Fax);

                        //newsletter
                        if (_customerSettings.NewsletterEnabled)
                        {
                            //save newsletter value
                            var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(model.Email, _storeContext.CurrentStore.Id);
                            if (newsletter != null)
                            {
                                if (model.Newsletter)
                                {
                                    newsletter.Active = true;
                                    _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);
                                }
                                //else
                                //{
                                //When registering, not checking the newsletter check box should not take an existing email address off of the subscription list.
                                //_newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                                //}
                            }
                            else
                            {
                                if (model.Newsletter)
                                {
                                    _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                                    {
                                        NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                        Email = model.Email,
                                        Active = true,
                                        StoreId = _storeContext.CurrentStore.Id,
                                        CreatedOnUtc = DateTime.UtcNow
                                    });
                                }
                            }
                        }

                        //save customer attributes
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CustomCustomerAttributes, customerAttributesXml);

                        //login customer now
                        if (isApproved)
                            _authenticationService.SignIn(customer, true);

                        //insert default address (if possible)
                        var defaultAddress = new Address
                        {
                            FirstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                            LastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName),
                            Email = customer.Email,
                            Company = customer.GetAttribute<string>(SystemCustomerAttributeNames.Company),
                            CountryId = customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId) > 0
                                ? (int?)customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId)
                                : null,
                            StateProvinceId = customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId) > 0
                                ? (int?)customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId)
                                : null,
                            City = customer.GetAttribute<string>(SystemCustomerAttributeNames.City),
                            Address1 = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress),
                            Address2 = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2),
                            ZipPostalCode = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode),
                            PhoneNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                            FaxNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.Fax),
                            CreatedOnUtc = customer.CreatedOnUtc
                        };
                        if (this._addressService.IsAddressValid(defaultAddress))
                        {
                            //some validation
                            if (defaultAddress.CountryId == 0)
                                defaultAddress.CountryId = null;
                            if (defaultAddress.StateProvinceId == 0)
                                defaultAddress.StateProvinceId = null;
                            //set default address
                            customer.Addresses.Add(defaultAddress);
                            customer.BillingAddress = defaultAddress;
                            customer.ShippingAddress = defaultAddress;
                            _customerService.UpdateCustomer(customer);
                        }

                        //notifications
                        if (_customerSettings.NotifyNewCustomerRegistration)
                            _workflowMessageService.SendCustomerRegisteredNotificationMessage(customer,
                                _localizationSettings.DefaultAdminLanguageId);

                        //raise event       
                        _eventPublisher.Publish(new CustomerRegisteredEvent(customer));

                        switch (_customerSettings.UserRegistrationType)
                        {
                            case UserRegistrationType.EmailValidation:
                                {
                                    //email validation message
                                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AccountActivationToken, Guid.NewGuid().ToString());
                                    _workflowMessageService.SendCustomerEmailValidationMessage(customer, _workContext.WorkingLanguage.Id);

                                    //result
                                    return RedirectToRoute("RegisterResult",
                                        new { resultId = (int)UserRegistrationType.EmailValidation });
                                }
                            case UserRegistrationType.AdminApproval:
                                {
                                    return RedirectToRoute("RegisterResult",
                                        new { resultId = (int)UserRegistrationType.AdminApproval });
                                }
                            case UserRegistrationType.Standard:
                                {
                                    //send customer welcome message
                                    _workflowMessageService.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);

                                    var redirectUrl = Url.RouteUrl("RegisterResult", new { resultId = (int)UserRegistrationType.Standard });
                                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                                        redirectUrl = _webHelper.ModifyQueryString(redirectUrl, "returnurl=" + WebUtility.UrlEncode(returnUrl), null);
                                    return Redirect(redirectUrl);
                                }
                            default:
                                {
                                    return RedirectToRoute("HomePage");
                                }
                        }
                    }
                }

                //errors
                foreach (var error in registrationResult.Errors)
                    ModelState.AddModelError("", error);
            }

            //If we got this far, something failed, redisplay form
            model = _customerModelFactory.PrepareRegisterModel(model, true, customerAttributesXml);
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult RegisterAjax(string name, string lastName, string mail)
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

            if (_workContext.CurrentCustomer.IsRegistered())
            {
                //Already registered customer. 
                _authenticationService.SignOut();

                //raise logged out event       
                _eventPublisher.Publish(new CustomerLoggedOutEvent(_workContext.CurrentCustomer));

                //Save a new record
                _workContext.CurrentCustomer = _customerService.InsertGuestCustomer();
            }
            var customer = _workContext.CurrentCustomer;
            customer.RegisteredInStoreId = _storeContext.CurrentStore.Id;

            if (ModelState.IsValid)
            {
                var isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;

                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, name);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, lastName);

                var registrationRequest = new CustomerRegistrationRequest(customer,
                    mail,
                    _customerSettings.UsernamesEnabled ? name : mail,
                    Guid.NewGuid().ToString(),
                    _customerSettings.DefaultPasswordFormat,
                    _storeContext.CurrentStore.Id,
                    isApproved);
                var registrationResult = _customerRegistrationService.RegisterCustomer(registrationRequest);
                if (registrationResult.Success)
                {
                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        //save newsletter value
                        var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(mail, _storeContext.CurrentStore.Id);
                        if (newsletter != null)
                        {
                            newsletter.Active = true;
                        }
                        else
                        {
                            _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                            {
                                NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                Email = mail,
                                Active = true,
                                StoreId = _storeContext.CurrentStore.Id,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                        }
                    }

                    //login customer now
                    if (isApproved)
                        _authenticationService.SignIn(customer, true);

                    //notifications
                    if (_customerSettings.NotifyNewCustomerRegistration)
                        _workflowMessageService.SendCustomerRegisteredNotificationMessage(customer,
                            _localizationSettings.DefaultAdminLanguageId);

                    //raise event       
                    _eventPublisher.Publish(new CustomerRegisteredEvent(customer));

                    //save token and current date
                    var passwordRecoveryToken = Guid.NewGuid();
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.PasswordRecoveryToken,
                        passwordRecoveryToken.ToString());
                    DateTime? generatedDateTime = DateTime.UtcNow;
                    _genericAttributeService.SaveAttribute(customer,
                        SystemCustomerAttributeNames.PasswordRecoveryTokenDateGenerated, generatedDateTime);

                    var discounts = _discountService.GetAllDiscounts();

                    foreach (var item in discounts)
                    {
                        if (item.IsPopUpDiscount)
                        {
                            //send email
                            _workflowMessageService.SendCustomerWelcomeMessage(customer,
                                _workContext.WorkingLanguage.Id,
                                item.Id);

                            return Ok("success");
                        }
                    }
                }
            }

            return Ok("error");
        }

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult RegisterResult(int resultId)
        {
            var model = _customerModelFactory.PrepareRegisterResultModel(resultId);
            return View(model);
        }

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        [HttpPost]
        public virtual IActionResult RegisterResult(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                return RedirectToRoute("HomePage");

            return Redirect(returnUrl);
        }

        [HttpPost]
        [PublicAntiForgery]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult CheckUsernameAvailability(string username)
        {
            var usernameAvailable = false;
            var statusText = _localizationService.GetResource("Account.CheckUsernameAvailability.NotAvailable");

            if (_customerSettings.UsernamesEnabled && !string.IsNullOrWhiteSpace(username))
            {
                if (_workContext.CurrentCustomer != null &&
                    _workContext.CurrentCustomer.Username != null &&
                    _workContext.CurrentCustomer.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                {
                    statusText = _localizationService.GetResource("Account.CheckUsernameAvailability.CurrentUsername");
                }
                else
                {
                    var customer = _customerService.GetCustomerByUsername(username);
                    if (customer == null)
                    {
                        statusText = _localizationService.GetResource("Account.CheckUsernameAvailability.Available");
                        usernameAvailable = true;
                    }
                }
            }

            return Json(new { Available = usernameAvailable, Text = statusText });
        }

        [HttpsRequirement(SslRequirement.Yes)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult AccountActivation(string token, string email)
        {
            var customer = _customerService.GetCustomerByEmail(email);
            if (customer == null)
                return RedirectToRoute("HomePage");

            var cToken = customer.GetAttribute<string>(SystemCustomerAttributeNames.AccountActivationToken);
            if (string.IsNullOrEmpty(cToken))
                return
                    View(new AccountActivationModel
                    {
                        Result = _localizationService.GetResource("Account.AccountActivation.AlreadyActivated")
                    });

            if (!cToken.Equals(token, StringComparison.InvariantCultureIgnoreCase))
                return RedirectToRoute("HomePage");

            //activate user account
            customer.Active = true;
            _customerService.UpdateCustomer(customer);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AccountActivationToken, "");
            //send welcome message
            _workflowMessageService.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);

            var model = new AccountActivationModel
            {
                Result = _localizationService.GetResource("Account.AccountActivation.Activated")
            };
            return View(model);
        }

        #endregion

        #region My account / Info

        [HttpsRequirement(SslRequirement.Yes)]
        public virtual IActionResult Info()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = new CustomerInfoModel();
            model = _customerModelFactory.PrepareCustomerInfoModel(model, _workContext.CurrentCustomer, false);
            UploadOrGetProfilePicture(model, false);

            return View(model);
        }

        [HttpsRequirement(SslRequirement.Yes)]
        public virtual IActionResult ShareAndWin()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var growthHackingSettings = _settingService.LoadSetting<GrowthHackingSettings>();
            if (growthHackingSettings == null || !growthHackingSettings.IsActive) return NotFound();

            var customerDiscount = _discountService.GetAllDiscounts(showHidden: true).Where(x => x.CustomerOwnerId == _workContext.CurrentCustomer.Id).FirstOrDefault();
            ShareAndWinModel model = new ShareAndWinModel()
            {
                CustomerCode = customerDiscount?.CouponCode,
                CouponDesactivated = customerDiscount == null ? false : customerDiscount.EndDateUtc.HasValue,
                RewardAmount = growthHackingSettings.RewardAmount.ToString("C"),
                CouponValue = growthHackingSettings.UserCodeCouponAmount.ToString("C"),
                MinimumAmountToCreateFriendCode = growthHackingSettings.MinimumAmountToCreateFriendCode,
                UserCodeCouponOrderMinimumAmount = growthHackingSettings.UserCodeCouponOrderMinimumAmount
            };

            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual IActionResult Info(CustomerInfoModel model)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var customer = _workContext.CurrentCustomer;

            //custom customer attributes
            var customerAttributesXml = ParseCustomCustomerAttributes(model.Form);
            var customerAttributeWarnings = _customerAttributeParser.GetAttributeWarnings(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var continueCreation = true;
                    // SMS Verification
                    var smsSettings = _settingService.LoadSetting<SmsVerificationSettings>();
                    var originalPhoneNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                    if (!string.IsNullOrEmpty(model.Phone) && model.Phone != originalPhoneNumber && smsSettings.IsActive)
                    {
                        if (smsSettings.IsActive)
                        {
                            var smsVerification = _smsVerificationService.GetSmsVerificationByCustomerEmail(customer.Email);
                            if (smsVerification == null)
                            {
                                ModelState.AddModelError("", "Verificación por SMS no encontrada, necesaria para crear nuevo cliente.");
                                continueCreation = false;
                            }
                            else
                            {
                                smsVerification.CustomerId = customer.Id;
                                smsVerification.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El código se verificó con éxito por el nuevo usuario \"{string.Join(" ", new { model.FirstName, model.LastName })}\" con mail ingresado de registro \"{model.Email}\".\n";
                                _smsVerificationService.UpdateSmsVerification(smsVerification);
                            }
                        }
                    }

                    if (continueCreation)
                    {
                        //username 
                        if (_customerSettings.UsernamesEnabled && this._customerSettings.AllowUsersToChangeUsernames)
                        {
                            if (
                                !customer.Username.Equals(model.Username.Trim(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                //change username
                                _customerRegistrationService.SetUsername(customer, model.Username.Trim());

                                //re-authenticate
                                //do not authenticate users in impersonation mode
                                if (_workContext.OriginalCustomerIfImpersonated == null)
                                    _authenticationService.SignIn(customer, true);
                            }
                        }
                        //email
                        if (!customer.Email.Equals(model.Email.Trim(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            //change email
                            var requireValidation = _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation;
                            _customerRegistrationService.SetEmail(customer, model.Email.Trim(), requireValidation);

                            //do not authenticate users in impersonation mode
                            if (_workContext.OriginalCustomerIfImpersonated == null)
                            {
                                //re-authenticate (if usernames are disabled)
                                if (!_customerSettings.UsernamesEnabled && !requireValidation)
                                    _authenticationService.SignIn(customer, true);
                            }
                        }

                        //properties
                        if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                        {
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.TimeZoneId,
                                model.TimeZoneId);
                        }
                        //VAT number
                        if (_taxSettings.EuVatEnabled)
                        {
                            var prevVatNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);

                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumber,
                                model.VatNumber);
                            if (prevVatNumber != model.VatNumber)
                            {
                                var vatNumberStatus = _taxService.GetVatNumberStatus(model.VatNumber, out string _, out string vatAddress);
                                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumberStatusId, (int)vatNumberStatus);
                                //send VAT number admin notification
                                if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                                    _workflowMessageService.SendNewVatSubmittedStoreOwnerNotification(customer,
                                        model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                            }
                        }

                        //form fields
                        if (_customerSettings.GenderEnabled)
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, model.FirstName);
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, model.LastName);
                        if (_customerSettings.DateOfBirthEnabled)
                        {
                            var dateOfBirth = model.ParseDateOfBirth();
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, dateOfBirth);
                        }
                        if (_customerSettings.CompanyEnabled)
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Company, model.Company);
                        if (_customerSettings.StreetAddressEnabled)
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress, model.StreetAddress);
                        if (_customerSettings.StreetAddress2Enabled)
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress2, model.StreetAddress2);
                        if (_customerSettings.ZipPostalCodeEnabled)
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZipPostalCode, model.ZipPostalCode);
                        if (_customerSettings.CityEnabled)
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.City, model.City);
                        if (_customerSettings.CountryEnabled)
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CountryId, model.CountryId);
                        if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StateProvinceId, model.StateProvinceId);
                        if (_customerSettings.PhoneEnabled)
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, model.Phone);
                        if (_customerSettings.FaxEnabled)
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Fax, model.Fax);

                        //newsletter
                        if (_customerSettings.NewsletterEnabled)
                        {
                            //save newsletter value
                            var newsletter =
                                _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, _storeContext.CurrentStore.Id);
                            if (newsletter != null)
                            {
                                if (model.Newsletter)
                                {
                                    newsletter.Active = true;
                                    _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);
                                }
                                else
                                    _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                            }
                            else
                            {
                                if (model.Newsletter)
                                {
                                    _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                                    {
                                        NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                        Email = customer.Email,
                                        Active = true,
                                        StoreId = _storeContext.CurrentStore.Id,
                                        CreatedOnUtc = DateTime.UtcNow
                                    });
                                }
                            }
                        }

                        if (_forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled)
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Signature, model.Signature);

                        //save customer attributes
                        _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                            SystemCustomerAttributeNames.CustomCustomerAttributes, customerAttributesXml);

                        //profile picture
                        UploadOrGetProfilePicture(model, true);

                        return RedirectToRoute("CustomerInfo");
                    }
                }
            }
            catch (Exception exc)
            {
                ModelState.AddModelError("", exc.Message);
            }

            //If we got this far, something failed, redisplay form
            model = _customerModelFactory.PrepareCustomerInfoModel(model, customer, true, customerAttributesXml);
            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual IActionResult RemoveExternalAssociation(int id)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            //ensure it's our record
            var ear = _workContext.CurrentCustomer.ExternalAuthenticationRecords.FirstOrDefault(x => x.Id == id);

            if (ear == null)
            {
                return Json(new
                {
                    redirect = Url.Action("Info"),
                });
            }

            _externalAuthenticationService.DeleteExternalAuthenticationRecord(ear);

            return Json(new
            {
                redirect = Url.Action("Info"),
            });
        }

        [HttpsRequirement(SslRequirement.Yes)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult EmailRevalidation(string token, string email)
        {
            var customer = _customerService.GetCustomerByEmail(email);
            if (customer == null)
                return RedirectToRoute("HomePage");

            var cToken = customer.GetAttribute<string>(SystemCustomerAttributeNames.EmailRevalidationToken);
            if (string.IsNullOrEmpty(cToken))
                return View(new EmailRevalidationModel
                {
                    Result = _localizationService.GetResource("Account.EmailRevalidation.AlreadyChanged")
                });

            if (!cToken.Equals(token, StringComparison.InvariantCultureIgnoreCase))
                return RedirectToRoute("HomePage");

            if (string.IsNullOrEmpty(customer.EmailToRevalidate))
                return RedirectToRoute("HomePage");

            if (_customerSettings.UserRegistrationType != UserRegistrationType.EmailValidation)
                return RedirectToRoute("HomePage");

            //change email
            try
            {
                _customerRegistrationService.SetEmail(customer, customer.EmailToRevalidate, false);
            }
            catch (Exception exc)
            {
                return View(new EmailRevalidationModel
                {
                    Result = _localizationService.GetResource(exc.Message)
                });
            }
            customer.EmailToRevalidate = null;
            _customerService.UpdateCustomer(customer);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.EmailRevalidationToken, "");

            //re-authenticate (if usernames are disabled)
            if (!_customerSettings.UsernamesEnabled)
            {
                _authenticationService.SignIn(customer, true);
            }

            var model = new EmailRevalidationModel()
            {
                Result = _localizationService.GetResource("Account.EmailRevalidation.Changed")
            };
            return View(model);
        }

        #endregion

        #region My account / Rewards

        //My account / My Points
        [HttpsRequirement(SslRequirement.Yes)]
        public virtual IActionResult CustomerPoints(int months = 1)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = _customerModelFactory.PreparePointsListModel(monthsFilteringAmount: months);
            return View(model);
        }

        //My account / My Balance
        [HttpsRequirement(SslRequirement.Yes)]
        public virtual IActionResult CustomerBalance(int months = 1)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = _customerModelFactory.PrepareBalanceListModel(monthsFilteringAmount: months);
            return View(model);
        }

        //My account / Leaderboards
        [HttpsRequirement(SslRequirement.Yes)]
        public virtual IActionResult Leaderboards()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = _customerModelFactory.PrepareLeaderboardsListModel();
            return View(model);
        }

        //My account / My Badges
        [HttpsRequirement(SslRequirement.Yes)]
        public virtual IActionResult CustomerBadges()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = _customerModelFactory.PrepareBadgesListModel();
            return View(model);
        }

        #endregion

        #region My account / Addresses

        [HttpsRequirement(SslRequirement.Yes)]
        public virtual IActionResult Addresses()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = _customerModelFactory.PrepareCustomerAddressListModel();
            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        [HttpsRequirement(SslRequirement.Yes)]
        public virtual IActionResult AddressDelete(int addressId)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var customer = _workContext.CurrentCustomer;

            //find address (ensure that it belongs to the current customer)
            var address = customer.Addresses.FirstOrDefault(a => a.Id == addressId);
            if (address != null)
            {
                customer.RemoveAddress(address);
                _customerService.UpdateCustomer(customer);
                //now delete the address record
                _addressService.DeleteAddress(address);

                // ActivityLog.DeleteAddress
                if (TeedCommerceStores.CurrentStore == TeedStores.CentralEnLinea)
                    _customerActivityService.InsertActivity(customer, "DeleteAddress", _localizationService.GetResource("ActivityLog.DeleteAddress"),
                    new string[] { $"{customer.Email} ({customer.Id})", address.Id.ToString(), address.JoinAddress() });
            }

            //redirect to the address list page
            return Json(new
            {
                redirect = Url.RouteUrl("CustomerAddresses"),
            });
        }

        [HttpsRequirement(SslRequirement.Yes)]
        public async virtual Task<IActionResult> AddressAdd()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = new CustomerAddressEditModel();
            _addressModelFactory.PrepareAddressModel(model.Address,
                address: null,
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id));

            if (TeedCommerceStores.CurrentStore == TeedStores.CentralEnLinea)
            {
                using (HttpClient client = new HttpClient())
                {
                    //var url = "https://localhost:44345/ShippingArea/GetValidPostalCodes";
                    var url = "https://www.centralenlinea.com/ShippingArea/GetValidPostalCodes";
                    var result = await client.GetAsync(url);
                    if (result.IsSuccessStatusCode)
                    {
                        model.ValidPostalCodes = await result.Content.ReadAsStringAsync();
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual IActionResult AddressAdd(CustomerAddressEditModel model)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var customer = _workContext.CurrentCustomer;

            //custom address attributes
            var customAttributes = model.Form.ParseCustomAddressAttributes(_addressAttributeParser, _addressAttributeService);
            var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                var address = model.Address.ToEntity();
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;
                customer.Addresses.Add(address);
                _customerService.UpdateCustomer(customer);

                // ActivityLog.AddNewAddress
                if (TeedCommerceStores.CurrentStore == TeedStores.CentralEnLinea)
                    _customerActivityService.InsertActivity(customer, "AddNewAddress", _localizationService.GetResource("ActivityLog.AddNewAddress"),
                    new string[] { $"{customer.Email} ({customer.Id})", address.Id.ToString(), address.JoinAddress() });

                return RedirectToRoute("CustomerAddresses");
            }

            //If we got this far, something failed, redisplay form
            _addressModelFactory.PrepareAddressModel(model.Address,
                address: null,
                excludeProperties: true,
                addressSettings: _addressSettings,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id),
                overrideAttributesXml: customAttributes);

            return View(model);
        }

        [HttpsRequirement(SslRequirement.Yes)]
        public virtual IActionResult AddressEdit(int addressId)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var customer = _workContext.CurrentCustomer;
            //find address (ensure that it belongs to the current customer)
            var address = customer.Addresses.FirstOrDefault(a => a.Id == addressId);
            if (address == null)
                //address is not found
                return RedirectToRoute("CustomerAddresses");

            var model = new CustomerAddressEditModel();
            _addressModelFactory.PrepareAddressModel(model.Address,
                address: address,
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id));

            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual IActionResult AddressEdit(CustomerAddressEditModel model, int addressId)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var customer = _workContext.CurrentCustomer;
            //find address (ensure that it belongs to the current customer)
            var address = customer.Addresses.FirstOrDefault(a => a.Id == addressId);
            if (address == null)
                //address is not found
                return RedirectToRoute("CustomerAddresses");

            //custom address attributes
            var customAttributes = model.Form.ParseCustomAddressAttributes(_addressAttributeParser, _addressAttributeService);
            var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                address = model.Address.ToEntity(address);
                address.CustomAttributes = customAttributes;
                _addressService.UpdateAddress(address);

                // ActivityLog.EditAddress
                if (TeedCommerceStores.CurrentStore == TeedStores.CentralEnLinea)
                    _customerActivityService.InsertActivity(customer, "EditAddress", _localizationService.GetResource("ActivityLog.EditAddress"),
                    new string[] { $"{customer.Email} ({customer.Id})", address.Id.ToString(), address.JoinAddress() });

                return RedirectToRoute("CustomerAddresses");
            }

            //If we got this far, something failed, redisplay form
            _addressModelFactory.PrepareAddressModel(model.Address,
                address: address,
                excludeProperties: true,
                addressSettings: _addressSettings,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id),
                overrideAttributesXml: customAttributes);
            return View(model);
        }


        [HttpPost]
        public virtual IActionResult SaveNewAddress(string firstName,
          string lastName,
          string email,
          string company,
          string countryId,
          string stateId,
          string city,
          string address1,
          string address2,
          string zcp,
          string phone,
          string attrAddress,
          bool pickUpInStore = false,
          string pickUpPoint = null,
          string longitude = null,
          string latitude = null)
        {
            try
            {
                if (countryId == null || stateId == null)
                {
                    countryId = _countryService.GetCountryByTwoLetterIsoCode("MX").Id.ToString();
                    if (address1.Contains("EDMX") || address1.Contains("State of Mexico") || address1.Contains("Estado de México"))
                        stateId = _stateProvinceService.GetStateProvinceByAbbreviation("MEX", Convert.ToInt32(countryId)).Id.ToString();
                    else
                        stateId = _stateProvinceService.GetStateProvinceByAbbreviation("CMX", Convert.ToInt32(countryId)).Id.ToString();
                }

                //new address
                var newAddress = new CheckoutBillingAddressModel(); //model.BillingNewAddress;
                newAddress.BillingNewAddress.Address1 = address1;
                newAddress.BillingNewAddress.Address2 = address2;
                newAddress.BillingNewAddress.City = city;
                newAddress.BillingNewAddress.Company = company;
                newAddress.BillingNewAddress.CountryId = Convert.ToInt32(countryId);
                newAddress.BillingNewAddress.FirstName = string.IsNullOrWhiteSpace(firstName) ? _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName) : firstName;
                newAddress.BillingNewAddress.LastName = string.IsNullOrWhiteSpace(lastName) ? _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.LastName) : lastName;
                newAddress.BillingNewAddress.PhoneNumber = string.IsNullOrWhiteSpace(phone) ? _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.Phone) : phone; ;
                newAddress.BillingNewAddress.StateProvinceId = Convert.ToInt32(stateId);
                newAddress.BillingNewAddress.ZipPostalCode = zcp;
                newAddress.BillingNewAddress.Email = string.IsNullOrWhiteSpace(email) ? _workContext.CurrentCustomer.Email : email;
                newAddress.ShipToSameAddress = false;
                newAddress.ShipToSameAddressAllowed = true;

                //custom address attributes
                string attr = "";
                if (!string.IsNullOrEmpty(attrAddress) && !attrAddress.Contains("<Attributes>"))
                {
                    attr = "<Attributes><AddressAttribute ID=\"1\"><AddressAttributeValue><Value>" + attrAddress + "</Value></AddressAttributeValue></AddressAttribute></Attributes>";
                }
                else if (!string.IsNullOrEmpty(attrAddress) && attrAddress.Contains("<Attributes>"))
                {
                    attr = attrAddress;
                }

                var customAttributes = attr;//model.Form.ParseCustomAddressAttributes(_addressAttributeParser, _addressAttributeService);
                var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
                foreach (var error in customAttributeWarnings)
                {
                    ModelState.AddModelError("", error);
                }


                //try to find an address with the same values (don't duplicate records)
                var address = _workContext.CurrentCustomer.Addresses.ToList().FindAddress(
                    newAddress.BillingNewAddress.FirstName, newAddress.BillingNewAddress.LastName, newAddress.BillingNewAddress.PhoneNumber,
                    newAddress.BillingNewAddress.Email, newAddress.BillingNewAddress.FaxNumber, newAddress.BillingNewAddress.Company,
                    newAddress.BillingNewAddress.Address1, newAddress.BillingNewAddress.Address2, newAddress.BillingNewAddress.City,
                    newAddress.BillingNewAddress.StateProvinceId, newAddress.BillingNewAddress.ZipPostalCode,
                    newAddress.BillingNewAddress.CountryId, customAttributes);
                if (address == null)
                {
                    //address is not found. let's create a new one
                    address = newAddress.BillingNewAddress.ToEntity();
                    address.CustomAttributes = customAttributes;
                    address.CreatedOnUtc = DateTime.UtcNow;
                    //some validation
                    if (address.CountryId == 0)
                        address.CountryId = null;
                    if (address.StateProvinceId == 0)
                        address.StateProvinceId = null;
                    if (address.CountryId.HasValue && address.CountryId.Value > 0)
                    {
                        address.Country = _countryService.GetCountryById(address.CountryId.Value);
                    }
                    _workContext.CurrentCustomer.Addresses.Add(address);
                }

                // Update latitude and longitude for address if empty
                if (!string.IsNullOrEmpty(latitude) && string.IsNullOrEmpty(address.Latitude))
                    address.Latitude = latitude;
                if (!string.IsNullOrEmpty(longitude) && string.IsNullOrEmpty(address.Longitude))
                    address.Longitude = longitude;
                _addressService.UpdateAddress(address);

                _workContext.CurrentCustomer.BillingAddress = address;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                var test = _webHelper.GetStoreLocation();
                var url = (Request.IsHttps ? "https" : "http") + $"://{Request.Host}/customer/addresses";
                return Json(new
                {
                    redirect = url
                }); ;
            }
            catch (Exception exc)
            {
                return BadRequest();
            }
        }


        #endregion

        #region My account / Downloadable products

        [HttpsRequirement(SslRequirement.Yes)]
        public virtual IActionResult DownloadableProducts()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (_customerSettings.HideDownloadableProductsTab)
                return RedirectToRoute("CustomerInfo");

            var model = _customerModelFactory.PrepareCustomerDownloadableProductsModel();
            return View(model);
        }

        public virtual IActionResult UserAgreement(Guid orderItemId)
        {
            var orderItem = _orderService.GetOrderItemByGuid(orderItemId);
            if (orderItem == null)
                return RedirectToRoute("HomePage");

            var product = orderItem.Product;
            if (product == null || !product.HasUserAgreement)
                return RedirectToRoute("HomePage");

            var model = _customerModelFactory.PrepareUserAgreementModel(orderItem, product);
            return View(model);
        }

        #endregion

        #region My account / Change password

        [HttpsRequirement(SslRequirement.Yes)]
        public virtual IActionResult ChangePassword()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = _customerModelFactory.PrepareChangePasswordModel();

            //display the cause of the change password 
            if (_workContext.CurrentCustomer.PasswordIsExpired())
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Account.ChangePassword.PasswordIsExpired"));

            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual IActionResult ChangePassword(ChangePasswordModel model)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var customer = _workContext.CurrentCustomer;

            if (ModelState.IsValid)
            {
                var changePasswordRequest = new ChangePasswordRequest(customer.Email,
                    true, _customerSettings.DefaultPasswordFormat, model.NewPassword, model.OldPassword);
                var changePasswordResult = _customerRegistrationService.ChangePassword(changePasswordRequest);
                if (changePasswordResult.Success)
                {
                    model.Result = _localizationService.GetResource("Account.ChangePassword.Success");
                    return View(model);
                }

                //errors
                foreach (var error in changePasswordResult.Errors)
                    ModelState.AddModelError("", error);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region My account / Avatar

        [HttpsRequirement(SslRequirement.Yes)]
        public virtual IActionResult Avatar()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return RedirectToRoute("CustomerInfo");

            var model = new CustomerAvatarModel();
            model = _customerModelFactory.PrepareCustomerAvatarModel(model);
            return View(model);
        }

        [HttpPost, ActionName("Avatar")]
        [PublicAntiForgery]
        [FormValueRequired("upload-avatar")]
        public virtual IActionResult UploadAvatar(CustomerAvatarModel model, IFormFile uploadedFile)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return RedirectToRoute("CustomerInfo");

            var customer = _workContext.CurrentCustomer;

            if (ModelState.IsValid)
            {
                try
                {
                    var customerAvatar = _pictureService.GetPictureById(customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId));
                    if (uploadedFile != null && !string.IsNullOrEmpty(uploadedFile.FileName))
                    {
                        var avatarMaxSize = _customerSettings.AvatarMaximumSizeBytes;
                        if (uploadedFile.Length > avatarMaxSize)
                            throw new NopException(string.Format(_localizationService.GetResource("Account.Avatar.MaximumUploadedFileSize"), avatarMaxSize));

                        var customerPictureBinary = uploadedFile.GetPictureBits();
                        if (customerAvatar != null)
                            customerAvatar = _pictureService.UpdatePicture(customerAvatar.Id, customerPictureBinary, uploadedFile.ContentType, null);
                        else
                            customerAvatar = _pictureService.InsertPicture(customerPictureBinary, uploadedFile.ContentType, null);
                    }

                    var customerAvatarId = 0;
                    if (customerAvatar != null)
                        customerAvatarId = customerAvatar.Id;

                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AvatarPictureId, customerAvatarId);

                    model.AvatarUrl = _pictureService.GetPictureUrl(
                        customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId),
                        _mediaSettings.AvatarPictureSize,
                        false);
                    return View(model);
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError("", exc.Message);
                }
            }

            //If we got this far, something failed, redisplay form
            model = _customerModelFactory.PrepareCustomerAvatarModel(model);
            return View(model);
        }

        [HttpPost, ActionName("Avatar")]
        [PublicAntiForgery]
        [FormValueRequired("remove-avatar")]
        public virtual IActionResult RemoveAvatar(CustomerAvatarModel model)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return RedirectToRoute("CustomerInfo");

            var customer = _workContext.CurrentCustomer;

            var customerAvatar = _pictureService.GetPictureById(customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId));
            if (customerAvatar != null)
                _pictureService.DeletePicture(customerAvatar);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AvatarPictureId, 0);

            return RedirectToRoute("CustomerAvatar");
        }

        public void UploadOrGetProfilePicture(CustomerInfoModel model, bool isUpload)
        {
            model.DefaultPicture = _pictureService.GetDefaultPictureUrl(100);
            if (isUpload)
            {
                if (model.ProfilePictureId > 0)
                {
                    var customer = _customerService.GetCustomerById(_workContext.CurrentCustomer.Id);
                    if (customer != null)
                    {
                        customer.ProfilePictureId = model.ProfilePictureId;
                        _customerService.UpdateCustomer(customer);
                    }
                }
            }
            else
            {
                var customer = _customerService.GetCustomerById(_workContext.CurrentCustomer.Id);
                if (customer != null)
                {
                    model.ProfilePicture = _pictureService.GetPictureUrl(customer.ProfilePictureId ?? 0);
                    model.ProfilePictureId = customer.ProfilePictureId ?? 0;
                }
            }
        }

        #region My account / Share code

        [HttpPost]
        public IActionResult UpdateCode(string newCode)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Unauthorized();

            if (string.IsNullOrEmpty(newCode))
                return BadRequest("El código no debe estar vacío.");

            newCode = newCode.Trim().ToUpper();

            if (newCode.Count() > 12)
                return BadRequest("El código no puede tener más de 12 caracteres.");

            if (newCode.Count() < 5)
                return BadRequest("El código debe tener al menos 5 caracteres.");

            if (!Regex.IsMatch(newCode, @"^[a-zA-Z0-9]+$"))
                return BadRequest("El código solo puede tener caracteres alfanuméricos.");

            var discountWithSameCode = _discountService.GetAllDiscounts(couponCode: newCode).Any();
            if (discountWithSameCode)
                return BadRequest("El código ya está siendo utilizado, por favor, ingresa otro.");

            var discount = _discountService.GetAllDiscounts().Where(x => x.CustomerOwnerId == _workContext.CurrentCustomer.Id).FirstOrDefault();
            if (discount == null) return BadRequest("Ocurrió un problema al intentar actualizar tu código. Por favor, inténtalo de nuevo más tarde.");

            discount.CouponCode = newCode;
            _discountService.UpdateDiscount(discount);

            return NoContent();
        }

        #endregion

        #endregion

        #region Sms Verificaiton

        [HttpPost]
        [AllowAnonymous]
        public IActionResult SmsVerification(RegisterModel model)
        {
            var number = model.Phone.Trim();
            var canBeRegistered = _customerService.GetCustomerByEmail(model.Email.Trim()) == null;
            if (canBeRegistered || model.VerifyOnlyNumber)
            {
                if (!string.IsNullOrEmpty(number))
                {
                    Regex rg = new Regex(@"^\d{10}$");
                    if (IsDigitsOnly(number) && rg.Matches(number).Count > 0)
                    {
                        var alreadyValidate = _smsVerificationService.GetSmsVerificationsByPhoneNumberAndCode(number)
                            .Where(x => x.IsVerified).ToList();

                        string numberExistsInDb =
                            @"SELECT TOP (1) [Value] 
	                        FROM [dbo].[GenericAttribute] 
	                        where KeyGroup = 'Customer' 
	                        AND [Key] = 'Phone'
	                        AND [Value] = '" + number + "'";
                        string numberExists = _dbContext.SqlQuery<string>(numberExistsInDb).FirstOrDefault();
                        if (!alreadyValidate.Any() && string.IsNullOrEmpty(numberExists))
                            using (HttpClient httpClient = new HttpClient())
                            {
                                var url =
                                    _storeContext.CurrentStore.SecureUrl
                                    //"https://localhost:44387/"
                                    ;
                                var branchResult = httpClient.GetAsync(url + "/MessageBird/SetCurrentKeysForNopWeb").Result;
                                var settings = _settingService.LoadSetting<SmsVerificationSettings>();
                                if (settings.IsActive)
                                {
                                    var key = settings.IsSandbox ? settings.SandboxApiKey : settings.ApiKey;
                                    if (!string.IsNullOrEmpty(key))
                                    {
                                        var code = RandomString();
                                        var smsVerification = new Core.Domain.Customers.SmsVerification
                                        {
                                            CustomerEmail = model.Email,
                                            PhoneNumber = number,
                                            VerificationCode = code,
                                            Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - Se creó el código de verificación para el pre-usuario \"{string.Join(" ", new { model.FirstName, model.LastName })}\" con mail ingresado de registro \"{model.Email}\".\n"
                                        };
                                        _smsVerificationService.InsertSmsVerification(smsVerification);

                                        Client client = Client.CreateDefault(key);
                                        long.TryParse("+521" + number, out long Msisdn);
                                        if (Msisdn == 0)
                                            return BadRequest("Couldn't convert phone number to long value");
                                        string body = $"[{code}] - ¡Hola desde Central en Línea! Este es tu código de verificación:\n\n{code}";

                                        try
                                        {
                                            var result = client.SendMessage("CEL",
                                                body, new[] { Msisdn });

                                            // Message sent succesfully
                                            return Ok(code);
                                        }
                                        catch (Exception e)
                                        {
                                            return BadRequest("[C] - Error sending SMS: " + e.Message);
                                        }
                                    }
                                    else
                                        return BadRequest("[C] - No API Key registered");
                                }
                                else
                                    return BadRequest("[C] - SMS Verification is not active, this process is not needed");
                            }
                        else
                            return BadRequest("Este número de celular ya está en uso por otra cuenta, por favor, ingresa otro");
                    }
                    else
                        return BadRequest("Número de celular invalido, asegúrate de que el número que ingresaste no contenga símbolos, letras o espacios y tenga 10 dígitos");
                }
                else
                    return BadRequest("Número de celular vacío, ingresa un número de celular valido");
            }
            else
                return BadRequest("El email ingresado ya está siendo usado por otra cuenta, por favor, ingresa otro");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ValidateSmsVerificationCode(string phoneNumber, string code)
        {
            var number = phoneNumber?.Trim();
            var settings = _settingService.LoadSetting<SmsVerificationSettings>();
            if (!string.IsNullOrEmpty(number))
            {
                if (!string.IsNullOrEmpty(code))
                {
                    Regex rg = new Regex(@"^\d{10}$");
                    if (IsDigitsOnly(number) && rg.Matches(number).Count > 0)
                    {
                        var alreadyValidate = _smsVerificationService.GetSmsVerificationsByPhoneNumberAndCode(number)
                            .Where(x => x.IsVerified).ToList();

                        string numberExistsInDb =
                            @"SELECT TOP (1) [Value] 
	                        FROM [dbo].[GenericAttribute] 
	                        where KeyGroup = 'Customer' 
	                        AND [Key] = 'Phone'
	                        AND [Value] = '" + number + "'";
                        string numberExists = _dbContext.SqlQuery<string>(numberExistsInDb).FirstOrDefault();
                        if (!alreadyValidate.Any() && string.IsNullOrEmpty(numberExists))
                        {
                            var smsValidation = _smsVerificationService.GetSmsVerificationsByPhoneNumberAndCode(phoneNumber, code).FirstOrDefault();
                            if (smsValidation != null)
                            {
                                var time = (DateTime.UtcNow - smsValidation.CreatedOnUtc).TotalMinutes;
                                if (time < settings.MinutesForCodeRequest)
                                {
                                    smsValidation.IsVerified = true;
                                    _smsVerificationService.UpdateSmsVerification(smsValidation);
                                    return Ok(code);
                                }
                                else
                                    return BadRequest("El código de verificación ha expirado, por favor, solicita otro");
                            }
                            else
                                return BadRequest("El código de verificación y/o número de celular es/son invalido/s");
                        }
                        else
                            return BadRequest("Este número de celular ya está en uso por otra cuenta, por favor, ingresa otro");
                    }
                    else
                        return BadRequest("Número de celular invalido, asegúrate de que el número que ingresaste no contenga símbolos, letras o espacios y tenga 10 dígitos");
                }
                else
                    return BadRequest("Código de verificación invalido, por favor, ingresa el codigo de verificación");
            }
            else
                return BadRequest("Número de celular vacío, ingresa un número de celular valido");
        }

        protected bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        protected string RandomString(int length = 6)
        {
            Random random = new Random();
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #endregion

        #endregion
    }

    public class SmsVerificationModel
    {
        public string PhoneNumber { get; set; }
    }
}