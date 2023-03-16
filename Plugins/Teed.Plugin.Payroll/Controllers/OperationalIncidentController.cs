using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
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
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Security;
using Teed.Plugin.Payroll.Services;
using Teed.Plugin.Payroll.Models.PayrollEmployee;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;
using Nop.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Teed.Plugin.Payroll.Models.Incident;
using Teed.Plugin.Payroll.Domain.Incidents;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Teed.Plugin.Payroll.Domain.MinimumWagesCatalogs;
using Teed.Plugin.Payroll.Helpers;
using OfficeOpenXml;
using Teed.Plugin.Payroll.Models.BiweeklyPayment;
using Teed.Plugin.Payroll.Models;
using System.Text;

namespace Teed.Plugin.Payroll.Controllers
{
    [Area(AreaNames.Admin)]
    public class OperationalIncidentController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly PayrollEmployeeService _payrollEmployeeService;
        private readonly PayrollEmployeeFileService _payrollEmployeeFileService;
        private readonly PayrollSalaryService _payrollSalaryService;
        private readonly JobCatalogService _jobCatalogService;
        private readonly OperationalIncidentService _operationalIncidentService;
        private readonly IWorkContext _workContext;
        private readonly MinimumWagesCatalogService _minimumWagesCatalogService;
        private readonly BonusService _bonusService;
        private readonly BonusApplicationService _bonusApplicationService;
        private readonly AssistanceService _assistanceService;
        private readonly IOrderService _orderService;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;

        public OperationalIncidentController(IPermissionService permissionService, PayrollEmployeeService payrollEmployeeService,
            PayrollEmployeeFileService payrollEmployeeFileService, PayrollSalaryService payrollSalaryService,
            JobCatalogService jobCatalogService, OperationalIncidentService operationalIncidentService,
            IWorkContext workContext, MinimumWagesCatalogService minimumWagesCatalogService,
            BonusService bonusService, BonusApplicationService bonusApplicationService,
            AssistanceService assistanceService, IOrderService orderService,
            IStoreContext storeContext, ICustomerService customerService)
        {
            _permissionService = permissionService;
            _payrollEmployeeService = payrollEmployeeService;
            _payrollEmployeeFileService = payrollEmployeeFileService;
            _payrollSalaryService = payrollSalaryService;
            _jobCatalogService = jobCatalogService;
            _operationalIncidentService = operationalIncidentService;
            _workContext = workContext;
            _minimumWagesCatalogService = minimumWagesCatalogService;
            _bonusService = bonusService;
            _bonusApplicationService = bonusApplicationService;
            _assistanceService = assistanceService;
            _orderService = orderService;
            _storeContext = storeContext;
            _customerService = customerService;
        }

