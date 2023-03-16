using iText.Forms.Xfdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.Franchise;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;
using Teed.Plugin.Groceries.Domain.ShippingVehicles;
using Teed.Plugin.Groceries.Models.Franchise;
using Teed.Plugin.Groceries.Models.RatesAndBonuses;
using Teed.Plugin.Groceries.Models.ShippingUserRoute;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class FranchiseController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly ShippingRouteService _shippingRouteService;
        private readonly ShippingRouteUserService _shippingRouteUserService;
        private readonly IOrderService _orderService;
        private readonly FranchiseService _franchiseService;
        private readonly ISettingService _settingService;
        private readonly BillingService _billingService;
        private readonly IncidentsService _incidentsService;
        private readonly PaymentFileService _billingFileService;
        private readonly ShippingVehicleService _shippingVehicleService;
        private readonly ShippingVehicleRouteService _shippingVehicleRouteService;
        private readonly PaymentService _paymentService;
        private readonly FranchiseBonusService _franchiseBonusService;
        private readonly PenaltiesCatalogService _penaltiesCatalogService;
        private readonly FranchiseMonthlyChargeService _franchiseMonthlyChargeService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public FranchiseController(IPermissionService permissionService, ShippingRouteService shippingRouteService,
            ShippingRouteUserService shippingRouteUserService, IWorkContext workContext, ICustomerService customerService,
            IOrderService orderService, FranchiseService franchiseService, ISettingService settingService,
            BillingService billingService, IncidentsService incidentsService, PaymentFileService billingFileService,
            IDateTimeHelper dateTimeHelper, ShippingVehicleService shippingVehicleService, ShippingVehicleRouteService shippingVehicleRouteService,
            PaymentService paymentService, FranchiseBonusService franchiseBonusService, PenaltiesCatalogService penaltiesCatalogService,
            FranchiseMonthlyChargeService franchiseMonthlyChargeService)
        {
            _permissionService = permissionService;
            _shippingRouteService = shippingRouteService;
            _shippingRouteUserService = shippingRouteUserService;
            _workContext = workContext;
            _customerService = customerService;
            _orderService = orderService;
            _franchiseService = franchiseService;
            _settingService = settingService;
            _billingService = billingService;
            _incidentsService = incidentsService;
            _billingFileService = billingFileService;
            _dateTimeHelper = dateTimeHelper;
            _shippingVehicleService = shippingVehicleService;
            _shippingVehicleRouteService = shippingVehicleRouteService;
            _paymentService = paymentService;
            _franchiseBonusService = franchiseBonusService;
            _penaltiesCatalogService = penaltiesCatalogService;
            _franchiseMonthlyChargeService = franchiseMonthlyChargeService;
        }

        public string GetStatus(Payment payment)
        {
            var files = _billingFileService.GetAll()
                .Where(x => x.PaymentId == payment.Id)
                .ToList();
            var pdfs = files.Where(x =>
                x.FileTypeId == (int)PaymentFileType.BillPdf)
                .Count();
            var xmls = files.Where(x =>
                x.FileTypeId == (int)PaymentFileType.BillXml)
                .Count();
            var final = string.Empty;
            if (payment.StatusId == (int)PaymentStatus.Paid)
                final = "green";
            else if (payment.StatusId == (int)PaymentStatus.Pending &&
                pdfs > 0 && xmls > 0)
                final = "red";
            else
                final = "yellow";
            return final;
        }

        public void PrepareInfoModel(Franchise franchise, InfoModel model)
        {
            model.FranchiseId = franchise.Id;
            model.FranchiseName = franchise.Name;

            var franchiseVehicles = _shippingVehicleService.GetAll()
                .Where(x => x.FranchiseId == franchise.Id)
                .ToList();

            RatesAndBonusesSettings settings = _settingService.LoadSetting<RatesAndBonusesSettings>();

            if (franchiseVehicles.Any())
            {
                DateTime now = DateTime.Now.Date;
                var yesterday = now.AddDays(-1);
                var fullStatmentData = GetFullStatmentData(franchise.Id);
                List<int> franchiseVehicleIds = franchiseVehicles.Select(x => x.Id).ToList();

                var incidents = _incidentsService.GetAll()
                    .Where(x => x.FranchiseId == franchise.Id && x.AuthorizedStatusId == 1)
                    .ToList();

                model.ActiveVehicles = franchiseVehicles.Where(x => x.Active).Count();

                var yesterdayStatmentData = fullStatmentData.Where(x => x.Date == yesterday).FirstOrDefault();
                var balanceDue = yesterdayStatmentData?.TotalCapital ?? 0;
                model.BalanceDue = balanceDue;
                model.BalanceToBeReleased = (yesterdayStatmentData?.BalanceToBeReleased ?? 0) - balanceDue;
                model.Capital = fullStatmentData.Where(x => x.Date <= yesterday).Select(x => x.Capital).DefaultIfEmpty().Sum();
                model.TotalPayments = fullStatmentData.Where(x => x.Date <= yesterday).Select(x => x.TotalPayments).DefaultIfEmpty().Sum();

                var lastPayment = _paymentService.GetAll().Where(x => x.FranchiseId == franchise.Id && x.StatusId == 1 && x.PaymentDate.HasValue).OrderByDescending(x => x.PaymentDate).FirstOrDefault();
                model.LastPaymentDate = lastPayment?.PaymentDate;

                var controlDate = now.AddDays(-30);
                model.TotalIncidentsLast30Days = incidents.Where(x => x.Date >= controlDate && x.Date <= now && x.AuthorizedStatusId == 1).Count();

                // customers & routes
                var customerIds = string.IsNullOrWhiteSpace(franchise.BuyersIds) ? new int[0] : franchise.BuyersIds.Split(',')
                    .Select(x => int.Parse(x)).ToArray();
                var customers = _customerService.GetCustomersByIds(customerIds);

                model.Customers = customers.Select(x => new CustomerData
                {
                    Id = x.Id,
                    Name = x.GetFullName(),
                    Active = x.Active
                }).ToList();
                model.Vehicles = franchiseVehicles.Select(x => new VehicleData
                {
                    Id = x.Id,
                    Name = VehicleUtils.GetVehicleName(x),
                    Active = x.Active
                }).ToList();

                var startOfWeek = DateTime.Today;
                var addDays = -((int)startOfWeek.DayOfWeek) + 1;
                if (addDays > 0 || addDays < 0)
                    startOfWeek = startOfWeek.AddDays(addDays);
                var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);

                model.InitDate = startOfWeek;
                model.EndDate = endOfWeek;

                for (int i = 0; i < 10; i++)
                {
                    if (i > 0)
                    {
                        startOfWeek = startOfWeek.AddDays(-7);
                        endOfWeek = endOfWeek.AddDays(-7);
                    }
                }

                model.VehiclesSelect = franchiseVehicles.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = VehicleUtils.GetVehicleName(x),
                }).ToList();
            }
        }


        public IActionResult FranchiseList()
        {
            var buyerRole = _customerService.GetCustomerRoleBySystemName("delivery");
            var deliveries = _customerService.GetAllCustomers(customerRoleIds: new int[] { buyerRole.Id });

            var deliveryIds = _franchiseService.GetAll().Select(x => x.BuyersIds).ToList();
            var assignedDeliveryIds = string.Join(",", deliveryIds).Split(',').Select(x => string.IsNullOrWhiteSpace(x) ? 0 : int.Parse(x.Trim())).ToList();
            List<Nop.Core.Domain.Customers.Customer> pendingDeliveries = deliveries.Where(x => !assignedDeliveryIds.Contains(x.Id)).ToList();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Franchise/FranchiseList.cshtml", pendingDeliveries);
        }

        public IActionResult AssignmentCheck(int franchiseId = 0)
        {
            var franchises = _franchiseService.GetAll().OrderBy(x => x.Name);
            var model = new PaymentListModel()
            {
                SelectedFranchiseId = franchiseId == 0 ? franchises.Select(x => x.Id).FirstOrDefault() : franchiseId,
                Franchises = franchises.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList()
            };

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Franchise/AssignmentCheck.cshtml", model);
        }

        [HttpPost]
        public IActionResult AssignmentCheckData(DataSourceRequest command, int franchiseId = 0)
        {
            var franchiseVehicles = _shippingVehicleService.GetAll().Where(x => x.FranchiseId == franchiseId).ToList();
            var franchiseOrders = OrderUtils.GetAllFranchiseOrders(franchiseVehicles, _shippingVehicleRouteService, _orderService);
            var franchiseOrdersDates = franchiseOrders.Select(x => x.SelectedShippingDate).GroupBy(x => x).Select(x => x.Key).OrderByDescending(x => x);

            var pagedList = new PagedList<DateTime?>(franchiseOrdersDates, command.Page - 1, command.PageSize);
            var vehicleRoutes = _shippingVehicleRouteService.GetAll().Where(x => x.Vehicle.FranchiseId == franchiseId && pagedList.Contains(x.ShippingDate)).ToList();
            var franchiseUserIds = _franchiseService.GetAll().Where(x => x.Id == franchiseId).Select(x => x.BuyersIds).FirstOrDefault().Split(',').Select(x => x.Trim()).ToList();

            var franchises = _franchiseService.GetAll().Select(x => new { x.BuyersIds, x.Name }).ToList();

            var customers = _customerService.GetAllCustomersQuery().Where(x => x.Email != null).Select(x => new { Id = x.Id.ToString(), x.Email }).ToList();
            var routeUserInCharge = _shippingRouteUserService.GetAll().Select(x => new { x.ResponsableDateUtc, x.ShippingRouteId, x.UserInChargeId }).ToList();

            var data = new List<dynamic>();
            foreach (var item in pagedList)
            {
                var dateVehicleRoute = vehicleRoutes.Where(x => x.ShippingDate == item).ToList();

                foreach (var vehicleRoute in dateVehicleRoute)
                {
                    var routeUserInChargeIds = routeUserInCharge
                    .Where(x => x.ResponsableDateUtc == item && x.ShippingRouteId == vehicleRoute.RouteId)
                    .Select(x => x.UserInChargeId.ToString())
                    .ToList();

                    data.Add(new
                    {
                        Date = item.Value.ToString("dd-MM-yyyy"),
                        Vehicle = VehicleUtils.GetVehicleName(vehicleRoute.Vehicle),
                        UsedExternalUser = routeUserInChargeIds.Except(franchiseUserIds).Any(),
                        DeliveryUsers = customers
                        .Where(x => routeUserInChargeIds.Contains(x.Id))
                        .Select(x => x.Email + $" ({franchises.Where(y => y.BuyersIds.Contains(x.Id.ToString())).Select(y => y.Name).FirstOrDefault()})")
                        .ToList(),
                        Route = vehicleRoute.Route.RouteName
                    });
                }
            }

            var gridModel = new DataSourceResult
            {
                Data = data,
                Total = franchiseOrdersDates.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult FranchiseListData(DataSourceRequest command)
        {
            var query = _franchiseService.GetAll();
            var userId = _workContext.CurrentCustomer.Id;
            var userRolefranchisIds = _workContext.CurrentCustomer.CustomerRoles.Select(x => x.Id);

            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.Franchise))
            {
                query = query.Where(x => x.UserInChargeId == userId);
            }


            var queryList = query.OrderByDescending(m => m.CreatedOnUtc).ToList();
            var pagedList = new PagedList<Franchise>(queryList, command.Page - 1, command.PageSize);

            var allFranchiseVehicles = _shippingVehicleService.GetAll().ToList();

            var allBills = _billingService.GetAll().ToList();

            var allIncidents = _incidentsService.GetAll()
                .Where(x => x.AuthorizedStatusId == 1)
                .ToList();

            var allFranchisePayments = _paymentService.GetAll().Where(x => x.StatusId == 1 && x.PaymentDate.HasValue).ToList();

            var twoWeeks = DateTime.Now.AddDays(-14).Date;

            var monthlyCharges = _franchiseMonthlyChargeService.GetAll()
                    .Where(x => x.AuthorizedStatusId == 1).ToList();

            var allBonus = _franchiseBonusService.GetAll().ToList();
            var data = new List<object>();

            foreach (var franchise in pagedList)
            {
                var franchiseVehicleIds = allFranchiseVehicles.Where(x => x.FranchiseId == franchise.Id).Select(x => x.Id).ToList();
                var franchiseBonus = allBonus.Where(x => x.FranchiseId == franchise.Id).ToList();
                var franchiseBills = allBills.Where(x => franchiseVehicleIds.Contains(x.VehicleId)).ToList();
                var franchiseIncidents = allIncidents.Where(x => x.FranchiseId == franchise.Id).ToList();

                var franchiseMonthlyCharges = monthlyCharges
                    .Where(x => x.FranchiseId == franchise.Id);

                var franchisePayments = allFranchisePayments.Where(x => x.FranchiseId == franchise.Id);

                var twoWeeksBills = franchiseBills.Where(x => x.EndDate < twoWeeks).Select(x => x.Billed * 1.16m).DefaultIfEmpty().Sum();
                var twoWeeksBonus = franchiseBonus.Where(x => x.Date < twoWeeks).Select(x => x.Amount * 1.16m).DefaultIfEmpty().Sum();
                var twoWeeksIncidents = franchiseIncidents.Where(x => x.Date < twoWeeks).Select(x => x.Amount * 1.16m).DefaultIfEmpty().Sum();
                var twoWeeksMonthlyCharges = franchiseMonthlyCharges.Where(x => x.Date < twoWeeks).Select(x => x.Amount).DefaultIfEmpty().Sum();

                var totalBilled = franchiseBills.Select(x => x.Billed * 1.16m).DefaultIfEmpty().Sum() +
                    franchiseBonus.Select(x => x.Amount * 1.16m).DefaultIfEmpty().Sum() -
                    franchiseIncidents.Select(x => x.Amount * 1.16m).DefaultIfEmpty().Sum() -
                    franchiseMonthlyCharges.Select(x => x.Amount).DefaultIfEmpty().Sum() -
                    franchisePayments.Select(x => x.PaymentAmount).DefaultIfEmpty().Sum();

                var balanceDue = twoWeeksBills + twoWeeksBonus - twoWeeksIncidents - twoWeeksMonthlyCharges - franchisePayments.Select(x => x.PaymentAmount).DefaultIfEmpty().Sum();
                var lastPayment = franchisePayments.OrderByDescending(x => x.PaymentDate).Select(x => x.PaymentDate).FirstOrDefault();

                data.Add(new
                {
                    Id = franchise.Id,
                    Name = franchise.Name,
                    TotalBilled = totalBilled < 0 ? "-" + Math.Abs(totalBilled).ToString("C") : totalBilled.ToString("C"),
                    BalanceDue = balanceDue < 0 ? "-" + Math.Abs(balanceDue).ToString("C") : balanceDue.ToString("C"),
                    LastPaymentDate = lastPayment.HasValue ? lastPayment.Value.ToString("dd-MM-yyyy") : "Nunca pagado"
                });
            }

            var gridModel = new DataSourceResult
            {
                Data = data,
                Total = query.Count()
            };

            return Json(gridModel);
        }

        private dynamic GetPaymentsData(DataSourceRequest command, int franchiseId, bool isAdmin)
        {
            var payments = _paymentService.GetAll().Where(x => x.FranchiseId == franchiseId).OrderByDescending(x => x.CreatedOnUtc);
            var pagedList = new PagedList<Payment>(payments, command.Page - 1, command.PageSize);

            var data = new List<dynamic>();
            foreach (var item in pagedList)
            {
                var pdfIds = item.PaymentFiles.Where(x => !x.Deleted && x.FileTypeId == (int)PaymentFileType.BillPdf).Select(x => x.Id).ToList();
                var xmlIds = item.PaymentFiles.Where(x => !x.Deleted && x.FileTypeId == (int)PaymentFileType.BillXml).Select(x => x.Id).ToList();
                var otherFileIds = item.PaymentFiles.Where(x => !x.Deleted && x.FileTypeId == (int)PaymentFileType.ProofOfPaiment).Select(x => x.Id).ToList();
                var customer = _customerService.GetCustomerById(item.VerifiedByCustomerId ?? 0);
                var customerName = customer != null ? customer.GetFullName() + " (" + customer.Email + ")" : string.Empty;
                data.Add(new
                {
                    item.Id,
                    PdfIds = pdfIds,
                    XmlIds = xmlIds,
                    OtherFileIds = otherFileIds,
                    Date = item.PaymentDate.HasValue ? item.PaymentDate.Value.ToString("dd-MM-yyyy") : "No pagado",
                    Amount = item.PaymentAmount.ToString("C"),
                    Status = !item.PaymentFiles.Where(y => y.FileTypeId == 0 && !y.Deleted).Any() || !item.PaymentFiles.Where(y => y.FileTypeId == 1 && !y.Deleted).Any() ? "red" : item.StatusId == 1 && item.PaymentFiles.Where(y => y.FileTypeId != 2 && !y.Deleted).Any() ? "green" : "yellow",
                    item.Comment,
                    VerifiedByCustomerId = isAdmin ? (item.VerifiedByCustomerId ?? 0) : 0,
                    VerifiedByCustomer = isAdmin ? customerName : string.Empty,
                    VerifiedDate = isAdmin ? (item.VerifiedDate.HasValue ? item.VerifiedDate.Value.ToString("dd-MM-yyyy hh:mm:ss tt") : "No verificado") : "No verificado",
                });
            }
            return data;
        }

        [HttpPost]
        public IActionResult PaymentsData(DataSourceRequest command, int franchiseId)
        {
            bool userIsInCharge = _franchiseService.GetAll().Where(x => x.UserInChargeId == _workContext.CurrentCustomer.Id && x.Id == franchiseId).Any();
            if (!userIsInCharge && !_permissionService.Authorize(TeedGroceriesPermissionProvider.Franchise))
                return AccessDeniedView();

            var payments = _paymentService.GetAll().Where(x => x.FranchiseId == franchiseId).OrderByDescending(x => x.CreatedOnUtc);
            var gridModel = new DataSourceResult
            {
                Data = GetPaymentsData(command, franchiseId, _permissionService.Authorize(TeedGroceriesPermissionProvider.Franchise)),
                Total = payments.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult StatementData(DataSourceRequest command, int franchiseId)
        {
            var result = GetStatementData(franchiseId);

            var gridModel = new DataSourceResult
            {
                Data = ((List<dynamic>)result.data).OrderByDescending(x => x.DateObject).ThenByDescending(x => x.ElementId).ThenByDescending(x => x.Balance + x.Amount),
                Total = result.total
            };

            return Json(gridModel);
        }

        public IActionResult ExportStatement(int franchiseId)
        {
            bool userIsInCharge = _franchiseService.GetAll().Where(x => x.UserInChargeId == _workContext.CurrentCustomer.Id && x.Id == franchiseId).Any();
            if (!userIsInCharge && !_permissionService.Authorize(TeedGroceriesPermissionProvider.Franchise))
                return AccessDeniedView();

            var result = GetStatementData(franchiseId);
            var data = (List<dynamic>)result.data;
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("Detalle de movimientos");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Fecha";
                    worksheet.Cells[row, 2].Value = "Comisiones / abonos a cuenta";
                    worksheet.Cells[row, 3].Value = "Importe";
                    worksheet.Cells[row, 4].Value = "IVA";
                    worksheet.Cells[row, 5].Value = "Total";
                    //worksheet.Cells[row, 6].Value = "Saldo";

                    foreach (var item in data)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = item.Date;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 2].Value = item.Description;
                        worksheet.Cells[row, 3].Value = (!item.IsAdding ? -1 : 1) * (item.Amount == "-" ? 0 : decimal.Parse(item.Amount, NumberStyles.Currency));
                        worksheet.Cells[row, 4].Value = (!item.IsAdding ? -1 : 1) * (item.Tax == "-" ? 0 : decimal.Parse(item.Tax, NumberStyles.Currency));
                        worksheet.Cells[row, 5].Value = (!item.IsAdding ? -1 : 1) * (item.Total == "-" ? 0 : decimal.Parse(item.Total, NumberStyles.Currency));
                        //worksheet.Cells[row, 6].Value = item.Balance == "-" ? 0 : decimal.Parse(item.Balance, NumberStyles.Currency);
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    var worksheet2 = xlPackage.Workbook.Worksheets.Add("Estado de cuenta");
                    row = 1;
                    int col = 1;
                    worksheet2.Cells[row, col].Value = "Fecha de movimiento";
                    col++;
                    worksheet2.Cells[row, col].Value = "Fecha de exigibilidad";
                    col++;
                    worksheet2.Cells[row, col].Value = "Fecha de vencimiento";
                    col++;
                    worksheet2.Cells[row, col].Value = "Cargos";
                    col++;
                    worksheet2.Cells[row, col].Value = "Cargos exigibles";
                    col++;
                    worksheet2.Cells[row, col].Value = "Cargos vencidos";
                    col++;
                    worksheet2.Cells[row, col].Value = "Abonos";
                    col++;
                    worksheet2.Cells[row, col].Value = "Saldo por liberar";
                    col++;
                    worksheet2.Cells[row, col].Value = "Saldo exigible";
                    col++;
                    worksheet2.Cells[row, col].Value = "Saldo vencido";
                    col++;

                    //
                    worksheet2.Cells[row, col].Value = "   ";
                    col++;
                    //

                    worksheet2.Cells[row, col].Value = "Capital";
                    col++;
                    worksheet2.Cells[row, col].Value = "Interés simple del periodo";
                    col++;
                    worksheet2.Cells[row, col].Value = "Abonos a capital";
                    col++;
                    worksheet2.Cells[row, col].Value = "Abonos a intereses";
                    col++;
                    worksheet2.Cells[row, col].Value = "Abonos totales";
                    col++;
                    worksheet2.Cells[row, col].Value = "Saldo Capital";
                    col++;
                    worksheet2.Cells[row, col].Value = "Saldo Intereses";
                    col++;
                    worksheet2.Cells[row, col].Value = "Saldo total";
                    col++;

                    var FullStatmentData = GetFullStatmentData(franchiseId);
                    foreach (var statmentData in FullStatmentData)
                    {
                        col = 1;
                        row++;

                        // Fecha de movimiento
                        worksheet2.Cells[row, col].Value = statmentData.Date;
                        worksheet2.Cells[row, col].Style.Numberformat.Format = "dd-mm-yyyy";
                        col++;

                        // Fecha de exigibilidad
                        worksheet2.Cells[row, col].Value = statmentData.PayableDate;
                        worksheet2.Cells[row, col].Style.Numberformat.Format = "dd-mm-yyyy";
                        col++;

                        // Fecha de vencimiento
                        worksheet2.Cells[row, col].Value = statmentData.ExpiredDate;
                        worksheet2.Cells[row, col].Style.Numberformat.Format = "dd-mm-yyyy";
                        col++;

                        // Cargos
                        worksheet2.Cells[row, col].Value = statmentData.Charges;
                        worksheet2.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        col++;

                        // Cargos exigibles
                        worksheet2.Cells[row, col].Value = statmentData.PayableCharges;
                        worksheet2.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        col++;

                        // Cargos vencidos
                        worksheet2.Cells[row, col].Value = statmentData.ExpiredCharges;
                        worksheet2.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        col++;

                        // Abonos
                        worksheet2.Cells[row, col].Value = statmentData.Payments;
                        worksheet2.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        col++;

                        // Saldo por liberar
                        worksheet2.Cells[row, col].Value = statmentData.BalanceToBeReleased;
                        worksheet2.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        col++;

                        // Saldo exigible
                        worksheet2.Cells[row, col].Value = statmentData.PayableToBeReleased;
                        worksheet2.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        col++;

                        // Saldo vencido
                        worksheet2.Cells[row, col].Value = statmentData.ExpiredToBeReleased;
                        worksheet2.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        col++;

                        //
                        col++;
                        //

                        // Capital
                        worksheet2.Cells[row, col].Value = statmentData.Capital;
                        worksheet2.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        col++;

                        // Interés simple del periodo
                        worksheet2.Cells[row, col].Value = statmentData.SimpleInterestPeriod;
                        worksheet2.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        col++;

                        // Abonos a capital
                        worksheet2.Cells[row, col].Value = statmentData.CapitalBonus;
                        worksheet2.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        col++;

                        // Abonos a intereses
                        worksheet2.Cells[row, col].Value = statmentData.PaymentInterest;
                        worksheet2.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        col++;

                        // Abonos totales
                        worksheet2.Cells[row, col].Value = statmentData.TotalPayments;
                        worksheet2.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        col++;

                        // Saldo Capital
                        worksheet2.Cells[row, col].Value = statmentData.CapitalAmount;
                        worksheet2.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        col++;

                        // Saldo Intereses
                        worksheet2.Cells[row, col].Value = statmentData.InterestAmount;
                        worksheet2.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        col++;

                        // Saldo total
                        worksheet2.Cells[row, col].Value = statmentData.TotalCapital;
                        worksheet2.Cells[row, col].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        col++;

                        // 19
                    }

                    for (int i = 1; i <= worksheet2.Dimension.End.Column; i++)
                    {
                        worksheet2.Column(i).AutoFit();
                        worksheet2.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"estado_de_cuenta_{DateTime.Now:ddMMyyyy}.xlsx");
            }
        }

        private List<FullStatementData> GetFullStatmentData(int franchiseId)
        {
            var result = GetStatementData(franchiseId);
            var data = (List<dynamic>)result.data;

            var dateGrouping = data.GroupBy(x => DateTime.ParseExact(x.Date, "dd-MM-yyyy", CultureInfo.InvariantCulture))
                .ToList();
            var datesOfGrouping = dateGrouping.Select(x => (DateTime)x.Key).ToList();
            var yesterday = DateTime.Now.Date.AddDays(-1);
            var datesBetween = Enumerable.Range(0, 1 + yesterday.Subtract(datesOfGrouping.FirstOrDefault()).Days)
                                     .Select(offset => datesOfGrouping.FirstOrDefault().AddDays(offset))
                                     .OrderBy(x => x)
                                     .ToList();
            var monetaryValue = (decimal)(0.18 / 365);

            var fullStatementDatas = new List<FullStatementData>();
            var count = 0;
            foreach (var date in datesBetween)
            {
                var fullStatementData = new FullStatementData();

                // Fecha de movimiento
                fullStatementData.Date = date;

                // Fecha de exigibilidad
                var payableDate = count > 0 ? date.AddDays(-30) : date.AddDays(-15);
                fullStatementData.PayableDate = payableDate;

                // Fecha de vencimiento
                var expiredDate = payableDate.AddDays(-30);
                fullStatementData.ExpiredDate = expiredDate;

                var grouping = dateGrouping.Where(x => x.Key == date).FirstOrDefault();
                var items = grouping != null ? grouping.ToList() : null;
                // Cargos
                var charges = items != null ? items.Where(x => (bool)x.IsAdding)
                    .Select(x => (decimal)(x.Total == "-" ? 0 : decimal.Parse(x.Total, NumberStyles.Currency)))
                    .DefaultIfEmpty().Sum() : 0;
                fullStatementData.Charges = charges;

                // Cargos exigibles
                var payableCharges = (decimal)0;
                var itemsOfPayableDate = dateGrouping.Where(x => x.Key == payableDate).FirstOrDefault();
                if (itemsOfPayableDate != null)
                    payableCharges = itemsOfPayableDate.Where(x => (bool)x.IsAdding)
                        .Select(x => (decimal)(x.Total == "-" ? 0 : decimal.Parse(x.Total, NumberStyles.Currency)))
                        .DefaultIfEmpty().Sum();
                fullStatementData.PayableCharges = payableCharges;

                // Cargos vencidos
                var expiredCharges = (decimal)0;
                var itemsOfExpiredDate = dateGrouping.Where(x => x.Key == expiredDate).FirstOrDefault();
                if (itemsOfExpiredDate != null)
                    expiredCharges = itemsOfExpiredDate.Where(x => (bool)x.IsAdding)
                        .Select(x => (decimal)(x.Total == "-" ? 0 : decimal.Parse(x.Total, NumberStyles.Currency)))
                        .DefaultIfEmpty().Sum();
                fullStatementData.ExpiredCharges = expiredCharges;

                // Abonos
                var payments = -1 * (items != null ? items.Where(x => !(bool)x.IsAdding)
                    .Select(x => (decimal)(x.Total == "-" ? 0 : decimal.Parse(x.Total, NumberStyles.Currency)))
                    .DefaultIfEmpty().Sum() : 0);
                fullStatementData.Payments = payments;

                // Saldo por liberar
                var balanceToBeReleased = (count > 0 ? fullStatementDatas.LastOrDefault().BalanceToBeReleased : 0) + charges + payments;
                fullStatementData.BalanceToBeReleased = balanceToBeReleased;

                // Saldo exigible
                var payableToBeReleased = (count > 0 ? fullStatementDatas.LastOrDefault().PayableToBeReleased : 0) + payableCharges + payments;
                fullStatementData.PayableToBeReleased = payableToBeReleased;

                // Saldo vencido
                var expiredToBeReleased = (count > 0 ? fullStatementDatas.LastOrDefault().ExpiredToBeReleased : 0) + expiredCharges + payments;
                fullStatementData.ExpiredToBeReleased = expiredToBeReleased;

                // Capital
                var capital = payableCharges;
                fullStatementData.Capital = capital;

                // Interés simple del periodo
                var simpleInterestPeriod = expiredToBeReleased > 0 ? expiredToBeReleased * monetaryValue : 0;
                fullStatementData.SimpleInterestPeriod = simpleInterestPeriod;

                var lastInterestBlance = count > 0 ? fullStatementDatas.LastOrDefault().InterestAmount : 0;
                var totalPayments = payments;
                var capitalBonus = count > 0 ?
                    ((lastInterestBlance + simpleInterestPeriod) > (-1 * totalPayments) ? 0 : (lastInterestBlance + simpleInterestPeriod) + totalPayments)
                    : totalPayments;
                var paymentInterest = count > 0 ?
                    (lastInterestBlance + simpleInterestPeriod == 0 ? 0 :
                        ((lastInterestBlance + simpleInterestPeriod) > (-1 * totalPayments) ? totalPayments : -1 * (lastInterestBlance + simpleInterestPeriod)))
                    : 0;

                // Abonos a capital
                fullStatementData.CapitalBonus = capitalBonus;

                // Abonos a intereses
                fullStatementData.PaymentInterest = paymentInterest;

                // Abonos totales
                fullStatementData.TotalPayments = totalPayments;

                // Saldo Capital
                var capitalAmount = capital + capitalBonus + (count > 0 ? fullStatementDatas.LastOrDefault().CapitalAmount : 0);
                fullStatementData.CapitalAmount = capitalAmount;

                // Saldo Intereses
                var interestAmount = simpleInterestPeriod + paymentInterest + (count > 0 ? fullStatementDatas.LastOrDefault().InterestAmount : 0);
                fullStatementData.InterestAmount = interestAmount;

                // Saldo total
                fullStatementData.TotalCapital = capitalAmount + interestAmount;

                // 19

                fullStatementDatas.Add(fullStatementData);
                count++;
            }

            return fullStatementDatas;
        }

        [HttpGet]
        public IActionResult ExportPayments(int franchiseId)
        {
            bool userIsInCharge = _franchiseService.GetAll().Where(x => x.UserInChargeId == _workContext.CurrentCustomer.Id && x.Id == franchiseId).Any();
            if (!userIsInCharge && !_permissionService.Authorize(TeedGroceriesPermissionProvider.Franchise))
                return AccessDeniedView();

            var paymentsOfFranchise = GetPaymentsData(new DataSourceRequest { Page = 1, PageSize = int.MaxValue }, franchiseId, _permissionService.Authorize(TeedGroceriesPermissionProvider.Franchise));
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("Detalle de pagos");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Identificador";
                    worksheet.Cells[row, 2].Value = "Fecha de pago";
                    worksheet.Cells[row, 3].Value = "Monto";
                    worksheet.Cells[row, 4].Value = "Estatus";
                    worksheet.Cells[row, 5].Value = "Comentarios";
                    worksheet.Cells[row, 6].Value = "Factura en PDF subida";
                    worksheet.Cells[row, 7].Value = "Factura en XML subida";
                    worksheet.Cells[row, 8].Value = "Comprobante de pago subido";
                    worksheet.Cells[row, 9].Value = "Verificado por";
                    worksheet.Cells[row, 10].Value = "Fecha de verificación";

                    foreach (var paymentOfFranchise in paymentsOfFranchise)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = paymentOfFranchise.Id;
                        worksheet.Cells[row, 2].Value = paymentOfFranchise.Date != "No pagado" ? DateTime.ParseExact(paymentOfFranchise.Date, "dd-MM-yyyy", CultureInfo.InvariantCulture) : "";
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells[row, 3].Value = decimal.Parse(((string)paymentOfFranchise.Amount).Replace("$", "").Replace(",", ""));
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        worksheet.Cells[row, 4].Value = paymentOfFranchise.Status == "yellow" ? "Pendiente de pago" : paymentOfFranchise.Status == "red" ? "Factura pendiente" : "Pagado";
                        worksheet.Cells[row, 5].Value = paymentOfFranchise.Comment;
                        worksheet.Cells[row, 6].Value = ((List<int>)paymentOfFranchise.PdfIds).Count() > 0 ? "SI" : "NO";
                        worksheet.Cells[row, 7].Value = ((List<int>)paymentOfFranchise.XmlIds).Count() > 0 ? "SI" : "NO";
                        worksheet.Cells[row, 8].Value = ((List<int>)paymentOfFranchise.OtherFileIds).Count() > 0 ? "SI" : "NO";
                        worksheet.Cells[row, 9].Value = paymentOfFranchise.VerifiedByCustomer;
                        worksheet.Cells[row, 10].Value = paymentOfFranchise.VerifiedDate != "No verificado" ? DateTime.ParseExact(paymentOfFranchise.VerifiedDate, "dd-MM-yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) : "";
                        worksheet.Cells[row, 10].Style.Numberformat.Format = "dd-MM-yyyy hh:mm:ss tt";
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"pagos_a_franquicia_{DateTime.Now:ddMMyyyy}.xlsx");
            }
        }

        [HttpGet]
        public IActionResult VerifyPayment(int id)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.Franchise))
                return BadRequest("Acceso denegado.");

            var payment = _paymentService.GetAll()
                .Where(x => x.Id == id).FirstOrDefault();
            if (payment == null)
                return BadRequest("Pago no encontrado con id dado: " + id + ".");

            payment.VerifiedDate = DateTime.Now;
            payment.VerifiedByCustomerId = _workContext.CurrentCustomer.Id;
            _paymentService.Update(payment);

            return Ok(_workContext.CurrentCustomer.GetFullName() + " (" + _workContext.CurrentCustomer.Email + ")" + "<br>" + payment.VerifiedDate.Value.ToString("dd-MM-yyyy hh:mm:ss tt"));
        }

        private dynamic GetStatementData(int franchiseId)
        {
            var billsQuery = _billingService.GetAll().Where(x => x.Vehicle.FranchiseId == franchiseId).OrderByDescending(x => x.InitDate);
            var paymentsQuery = _paymentService.GetAll().Where(x => x.FranchiseId == franchiseId && x.StatusId == 1 && x.PaymentDate.HasValue).OrderByDescending(x => x.PaymentDate);
            var bonusQuery = _franchiseBonusService.GetAll().Where(x => x.FranchiseId == franchiseId).OrderByDescending(x => x.Date);
            var incidentsQuery = _incidentsService.GetAll().Where(x => x.FranchiseId == franchiseId && x.AuthorizedStatusId == 1).OrderByDescending(x => x.Date);
            var monthlyChargeQuery = _franchiseMonthlyChargeService.GetAll().Where(x => x.FranchiseId == franchiseId && x.AuthorizedStatusId == 1).OrderByDescending(x => x.Date);

            var billData = billsQuery.ToList().Select(x => new StatementData()
            {
                Description = $"Comisión semana del {x.InitDate.ToString("dd")} al {x.EndDate.ToString("dd 'de' MMMM 'de' yyyy", new CultureInfo("es-MX"))} {VehicleUtils.GetVehicleName(x.Vehicle)}",
                Payment = 0,
                Charge = x.Billed,
                Date = x.InitDate.AddSeconds(1),
                ElementId = x.Id,
                IsAdding = true,
            });

            var paymentsData = paymentsQuery.Select(x => new StatementData()
            {
                Description = "Abono a cuenta",
                Payment = x.PaymentAmount / 1.16m,
                Charge = 0,
                Date = x.PaymentDate.Value,
                ElementId = x.Id,
                IsAdding = false,
            });

            var bonusData = bonusQuery.Select(x => new StatementData()
            {
                Description = x.Comments,
                Payment = 0,
                Charge = x.Amount,
                Date = x.Date,
                ElementId = x.Id,
                IsAdding = true,
            });

            var incidentsData = incidentsQuery.Select(x => new StatementData()
            {
                Description = x.IncidentCode != "Z00" ? (x.Comments + ". Orden #" + x.OrderIds + ".") : x.Comments,
                Payment = x.Amount,
                Charge = 0,
                Date = x.Date,
                ElementId = x.Id,
                IsAdding = false,
            });

            var monthlyChargeData = monthlyChargeQuery.Select(x => new StatementData()
            {
                Description = x.Comments,
                Payment = x.Amount / 1.16m,
                Charge = 0,
                Date = x.Date,
                ElementId = x.Id,
                IsAdding = false,
            });

            var globalData = billData.Concat(paymentsData).Concat(bonusData).Concat(incidentsData).Concat(monthlyChargeData)
                .OrderBy(x => x.Date)
                .ThenBy(x => x.ElementId)
                .ThenBy(x => x.Balance + x.Charge)
                .ToList();
            decimal currentBalance = 0;

            var data = new List<dynamic>();
            foreach (var item in globalData)
            {
                decimal chargeTax = 0;
                decimal paymentTax = 0;
                if (item.Charge > 0)
                    chargeTax = item.Charge * 0.16m;
                else
                    paymentTax = item.Payment * 0.16m;
                currentBalance = currentBalance + (item.Charge + chargeTax) - (item.Payment + paymentTax);

                decimal tax = chargeTax > 0 ? chargeTax : paymentTax;
                data.Add(new
                {
                    Date = item.Date.ToString("dd-MM-yyyy"),
                    DateObject = item.Date,
                    Amount = item.Charge > 0 ? item.Charge.ToString("C") : item.Payment.ToString("C"),
                    Tax = tax == 0 ? "-" : tax.ToString("C"),
                    Total = item.Payment == 0 ? (item.Charge + tax).ToString("C") : (item.Payment + tax).ToString("C"),
                    Balance = (currentBalance < 0 ? "-" : "") + Math.Abs(currentBalance).ToString("C"),
                    Description = item.Description,
                    item.ElementId,
                    item.IsAdding
                });
            };

            return new { data, total = globalData.Count };
        }

        [HttpPost]
        public IActionResult FranchiseeList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var users = _customerService.GetAllCustomersQuery().Where(x => x.Email != null).ToList().Where(x => x.GetCustomerRoleIds().Count() > 0);
            var elements = users.Select(x => new
            {
                Id = x.Id,
                User = x.GetFullName()
            }).ToList();

            return Json(elements);
        }

        public IActionResult Create()
        {
            FranchiseData model = new FranchiseData();
            var franchiseeRole = _customerService.GetCustomerRoleBySystemName("franchise");
            var franchiseeList = _customerService.GetAllCustomers(customerRoleIds: new int[] { franchiseeRole.Id });
            model.AvailablesFranchisee = franchiseeList.Select(x => new SelectListItem()
            {
                Text = x.GetFullName(),
                Value = x.Id.ToString()
            }).ToList();

            model.AvailablesFranchisee.Add(new SelectListItem()
            {
                Value = "0",
                Text = "Selecciona un encargado para la franquicia",
                Selected = true
            });

            model.AvailablesFranchisee = model.AvailablesFranchisee.OrderBy(x => x.Text).ToList();

            var alreadyAssigned = _franchiseService.GetAll().Where(x => x.BuyersIds != null && x.BuyersIds != "").Select(x => x.BuyersIds).ToList();
            var alreadyAssignedIds = new List<int>();
            if (alreadyAssigned.Count > 0)
                alreadyAssignedIds = string.Join(",", alreadyAssigned).Split(',').Select(x => int.Parse(x.Trim())).ToList();

            var buyerRole = _customerService.GetCustomerRoleBySystemName("delivery");
            var buyersList = _customerService.GetAllCustomers(customerRoleIds: new int[] { buyerRole.Id }).Where(x => !alreadyAssignedIds.Contains(x.Id));

            model.AvailablesBuyers = buyersList.Select(x => new SelectListItem()
            {
                Text = x.GetFullName() + " (" + x.Email + ")",
                Value = x.Id.ToString()
            }).ToList();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Franchise/Create.cshtml", model);
        }

        [HttpPost]
        public IActionResult Create(FranchiseData model)
        {

            if (!ModelState.IsValid) return BadRequest();

            var countDelivery = 0;
            if (model.SelectedBuyersIds != null)
            {
                var otherFranchises = _franchiseService.GetAll().Where(x => x.Id != model.Id).ToList();
                var existigDeliveries = new List<string>();
                var selectedDeliverIds = model.SelectedBuyersIds;
                foreach (var item in otherFranchises)
                {
                    if (!string.IsNullOrWhiteSpace(item.BuyersIds))
                    {
                        var otherFranchiseDeliverIds = item.BuyersIds.Split(',').Select(x => int.Parse(x.Trim())).ToList();
                        foreach (var deliverId in otherFranchiseDeliverIds)
                        {
                            if (selectedDeliverIds.Contains(deliverId))
                                existigDeliveries.Add($"{_customerService.GetCustomerById(deliverId).Email} ({item.Name})");
                        }
                    }
                }

                if (existigDeliveries.Count > 0)
                {
                    var franchiseeRole = _customerService.GetCustomerRoleBySystemName("franchise");
                    var franchiseeList = _customerService.GetAllCustomers(customerRoleIds: new int[] { franchiseeRole.Id });
                    model.AvailablesFranchisee = franchiseeList.Select(x => new SelectListItem()
                    {
                        Text = x.GetFullName(),
                        Value = x.Id.ToString()
                    }).OrderBy(x => x.Text).ToList();

                    var alreadyAssigned = _franchiseService.GetAll().Where(x => x.BuyersIds != null && x.BuyersIds != "").Select(x => x.BuyersIds).ToList();
                    var alreadyAssignedIds = new List<int>();
                    if (alreadyAssigned.Count > 0)
                        alreadyAssignedIds = string.Join(",", alreadyAssigned).Split(',').Select(x => int.Parse(x.Trim())).ToList();

                    var buyerRole = _customerService.GetCustomerRoleBySystemName("delivery");
                    var buyersList = _customerService.GetAllCustomers(customerRoleIds: new int[] { buyerRole.Id }).Where(x => !alreadyAssignedIds.Contains(x.Id));

                    model.AvailablesBuyers = buyersList.Select(x => new SelectListItem()
                    {
                        Text = x.GetFullName() + " (" + x.Email + ")",
                        Value = x.Id.ToString()
                    }).ToList();

                    model.ExistigDeliveries = existigDeliveries;

                    return View("~/Plugins/Teed.Plugin.Groceries/Views/Franchise/Create.cshtml", model);
                }

                foreach (var item in model.SelectedBuyersIds)
                {
                    countDelivery++;
                    if (countDelivery < model.SelectedBuyersIds.Count())
                    {
                        model.BuyersIds += item + ", ";
                    }
                    else
                    {
                        model.BuyersIds += item;
                    }

                }
            }

            Franchise shippingRoute = new Franchise()
            {
                Name = model.Name,
                IsActive = model.IsActive,
                BuyersIds = model.BuyersIds,
                UserInChargeId = model.UserInChargeId,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) creó la franquicia {model.Name}.\n"
            };
            _franchiseService.Insert(shippingRoute);

            return RedirectToAction("FranchiseList");
        }

        public IActionResult Edit(int id)
        {
            Franchise franchise = _franchiseService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (franchise == null)
            {
                return NotFound();
            }

            FranchiseData model = new FranchiseData()
            {
                Id = franchise.Id,
                Name = franchise.Name,
                IsActive = franchise.IsActive,
                UserInChargeId = franchise.UserInChargeId,
                Log = franchise.Log
            };

            if (!string.IsNullOrEmpty(franchise.BuyersIds))
            {
                model.SelectedBuyersIds = new List<int>();
                var selectedBuyers = franchise.BuyersIds.Split(',');
                if (selectedBuyers.Length > 0)
                {
                    foreach (var item in selectedBuyers)
                    {
                        if (!string.IsNullOrWhiteSpace(item))
                        {
                            int number = int.Parse(item);
                            model.SelectedBuyersIds.Add(number);
                        }
                    }
                }
            }

            var franchiseeRole = _customerService.GetCustomerRoleBySystemName("franchise");
            var franchiseeList = _customerService.GetAllCustomers(customerRoleIds: new int[] { franchiseeRole.Id });
            model.AvailablesFranchisee = franchiseeList.Select(x => new SelectListItem()
            {
                Text = x.GetFullName(),
                Value = x.Id.ToString()
            }).OrderBy(x => x.Text).ToList();

            var alreadyAssigned = _franchiseService.GetAll().Where(x => x.BuyersIds != null && x.BuyersIds != "" && x.Id != id).Select(x => x.BuyersIds).ToList();
            var alreadyAssignedIds = new List<int>();
            if (alreadyAssigned.Count > 0)
                alreadyAssignedIds = string.Join(",", alreadyAssigned).Split(',').Select(x => int.Parse(x.Trim())).ToList();
            var buyerRole = _customerService.GetCustomerRoleBySystemName("delivery");
            var buyersList = _customerService.GetAllCustomers(customerRoleIds: new int[] { buyerRole.Id }).Where(x => !alreadyAssignedIds.Contains(x.Id));

            model.AvailablesBuyers = buyersList.Select(x => new SelectListItem()
            {
                Text = x.GetFullName() + " (" + x.Email + ")",
                Value = x.Id.ToString()
            }).ToList();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Franchise/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult Edit(FranchiseData model)
        {
            Franchise franchise = _franchiseService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (franchise == null) { return NotFound(); }

            if (!ModelState.IsValid) return BadRequest();

            var countBuyers = 0;
            if (model.SelectedBuyersIds != null && model.SelectedBuyersIds.Count() > 0)
            {
                var otherFranchises = _franchiseService.GetAll().Where(x => x.Id != model.Id).ToList();
                var existigDeliveries = new List<string>();
                var selectedDeliverIds = model.SelectedBuyersIds;
                foreach (var item in otherFranchises)
                {
                    if (!string.IsNullOrWhiteSpace(item.BuyersIds))
                    {
                        var otherFranchiseDeliverIds = item.BuyersIds.Split(',').Select(x => int.Parse(x.Trim())).ToList();
                        foreach (var deliverId in otherFranchiseDeliverIds)
                        {
                            if (selectedDeliverIds.Contains(deliverId))
                                existigDeliveries.Add($"{_customerService.GetCustomerById(deliverId).Email} ({item.Name})");
                        }
                    }
                }

                if (existigDeliveries.Count > 0)
                {
                    var franchiseeRole = _customerService.GetCustomerRoleBySystemName("franchise");
                    var franchiseeList = _customerService.GetAllCustomers(customerRoleIds: new int[] { franchiseeRole.Id });
                    model.AvailablesFranchisee = franchiseeList.Select(x => new SelectListItem()
                    {
                        Text = x.GetFullName(),
                        Value = x.Id.ToString()
                    }).OrderBy(x => x.Text).ToList();

                    var alreadyAssigned = _franchiseService.GetAll().Where(x => x.BuyersIds != null && x.BuyersIds != "" && x.Id != franchise.Id).Select(x => x.BuyersIds).ToList();
                    var alreadyAssignedIds = new List<int>();
                    if (alreadyAssigned.Count > 0)
                        alreadyAssignedIds = string.Join(",", alreadyAssigned).Split(',').Select(x => int.Parse(x.Trim())).ToList();
                    var buyerRole = _customerService.GetCustomerRoleBySystemName("delivery");
                    var buyersList = _customerService.GetAllCustomers(customerRoleIds: new int[] { buyerRole.Id }).Where(x => !alreadyAssignedIds.Contains(x.Id));

                    model.AvailablesBuyers = buyersList.Select(x => new SelectListItem()
                    {
                        Text = x.GetFullName() + " (" + x.Email + ")",
                        Value = x.Id.ToString()
                    }).ToList();

                    model.ExistigDeliveries = existigDeliveries;

                    return View("~/Plugins/Teed.Plugin.Groceries/Views/Franchise/Edit.cshtml", model);
                }

                foreach (var item in model.SelectedBuyersIds)
                {
                    countBuyers++;
                    if (countBuyers < model.SelectedBuyersIds.Count())
                    {
                        model.BuyersIds += item + ", ";
                    }
                    else
                    {
                        model.BuyersIds += item;
                    }
                }
            }

            string log = string.Empty;
            if (franchise.Name != model.Name)
            {
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el nombre de la franquicia de {franchise.Name} por {model.Name}.\n";
                franchise.Log += log;

                franchise.Name = model.Name;
            }

            if (franchise.UserInChargeId != model.UserInChargeId)
            {
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el encargado de la franquicia {(franchise.UserInChargeId > 0 ? $"de {_customerService.GetCustomerById(franchise.UserInChargeId).GetFullName()} ({franchise.UserInChargeId}) " : "")} a {_customerService.GetCustomerById(model.UserInChargeId).GetFullName()} ({model.UserInChargeId}).\n";
                franchise.Log += log;

                franchise.UserInChargeId = model.UserInChargeId;
            }

            if (franchise.IsActive != model.IsActive)
            {
                string status = model.IsActive ? "Activada" : "Desactivada";
                string oldStatus = franchise.IsActive ? "Activada" : "Desactivada";
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el status de la franquicia de {oldStatus} por {status}.\n";
                franchise.Log += log;

                franchise.IsActive = model.IsActive;
            }

            if (franchise.BuyersIds != model.BuyersIds)
            {
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió la lista de compradores de la franquicia de [{franchise.BuyersIds}] por [{model.BuyersIds}].\n";
                franchise.Log += log;

                franchise.BuyersIds = model.BuyersIds;
            }

            _franchiseService.Update(franchise);

            return RedirectToAction("Edit", new { Id = franchise.Id });
        }

        //delete
        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            var franchise = _franchiseService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (franchise == null)
                //No recurring payment found with the specified id
                return RedirectToAction("FranchiseList");

            _franchiseService.Delete(franchise);

            return RedirectToAction("FranchiseList");
        }

        public virtual IActionResult Info(int franchiseId = 0)
        {
            InfoModel model = new InfoModel();
            var userRolesIds = _workContext.CurrentCustomer.CustomerRoles.Select(x => x.Id);
            var franchiseRole = _customerService.GetCustomerRoleBySystemName("franchise");
            model.IsAdmin = _permissionService.Authorize(TeedGroceriesPermissionProvider.Franchise);

            Franchise franchise = new Franchise();
            if (franchiseId > 0)
            {
                if (userRolesIds.Contains(franchiseRole.Id))
                    franchise = _franchiseService.GetAll()
                        .Where(x => x.Id == franchiseId &&
                        x.UserInChargeId == _workContext.CurrentCustomer.Id)
                        .FirstOrDefault();
                else
                    franchise = _franchiseService.GetAll()
                        .Where(x => x.Id == franchiseId)
                        .FirstOrDefault();
                if (franchise == null)
                    return RedirectToAction("Index", "Home");
                PrepareInfoModel(franchise, model);
            }
            else
            {
                if (userRolesIds.Contains(franchiseRole.Id))
                {
                    franchise = _franchiseService.GetAll()
                        .Where(x => x.UserInChargeId == _workContext.CurrentCustomer.Id)
                        .FirstOrDefault();
                    if (franchise == null)
                        return RedirectToAction("Index", "Home");
                    PrepareInfoModel(franchise, model);
                }
                else
                    return RedirectToAction("Index", "Home");
            }
            return View("~/Plugins/Teed.Plugin.Groceries/Views/Franchise/Info.cshtml", model);
        }

        [HttpPost]
        public virtual IActionResult ChangeCustomerActive(CustomerData model)
        {
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return BadRequest();

            customer.Active = model.Active;
            _customerService.UpdateCustomer(customer);
            return Ok();
        }

        [HttpPost]
        public virtual IActionResult ChangeRouteActive(CustomerData model)
        {
            var route = _shippingRouteService.GetAll()
                .Where(x => x.Id == model.Id).FirstOrDefault();
            if (route == null)
                return BadRequest();

            route.Active = model.Active;
            _shippingRouteService.Update(route);
            return Ok();
        }

        [HttpPost]
        public virtual IActionResult ResumeLast10Weeks(int franchiseId)
        {
            if (franchiseId < 1) return BadRequest();

            var franchiseVehicles = _shippingVehicleService.GetAll().Where(x => x.FranchiseId == franchiseId).ToList();
            var orders = OrderUtils.GetAllFranchiseOrders(franchiseVehicles, _shippingVehicleRouteService, _orderService);

            var incidents = _incidentsService.GetAll()
                .Where(x => x.FranchiseId == franchiseId && x.AuthorizedStatusId == 1)
                .ToList();

            RatesAndBonusesSettings settings = _settingService.LoadSetting<RatesAndBonusesSettings>();

            var data = new List<WeekData>();

            var startOfWeek = DateTime.Today;
            var addDays = -((int)startOfWeek.DayOfWeek) + 1;
            if (addDays > 0 || addDays < 0)
                startOfWeek = startOfWeek.AddDays(addDays);
            var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);

            for (int i = 0; i < 10; i++)
            {
                var dataOfWeek = new WeekData();
                if (i > 0)
                {
                    startOfWeek = startOfWeek.AddDays(-7);
                    endOfWeek = endOfWeek.AddDays(-7);
                }

                dataOfWeek.WeekName = $"{startOfWeek.ToString("dddd dd-MM-yyyy", new CultureInfo("es-MX"))} a {endOfWeek.ToString("dddd dd-MM-yyyy", new CultureInfo("es-MX"))}";

                var filteredOrders = orders.Where(x => startOfWeek <= x.SelectedShippingDate && x.SelectedShippingDate <= endOfWeek);
                decimal orderTotal = filteredOrders
                    .Select(x => x.OrderTotal - x.OrderShippingInclTax + (x.CustomerBalanceUsedAmount ?? 0))
                    .DefaultIfEmpty()
                    .Sum();

                var bonus = FranchiseUtils.GetFranchiseBonus(startOfWeek, endOfWeek, franchiseVehicles.Select(x => x.Id).ToList(), _franchiseBonusService);
                var bonusAmount = bonus.Select(x => x.Amount).DefaultIfEmpty().Sum();

                dataOfWeek.Base = orderTotal.ToString("C");
                decimal comission = FranchiseUtils.GetBilled(startOfWeek, endOfWeek, franchiseVehicles.Select(x => x.Id).ToList(), _billingService);
                dataOfWeek.Comission = comission.ToString("C");
                dataOfWeek.Bonus = bonusAmount.ToString("C");
                decimal incidentsAmount = incidents.Where(x => startOfWeek <= x.Date && x.Date <= endOfWeek).Select(x => x.Amount).DefaultIfEmpty().Sum();
                dataOfWeek.Incidents = incidentsAmount.ToString("C");
                dataOfWeek.Billed = (comission + bonusAmount - incidentsAmount).ToString("C").Replace("(", "-").Replace(")", "");
                data.Add(dataOfWeek);
            }

            var gridModel = new DataSourceResult
            {
                Data = data,
                Total = data.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public virtual IActionResult ResumeWeek(int franchiseId,
            int vehicleId,
            string initDate)
        {
            if (franchiseId < 1 || string.IsNullOrEmpty(initDate))
                return BadRequest();

            var franchise = _franchiseService.GetAll()
                .Where(x => x.Id == franchiseId)
                .FirstOrDefault();
            if (franchise == null) return NotFound();

            DateTime dateParse = DateTime.ParseExact(initDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var dataOfWeek = new WeekData();
            var startOfWeek = dateParse;
            var addDays = -((int)startOfWeek.DayOfWeek) + 1;
            if (addDays > 0 || addDays < 0)
                startOfWeek = startOfWeek.AddDays(addDays);
            var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);

            var franchiseVehiclesQuery = _shippingVehicleService.GetAll().Where(x => x.FranchiseId == franchiseId);
            if (vehicleId > 0)
            {
                franchiseVehiclesQuery = franchiseVehiclesQuery.Where(x => x.Id == vehicleId);
            }
            var franchiseVehicles = franchiseVehiclesQuery.ToList();

            var orders = OrderUtils.GetAllFranchiseOrders(franchiseVehicles.ToList(), _shippingVehicleRouteService, _orderService).Where(x => !x.RescuedByRouteId.HasValue || x.RescuedByRouteId.Value == 0)
                .Where(x => x.SelectedShippingDate >= startOfWeek && x.SelectedShippingDate <= endOfWeek).Where(x => x.OrderStatusId != 50);

            var incidentsQuery = _incidentsService.GetAll()
                .Where(x => x.AuthorizedStatusId == 1);
            if (vehicleId > 0)
            {
                incidentsQuery = incidentsQuery.Where(x => x.VehicleId == vehicleId);
            }
            var incidents = incidentsQuery.ToList();

            var baseNum = orders.Where(x => startOfWeek <= x.SelectedShippingDate &&
            x.SelectedShippingDate <= endOfWeek).Select(x => x.OrderTotal - x.OrderShippingInclTax + (x.CustomerBalanceUsedAmount ?? 0))
            .DefaultIfEmpty().Sum();

            RatesAndBonusesSettings settings = _settingService.LoadSetting<RatesAndBonusesSettings>();

            var bonus = FranchiseUtils.GetFranchiseBonus(startOfWeek, endOfWeek, franchiseVehicles.Select(x => x.Id).ToList(), _franchiseBonusService);
            var bonusAmount = bonus.Select(x => x.Amount).DefaultIfEmpty().Sum();

            string s_date = StringHelpers.FirstCharToUpper(startOfWeek.ToString("dddd", new CultureInfo("es-MX")));
            string e_date = endOfWeek.ToString("dddd", new CultureInfo("es-MX"));
            dataOfWeek.WeekName = $"{s_date} {startOfWeek.ToString("dd/MM/yyyy")} a {e_date} {endOfWeek.ToString("dd/MM/yyyy")}";
            dataOfWeek.OrdersCount = OrderUtils.GetPedidosOnly(orders.Where(x => startOfWeek <= x.SelectedShippingDate && x.SelectedShippingDate <= endOfWeek)).Count();

            dataOfWeek.Base = StringHelpers.ToCurrency(baseNum);
            var comission = (baseNum * settings.BaseVariableRateCDMX) + settings.BaseFixedRateCDMX;
            dataOfWeek.Comission = StringHelpers.ToCurrency(comission);
            var hasIncidents = incidents.Where(x => startOfWeek <= x.Date && x.Date <= endOfWeek).Any();
            dataOfWeek.Bonus = bonusAmount.ToString("C");
            var incidentsNum = incidents.Where(x => startOfWeek <= x.Date && x.Date <= endOfWeek)
                .Select(x => x.Amount).DefaultIfEmpty().Sum();
            dataOfWeek.Incidents = StringHelpers.ToCurrency(incidentsNum);
            dataOfWeek.Billed = (comission + bonusAmount - incidentsNum).ToString("C").Replace("(", "-").Replace(")", "");
            var gridModel = new DataSourceResult
            {
                Data = new List<WeekData> { dataOfWeek },
                Total = new List<WeekData> { dataOfWeek }.Count
            };
            return Json(gridModel);
        }


        [HttpPost]
        public virtual IActionResult ResumeByDay(int franchiseId,
            int vehicleId,
            string initDate)
        {
            if (franchiseId < 1 || string.IsNullOrEmpty(initDate))
                return BadRequest();

            var franchise = _franchiseService.GetAll()
                .Where(x => x.Id == franchiseId)
                .FirstOrDefault();
            if (franchise == null) return NotFound();

            var dateParse = DateTime.ParseExact(initDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var franchiseVehiclesQuery = _shippingVehicleService.GetAll().Where(x => x.FranchiseId == franchiseId);
            if (vehicleId > 0)
            {
                franchiseVehiclesQuery = franchiseVehiclesQuery.Where(x => x.Id == vehicleId);
            }
            var franchiseVehicles = franchiseVehiclesQuery.ToList();
            var orders = OrderUtils.GetAllFranchiseOrders(franchiseVehicles.ToList(), _shippingVehicleRouteService, _orderService)
                .Where(x => !x.RescuedByRouteId.HasValue || x.RescuedByRouteId.Value == 0)
                .Where(x => x.OrderStatusId != 50);

            var incidentsQuery = _incidentsService.GetAll()
                .Where(x => x.AuthorizedStatusId == 1);
            if (vehicleId > 0)
            {
                incidentsQuery = incidentsQuery.Where(x => x.VehicleId == vehicleId);
            }
            var incidents = incidentsQuery.ToList();

            RatesAndBonusesSettings settings = _settingService.LoadSetting<RatesAndBonusesSettings>();
            var data = new DaysData();
            var day = dateParse;
            for (int i = 0; i < 7; i++)
            {
                var dataOfDay = new DayData();
                if (i > 0)
                    day = dateParse.AddDays(i);

                string s_date = StringHelpers.FirstCharToUpper(day.ToString("dddd", new CultureInfo("es-MX")));
                dataOfDay.DayName = $"{s_date} {day:dd/MM/yyyy}";

                var ordersOfDay = orders.Where(x => day == x.SelectedShippingDate);
                var baseNum = ordersOfDay
                    .Select(x => x.OrderTotal - x.OrderShippingInclTax + (x.CustomerBalanceUsedAmount ?? 0))
                    .DefaultIfEmpty().Sum();

                var bonuses = FranchiseUtils.GetFranchiseBonus(day, day, franchiseVehicles.Select(x => x.Id).ToList(), _franchiseBonusService);
                var bonusNum = bonuses.Select(x => x.Amount).DefaultIfEmpty().Sum();
                dataOfDay.Orders = OrderUtils.GetPedidosOnly(ordersOfDay).Count();

                dataOfDay.BaseNum = baseNum;
                dataOfDay.Base = baseNum.ToString("C");
                var comission = (baseNum * 8) / 100;
                dataOfDay.ComissionNum = comission;
                dataOfDay.Comission = comission.ToString("C");
                var incidentsOfDay = incidents.Where(x => day == x.Date && x.AuthorizedStatusId == (int)IncidentStatus.Authorized);
                var hasIncidents = incidentsOfDay.Any();
                dataOfDay.BonusTexts = string.Join("<br />",
                    bonuses.Select(x => $"{x.BonusCode} - {x.Amount:C}"));
                if (!string.IsNullOrEmpty(dataOfDay.BonusTexts))
                    dataOfDay.BonusTexts += "<br />Total: " + bonusNum.ToString("C");
                else
                    dataOfDay.BonusTexts += 0.ToString("C");
                dataOfDay.BonusNum = bonusNum;
                dataOfDay.Bonus = bonusNum.ToString("C");
                var incidentsNum = incidents.Where(x => day == x.Date && x.AuthorizedStatusId == (int)IncidentStatus.Authorized).Select(x => x.Amount).DefaultIfEmpty().Sum();
                dataOfDay.IncidentsNum = incidentsNum;
                dataOfDay.Incidents = StringHelpers.ToCurrency(incidentsNum);
                dataOfDay.IncidentTexts = string.Join("<br />",
                    incidentsOfDay.Select(x => $"{x.IncidentCode} - {StringHelpers.ToCurrency(x.Amount)}"));
                if (!string.IsNullOrEmpty(dataOfDay.IncidentTexts))
                    dataOfDay.IncidentTexts += "<br />Total: " + StringHelpers.ToCurrency(incidentsNum);
                else
                    dataOfDay.IncidentTexts += 0.ToString("C");
                dataOfDay.BilledNum = comission + bonusNum - incidentsNum;
                dataOfDay.Billed = dataOfDay.BilledNum.ToString("C").Replace("(", "-").Replace(")", "");
                data.Data.Add(dataOfDay);
            }
            data.Data.FirstOrDefault().OrdersTotal = data.Data.Select(x => x.Orders).Sum().ToString();
            data.Data.FirstOrDefault().BaseTotal = StringHelpers.ToCurrency(data.Data.Select(x => x.BaseNum).Sum())
                .Replace("(", "-").Replace(")", "");
            data.Data.FirstOrDefault().ComissionTotal = StringHelpers.ToCurrency(data.Data.Select(x => x.ComissionNum).Sum());
            data.Data.FirstOrDefault().BonusTotal = StringHelpers.ToCurrency(data.Data.Select(x => x.BonusNum).Sum());
            data.Data.FirstOrDefault().IncidentsTotal = StringHelpers.ToCurrency(data.Data.Select(x => x.IncidentsNum).Sum());
            data.Data.FirstOrDefault().BilledTotal = StringHelpers.ToCurrency(data.Data.Select(x => x.BilledNum).Sum())
                    .Replace("(", "-").Replace(")", "");

            var gridModel = new DataSourceResult
            {
                Data = data.Data,
                Total = data.Data.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public virtual IActionResult ResumeBills(int franchiseId)
        {
            if (franchiseId < 1) return BadRequest();

            var franchiseVehicles = _shippingVehicleService.GetAll().Where(x => x.FranchiseId == franchiseId).ToList();
            var franchiseVehiclesIds = franchiseVehicles.Select(x => x.Id);

            var bills = _billingService.GetAll()
                .Where(x => franchiseVehiclesIds.Contains(x.VehicleId))
                .OrderByDescending(x => x.InitDate)
                .ToList();

            if (!bills.Any()) return Ok();

            RatesAndBonusesSettings settings = _settingService.LoadSetting<RatesAndBonusesSettings>();

            var data = new List<BillData>();
            foreach (var bill in bills)
            {
                var dataOfBill = new BillData()
                {
                    Billed = bill.Billed.ToString("C"),
                    Vehicle = VehicleUtils.GetVehicleName(bill.Vehicle),
                    WeekName = bill.InitDate.ToString("dddd, dd-MM-yyyy", new CultureInfo("es-MX")) + " a " + bill.EndDate.ToString("dddd, dd-MM-yyyy", new CultureInfo("es-MX")),
                    Id = bill.Id,
                    InitDate = bill.InitDate
                };

                data.Add(dataOfBill);
            }

            var gridModel = new DataSourceResult
            {
                Data = data.OrderByDescending(x => x.InitDate),
                Total = data.Count
            };

            return Json(gridModel);
        }

        public virtual IActionResult RatesAndBonuses()
        {
            RatesAndBonusesSettings settings = _settingService.LoadSetting<RatesAndBonusesSettings>();
            var model = new RatesAndBonusesModel
            {
                BaseFixedRateCDMX = settings.BaseFixedRateCDMX,
                BaseFixedRateOtherStates = settings.BaseFixedRateOtherStates,
                BaseVariableRateCDMX = settings.BaseVariableRateCDMX,
                BaseVariableRateOtherStates = settings.BaseVariableRateOtherStates,
                FixedWeeklyBonusCeroIncidents = settings.FixedWeeklyBonusCeroIncidents,
                VariableWeeklyBonusCeroIncidents = settings.VariableWeeklyBonusCeroIncidents,
                Log = settings.Log
            };
            return View("~/Plugins/Teed.Plugin.Groceries/Views/RatesAndBonuses/Index.cshtml", model);
        }

        [HttpPost]
        public virtual IActionResult RatesAndBonuses(RatesAndBonusesModel model)
        {
            RatesAndBonusesSettings settings = _settingService.LoadSetting<RatesAndBonusesSettings>();
            var originalSettings = settings;

            var log = string.Empty;
            if (originalSettings.BaseFixedRateCDMX != model.BaseFixedRateCDMX)
            {
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió la tarifa base fija para CDMX de {originalSettings.BaseFixedRateCDMX} por {model.BaseFixedRateCDMX}.\n";
                settings.Log += log;
            }
            if (originalSettings.BaseFixedRateOtherStates != model.BaseFixedRateOtherStates)
            {
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió la tarifa base fija otros estados de {originalSettings.BaseFixedRateOtherStates} por {model.BaseFixedRateOtherStates}.\n";
                settings.Log += log;
            }
            if (originalSettings.BaseVariableRateCDMX != model.BaseVariableRateCDMX)
            {
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió la tarifa base variable para CDMX de {originalSettings.BaseVariableRateCDMX} por {model.BaseVariableRateCDMX}.\n";
                settings.Log += log;
            }
            if (originalSettings.BaseVariableRateOtherStates != model.BaseVariableRateOtherStates)
            {
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió la tarifa base variable otros estados de {originalSettings.BaseVariableRateOtherStates} por {model.BaseVariableRateOtherStates}.\n";
                settings.Log += log;
            }
            if (originalSettings.FixedWeeklyBonusCeroIncidents != model.FixedWeeklyBonusCeroIncidents)
            {
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el bono semanal fijo por cero incidencias de {originalSettings.FixedWeeklyBonusCeroIncidents} por {model.FixedWeeklyBonusCeroIncidents}.\n";
                settings.Log += log;
            }
            if (originalSettings.VariableWeeklyBonusCeroIncidents != model.VariableWeeklyBonusCeroIncidents)
            {
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el bono semanal variable por cero incidencias de {originalSettings.VariableWeeklyBonusCeroIncidents} por {model.VariableWeeklyBonusCeroIncidents}.\n";
                settings.Log += log;
            }

            settings.BaseFixedRateCDMX = model.BaseFixedRateCDMX;
            settings.BaseFixedRateOtherStates = model.BaseFixedRateOtherStates;
            settings.BaseVariableRateCDMX = model.BaseVariableRateCDMX;
            settings.BaseVariableRateOtherStates = model.BaseVariableRateOtherStates;
            settings.FixedWeeklyBonusCeroIncidents = model.FixedWeeklyBonusCeroIncidents;
            settings.VariableWeeklyBonusCeroIncidents = model.VariableWeeklyBonusCeroIncidents;
            model.Log = settings.Log;

            _settingService.SaveSetting(settings);
            SuccessNotification("Se actualizó la configuracíon de Tasas y bonos de forma correcta");

            return View("~/Plugins/Teed.Plugin.Groceries/Views/RatesAndBonuses/Index.cshtml", model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetRouteIdOfCurrentFranchise(int customerId)
        {
            var franchiseOfCustomer = _franchiseService.GetAll()
                .Where(x => x.UserInChargeId == customerId)
                .FirstOrDefault();
            if (franchiseOfCustomer == null)
                return Json(new List<int> { 0 });

            var routes = _shippingRouteService.GetAll()
                .Where(x => x.FranchiseId == franchiseOfCustomer.Id)
                .ToList();
            if (!routes.Any())
                return Json(new List<int> { 0 });

            return Json(routes.Select(x => x.Id).ToList());
        }

        [HttpGet]
        public virtual IActionResult LoadFranchiseStatistics(string prefixAndId)
        {
            //a vendor doesn't have access to this report
            if (_workContext.CurrentVendor != null)
                return Content("");

            var prex = prefixAndId.Split('-');
            var franchiseId = int.Parse(prex.LastOrDefault());

            var result = new List<StatisticsData>();

            var nowDt = _dateTimeHelper.ConvertToUserTime(DateTime.Now);
            var timeZone = _dateTimeHelper.CurrentTimeZone;

            var culture = new CultureInfo(_workContext.WorkingLanguage.LanguageCulture);
            var monthAgoDt = DateTime.Now;
            var searchMonthDateUser = DateTime.Now;

            var franchise = _franchiseService.GetAll()
                .Where(x => x.Id == franchiseId)
                .FirstOrDefault();

            if (franchise == null)
                return Content("");

            var franchiseVehicles = _shippingVehicleService.GetAll().Where(x => x.FranchiseId == franchiseId).ToList();

            var incidents = _incidentsService.GetAll()
                .Where(x => x.FranchiseId == franchise.Id && x.AuthorizedStatusId == 1)
                .ToList();
            var startOfWeek = DateTime.Now;
            var addDays = 0;
            var monthsAgoDt = DateTime.Now;
            var endOfWeek = DateTime.Now;
            int weeks = 0;
            switch (prex.FirstOrDefault())
            {
                case "incidents":
                    if (!incidents.Any())
                        return Content("");

                    startOfWeek = nowDt;
                    addDays = -((int)startOfWeek.DayOfWeek) + 1;
                    if (addDays > 0 || addDays < 0)
                        startOfWeek = startOfWeek.AddDays(addDays);
                    monthsAgoDt = startOfWeek.AddMonths(-6);
                    endOfWeek = startOfWeek.AddDays(6).AddSeconds(-1);
                    searchMonthDateUser = startOfWeek;
                    weeks = NumberOfWeeks(monthsAgoDt, startOfWeek);
                    if (!timeZone.IsInvalidTime(searchMonthDateUser))
                    {
                        for (var i = 0; i <= weeks; i++)
                        {
                            result.Add(new StatisticsData
                            {
                                date = $"{searchMonthDateUser.ToString("dd/MM/yyyy")} - {endOfWeek.ToString("dd/MM/yyyy")}",
                                value = incidents
                                .Where(x => searchMonthDateUser.Date <= x.Date &&
                                x.Date <= endOfWeek.Date).Count().ToString()
                            });

                            searchMonthDateUser = searchMonthDateUser.AddDays(-7);
                            endOfWeek = endOfWeek.AddDays(-7);
                        }
                    }
                    break;
                case "puntuality":
                    startOfWeek = nowDt;
                    addDays = -((int)startOfWeek.DayOfWeek) + 1;
                    if (addDays > 0 || addDays < 0)
                        startOfWeek = startOfWeek.AddDays(addDays);
                    monthsAgoDt = startOfWeek.AddMonths(-6);
                    endOfWeek = startOfWeek.AddDays(6).AddSeconds(-1);
                    searchMonthDateUser = startOfWeek;
                    weeks = NumberOfWeeks(monthsAgoDt, startOfWeek);
                    if (!timeZone.IsInvalidTime(searchMonthDateUser))
                    {
                        for (var i = 0; i <= weeks; i++)
                        {
                            result.Add(new StatisticsData
                            {
                                date = $"{searchMonthDateUser.ToString("dd/MM/yyyy")} - {endOfWeek.ToString("dd/MM/yyyy")}",
                                value = "0"
                                //orders
                                //.Where(x => searchMonthDateUser.Date <= x.SelectedShippingDate.Value &&
                                //x.SelectedShippingDate.Value <= endOfWeek.Date)
                                //.Count().ToString()
                            });

                            searchMonthDateUser = searchMonthDateUser.AddDays(-7);
                            endOfWeek = endOfWeek.AddDays(-7);
                        }
                    }
                    break;
                default:
                    break;
            }
            result.Reverse();
            return Json(result);
        }

        [HttpGet]
        public virtual IActionResult RemoveDuplicatedBilling()
        {
            var billingGroup = _billingService.GetAll().GroupBy(x => new { x.InitDate, x.VehicleId }).Where(x => x.Count() > 0).ToList();
            foreach (var item in billingGroup)
            {
                var lastCreated = item.OrderBy(x => x.CreatedOnUtc).LastOrDefault();
                foreach (var bill in item)
                {
                    if (bill.Id == lastCreated.Id) continue;
                    bill.Log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Se eliminó por estar duplicado.";
                    _billingService.Delete(bill);
                }
            }
            return Ok();
        }

        [HttpGet]
        public virtual IActionResult GetFranchisesFromExternal()
        {
            var franchises = _franchiseService.GetAll()
                .Select(x => new { x.Id, x.Name }).ToList();

            return Json(franchises);
        }

        public int NumberOfWeeks(DateTime dateFrom, DateTime dateTo)
        {
            TimeSpan Span = dateTo.Subtract(dateFrom);

            if (Span.Days <= 7)
            {
                if (dateFrom.DayOfWeek > dateTo.DayOfWeek)
                {
                    return 2;
                }

                return 1;
            }

            int Days = Span.Days - 7 + (int)dateFrom.DayOfWeek;
            int WeekCount = 1;
            int DayCount = 0;

            for (WeekCount = 1; DayCount < Days; WeekCount++)
            {
                DayCount += 7;
            }

            return WeekCount;
        }
    }

    public class BillData
    {
        public BillData()
        {
            OtherFileIds = new List<int>();
        }

        public int Id { get; set; }
        public string WeekName { get; set; }
        public string Billed { get; set; }
        public string Vehicle { get; set; }
        public DateTime InitDate { get; set; }
        public List<int> PdfIds { get; set; }
        public List<int> XmlIds { get; set; }
        public List<int> OtherFileIds { get; set; }
    }

    public class WeekData
    {
        public int OrdersCount { get; set; }
        public string WeekName { get; set; }
        public string Base { get; set; }
        public string Comission { get; set; }
        public string Bonus { get; set; }
        public string Incidents { get; set; }
        public string Billed { get; set; }
    }

    public class DaysData
    {
        public DaysData()
        {
            Data = new List<DayData>();
        }

        public List<DayData> Data { get; set; }
    }

    public class DayData
    {
        public string DayName { get; set; }
        public int Orders { get; set; }
        public string Base { get; set; }
        public decimal BaseNum { get; set; }
        public string Comission { get; set; }
        public decimal ComissionNum { get; set; }
        public string BonusTexts { get; set; }
        public string Bonus { get; set; }
        public decimal BonusNum { get; set; }
        public string IncidentTexts { get; set; }
        public string Incidents { get; set; }
        public decimal IncidentsNum { get; set; }
        public string Billed { get; set; }
        public decimal BilledNum { get; set; }

        public string OrdersTotal { get; set; }
        public string BaseTotal { get; set; }
        public string ComissionTotal { get; set; }
        public string BonusTotal { get; set; }
        public string IncidentsTotal { get; set; }
        public string BilledTotal { get; set; }
    }

    public class StatisticsData
    {
        public string date { get; set; }
        public string value { get; set; }
    }

    public class StatementData
    {
        public int ElementId { get; set; }
        public decimal Charge { get; set; }
        public decimal Payment { get; set; }
        public decimal Balance { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public bool IsAdding { get; set; }
    }

    public class FullStatementData
    {
        public DateTime Date { get; set; }
        public DateTime PayableDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public decimal Charges { get; set; }
        public decimal PayableCharges { get; set; }
        public decimal ExpiredCharges { get; set; }
        public decimal Payments { get; set; }
        public decimal BalanceToBeReleased { get; set; }
        public decimal PayableToBeReleased { get; set; }
        public decimal ExpiredToBeReleased { get; set; }
        public decimal Capital { get; set; }
        public decimal SimpleInterestPeriod { get; set; }
        public decimal CapitalBonus { get; set; }
        public decimal PaymentInterest { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal CapitalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal TotalCapital { get; set; }
    }
}
