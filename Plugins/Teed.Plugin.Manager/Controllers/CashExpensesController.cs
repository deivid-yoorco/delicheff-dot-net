using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Manager.Domain.CashExpenses;
using Teed.Plugin.Manager.Models.CashExpenses;
using Teed.Plugin.Manager.Services;

namespace Teed.Plugin.Manager.Controllers
{
    [Area(AreaNames.Admin)]
    public class CashExpensesController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly ExpensesService _expensesService;
        private readonly ExpenseFileService _expenseFileService;
        private readonly CashExpenseService _cashExpenseService;
        private readonly CashExpenseFileService _cashExpenseFileService;

        public CashExpensesController(IPermissionService permissionService, CashExpenseService cashExpenseService,
            IOrderService orderService, ICustomerService customerService, CashExpenseFileService cashExpenseFileService,
            IWorkContext workContext, ExpensesService expensesService, ExpenseFileService expenseFileService)
        {
            _permissionService = permissionService;
            _cashExpenseService = cashExpenseService;
            _orderService = orderService;
            _customerService = customerService;
            _cashExpenseFileService = cashExpenseFileService;
            _workContext = workContext;
            _expensesService = expensesService;
            _expenseFileService = expenseFileService;
        }

        public IActionResult List()
        {
            return View("~/Plugins/Teed.Plugin.Manager/Views/CashExpenses/List.cshtml");
        }

        public IActionResult Create()
        {
            return View("~/Plugins/Teed.Plugin.Manager/Views/CashExpenses/Create.cshtml");
        }