        public void PrepareModel(OperationalIncidentListModel model, bool getBiweeks = true)
        {
            var franchiseAndRouteInfo = ExternalHelper.GetExternalFranchiseAndRoutesInfoByDates(_storeContext);

            var customerRoles = _customerService.GetAllCustomerRoles()
                .Where(x => x.SystemName == "employee" ||
                x.SystemName == "exemployee" ||
                x.SystemName == "delivery")
                .ToList();
            var customerRoleIds = customerRoles.Select(x => x.Id).ToArray();
            var employees = _customerService.GetAllCustomers(customerRoleIds: customerRoleIds)
                .ToList()
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.GetFullName()
                })
                .OrderBy(x => x.Text)
                .ToList();

            model.ResponsibleAreaTypes = Enum.GetValues(typeof(ResponsibleAreaType)).Cast<ResponsibleAreaType>()
                .Select(x => new SelectListItem
                {
                    Value = ((int)x).ToString(),
                    Text = x.GetDisplayName()
                }).ToList();
            model.SolutionTypes = Enum.GetValues(typeof(SolutionType)).Cast<SolutionType>()
                .Select(x => new SelectListItem
                {
                    Value = ((int)x).ToString(),
                    Text = x.GetDisplayName()
                }).ToList();
            model.FranchiseInfos = franchiseAndRouteInfo.FranchiseInfos;
            model.RouteInfos = franchiseAndRouteInfo.RouteInfos;
            model.Employees = employees;

            if (getBiweeks)
            {
                var festiveDates = _assistanceService.GetFestiveDatesFromGroceries()
                    .Where(x => !x.DontApplyToPayroll).ToList();
                model.Biweeks = BiweeklyDatesHelper.GetAllBiweeklyDatesSinceGivenDate(new DateTime(2021, 1, 1), festiveDates)
                    .Select(x => new SelectListItem
                    {
                        Value = x.EndOfBiweekly.ToString("dd/MM/yyyy"),
                        Text = $"Del {x.StartOfBiweekly:dd/MM/yyyy} al {x.EndOfBiweekly:dd/MM/yyyy}"
                    }).ToList();
            }
        }

        public int GetFranchiseIdOfOrder(List<FranchiseAndRouteDatesInfo> franchiseAndRouteDatesInfo,
            int routeId, DateTime shippingDate)
        {
            return franchiseAndRouteDatesInfo.Where(x => x.RouteId == routeId && x.ShippingDate == shippingDate)
                .FirstOrDefault()?.FranchiseId ?? 0;
        }

        [HttpGet]
        public IActionResult GetOrderInfo(int id)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.OperationalIncident))
                return AccessDeniedView();

            var orderInfo = ExternalHelper.GetExternalFranchiseAndRouteOrderInfo(id, _storeContext);
            return Json(orderInfo);
        }

        public IActionResult Index()
        {
            return RedirectToAction("OperationalIncidentDelivery");
        }

        public IActionResult Deliveries()
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.OperationalIncident))
                return AccessDeniedView();

            var model = new OperationalIncidentListModel();
            PrepareModel(model);
            model.OperationalIncidentType = OperationalIncidentType.Delivery;
            return View("~/Plugins/Teed.Plugin.Payroll/Views/OperationalIncident/Index.cshtml", model);
        }

        public IActionResult Buyers()
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.OperationalIncident))
                return AccessDeniedView();

            var model = new OperationalIncidentListModel();
            PrepareModel(model);
            model.OperationalIncidentType = OperationalIncidentType.Buyer;
            return View("~/Plugins/Teed.Plugin.Payroll/Views/OperationalIncident/Index.cshtml", model);
        }

        [HttpPost]
        public IActionResult OperationalIncidentList(DataSourceRequest command, int typeId = 1)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.OperationalIncident))
                return AccessDeniedView();
            var operationalIncidents = _operationalIncidentService.GetAll()
                .Where(x => x.OperationalIncidentTypeId == typeId)
                .OrderByDescending(x => x.ReportDate);

            var pagedList = new PagedList<OperationalIncident>(operationalIncidents, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x =>
                {
                    return new OperationalIncidentModel
                    {
                        Id = x.Id,
                        AppliedInBiweek = x.AppliedInBiweek != null ? x.AppliedInBiweek.Value.ToString("dd/MM/yyyy") : "",
                        Authorized = x.Authorized,
                        Comments = x.Comments,
                        IncidentAmount = x.IncidentAmount,
                        IncidentDetails = x.IncidentDetails,
                        Log = x.Log.Replace("\n", "<br>"),

                        OrderId = x.OrderId,
                        OrderDeliveryDate = x.OrderDeliveryDate != null ? x.OrderDeliveryDate.Value.ToString("dd/MM/yyyy") : "",
                        OrderDeliveryFranchiseId = x.OrderDeliveryFranchiseId,
                        OrderDeliveryRescuedFranchiseId = x.OrderDeliveryRescuedFranchiseId,
                        OrderDeliveryRouteId = x.OrderDeliveryRouteId,
                        OrderDeliveryRescuedRouteId = x.OrderDeliveryRescuedRouteId,

                        ReportDate = x.ReportDate.ToString("dd/MM/yyyy"),
                        ResponsibleArea1 = x.ResponsibleArea1,
                        ResponsibleArea2 = x.ResponsibleArea2,
                        ResponsibleCustomerId1 = x.ResponsibleCustomerId1,
                        ResponsibleCustomerId2 = x.ResponsibleCustomerId2,
                        SolutionAmount = x.SolutionAmount,
                        SolutionTypeId = x.SolutionTypeId,
                    };
                }).ToList(),
                Total = operationalIncidents.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult OperationalIncidentAdd(OperationalIncidentModel model, int typeId = 1)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.OperationalIncident))
                return AccessDeniedView();
            if (string.IsNullOrEmpty(model.ReportDate?.Trim()))
                return BadRequest("Report date cannot be null");
            if (typeId < 1)
                return BadRequest("Operational incident type error");
            var type = (OperationalIncidentType)typeId;
            try
            {
                var reportDate = DateTime.ParseExact(model.ReportDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var operationalIncident = new OperationalIncident
                {
                    OperationalIncidentTypeId = (int)type,
                    AppliedInBiweek = !string.IsNullOrEmpty(model.AppliedInBiweek?.Trim()) ? DateTime.ParseExact(model.AppliedInBiweek, "dd/MM/yyyy", CultureInfo.InvariantCulture) : (DateTime?)null,
                    Authorized = model.Authorized,
                    Comments = model.Comments,
                    IncidentAmount = model.IncidentAmount,
                    IncidentDetails = model.IncidentDetails,
                    OrderDeliveryDate = !string.IsNullOrEmpty(model.OrderDeliveryDate?.Trim()) ? DateTime.ParseExact(model.OrderDeliveryDate, "dd/MM/yyyy", CultureInfo.InvariantCulture) : (DateTime?)null,
                    OrderDeliveryFranchiseId = model.OrderDeliveryFranchiseId,
                    OrderDeliveryRouteId = model.OrderDeliveryRouteId,
                    OrderDeliveryRescuedRouteId = model.OrderDeliveryRescuedRouteId,
                    OrderDeliveryRescuedFranchiseId = model.OrderDeliveryRescuedFranchiseId,
                    OrderId = model.OrderId,
                    ReportDate = reportDate,
                    ResponsibleArea1 = model.ResponsibleArea1,
                    ResponsibleArea2 = model.ResponsibleArea2,
                    ResponsibleCustomerId1 = model.ResponsibleCustomerId1,
                    ResponsibleCustomerId2 = model.ResponsibleCustomerId2,
                    SolutionAmount = model.SolutionAmount,
                    SolutionTypeId = model.SolutionTypeId,
                    Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) agregó una nueva incidencia operacional para el día {model.ReportDate}.\n"
                };
                _operationalIncidentService.Insert(operationalIncident);

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        public IActionResult OperationalIncidentUpdate(OperationalIncidentModel model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.OperationalIncident))
                return AccessDeniedView();
            if (string.IsNullOrEmpty(model.ReportDate?.Trim()))
                return BadRequest("Report date cannot be null");
            var incident = _operationalIncidentService.GetById(model.Id);
            if (incident == null)
                return Ok();

            var info = new OperationalIncidentListModel();
            PrepareModel(info, false);

            var originalReportDate = incident.ReportDate.ToString("dd/MM/yyyy");
            if (originalReportDate != model.ReportDate)
            {
                incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó " +
                    $"la fecha de reporte de queja de {originalReportDate} a {model.ReportDate}.\n";
                incident.ReportDate = DateTime.ParseExact(model.ReportDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            var originalOrderDeliveryDate = incident.OrderDeliveryDate != null ? incident.OrderDeliveryDate.Value.ToString("dd/MM/yyyy") : null;
            if (string.IsNullOrEmpty(model.OrderDeliveryDate?.Trim()))
                model.OrderDeliveryDate = null;
            if (originalOrderDeliveryDate != model.OrderDeliveryDate)
            {
                incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó " +
                    $"la fecha de entrega de la orden de {(originalOrderDeliveryDate != null ? originalOrderDeliveryDate : "Ninguna")} a {(model.OrderDeliveryDate != null ? model.OrderDeliveryDate : "Ninguna")}.\n";
                incident.OrderDeliveryDate = !string.IsNullOrEmpty(model.OrderDeliveryDate?.Trim()) ? DateTime.ParseExact(model.OrderDeliveryDate, "dd/MM/yyyy", CultureInfo.InvariantCulture) : (DateTime?)null;
            }

            if (incident.OrderId != model.OrderId)
            {
                incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó " +
                    $"el número de orden de {(incident.OrderId > 0 ? "#" + incident.OrderId : "Ninguna")} a {(model.OrderId > 0 ? "#" + model.OrderId : "Ninguna")}.\n";
                incident.OrderId = model.OrderId;
            }

            if (incident.OrderDeliveryRouteId != model.OrderDeliveryRouteId)
            {
                var originalRoute = info.RouteInfos.Where(x => x.Id == incident.OrderDeliveryRouteId).FirstOrDefault();
                var newRoute = info.RouteInfos.Where(x => x.Id == model.OrderDeliveryRouteId).FirstOrDefault();

                incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó " +
                    $"la ruta que entregó de {(originalRoute != null ? originalRoute.Name + " (" + originalRoute.Id + ")" : "Ninguna")} a {(newRoute != null ? newRoute.Name + " (" + newRoute.Id + ")" : "Ninguna")}.\n";
                incident.OrderDeliveryRouteId = model.OrderDeliveryRouteId;
            }

            if (incident.OrderDeliveryFranchiseId != model.OrderDeliveryFranchiseId)
            {
                var originalFranchise = info.FranchiseInfos.Where(x => x.Id == incident.OrderDeliveryFranchiseId).FirstOrDefault();
                var newFranchise = info.FranchiseInfos.Where(x => x.Id == model.OrderDeliveryFranchiseId).FirstOrDefault();

                incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó " +
                    $"la franquicia que entregó de {(originalFranchise != null ? originalFranchise.Name + " (" + originalFranchise.Id + ")" : "Ninguna")} a {(newFranchise != null ? newFranchise.Name + " (" + newFranchise.Id + ")" : "Ninguna")}.\n";
                incident.OrderDeliveryFranchiseId = model.OrderDeliveryFranchiseId;
            }

            if (incident.OrderDeliveryRescuedRouteId != model.OrderDeliveryRescuedRouteId)
            {
                var originalRoute = info.RouteInfos.Where(x => x.Id == incident.OrderDeliveryRescuedRouteId).FirstOrDefault();
                var newRoute = info.RouteInfos.Where(x => x.Id == model.OrderDeliveryRescuedRouteId).FirstOrDefault();

                incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó " +
                    $"la ruta que rescató y entregó de {(originalRoute != null ? originalRoute.Name + " (" + originalRoute.Id + ")" : "Ninguna")} a {(newRoute != null ? newRoute.Name + " (" + newRoute.Id + ")" : "Ninguna")}.\n";
                incident.OrderDeliveryRescuedRouteId = model.OrderDeliveryRescuedRouteId;
            }

            if (incident.OrderDeliveryRescuedFranchiseId != model.OrderDeliveryRescuedFranchiseId)
            {
                var originalFranchise = info.FranchiseInfos.Where(x => x.Id == incident.OrderDeliveryRescuedFranchiseId).FirstOrDefault();
                var newFranchise = info.FranchiseInfos.Where(x => x.Id == model.OrderDeliveryRescuedFranchiseId).FirstOrDefault();

                incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó " +
                    $"la franquicia que rescató y entregó de {(originalFranchise != null ? originalFranchise.Name + " (" + originalFranchise.Id + ")" : "Ninguna")} a {(newFranchise != null ? newFranchise.Name + " (" + newFranchise.Id + ")" : "Ninguna")}.\n";
                incident.OrderDeliveryRescuedFranchiseId = model.OrderDeliveryRescuedFranchiseId;
            }

            if (incident.ResponsibleArea1 != model.ResponsibleArea1)
            {
                var originalResponsibleArea = info.ResponsibleAreaTypes.Where(x => x.Value == incident.ResponsibleArea2.ToString()).FirstOrDefault();
                var newResponsibleArea = info.ResponsibleAreaTypes.Where(x => x.Value == model.ResponsibleArea2.ToString()).FirstOrDefault();

                incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó " +
                    $"el área responsable 1 de {(originalResponsibleArea != null ? originalResponsibleArea.Text : "Ninguno")} a {(newResponsibleArea != null ? newResponsibleArea.Text : "Ninguno")}.\n";
                incident.ResponsibleArea1 = model.ResponsibleArea1;
            }

            if (incident.ResponsibleArea2 != model.ResponsibleArea2)
            {
                var originalResponsibleArea = info.ResponsibleAreaTypes.Where(x => x.Value == incident.ResponsibleArea2.ToString()).FirstOrDefault();
                var newResponsibleArea = info.ResponsibleAreaTypes.Where(x => x.Value == model.ResponsibleArea2.ToString()).FirstOrDefault();

                incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó " +
                    $"el área responsable 2 de {(originalResponsibleArea != null ? originalResponsibleArea.Text : "Ninguno")} a {(newResponsibleArea != null ? newResponsibleArea.Text : "Ninguno")}.\n";
                incident.ResponsibleArea2 = model.ResponsibleArea2;
            }

            if (incident.ResponsibleCustomerId1 != model.ResponsibleCustomerId1)
            {
                var originalResponsible = info.Employees.Where(x => x.Value == incident.ResponsibleCustomerId1.ToString()).FirstOrDefault();
                var newResponsible = info.Employees.Where(x => x.Value == model.ResponsibleCustomerId1.ToString()).FirstOrDefault();

                incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó " +
                    $"el responsable 1 de {(originalResponsible != null ? originalResponsible.Text + " (" + originalResponsible.Value + ")" : "Ninguno")} a {(newResponsible != null ? newResponsible.Text + " (" + newResponsible.Value + ")" : "Ninguno")}.\n";
                incident.ResponsibleCustomerId1 = model.ResponsibleCustomerId1;
            }

            if (incident.ResponsibleCustomerId2 != model.ResponsibleCustomerId2)
            {
                var originalResponsible = info.Employees.Where(x => x.Value == incident.ResponsibleCustomerId2.ToString()).FirstOrDefault();
                var newResponsible = info.Employees.Where(x => x.Value == model.ResponsibleCustomerId2.ToString()).FirstOrDefault();

                incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó " +
                    $"el responsable 2 de {(originalResponsible != null ? originalResponsible.Text + " (" + originalResponsible.Value + ")" : "Ninguno")} a {(newResponsible != null ? newResponsible.Text + " (" + newResponsible.Value + ")" : "Ninguno")}.\n";
                incident.ResponsibleCustomerId2 = model.ResponsibleCustomerId2;
            }

            if (incident.IncidentAmount != model.IncidentAmount)
            {
                incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó " +
                    $"el monto total de la incidencia de {incident.IncidentAmount:C} a {model.IncidentAmount:C}.\n";
                incident.IncidentAmount = model.IncidentAmount;
            }

            if (incident.IncidentDetails != model.IncidentDetails)
            {
                incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó " +
                    $"la descripción detallada de la incidencia de \"{incident.IncidentDetails}\" a \"{model.IncidentDetails}\".\n";
                incident.IncidentDetails = model.IncidentDetails;
            }

            if (incident.Authorized != model.Authorized)
            {
                incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó " +
                    $"la autorización de \"{(incident.Authorized ? "Autorizado" : "No autorizado")}\" a \"{(model.Authorized ? "Autorizado" : "No autorizado")}\".\n";
                incident.Authorized = model.Authorized;
            }

            var originalAppliedInBiweek = incident.AppliedInBiweek != null ? incident.AppliedInBiweek.Value.ToString("dd/MM/yyyy") : null;
            if (string.IsNullOrEmpty(model.AppliedInBiweek?.Trim()))
                model.AppliedInBiweek = null;
            if (originalAppliedInBiweek != model.AppliedInBiweek)
            {
                incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó " +
                    $"la quincena en que se aplicó el descuento de {(originalAppliedInBiweek != null ? originalAppliedInBiweek : "Ninguna")} a {(model.AppliedInBiweek != null ? model.AppliedInBiweek : "Ninguna")}.\n";
                incident.AppliedInBiweek = !string.IsNullOrEmpty(model.AppliedInBiweek?.Trim()) ? DateTime.ParseExact(model.AppliedInBiweek, "dd/MM/yyyy", CultureInfo.InvariantCulture) : (DateTime?)null;
            }

            if (incident.SolutionTypeId != model.SolutionTypeId)
            {
                var originalSolutionType = info.SolutionTypes.Where(x => x.Value == incident.SolutionTypeId.ToString()).FirstOrDefault();
                var newSolutionType = info.SolutionTypes.Where(x => x.Value == model.SolutionTypeId.ToString()).FirstOrDefault();

                incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó " +
                    $"el área responsable 1 de {(originalSolutionType != null ? originalSolutionType.Text : "Ninguno")} a {(newSolutionType != null ? newSolutionType.Text : "Ninguno")}.\n";
                incident.SolutionTypeId = model.SolutionTypeId;
            }

            if (incident.SolutionAmount != model.SolutionAmount)
            {
                incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó " +
                    $"el monto de la solución de {incident.SolutionAmount:C} a {model.SolutionAmount:C}.\n";
                incident.SolutionAmount = model.SolutionAmount;
            }

            if (incident.Comments != model.Comments)
            {
                incident.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó " +
                    $"el comentario de \"{incident.Comments}\" a \"{model.Comments}\".\n";
                incident.Comments = model.Comments;
            }
            _operationalIncidentService.Update(incident);

            return Ok();
        }

        [HttpPost]
        public IActionResult OperationalIncidentDelete(OperationalIncidentModel model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.OperationalIncident))
                return AccessDeniedView();
            var incident = _operationalIncidentService.GetById(model.Id);
            if (incident == null)
                return Ok();

            _operationalIncidentService.Delete(incident);
            return Ok();
        }

        [HttpGet]
        public IActionResult UploadXlsInfo()
        {
            // path to your excel file
            string path = "C:/Users/Ivan Salazar/Desktop/incidencias.xlsx";
            FileInfo fileInfo = new FileInfo(path);
            var errorList = new List<CellErrorModel>();
            var festiveDates = _assistanceService.GetFestiveDatesFromGroceries()
                .Where(x => !x.DontApplyToPayroll).ToList();
            var model = new OperationalIncidentListModel();
            PrepareModel(model, false);

            using (var memoryStream = new MemoryStream())
            {
                using (var package = new ExcelPackage(fileInfo.OpenRead()))
                {
                    var worksheets = package.Workbook.Worksheets.Where(x => x.Name == "Reparto" || x.Name == "Compras").ToList();
                    foreach (var worksheet in worksheets)
                    {
                        if (worksheet != null)
                        {
                            int operationalTypeId = worksheet.Name == "Reparto" ? 1 : 2;
                            List<string> headers = new List<string>();
                            int totalRows = worksheet.Dimension.End.Row;
                            int totalColumns = worksheet.Dimension.End.Column;
                            var range = worksheet.Cells[1, 1, 1, totalColumns];

                            try
                            {
                                var cells = worksheet.Cells.ToList();
                                var groups = GetCellGroups(cells, worksheet.Dimension.End.Row - 1);
                                if (groups == null) return BadRequest("Ocurrió un problema creando los grupos para la carga de datos");
                                var dataList = new List<OperationalIncidentModel>();
                                for (int g = 0; g < groups.Count; g++)
                                {
                                    int currentColumn = 0;
                                    var data = new OperationalIncidentModel();
                                    if (g == 217)
                                        _ = 0;
                                    try
                                    {
                                        var reportDateString = groups[g][currentColumn].Text != null ? groups[g][currentColumn].Text.ToString().Trim() : null;
                                        if (!string.IsNullOrEmpty(reportDateString))
                                        {
                                            var dateInfo = reportDateString.Split('-');
                                            var date = DateTime.ParseExact($"{dateInfo[2]}-{dateInfo[1]}-{dateInfo[0]}", "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                            data.ReportDate = date.ToString("dd/MM/yyyy");
                                        }
                                        currentColumn++;
                                        var orderDeliveryDateString = groups[g][currentColumn].Text != null ? groups[g][currentColumn].Text.ToString().Trim() : null;
                                        if (!string.IsNullOrEmpty(orderDeliveryDateString))
                                        {
                                            var dateInfo = orderDeliveryDateString.Split('-');
                                            var date = DateTime.ParseExact($"{dateInfo[2]}-{dateInfo[1]}-{dateInfo[0]}", "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                            data.OrderDeliveryDate = date.ToString("dd/MM/yyyy");
                                        }
                                        currentColumn++;

                                        var orderIdString = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString().Trim() : null;
                                        if (IsDigitsOnly(orderIdString))
                                            data.OrderId = int.Parse(orderIdString);
                                        else
                                            data.OrderId = 0;
                                        currentColumn++;

                                        var orderDeliveryRouteName = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString().Trim() : null;
                                        if (!string.IsNullOrEmpty(orderDeliveryRouteName))
                                        {
                                            var route = model.RouteInfos.Where(x => x.Name.Trim().ToLower() == orderDeliveryRouteName.ToLower()).FirstOrDefault();
                                            if (route != null)
                                                data.OrderDeliveryRouteId = route.Id;
                                        }
                                        currentColumn++;
                                        var orderDeliveryFranchiseName = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString().Trim() : null;
                                        if (!string.IsNullOrEmpty(orderDeliveryFranchiseName))
                                        {
                                            orderDeliveryFranchiseName = orderDeliveryFranchiseName.Split('.')[0].Replace("0", "");
                                            var franchise = model.FranchiseInfos.Where(x => x.Name.Split('(')[0].Trim().ToLower() == orderDeliveryFranchiseName.ToLower()).FirstOrDefault();
                                            if (franchise != null)
                                                data.OrderDeliveryFranchiseId = franchise.Id;
                                        }
                                        currentColumn++;
                                        var responsibleArea1Name = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString().Trim() : null;
                                        if (!string.IsNullOrEmpty(responsibleArea1Name))
                                        {
                                            var responsibleArea = model.ResponsibleAreaTypes.Where(x => x.Text.Trim().ToLower() == responsibleArea1Name.ToLower()).FirstOrDefault();
                                            if (responsibleArea != null)
                                                data.ResponsibleArea1 = int.Parse(responsibleArea.Value);
                                        }
                                        currentColumn++;
                                        var responsibleArea2Name = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString().Trim() : null;
                                        if (!string.IsNullOrEmpty(responsibleArea2Name))
                                        {
                                            var responsibleArea = model.ResponsibleAreaTypes.Where(x => x.Text.Trim().ToLower() == responsibleArea2Name.ToLower()).FirstOrDefault();
                                            if (responsibleArea != null)
                                                data.ResponsibleArea2 = int.Parse(responsibleArea.Value);
                                        }
                                        currentColumn++;
                                        var responsibleEmployee1Name = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString().Trim() : null;
                                        if (!string.IsNullOrEmpty(responsibleEmployee1Name))
                                        {
                                            var employee = model.Employees.Where(x => RemoveDiacritics(x.Text.Trim().ToLower()) == RemoveDiacritics(responsibleEmployee1Name.ToLower())).FirstOrDefault();
                                            if (employee != null)
                                                data.ResponsibleCustomerId1 = int.Parse(employee.Value);
                                        }
                                        currentColumn++;
                                        var responsibleEmployee2Name = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString().Trim() : null;
                                        if (!string.IsNullOrEmpty(responsibleEmployee2Name))
                                        {
                                            var employee = model.Employees.Where(x => RemoveDiacritics(x.Text.Trim().ToLower()) == RemoveDiacritics(responsibleEmployee2Name.ToLower())).FirstOrDefault();
                                            if (employee != null)
                                                data.ResponsibleCustomerId2 = int.Parse(employee.Value);
                                        }
                                        currentColumn++;

                                        currentColumn++;
                                        currentColumn++;
                                        currentColumn++;

                                        var incidentAmountString = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString().Trim() : null;
                                        decimal incidentAmount;
                                        if (decimal.TryParse(incidentAmountString, out incidentAmount))
                                            data.IncidentAmount = incidentAmount;
                                        else
                                            data.IncidentAmount = 0;
                                        currentColumn++;

                                        currentColumn++;

                                        data.IncidentDetails = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString().Trim() : null;
                                        currentColumn++;

                                        var authorizedString = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString().Trim() : null;
                                        data.Authorized = authorizedString?.ToLower() == "autorizado";
                                        currentColumn++;

                                        var appliedInBiweekString = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString().Trim() : null;
                                        if (appliedInBiweekString?.ToLower() == "no asignado")
                                            data.AppliedInBiweek = null;
                                        else
                                        {
                                            var biweekInfo = appliedInBiweekString.Split('-').Select(x => x.Trim()).ToList();
                                            var date = DateTime.ParseExact(biweekInfo.FirstOrDefault(), "MMMM yyyy", new CultureInfo("es-ES", false));
                                            BiweeklyDatesModel biweek = null;
                                            if (biweekInfo.LastOrDefault()?.ToLower() == "qna 1")
                                                biweek = BiweeklyDatesHelper.GetBiweeklyDates(new DateTime(date.Year, date.Month, 1), festiveDates);
                                            else if (biweekInfo.LastOrDefault()?.ToLower() == "qna 2")
                                                biweek = BiweeklyDatesHelper.GetBiweeklyDates(new DateTime(date.Year, date.Month, 16), festiveDates);

                                            if (biweek != null)
                                                data.AppliedInBiweek = biweek.EndOfBiweekly.ToString("dd/MM/yyyy");
                                        }
                                        currentColumn++;

                                        var solutionTypeName = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString().Trim() : null;
                                        if (!string.IsNullOrEmpty(solutionTypeName))
                                        {
                                            var solution = model.SolutionTypes.Where(x => x.Text.Trim().ToLower() == solutionTypeName.ToLower()).FirstOrDefault();
                                            if (solution != null)
                                                data.SolutionTypeId = int.Parse(solution.Value);
                                        }
                                        currentColumn++;

                                        var solutionAmountString = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString().Trim() : null;
                                        decimal solutionAmount;
                                        if (decimal.TryParse(solutionAmountString, out solutionAmount))
                                            data.SolutionAmount = solutionAmount;
                                        else
                                            data.SolutionAmount = 0;
                                        currentColumn++;

                                        data.Comments = groups[g][currentColumn].Value != null ? groups[g][currentColumn].Value.ToString().Trim() : null;
                                        currentColumn++;

                                        dataList.Add(data);
                                    }
                                    catch (Exception e)
                                    {
                                        errorList.Add(new CellErrorModel
                                        {
                                            CellObjectModel = data,
                                            Error = e.Message
                                        });
                                    }
                                }

                                var orderIds = dataList.Select(x => x.OrderId).ToList();
                                var orders = _orderService.GetAllOrdersQuery()
                                    .Where(x => orderIds.Contains(x.Id))
                                    .Select(x => new { x.Id, x.RescuedByRouteId, x.SelectedShippingDate })
                                    .ToList();
                                var franchiseAndRouteInfo = ExternalHelper.GetExternalFranchiseAndRoutesInfoByDates(_storeContext);
                                var count = 0;
                                foreach (var data in dataList)
                                {
                                    try
                                    {
                                        if (count > 216)
                                            _ = 0;
                                        var rescuedByRouteId = 0;
                                        var rescuedByFranchiseId = 0;
                                        var order = orders.Where(x => x.Id == data.OrderId).FirstOrDefault();
                                        if (order?.RescuedByRouteId != null)
                                        {
                                            rescuedByRouteId = order?.RescuedByRouteId ?? 0;
                                            rescuedByFranchiseId = GetFranchiseIdOfOrder(franchiseAndRouteInfo.FranchiseAndRouteDatesInfos, rescuedByRouteId, order.SelectedShippingDate.Value);
                                        }
                                        var operationalIncident = new OperationalIncident
                                        {
                                            OperationalIncidentTypeId = operationalTypeId,
                                            AppliedInBiweek = !string.IsNullOrEmpty(data.AppliedInBiweek?.Trim()) ? DateTime.ParseExact(data.AppliedInBiweek, "dd/MM/yyyy", CultureInfo.InvariantCulture) : (DateTime?)null,
                                            Authorized = data.Authorized,
                                            Comments = data.Comments,
                                            IncidentAmount = data.IncidentAmount,
                                            IncidentDetails = data.IncidentDetails,
                                            OrderDeliveryDate = !string.IsNullOrEmpty(data.OrderDeliveryDate?.Trim()) ? DateTime.ParseExact(data.OrderDeliveryDate, "dd/MM/yyyy", CultureInfo.InvariantCulture) : (DateTime?)null,
                                            OrderDeliveryFranchiseId = data.OrderDeliveryFranchiseId,
                                            OrderDeliveryRouteId = data.OrderDeliveryRouteId,
                                            OrderDeliveryRescuedRouteId = rescuedByRouteId,
                                            OrderDeliveryRescuedFranchiseId = rescuedByFranchiseId,
                                            OrderId = data.OrderId,
                                            ReportDate = DateTime.ParseExact(data.ReportDate ?? data.AppliedInBiweek?.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                            ResponsibleArea1 = data.ResponsibleArea1,
                                            ResponsibleArea2 = data.ResponsibleArea2,
                                            ResponsibleCustomerId1 = data.ResponsibleCustomerId1,
                                            ResponsibleCustomerId2 = data.ResponsibleCustomerId2,
                                            SolutionAmount = data.SolutionAmount,
                                            SolutionTypeId = data.SolutionTypeId,
                                            Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - Se agregó de forma automática la incidencia operacional para el día {data.ReportDate}.\n"
                                        };
                                        _operationalIncidentService.Insert(operationalIncident);
                                    }
                                    catch (Exception e)
                                    {
                                        _ = e;
                                    }
                                    count++;
                                }
                            }
                            catch (Exception e)
                            {
                                return BadRequest("Ocurrió un problema cargando los datos: " + e.Message);
                            }
                        }
                        else
                        {
                            return BadRequest("No fue posible cargar el excel");
                        }
                    }
                }
            }
            if (errorList.Any())
                return Ok("Se actualizaron codigos del sat pero algunos tuvieron errores: \n"
                    + string.Join("\n", errorList.Select(x => x.Error)));
            else
                return Ok("Los codigos del sat se actualizaron correctamente.");
        }

        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }

        private bool IsDigitsOnly(string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        private void GetHeaders(ref int init, ref List<string> headerList, int totalColumns, ExcelRange range, ExcelWorksheet sheet)
        {
            string[] initCol;
            for (int i = 1; i <= totalColumns; ++i)
            {
                if (!string.IsNullOrEmpty(sheet.Cells[1, i].Text))
                {
                    if (headerList.Count() == 0)
                    {
                        initCol = System.Text.RegularExpressions.Regex.Split(range[1, i].Address, @"\D+");
                        init = int.Parse(initCol[1]);
                    }
                    headerList.Add(sheet.Cells[1, i].Text);
                }
            }
        }
        private List<List<CellDataModel>> GetCellGroups(List<ExcelRangeBase> elements, int finalRow)
        {
            int i = 0;
            int g = 0;
            try
            {
                var list = new List<List<CellDataModel>>();
                var headerLetters = elements.Where(x => x.Start.Row == 1).Select(x => x.Address).Select(x => new String(x.Where(y => Char.IsLetter(y)).ToArray())).ToList();
                for (i = 0; i < finalRow; i++)
                {
                    var listData = new List<CellDataModel>();
                    for (g = 0; g < headerLetters.Count; g++)
                    {
                        var address = headerLetters[g] + (i + 2).ToString();
                        var element = elements.Where(x => x.Address == address).FirstOrDefault();
                        if (element == null || element.Value == null)
                        {
                            listData.Add(new CellDataModel() { Address = address, Value = null, Text = null });
                        }
                        else
                        {
                            listData.Add(new CellDataModel() { Address = address, Value = element.Value.ToString(), Text = element.Text.ToString() });
                        }
                    }
                    list.Add(listData);
                }

                return list;
            }
            catch (Exception w)
            {
                return null;
            }
        }
    }

    public class CellErrorModel
    {
        public OperationalIncidentModel CellObjectModel { get; set; }
        public string Error { get; set; }
    }

    public class CellDataModel
    {
        public string Address { get; set; }
        public string Value { get; set; }
        public string Text { get; set; }
    }

    public class FranchiseAndRoutesExternal
    {
        public FranchiseAndRoutesExternal()
        {
            FranchiseInfos = new List<FranchiseInfo>();
            RouteInfos = new List<RouteInfo>();
            FranchiseAndRouteDatesInfos = new List<FranchiseAndRouteDatesInfo>();
        }

        public List<FranchiseInfo> FranchiseInfos { get; set; }
        public List<RouteInfo> RouteInfos { get; set; }
        public List<FranchiseAndRouteDatesInfo> FranchiseAndRouteDatesInfos { get; set; }
    }

    public class FranchiseInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class RouteInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class FranchiseAndRouteDatesInfo
    {
        public DateTime ShippingDate { get; set; }
        public int FranchiseId { get; set; }
        public int RouteId { get; set; }
    }

    public class OrderInfo
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public int RescuedByRouteId { get; set; }
        public int FranchiseId { get; set; }
        public int RescuedByFranchiseId { get; set; }
        public string SelectedShippingDate { get; set; }
    }
}