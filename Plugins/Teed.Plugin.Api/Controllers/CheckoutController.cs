using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Factories;
using Nop.Web.Models.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.Checkout;
using Nop.Services.Logging;
using Teed.Plugin.Api.Services;
using Teed.Plugin.Api.Domain.Payment;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Core.Domain.Discounts;
using Nop.Services.Catalog;
using Nop.Web.Utils;
using System.Net.Http;
using System.Globalization;
using Teed.Plugin.Api.Utils;
using Nop.Services.Rewards;
using Nop.Core;

namespace Teed.Plugin.Api.Controllers
{
    public class CheckoutController : ApiBaseController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ICheckoutModelFactory _checkoutModelFactory;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IAddressService _addressService;
        private readonly ILocalizationService _localizationService;
        private readonly IDiscountService _discountService;
        private readonly IOrderService _orderService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly CustomerSavedCardService _customerSavedCardService;
        private readonly OrderSettings _orderSettings;
        private readonly ILogger _logger;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;
        private readonly IPaymentService _paymentService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public CheckoutController(ICustomerService customerService,
            OrderSettings orderSettings,
            IShoppingCartModelFactory shoppingCartModelFactory,
            ICheckoutModelFactory checkoutModelFactory,
            IOrderProcessingService orderProcessingService,
            CustomerSavedCardService customerSavedCardService,
            IAddressService addressService,
            IDiscountService discountService,
            IPriceCalculationService priceCalculationService,
            ILocalizationService localizationService,
            ILogger logger,
            IOrderService orderService,
            IShoppingCartService shoppingCartService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IProductService productService,
            IPaymentService paymentService,
            IStoreContext storeContext)
        {
            _shoppingCartService = shoppingCartService;
            _customerService = customerService;
            _orderSettings = orderSettings;
            _checkoutModelFactory = checkoutModelFactory;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _orderProcessingService = orderProcessingService;
            _addressService = addressService;
            _logger = logger;
            _orderService = orderService;
            _customerSavedCardService = customerSavedCardService;
            _discountService = discountService;
            _localizationService = localizationService;
            _priceCalculationService = priceCalculationService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _productService = productService;
            _paymentService = paymentService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        [HttpGet]
        public IActionResult GetPaymentMethods()
        {
            int userId = int.Parse(UserId);
            Customer customer = _customerService.GetCustomerById(userId);
            List<ShoppingCartItem> cart = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            //if (!cart.Any()) return BadRequest("No tienes productos en el carrito.");

            if (customer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                return BadRequest("Las compras de invitado no están permitidas");

            string lastUserPaymentMethodSystemName = _orderService.GetAllOrdersQuery()
                .Where(x => x.CustomerId == userId)
                .OrderByDescending(x => x.CreatedOnUtc)
                .Select(x => x.PaymentMethodSystemName)
                .FirstOrDefault();

            var savedCard = _customerSavedCardService.GetAll().Where(x => x.CustomerId == userId).ToList();
            List<string> allowedMobilePaymentMethods = new List<string>()
            {
                "Payments.CardOnDelivery",
                "Payments.CashOnDelivery",
                "Payments.PaypalMobile",
                "Payments.Stripe",
                "Payments.Visa"
            };

            if (customer.CustomerRoles.Where(x => x.SystemName == "Administrators").Any())
            {
                allowedMobilePaymentMethods.Add("Payments.Benefits");
            }

            var paymentMethodModel = _checkoutModelFactory.PreparePaymentMethodModel(cart, 0);
            var dto = paymentMethodModel.PaymentMethods.Where(x => allowedMobilePaymentMethods.Contains(x.PaymentMethodSystemName)).Select(x => new PaymentMethodDto()
            {
                Description = x.Description,
                Fee = x.Fee,
                LogoUrl = x.LogoUrl,
                Name = x.Name,
                PaymentMethodSystemName = x.PaymentMethodSystemName,
                Selected = !string.IsNullOrWhiteSpace(lastUserPaymentMethodSystemName) && lastUserPaymentMethodSystemName == x.PaymentMethodSystemName,
                SavedCard = savedCard.Count > 0 && savedCard.Where(y => y.PaymentMethodSystemName == x.PaymentMethodSystemName).FirstOrDefault() != null ? new SavedCardDataDto()
                {
                    Brand = savedCard.Where(y => y.PaymentMethodSystemName == x.PaymentMethodSystemName).FirstOrDefault().Brand,
                    CardOwnerName = savedCard.Where(y => y.PaymentMethodSystemName == x.PaymentMethodSystemName).FirstOrDefault().CardOwnerName,
                    ServiceCustomerId = savedCard.Where(y => y.PaymentMethodSystemName == x.PaymentMethodSystemName).FirstOrDefault().ServiceCustomerId,
                    LastFourDigits = savedCard.Where(y => y.PaymentMethodSystemName == x.PaymentMethodSystemName).FirstOrDefault().LastFourDigits,
                    CardLogoUrl = "/Plugins/Teed.Plugin.Api/src/images/card/" + _customerSavedCardService.GetCardLogoName(savedCard.Where(y => y.PaymentMethodSystemName == x.PaymentMethodSystemName).FirstOrDefault().Brand) + ".jpg",
                    BillAddress1 = savedCard.Where(y => y.PaymentMethodSystemName == x.PaymentMethodSystemName).FirstOrDefault().BillAddress1,
                    BillAdministrativeArea = savedCard.Where(y => y.PaymentMethodSystemName == x.PaymentMethodSystemName).FirstOrDefault().BillAdministrativeArea,
                    BillCountry = savedCard.Where(y => y.PaymentMethodSystemName == x.PaymentMethodSystemName).FirstOrDefault().BillCountry,
                    BillEmail = savedCard.Where(y => y.PaymentMethodSystemName == x.PaymentMethodSystemName).FirstOrDefault().BillEmail,
                    BillFirstName = savedCard.Where(y => y.PaymentMethodSystemName == x.PaymentMethodSystemName).FirstOrDefault().BillFirstName,
                    BillLastName = savedCard.Where(y => y.PaymentMethodSystemName == x.PaymentMethodSystemName).FirstOrDefault().BillLastName,
                    BillLocality = savedCard.Where(y => y.PaymentMethodSystemName == x.PaymentMethodSystemName).FirstOrDefault().BillLocality,
                    BillPhoneNumber = savedCard.Where(y => y.PaymentMethodSystemName == x.PaymentMethodSystemName).FirstOrDefault().BillPhoneNumber,
                    BillPostalCode = savedCard.Where(y => y.PaymentMethodSystemName == x.PaymentMethodSystemName).FirstOrDefault().BillPostalCode
                } : null
            }).ToList();

            return Ok(dto);
        }

        [HttpGet]
        public virtual IActionResult ApplyGiftProduct()
        {
            int userId = int.Parse(UserId);
            Customer customer = _customerService.GetCustomerById(userId);
            if (customer == null) return NotFound();
            _shoppingCartService.AddGiftProduct(customer, 1);
            return NoContent();
        }

        [HttpPost]
        public virtual IActionResult ApplyDiscountCoupon(string discountcouponcode)
        {
            if (discountcouponcode != null)
                discountcouponcode = discountcouponcode.Trim();

            if (!string.IsNullOrWhiteSpace(discountcouponcode))
            {
                int userId = int.Parse(UserId);
                Customer customer = _customerService.GetCustomerById(userId);
                List<ShoppingCartItem> cart = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();

                var discounts = _discountService.GetAllDiscountsForCaching(couponCode: discountcouponcode, showHidden: true)
                   .Where(d => d.RequiresCouponCode)
                   .ToList();

                if (discounts.Any())
                {
                    var userErrors = new List<string>();
                    if (discounts.Where(x => !x.IsCumulative && x.RequiresCouponCode).Any() && discounts.Count > 1)
                    {
                        userErrors.Add("Los cupones que ya tienes agregados no pueden ser utilizados con el cupón ingresado.");
                        return BadRequest(string.Join(", ", userErrors));
                    }

                    var anyValidDiscount = discounts.Any(discount =>
                    {
                        var validationResult = _discountService.ValidateDiscount(discount, customer, new[] { discountcouponcode });
                        userErrors.AddRange(validationResult.Errors);
                        return validationResult.IsValid;
                    });

                    List<DiscountForCaching> discountsWithMinimumAmount = discounts.Where(x => x.OrderMinimumAmount > 0).ToList();
                    string minimumAmountError = null;
                    if (discountsWithMinimumAmount.Any())
                    {
                        foreach (var discount in discountsWithMinimumAmount)
                        {
                            minimumAmountError = ValidateMinimumAmount(customer, discount);
                            if (!string.IsNullOrWhiteSpace(minimumAmountError)) return BadRequest(minimumAmountError);
                        }
                    }

                    if (anyValidDiscount)
                    {
                        var discountsHavAddProducts = discounts.Where(x => x.ShouldAddProducts &&
                            x.DiscountTypeId == (int)DiscountType.AssignedToSkus).ToList();
                        if (discounts.Where(x => x.ShouldAddProducts).Any())
                        {
                            if (!customer.ParseAppliedDiscountCouponCodes().Contains(discountcouponcode.ToLower()))
                                foreach (var discountHavAddProducts in discountsHavAddProducts)
                                {
                                    var productIds = _discountService.GetAppliedProductIds(discountHavAddProducts, customer).ToArray();
                                    var products = _productService.GetProductsByIds(productIds).Where(x => x.Published).ToList();
                                    if (products.Any())
                                    {
                                        foreach (var product in products)
                                        {
                                            var cartItemWithProduct = customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart &&
                                                x.ProductId == product.Id).FirstOrDefault();
                                            var newQuantity = (cartItemWithProduct?.Quantity ?? 0) + 1;
                                            decimal customerEnteredPrice = _paymentService.CalculateGroceryPrice(product, cartItemWithProduct, newQuantity, true);

                                            var sci = customer.ShoppingCartItems.Where(x => x.ProductId == product.Id
                                                && x.ShoppingCartType == ShoppingCartType.ShoppingCart).FirstOrDefault();
                                            if (sci == null)
                                            {
                                                var warnings = _shoppingCartService.AddToCart(customer: customer,
                                                    product: product,
                                                    shoppingCartType: ShoppingCartType.ShoppingCart,
                                                    storeId: _storeContext.CurrentStore.Id,
                                                    attributesXml: string.Empty,
                                                    quantity: newQuantity, buyingBySecondary: false,
                                                    selectedPropertyOption: string.Empty, customerEnteredPrice: customerEnteredPrice);
                                                if (warnings.Any()) return BadRequest(warnings.FirstOrDefault());
                                                sci = customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart &&
                                                    x.ProductId == product.Id).FirstOrDefault();
                                            }
                                            else
                                                _shoppingCartService.UpdateShoppingCartItem(customer,
                                                                        sci.Id, sci.AttributesXml, customerEnteredPrice,
                                                                        sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                                                        newQuantity, true, sci.BuyingBySecondary, sci.SelectedPropertyOption);

                                            sci.AppliedByCodeByCoupon = discountcouponcode.ToLower();
                                            _customerService.UpdateCustomer(customer);
                                        }
                                    }
                                }
                        }

                        var discount = discounts.FirstOrDefault();
                        DiscountDto dto = new DiscountDto()
                        {
                            CouponCode = discount.CouponCode,
                            Name = discount.Name,
                            DiscountId = discount.Id
                        };

                        //valid
                        customer.ApplyDiscountCouponCode(discountcouponcode);
                        if (discounts.Where(x => x.CustomerOwnerId > 0).Any())
                        {
                            var customerId = discounts.Where(x => x.CustomerOwnerId > 0).Select(x => x.CustomerOwnerId).FirstOrDefault();
                            var couponOwner = _customerService.GetCustomerById(customerId);
                            dto.ResultMessage = $"Se aplicó el código de amigo de {customer.GetFullName()} correctamente.";
                        }
                        else
                            dto.ResultMessage = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.Applied");

                        return Ok(dto);
                    }
                    else
                    {
                        if (userErrors.Any())
                            return BadRequest(string.Join(", ", userErrors));
                        else
                            return BadRequest(_localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));
                    }
                }
            }

