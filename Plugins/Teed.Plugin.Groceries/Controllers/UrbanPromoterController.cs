using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.OrderReports;
using Teed.Plugin.Groceries.Domain.UrbanPromoters;
using Teed.Plugin.Groceries.Models.OperationalErrors;
using Teed.Plugin.Groceries.Models.UrbanPromoter;
using Teed.Plugin.Groceries.Models.Warnings;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class UrbanPromoterController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IDiscountService _discountService;
        private readonly IOrderService _orderService;
        private readonly UrbanPromoterService _urbanPromoterService;
        private readonly UrbanPromoterFeeService _urbanPromoterFeeService;
        private readonly UrbanPromoterCouponService _urbanPromoterCouponService;
        private readonly UrbanPromoterPaymentService _urbanPromoterPaymentService;

        public UrbanPromoterController(IPermissionService permissionService, IProductService productService,
            ICustomerService customerService, IWorkContext workContext,
            IDiscountService discountService, IOrderService orderService,
            UrbanPromoterService urbanPromoterService, UrbanPromoterFeeService urbanPromoterFeeService,
            UrbanPromoterCouponService urbanPromoterCouponService, UrbanPromoterPaymentService urbanPromoterPaymentService)
        {
            _permissionService = permissionService;
            _productService = productService;
            _customerService = customerService;
            _workContext = workContext;
            _discountService = discountService;
            _orderService = orderService;
            _urbanPromoterService = urbanPromoterService;
            _urbanPromoterFeeService = urbanPromoterFeeService;
            _urbanPromoterCouponService = urbanPromoterCouponService;
            _urbanPromoterPaymentService = urbanPromoterPaymentService;
        }

        #region Utilities

        [HttpGet]
        public IActionResult GetCustomersFiltering(string text, int byId = 0)
        {
            if (!string.IsNullOrEmpty(text) || byId > 0)
            {
                if (byId > 0)
                {
                    var customer = _customerService.GetCustomerById(byId);
                    return Json(new { name = $"{customer.GetFullName()} ({customer.Email})", id = customer.Id.ToString() });
                }
                text = text.Trim().ToLower();
                var promoters = _urbanPromoterService.GetAll().ToList();
                var customerIds = promoters.Select(x => x.CustomerId).ToList();
                var customers = _customerService.GetAllCustomersQuery()
                    .Where(x => !x.Deleted && !string.IsNullOrEmpty(x.Email) && x.Email.ToLower().Contains(text)).ToList()
                    .Select(x => new { Name = $"{x.GetFullName()} ({x.Email})", Id = x.Id.ToString() }).ToList();
                var customersFilter = customers.Select(x => new
                {
                    id = x.Id,
                    name = x.Name
                }).ToList();

                return Json(customersFilter);
            }
            return Ok();
        }

        [HttpGet]
        public IActionResult GetCouponsFiltering(string text, int byId = 0)
        {
            if (!string.IsNullOrEmpty(text) || byId > 0)
            {
                if (byId > 0)
                {
                    var discount = _discountService.GetDiscountById(byId);
                    return Json(new { name = $"{discount.Name} ({discount.CouponCode})", id = discount.Id.ToString() });
                }
                text = text.Trim().ToLower();
                var promoters = _urbanPromoterService.GetAll().ToList();
                var discountIds = promoters.SelectMany(x => x.GetUrbanPromoterCoupons()).Select(x => x.DiscountId).ToList();
                var coupons = _discountService.GetAllDiscounts(showHidden: true)
                    .Where(x => !discountIds.Contains(x.Id) && (x.Name.ToLower().Contains(text) || (x.CouponCode ?? "").ToLower().Contains(text))).ToList()
                    .Select(x => new { Name = $"{x.Name} ({x.CouponCode})", Id = x.Id.ToString() }).ToList();
                var couponsFilter = coupons.Select(x => new
                {
                    id = x.Id,
                    name = x.Name
                }).ToList();

                return Json(couponsFilter);
            }
            return Ok();
        }

        private string CreateCouponsHtml(UrbanPromoter urbanPromoter, List<DiscountListModel> discounts)
        {
            var final = string.Empty;
            var promoterCoupons = urbanPromoter.GetUrbanPromoterCoupons();
            if (promoterCoupons?.Any() ?? false)
            {
                final = "<ul>";
                var discountIds = promoterCoupons.Select(x => x.DiscountId).ToList();
                discounts = discounts
                    .Where(x => discountIds.Contains(x.Id))
                    .ToList();
                foreach (var coupon in promoterCoupons)
                {
                    var discount = discounts.Where(x => x.Id == coupon.DiscountId).FirstOrDefault();
                    if (discount != null)
                        final += $@"<li><a href=""/Admin/Discount/Edit/{discount.Id}"" target=""_blank"">{discount.DiscountName} ({discount.CouponCode})</a></li>";
                }
                final += "</ul>";
            }
            return final;
        }

        #endregion

        public IActionResult Index()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UrbanPromoters))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/UrbanPromoter/Index.cshtml");
        }

        [HttpPost]
        public IActionResult ListPromoters(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UrbanPromoters))
                return AccessDeniedView();

            var promoters = _urbanPromoterService.GetAll().OrderBy(x => x.AccountOwnerName).ToList();
            var customerIds = promoters.Select(x => x.CustomerId).ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => customerIds.Contains(x.Id))
                .Select(x => new { x.Email, x.Id })
                .ToList();
            var discounts = _discountService.GetAllDiscounts(showHidden: true)
                .Select(x => new DiscountListModel()
                {
                    CouponCode = x.CouponCode,
                    DiscountName = x.Name,
                    Id = x.Id
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = promoters.Select(x =>
                {
                    var promoterFees = x.GetUrbanPromoterFees();
                    var promoterPayments = x.GetUrbanPromoterPayment();
                    var promoterFee = promoterFees.Any() ? promoterFees.Select(y => y.FeeAmount).DefaultIfEmpty().Sum() : (decimal)0.00;
                    var promoterPayment = promoterPayments.Any() ? promoterPayments.Select(y => y.PaymentAmount).DefaultIfEmpty().Sum() : (decimal)0.00;
                    var amountToPay = promoterFee - promoterPayment;

                    return new
                    {
                        x.Id,
                        x.AccountOwnerName,
                        CustomerEmail = customers.Where(y => y.Id == x.CustomerId).Select(y => y.Email).FirstOrDefault(),
                        CustomerId = customers.Where(y => y.Id == x.CustomerId).Select(y => y.Id).FirstOrDefault(),
                        Coupons = CreateCouponsHtml(x, discounts),
                        ComissionsTotal = promoterFee.ToString("C"),
                        AmountToPay = amountToPay.ToString("C"),
                        AmountToPayValue = amountToPay,
                        x.IsActive,
                    };
                }).OrderByDescending(x => x.AmountToPayValue).ToList(),
                Total = promoters.Count()
            };

            return Json(gridModel);
        }

        public IActionResult View(int id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UrbanPromoters))
                return AccessDeniedView();

            var promoter = _urbanPromoterService.GetById(id);
            if (promoter == null)
                return RedirectToAction("Index");

            var model = new UrbanPromoterModel
            {
                Id = promoter.Id,
                AccountAddress = promoter.AccountAddress,
                AccountCLABE = promoter.AccountCLABE,
                AccountBankName = promoter.AccountBankName,
                AccountNumber = promoter.AccountNumber,
                AccountOwnerName = promoter.AccountOwnerName,
                IsActive = promoter.IsActive,
                CashPayment = promoter.CashPayment,
                CustomerId = promoter.CustomerId,
                Log = promoter.Log
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/UrbanPromoter/View.cshtml", model);
        }

        [HttpPost]
        public IActionResult ListPromoterOrders(int id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UrbanPromoters))
                return AccessDeniedView();

            var promoter = _urbanPromoterService.GetById(id);
            if (promoter == null)
                return BadRequest();
            var discountIds = promoter.GetUrbanPromoterCoupons().Select(x => x.DiscountId).ToList();
            var fees = promoter.GetUrbanPromoterFees().ToList();
            var discountUsage = _discountService.GetAllDiscountUsageHistory()
                .Where(x => discountIds.Contains(x.DiscountId))
                .ToList();
            var orderIds = discountUsage.Select(x => x.OrderId).ToList();
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => orderIds.Contains(x.Id)).OrderByDescending(x => x.SelectedShippingDate)
                .ToList();
            var customerIds = orders.Select(x => x.CustomerId).ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => customerIds.Contains(x.Id))
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = orders.Select(x => new
                {
                    SelectedShippingDate = x.SelectedShippingDate.Value.ToString("dd/MM/yyyy"),
                    CustomerEmail = customers.Where(y => y.Id == x.CustomerId).FirstOrDefault().Email,
                    CustomerId = customers.Where(y => y.Id == x.CustomerId).FirstOrDefault().Id,
                    x.Id,
                    TotalAmount = x.OrderTotal.ToString("C"),
                    Status = x.OrderStatusId == 10 ? "Pendiente" : x.OrderStatusId == 20 ? "Procesado" : x.OrderStatusId == 30 ? "Completado" : x.OrderStatusId == 40 ? "Cancelado" : "No enviado",
                    StatusId = x.OrderStatusId,
                    Comission = fees.Where(y => y.RelatedOrderId == x.Id).FirstOrDefault() != null ? fees.Where(y => y.RelatedOrderId == x.Id).FirstOrDefault().FeeAmount.ToString("C") : "Aún sin comisión"
                }).ToList(),
                Total = orders.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ListPromoterPayments(int id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UrbanPromoters))
                return AccessDeniedView();

            var promoter = _urbanPromoterService.GetById(id);
            if (promoter == null)
                return BadRequest();
            var payments = promoter.GetUrbanPromoterPayment().ToList();

            var gridModel = new DataSourceResult
            {
                Data = payments.Select(x => new
                {
                    x.Id,
                    PaymentDate = x.PaymentDateUtc.ToLocalTime().ToString("dd/MM/yyyy"),
                    Amount = x.PaymentAmount.ToString("C")
                }).ToList(),
                Total = payments.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ListPromoterCoupons(int id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UrbanPromoters))
                return AccessDeniedView();

            var promoter = _urbanPromoterService.GetById(id);
            if (promoter == null)
                return BadRequest();
            var coupons = promoter.GetUrbanPromoterCoupons().ToList();
            var discountIds = coupons.Select(x => x.DiscountId).ToList();
            var discounts = _discountService.GetAllDiscounts()
                .Where(x => discountIds.Contains(x.Id))
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = coupons.Select(x => new
                {
                    x.Id,
                    CouponId = discounts.Where(y => y.Id == x.DiscountId).FirstOrDefault().Id,
                    CouponName = discounts.Where(y => y.Id == x.DiscountId).FirstOrDefault().Name,
                    CouponCode = discounts.Where(y => y.Id == x.DiscountId).FirstOrDefault().CouponCode,
                }).ToList(),
                Total = coupons.Count()
            };

            return Json(gridModel);
        }

        public IActionResult AddCoupon(int id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UrbanPromoters))
                return AccessDeniedView();

            var promoter = _urbanPromoterService.GetById(id);
            if (promoter == null)
                return RedirectToAction("Index");

            var model = new UrbanPromoterCouponModel
            {
                UrbanPromoterId = promoter.Id,
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/UrbanPromoter/AddCoupon.cshtml", model);
        }

        [HttpPost]
        public IActionResult AddCoupon(UrbanPromoterCouponModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UrbanPromoters))
                return AccessDeniedView();

            var promoter = _urbanPromoterService.GetById(model.UrbanPromoterId);
            var discount = _discountService.GetDiscountById(model.DiscountId);
            if (promoter == null || discount == null)
                return RedirectToAction("View", new { id = model.UrbanPromoterId });

            var promoterCoupon = new UrbanPromoterCoupon
            {
                DiscountId = model.DiscountId,
                Fee = model.Fee,
                UrbanPromoterId = model.UrbanPromoterId
            };
            _urbanPromoterCouponService.Insert(promoterCoupon);

            promoter.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) agregó el cupón \"{discount.Name}\".\n";
            _urbanPromoterService.Update(promoter);

            return RedirectToAction("View", new { id = promoter.Id });
        }

        [HttpPost]
        public IActionResult PaymentVoucherAdd(AddPayment model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UrbanPromoters))
                return AccessDeniedView();
            try
            {
                byte[] bytes = new byte[0];
                using (var ms = new MemoryStream())
                {
                    model.File.CopyTo(ms);
                    bytes = ms.ToArray();
                }
                var payment = new UrbanPromoterPayment
                {
                    PaymentAmount = model.Amount,
                    PaymentDateUtc = model.Date.ToUniversalTime(),
                    UrbanPromoterId = model.UrbanPromoterId,
                    VoucherExtension = model.File.FileName.Substring(model.File.FileName.LastIndexOf(".") + 1),
                    VoucherFile = bytes
                };
                _urbanPromoterPaymentService.Insert(payment);
                var urbanPromoter = _urbanPromoterService.GetById(model.UrbanPromoterId);
                if (urbanPromoter != null)
                {
                    urbanPromoter.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) subío un nuevo pago con fecha {payment.PaymentDateUtc.ToLocalTime().ToString("dd-MM-yyyy")}.\n";
                    _urbanPromoterService.Update(urbanPromoter);
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet]
        public IActionResult PaymentVoucherDownload(int id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UrbanPromoters))
                return AccessDeniedView();
            var payment = _urbanPromoterPaymentService.GetById(id);
            if (payment == null)
                return Ok();
            var model = new
            {
                Title = $"Comprobante de pago - {payment.PaymentDateUtc.ToLocalTime():dd-MM-yyyy}",
                Extension = payment.VoucherExtension,
                FileArray = payment.VoucherFile
            };
            return Json(model);
        }

        [HttpPost]
        public IActionResult PromoterPaymentDelete(UrbanPromoterModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UrbanPromoters))
                return AccessDeniedView();
            var payment = _urbanPromoterPaymentService.GetById(model.Id);
            if (payment == null)
                return Ok();

            _urbanPromoterPaymentService.Delete(payment);
            return Ok();
        }

        [HttpPost]
        public IActionResult PromoterCouponDelete(UrbanPromoterCouponModel model)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UrbanPromoters))
                return AccessDeniedView();
            var promoterCoupon = _urbanPromoterCouponService.GetById(model.Id);
            if (promoterCoupon == null)
                return Ok();

            _urbanPromoterCouponService.Delete(promoterCoupon);
            return Ok();
        }

        [HttpPost]
        public IActionResult ListPromoterAccountStatus(int id)
        {
            var promoter = _urbanPromoterService.GetById(id);
            if (promoter == null)
                return BadRequest();

            var result = GetStatementData(promoter);

            var gridModel = new DataSourceResult
            {
                Data = ((List<dynamic>)result.data).OrderByDescending(x => x.DateObject).ThenByDescending(x => x.Balance + x.Amount),
                Total = result.total
            };

            return Json(gridModel);
        }

        private dynamic GetStatementData(UrbanPromoter urbanPromoter)
        {
            var fees = urbanPromoter.GetUrbanPromoterFees().OrderByDescending(x => x.FeeGenerationDateUtc);
            var payments = urbanPromoter.GetUrbanPromoterPayment().OrderByDescending(x => x.PaymentDateUtc);

            var billData = fees.Select(x => new StatementData()
            {
                Description = $"Comisión por orden #{x.RelatedOrderId}",
                Payment = 0,
                Charge = x.FeeAmount,
                Date = x.FeeGenerationDateUtc.ToLocalTime(),
                ElementId = x.Id
            });

            var paymentsData = payments.Select(x => new StatementData()
            {
                Description = "Abono a cuenta",
                Payment = x.PaymentAmount,
                Charge = 0,
                Date = x.PaymentDateUtc.ToLocalTime(),
                ElementId = x.Id
            });

            var globalData = billData.Concat(paymentsData)
                .OrderBy(x => x.Date)
                .ThenBy(x => x.Balance + x.Charge)
                .ToList();
            decimal currentBalance = 0;

            var data = new List<dynamic>();
            foreach (var item in globalData)
            {
                currentBalance = currentBalance + item.Charge - item.Payment;
                data.Add(new
                {
                    Date = item.Date.ToString("dd-MM-yyyy"),
                    DateObject = item.Date,
                    Amount = item.Charge > 0 ? item.Charge.ToString("C") : item.Payment.ToString("C"),
                    //Tax = tax == 0 ? "-" : tax.ToString("C"),
                    Total = item.Payment == 0 ? (item.Charge).ToString("C") : (item.Payment).ToString("C"),
                    Balance = (currentBalance < 0 ? "-" : "") + Math.Abs(currentBalance).ToString("C"),
                    Description = item.Description,
                    item.ElementId
                });
            };

            return new { data, total = globalData.Count };
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UrbanPromoters))
                return AccessDeniedView();

            var model = new UrbanPromoterModel();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/UrbanPromoter/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(UrbanPromoterModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UrbanPromoters))
                return AccessDeniedView();

            var promoter = new UrbanPromoter
            {
                AccountAddress = model.AccountAddress,
                AccountCLABE = model.AccountCLABE,
                AccountBankName = model.AccountBankName,
                AccountNumber = model.AccountNumber,
                AccountOwnerName = model.AccountOwnerName,
                IsActive = true,
                CashPayment = model.CashPayment,
                CustomerId = model.CustomerId,
                Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) creó el nuevo Promotor urbano con nombre {model.AccountOwnerName}\n"
            };
            _urbanPromoterService.Insert(promoter);

            if (continueEditing)
            {
                return RedirectToAction("Edit", new { id = promoter.Id });
            }
            return RedirectToAction("View", new { id = promoter.Id });
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UrbanPromoters))
                return AccessDeniedView();

            var promoter = _urbanPromoterService.GetById(id);
            if (promoter == null)
                return RedirectToAction("Index");

            var model = new UrbanPromoterModel
            {
                Id = promoter.Id,
                AccountAddress = promoter.AccountAddress,
                AccountCLABE = promoter.AccountCLABE,
                AccountBankName = promoter.AccountBankName,
                AccountNumber = promoter.AccountNumber,
                AccountOwnerName = promoter.AccountOwnerName,
                IsActive = promoter.IsActive,
                CashPayment = promoter.CashPayment,
                CustomerId = promoter.CustomerId,
                Log = promoter.Log
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/UrbanPromoter/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(UrbanPromoterModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.UrbanPromoters))
                return AccessDeniedView();

            var promoter = _urbanPromoterService.GetById(model.Id);
            if (promoter == null)
                return RedirectToAction("Index");

            promoter.AccountAddress = model.AccountAddress;
            promoter.AccountCLABE = model.AccountCLABE;
            promoter.AccountBankName = model.AccountBankName;
            promoter.AccountNumber = model.AccountNumber;
            promoter.AccountOwnerName = model.AccountOwnerName;
            promoter.IsActive = model.IsActive;
            promoter.CashPayment = model.CashPayment;
            promoter.CustomerId = model.CustomerId;

            _urbanPromoterService.Update(promoter);

            if (continueEditing)
            {
                return RedirectToAction("Edit", new { id = promoter.Id });
            }
            return RedirectToAction("View", new { id = promoter.Id });
        }
    }

    public class DiscountListModel
    {
        public int Id { get; set; }
        public string DiscountName { get; set; }
        public string CouponCode { get; set; }
    }
}