        public IActionResult Edit(int id)
        {
            CashExpense cashExpense = _cashExpenseService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (cashExpense == null) return NotFound();

            EditViewModel model = new EditViewModel()
            {
                Amount = cashExpense.Amount,
                Comments = cashExpense.Comments,
                Concept = cashExpense.Concept,
                SelectedDate = cashExpense.ExpenseDate.ToString("dd-MM-yyyy"),
                UploadedFiles = _cashExpenseFileService.GetAll().Where(x => x.CashExpenseId == cashExpense.Id).ToList(),
                ReceptorUserId = cashExpense.ReceiverUserId,
                TransactionType = cashExpense.TransactionType
            };

            return View("~/Plugins/Teed.Plugin.Manager/Views/CashExpenses/Edit.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateViewModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            DateTime selectedDate = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(model.SelectedDate))
            {
                selectedDate = DateTime.ParseExact(model.SelectedDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            }

            CashExpense cashExpenseTransfer = null;
            if (model.TransactionType == TransactionType.Transfer)
            {
                var userCurrentBalance = await GetCurrentUserBalance(_workContext.CurrentCustomer);
                if (userCurrentBalance < model.Amount)
                {
                    ModelState.AddModelError("", "Tu saldo no es suficiente como para realizar esta transferencia. Saldo disponible: $" + userCurrentBalance.ToString("0.##"));
                    return View("~/Plugins/Teed.Plugin.Manager/Views/CashExpenses/Create.cshtml", model);
                }
                else
                {
                    cashExpenseTransfer = new CashExpense()
                    {
                        Amount = model.Amount,
                        Concept = model.Concept,
                        Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + $"Transferencia de fondos realizada por {_workContext.CurrentCustomer.GetFullName()}",
                        Comments = model.Comments,
                        CreatedByUserId = model.ReceptorUserId,
                        TransactionType = model.TransactionType,
                        ExpenseDate = selectedDate,
                        ReceiverUserId = model.ReceptorUserId
                    };
                }
            }

            var extraLog = string.Empty;
            if (model.TransactionType == TransactionType.Deposit)
            {
                extraLog = $"un depósito a {_customerService.GetCustomerById(model.ReceptorUserId).GetFullName()} de {model.Amount} pesos";
            }
            else if (model.TransactionType == TransactionType.Transfer)
            {
                extraLog = $"una transferencia a {_customerService.GetCustomerById(model.ReceptorUserId).GetFullName()} de {model.Amount} pesos";
            }
            else
            {
                extraLog = $"un gasto por {model.Amount}";
            }

            CashExpense cashExpense = new CashExpense()
            {
                Amount = model.Amount,
                ExpenseDate = selectedDate,
                Comments = model.Comments,
                Concept = model.Concept,
                CreatedByUserId = _workContext.CurrentCustomer.Id,
                ReceiverUserId = model.ReceptorUserId,
                TransactionType = model.TransactionType,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + " realizó " + extraLog
            };
            _cashExpenseService.Insert(cashExpense);
            if (cashExpenseTransfer != null)
                _cashExpenseService.Insert(cashExpenseTransfer);

            foreach (var file in model.Files)
            {
                string baseDirectoryName = "cash-expenses-media";
                string directoryPath = $"./wwwroot/{baseDirectoryName}/{cashExpense.Id}";
                Directory.CreateDirectory(directoryPath);

                string rawPath = $"{directoryPath}/{file.FileName}";
                string newFileName = $"{DateTime.Now.ToString("ddMMyyyyhhmmss")}-" + file.FileName;
                string fullPath = $"{directoryPath}/{newFileName}";
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                CashExpenseFile expenseFile = new CashExpenseFile()
                {
                    Extension = Path.GetExtension(rawPath),
                    CashExpenseId = cashExpense.Id,
                    FileName = file.FileName,
                    FileType = file.ContentType,
                    FileUrl = $"/{baseDirectoryName}/{cashExpense.Id}/{newFileName}",
                    Size = (int)file.Length,
                    UploadedByUserId = _workContext.CurrentCustomer.Id
                };
                _cashExpenseFileService.Insert(expenseFile);
            }

            return RedirectToAction("List");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditViewModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            CashExpense cashExpense = _cashExpenseService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (cashExpense == null) return NotFound();

            DateTime selectedDate = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(model.SelectedDate))
            {
                selectedDate = DateTime.ParseExact(model.SelectedDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            }

            var log = string.Empty;
            if (model.Comments != cashExpense.Comments)
            {
                log += $". Cambió los comentarios de {cashExpense.Comments} a {model.Comments}";
            }

            if (model.Concept != cashExpense.Concept)
            {
                log += $". Cambió el concepto de {cashExpense.Concept} a {model.Concept}";
            }

            if (cashExpense.ExpenseDate.Date != selectedDate.Date)
            {
                log += $". Cambió la fecha del gasto de {cashExpense.ExpenseDate.ToString("dd-MM-yyyy")} a {selectedDate.ToString("dd-MM-yyyy")}";
            }

            if (model.Files.Count() > 0)
            {
                log += $". Agregó nuevos archivos";
            }

            cashExpense.Comments = model.Comments;
            cashExpense.Concept = model.Concept;
            cashExpense.Log += "/n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + $"{_workContext.CurrentCustomer.GetFullName()} ({_workContext.CurrentCustomer.Id}) editó el registro de efectivo" + log + ".";
            _cashExpenseService.Update(cashExpense);

            foreach (var file in model.Files)
            {
                string baseDirectoryName = "cash-expenses-media";
                string directoryPath = $"./wwwroot/{baseDirectoryName}/{cashExpense.Id}";
                Directory.CreateDirectory(directoryPath);

                string rawPath = $"{directoryPath}/{file.FileName}";
                string newFileName = $"{DateTime.Now.ToString("ddMMyyyyhhmmss")}-" + file.FileName;
                string fullPath = $"{directoryPath}/{newFileName}";
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                CashExpenseFile expenseFile = new CashExpenseFile()
                {
                    Extension = Path.GetExtension(rawPath),
                    CashExpenseId = cashExpense.Id,
                    FileName = file.FileName,
                    FileType = file.ContentType,
                    FileUrl = $"/{baseDirectoryName}/{cashExpense.Id}/{newFileName}",
                    Size = (int)file.Length,
                    UploadedByUserId = _workContext.CurrentCustomer.Id
                };
                _cashExpenseFileService.Insert(expenseFile);
            }

            return RedirectToAction("List");
        }

        private async Task<decimal> GetCurrentUserBalance(Customer customer)
        {
            var cashExpenses = _cashExpenseService.GetAll().Where(x => x.CreatedByUserId == customer.Id);
            var expensesOfCash = _expensesService.GetAll().Where(x => x.PaymentType == Domain.Expenses.PaymentType.Cash).Where(x => x.CreatedByUserId == customer.Id);
            var orders = _orderService.GetOrders()
                .Where(x => !x.Deleted)
                .Where(x => x.PaymentMethodSystemName == "Payments.CashOnDelivery" && x.PaymentStatus == PaymentStatus.Paid);
            var orderReports = await GetOrderReports(customer.Id);

            var cashExpensesList = cashExpenses.ToList().Select(x => new ListViewModel()
            {
                Charge = x.TransactionType == TransactionType.Expense ? x.Amount : x.TransactionType == TransactionType.Transfer && x.CreatedByUserId != x.ReceiverUserId ? x.Amount : 0,
                Deposit = x.TransactionType == TransactionType.Deposit ? x.Amount : x.TransactionType == TransactionType.Transfer && x.CreatedByUserId == x.ReceiverUserId ? x.Amount : 0
            }).ToList();

            var expensesOfCashList = expensesOfCash.ToList().Select(x => new ListViewModel()
            {
                Charge = x.Total,
                Deposit = 0,
            }).ToList();

            var ordersListTask = orders.ToList().Select(async x => new ListViewModel()
            {
                Charge = 0,
                Deposit = x.OrderTotal,
                User = await GetOrderResponsable(x),
            });
            var ordersList = await Task.WhenAll(ordersListTask);
            ordersList = ordersList.Where(x => x.User.Id == customer.Id).ToArray();

            var orderReportsList = orderReports.ToList().Select(x => new ListViewModel()
            {
                Charge = x.RequestedQtyCost,
                Deposit = 0,
            }).ToList();

            var globalList = cashExpensesList.Union(ordersList).Union(orderReportsList).Union(expensesOfCashList).OrderBy(x => x.DateObject);
            decimal currentBalance = 0;
            foreach (var element in globalList)
            {
                currentBalance += element.Deposit;
                currentBalance -= element.Charge;
            }

            return currentBalance;
        }

        [HttpPost]
        public IActionResult UserListData()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var users = _customerService.GetAllCustomers().Where(x => !string.IsNullOrWhiteSpace(x.Email)).Where(x => x.GetCustomerRoleIds().Count() > 1).ToList();
            var elements = users.Select(x => new
            {
                Id = x.Id,
                User = x.GetFullName()
            }).ToList();

            elements.Add(new
            {
                Id = 0,
                User = "Todos los usuarios"
            });

            return Json(elements);
        }

        [HttpPost]
        public async Task<IActionResult> ListData(DataSourceRequest command, int userId)
        {
            // REMEMBER TO EDIT GetCurrentUserBalance METHOD TOO
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var cashExpenses = _cashExpenseService.GetAll();
            var expensesOfCash = _expensesService.GetAll().Where(x => x.PaymentType == Domain.Expenses.PaymentType.Cash);
            var orders = _orderService.GetOrders()
                .Where(x => !x.Deleted)
                .Where(x => x.PaymentMethodSystemName == "Payments.CashOnDelivery" && x.PaymentStatus == PaymentStatus.Paid);
            var orderReports = await GetOrderReports(userId);

            if (userId > 0)
            {
                cashExpenses = cashExpenses.Where(x => x.CreatedByUserId == userId);
                expensesOfCash = expensesOfCash.Where(x => x.CreatedByUserId == userId);
            }

            var cashExpensesList = cashExpenses.ToList().Select(x => new ListViewModel()
            {
                Charge = x.TransactionType == TransactionType.Expense ? x.Amount : x.TransactionType == TransactionType.Transfer && x.CreatedByUserId != x.ReceiverUserId ? x.Amount : 0,
                Concept = x.Concept,
                DateObject = x.CreatedOnUtc,
                Date = x.CreatedOnUtc.ToLocalTime().ToString("dd-MM-yyyy"),
                Deposit = x.TransactionType == TransactionType.Deposit ? x.Amount : x.TransactionType == TransactionType.Transfer && x.CreatedByUserId == x.ReceiverUserId ? x.Amount : 0,
                Id = x.Id,
                User = ConvertToCustomerModel(_customerService.GetCustomerById(x.CreatedByUserId)),
                AttachmentsCount = _cashExpenseFileService.GetAll().Where(y => y.CashExpenseId == x.Id).Count(),
                Attachments = _cashExpenseFileService.GetAll().Where(y => y.CashExpenseId == x.Id).Select(y => y.FileUrl).ToList(),
                Type = "CashExpense"
            }).ToList();

            var expensesOfCashList = expensesOfCash.ToList().Select(x => new ListViewModel()
            {
                Charge = x.Total,
                Concept = x.Concept,
                DateObject = new DateTime(x.ExpenseDate.Year, x.ExpenseDate.Month, x.ExpenseDate.Day, x.CreatedOnUtc.Hour, x.CreatedOnUtc.Minute, x.CreatedOnUtc.Second),
                Date = x.ExpenseDate.ToLocalTime().ToString("dd-MM-yyyy"),
                Deposit = 0,
                Id = x.Id,
                User = ConvertToCustomerModel(_customerService.GetCustomerById(x.CreatedByUserId)),
                AttachmentsCount = _expenseFileService.GetAll().Where(y => y.ExpenseId == x.Id).Count(),
                Attachments = _expenseFileService.GetAll().Where(y => y.ExpenseId == x.Id).Select(y => y.FileUrl).ToList(),
                Type = "ExpenseOfCash"
            }).ToList();

            var ordersListTask = orders.ToList().Select(async x => new ListViewModel()
            {
                Charge = 0,
                Concept = "Pago de orden en efectivo",
                Date = x.PaidDateUtc.Value.ToLocalTime().ToString("dd-MM-yyyy"),
                Deposit = x.OrderTotal,
                DateObject = x.PaidDateUtc.Value,
                AttachmentsCount = 0,
                User = await GetOrderResponsable(x),
                Attachments = new List<string>(),
                Type = "OrderPayment",
                Id = x.Id
            });
            var ordersList = await Task.WhenAll(ordersListTask);

            var orderReportsList = orderReports.ToList().Select(x => new ListViewModel()
            {
                Charge = x.RequestedQtyCost,
                Concept = $"Pago por el producto '{_orderService.GetOrderItemById(x.OrderItemId)?.Product.Name}' de la orden #{_orderService.GetOrderById(x.OrderId).CustomOrderNumber}",
                Date = x.CreatedOnUtc.ToLocalTime().ToString("dd-MM-yyyy"),
                AttachmentsCount = x.FilesUrl.Count(),
                Attachments = x.FilesUrl,
                DateObject = x.CreatedOnUtc,
                Deposit = 0,
                User = ConvertToCustomerModel(_customerService.GetCustomerById(x.OrderResponsableId)),
                Type = "OrderReport",
                Id = x.OrderId
            }).ToList();

            if (userId > 0)
            {
                ordersList = ordersList.Where(x => x.User.Id == userId).ToArray();
            }

            var globalList = cashExpensesList.Union(ordersList).Union(orderReportsList).Union(expensesOfCashList).OrderBy(x => x.DateObject).GroupBy(x => x.User.Id);
            foreach (var user in globalList)
            {
                decimal currentBalance = 0;
                foreach (var element in user)
                {
                    currentBalance += element.Deposit;
                    currentBalance -= element.Charge;
                    element.Balance = currentBalance;
                }
            }
            
            //var pagedList = new PagedList<ListViewModel>(, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = globalList.SelectMany(x => x).Reverse().ToList(),
                Total = globalList.SelectMany(x => x).Count()
            };

            return Json(gridModel);
        }

        private async Task<CustomerModel> GetOrderResponsable(Order order)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = (Request.IsHttps ? "https" : "http") + $"://{Request.Host}/Admin/ShippingRoute/GetResponsable?routeId=" + order.RouteId + "&date=" + order.SelectedShippingDate.Value.ToString("dd-MM-yyyy");
                var result = await client.GetAsync(url);
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<CustomerModel>(json);
                }
            }
            return null;
        }

        private async Task<List<OrderReportModel>> GetOrderReports(int customerId)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = (Request.IsHttps ? "https" : "http") + $"://{Request.Host}/Admin/OrderReport/GetOrderReports?customerId=" + customerId;
                var result = await client.GetAsync(url);
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<List<OrderReportModel>>(json);
                }
            }
            return new List<OrderReportModel>();
        }

        private CustomerModel ConvertToCustomerModel(Customer customer)
        {
            return new CustomerModel()
            {
                Email = customer.Email,
                FullName = customer.GetFullName(),
                Id = customer.Id
            };
        }
    }
}