            return BadRequest(_localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));
        }

        [HttpGet]
        public virtual IActionResult GetAppliedDiscount()
        {
            int userId = int.Parse(UserId);
            Customer customer = _customerService.GetCustomerById(userId);
            List<DiscountDto> dto = new List<DiscountDto>();

            var discountCouponCodes = customer.ParseAppliedDiscountCouponCodes();
            foreach (var couponCode in discountCouponCodes)
            {
                var discount = _discountService.GetAllDiscountsForCaching(couponCode: couponCode)
                    .FirstOrDefault(d => d.RequiresCouponCode && _discountService.ValidateDiscount(d, customer).IsValid);

                if (discount != null)
                {
                    if (discount.OrderMinimumAmount > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(ValidateMinimumAmount(customer, discount)))
                            continue;
                    }

                    dto.Add(new DiscountDto()
                    {
                        CouponCode = discount.CouponCode,
                        DiscountId = discount.Id,
                        Name = discount.Name
                    });
                }
            }
            return Ok(dto);
        }

        [HttpGet]
        public virtual IActionResult GetCustomerBalance()
        {
            int userId = int.Parse(UserId);
            Customer customer = _customerService.GetCustomerById(userId);
            List<ShoppingCartItem> cart = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            var dto = new CustomerBalanceDto
            {
                CurrentBalance = _orderTotalCalculationService.GetBalanceTotal(customer),
                OrderUsableBalance = _orderTotalCalculationService.GetBalanceTotalByOrderTotalOrCart(customer, cart: cart),
                BalanceIsActive = customer.IsPaymentWithBalanceActive ?? false
            };

            return Ok(dto);
        }

        [HttpGet]
        public virtual IActionResult SetBalanceActive(bool isActive)
        {
            int userId = int.Parse(UserId);
            Customer customer = _customerService.GetCustomerById(userId);
            customer.IsPaymentWithBalanceActive = isActive;
            _customerService.UpdateCustomer(customer);
            return Ok(customer.IsPaymentWithBalanceActive);
        }

        [HttpDelete]
        public virtual IActionResult RemoveCoupon(int discountId)
        {
            int userId = int.Parse(UserId);
            Customer customer = _customerService.GetCustomerById(userId);
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null) return NotFound();
            var itemsOfDiscount = customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart &&
                x.AppliedByCodeByCoupon == discount.CouponCode.ToLower()).ToList();
            foreach (var sci in itemsOfDiscount)
                _shoppingCartService.DeleteShoppingCartItem(sci, ensureOnlyActiveCheckoutAttributes: true);
            customer.RemoveDiscountCouponCode(discount.CouponCode);
            return NoContent();
        }

        [HttpPost]
        public IActionResult SaveStripeCard([FromBody] SavedCardDataDto dto)
        {
            if (dto == null) return BadRequest();
            int userId = int.Parse(UserId);
            var savedCard = _customerSavedCardService.GetAll().Where(x => x.CustomerId == userId).FirstOrDefault();

            if (savedCard == null)
            {
                savedCard = new CustomerSavedCard()
                {
                    Brand = dto.Brand,
                    CardOwnerName = dto.CardOwnerName,
                    CustomerId = userId,
                    LastFourDigits = dto.LastFourDigits,
                    ServiceCustomerId = dto.ServiceCustomerId,
                    PaymentMethodSystemName = "Payments.Stripe"
                };
                _customerSavedCardService.Insert(savedCard);
            }
            dto.CardLogoUrl = "/Plugins/Teed.Plugin.Api/src/images/card/" + _customerSavedCardService.GetCardLogoName(savedCard.Brand) + ".jpg";

            return Ok(dto);
        }

        [HttpPost]
        public IActionResult SaveVisaCard([FromBody] SavedCardDataDto dto)
        {
            if (dto == null) return BadRequest();
            int userId = int.Parse(UserId);
            var savedCard = _customerSavedCardService.GetAll().Where(x => x.CustomerId == userId).FirstOrDefault();

            if (savedCard == null)
            {
                savedCard = new CustomerSavedCard()
                {
                    Brand = dto.Brand,
                    CardOwnerName = dto.CardOwnerName,
                    CustomerId = userId,
                    LastFourDigits = dto.LastFourDigits,
                    ServiceCustomerId = dto.ServiceCustomerId,
                    PaymentMethodSystemName = "Payments.Visa",
                    BillAddress1 = dto.BillAddress1,
                    BillAdministrativeArea = dto.BillAdministrativeArea,
                    BillCountry = dto.BillCountry,
                    BillEmail = dto.BillEmail,
                    BillFirstName = dto.BillFirstName,
                    BillLastName = dto.BillLastName,
                    BillLocality = dto.BillLocality,
                    BillPhoneNumber = dto.BillPhoneNumber,
                    BillPostalCode = dto.BillPostalCode
                };
                _customerSavedCardService.Insert(savedCard);
            }
            dto.CardLogoUrl = "/Plugins/Teed.Plugin.Api/src/images/card/" + _customerSavedCardService.GetCardLogoName(savedCard.Brand) + ".jpg";

            return Ok(dto);
        }

        [HttpDelete]
        public IActionResult DeleteVisaCard()
        {
            int userId = int.Parse(UserId);
            var savedCard = _customerSavedCardService.GetAll().Where(x => x.CustomerId == userId).FirstOrDefault();
            if (savedCard == null) return NotFound();
            _customerSavedCardService.Delete(savedCard);
            return NoContent();
        }

        [HttpDelete]
        public IActionResult DeleteStripeCard()
        {
            int userId = int.Parse(UserId);
            var savedCard = _customerSavedCardService.GetAll().Where(x => x.CustomerId == userId).FirstOrDefault();
            if (savedCard == null) return NotFound();
            _customerSavedCardService.Delete(savedCard);
            return NoContent();
        }

        [HttpGet]
        public IActionResult GetOrderTotals()
        {
            int userId = int.Parse(UserId);
            Customer customer = _customerService.GetCustomerById(userId);
            List<ShoppingCartItem> cart = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            if (!cart.Any()) return BadRequest("No tienes productos en el carrito.");

            var model = _shoppingCartModelFactory.PrepareOrderTotalsModel(cart, false);
            var dto = new OrderTotalDto()
            {
                OrderTotal = model.OrderTotalValue,
                OrderTotalDiscount = model.OrderTotalDiscountValue + model.SubTotalDiscountValue,
                OrderItemsDiscount = model.ProductDiscountsValue,
                Shipping = model.ShippingValue,
                SubTotal = model.SubtotalValue,
                ShoppingCartItemIds = cart.Select(x => x.Id).ToList()
            };

            return Ok(dto);
        }

        [HttpPost]
        public IActionResult PlaceOrder([FromBody] PlaceOrderDto dto)
        {
            int userId = int.Parse(UserId);
            Customer customer = _customerService.GetCustomerById(userId);
            if (!customer.Active) return Unauthorized();
            List<ShoppingCartItem> cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart && dto.ShoppingCartItemIds.Contains(sci.Id))
                .ToList();
            if (!cart.Any()) return BadRequest("No tienes productos en el carrito.");

            Address address = _addressService.GetAddressById(dto.AddressId);
            if (address == null) return BadRequest("La dirección es inválida");

            if (_orderTotalCalculationService.GetShoppingCartTotal(cart) == 0)
                dto.SelectedPaymentMethodSystemName = null;



            var processPaymentRequest = new ProcessPaymentRequest
            {
                StoreId = 1,
                CustomerId = userId,
                PaymentMethodSystemName = dto.SelectedPaymentMethodSystemName
            };

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            DateTime selectedDate = new DateTime();
            if (TeedCommerceStores.CurrentStore == TeedStores.CentralEnLinea && !string.IsNullOrWhiteSpace(dto.SelectedShippingDate))
            {
                selectedDate = DateTime.ParseExact(dto.SelectedShippingDate, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                DateTime today = DateTime.Now.Date;
                if (today == selectedDate) return BadRequest("No es posible crear una orden para el mismo día.");

                if (!customer.CheckoutSecurityIsDisabled.HasValue ||
                (customer.CheckoutSecurityIsDisabled.HasValue && !customer.CheckoutSecurityIsDisabled.Value))
                {
                    var orderTotal = _shoppingCartModelFactory.PrepareOrderTotalsModel(cart, false).OrderTotalValue;
                    if (dto.SelectedPaymentMethodSystemName == "Payments.CashOnDelivery" && !Nop.Web.Utils.OrderUtils.CashPaymentOrderIsValid(_orderService, customer.Id, selectedDate, orderTotal))
                    {
                        return BadRequest("El monto de tu orden y sus complementarias exceden el monto máximo de compra para pagos en efectivo.");
                    }

                    if (!Nop.Web.Utils.OrderUtils.OrderAmountsAreValid(_orderService, customer.Id, selectedDate, orderTotal))
                    {
                        return BadRequest("El monto de tu orden y sus complementarias exceden el monto máximo permitido de compra.");
                    }
                }
            }

            dictionary.Add("selectedShippingDate", selectedDate);
            dictionary.Add("selectedShippingTime", dto.SelectedShippingTime);
            dictionary.Add("namesigns", string.Empty);
            dictionary.Add("messagesigns", string.Empty);
            dictionary.Add("orderNote", dto.PaymentResult ?? "");

            processPaymentRequest.CustomValues = dictionary;

            customer.BillingAddress = address;
            customer.ShippingAddress = address;
            _customerService.UpdateCustomer(customer);

            var placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest, cart.Select(x => x.Id).ToList());
            if (placeOrderResult.Success)
            {
                var order = placeOrderResult.PlacedOrder;
                AddOrderNote(order, "Orden creada desde aplicación móvil.");

                if (customer.CheckoutSecurityIsDisabled.HasValue && customer.CheckoutSecurityIsDisabled.Value)
                {
                    customer.CheckoutSecurityIsDisabled = false;
                    _customerService.UpdateCustomer(customer);
                }

                if ((dto.SelectedPaymentMethodSystemName == "Payments.Visa" || dto.SelectedPaymentMethodSystemName == "Payments.Stripe" || dto.SelectedPaymentMethodSystemName == "Payments.PaypalMobile") &&
                    !string.IsNullOrWhiteSpace(dto.PaymentResult))
                {
                    _orderProcessingService.MarkOrderAsPaid(order);
                    AddOrderNote(order, $"Pago con {dto.SelectedPaymentMethodSystemName} registrado desde la app móvil. Resultado: " + dto.PaymentResult);
                }
                else if (dto.SelectedPaymentMethodSystemName == "Payments.Benefits")
                {
                    _orderProcessingService.MarkOrderAsPaid(order);
                }

                return Ok(placeOrderResult.PlacedOrder.Id);
            }
            else
            {
                _logger.Error($"[APP MOVIL]: ERROR CREANDO LA ÓRDEN", new Exception(string.Join(". ", placeOrderResult.Errors)), customer);
                return BadRequest("No fue posible crear la orden, por favor inténtalo más tarde o contáctanos.");
            }
        }

        #endregion

        #region private methods

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

        /// <summary>
        /// Validate minimimum amount
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="discountsWithMinimumAmount"></param>
        /// <returns></returns>
        private string ValidateMinimumAmount(Customer customer, DiscountForCaching discountsWithMinimumAmount)
        {
            string isValid = string.Empty;
            decimal subtotalWithDiscount = customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .Select(x => _priceCalculationService.GetSubTotal(x)).DefaultIfEmpty().Sum();
            if (subtotalWithDiscount < discountsWithMinimumAmount.OrderMinimumAmount)
            {
                customer.RemoveDiscountCouponCode(discountsWithMinimumAmount.CouponCode);
                decimal diff = discountsWithMinimumAmount.OrderMinimumAmount - subtotalWithDiscount;
                return $"El mínimo de compra del cupón es {discountsWithMinimumAmount.OrderMinimumAmount:C} MXN. Agrega {diff:C} MXN más para poder utilizar este cupón.";
            }
            return string.Empty;
        }

        #endregion
    }
}