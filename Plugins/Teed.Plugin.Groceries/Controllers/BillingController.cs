using iText.Forms.Xfdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.Franchise;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;
using Teed.Plugin.Groceries.Models.Franchise;
using Teed.Plugin.Groceries.Models.RatesAndBonuses;
using Teed.Plugin.Groceries.Models.ShippingUserRoute;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;
using Nop.Services.Helpers;
using System.IO;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class BillingController : BasePluginController
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
        private readonly PaymentFileService _billingFileService;
        private readonly IncidentsService _incidentsService;

        public BillingController(IPermissionService permissionService, ShippingRouteService shippingRouteService,
            ShippingRouteUserService shippingRouteUserService, IWorkContext workContext, ICustomerService customerService,
            IOrderService orderService, FranchiseService franchiseService, ISettingService settingService,
            BillingService billingService, PaymentFileService billingFileService, IncidentsService incidentsService)
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
            _billingFileService = billingFileService;
            _incidentsService = incidentsService;
        }

        public void PrepareModel(BillingModel model)
        {
            //model.FileTypes = Enum.GetValues(typeof(BillingFileType))
            //    .Cast<BillingFileType>().Select(v => new SelectListItem
            //    {
            //        Text = v.GetDisplayName(),
            //        Value = ((int)v).ToString()
            //    }).ToList();

            //model.Status = Enum.GetValues(typeof(PaymentStatus))
            //    .Cast<PaymentStatus>().Select(v => new SelectListItem
            //    {
            //        Text = v.GetDisplayName(),
            //        Value = ((int)v).ToString()
            //    }).ToList();

            model.Franchises = _franchiseService.GetAll()
                .Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                }).ToList();
        }

        //public string GetStatus(Billing billing)
        //{
        //    var files = _billingFileService.GetAll()
        //        .Where(x => x.BillingId == billing.Id)
        //        .ToList();
        //    var pdfs = files.Where(x =>
        //        x.FileTypeId == (int)BillingFileType.BillPdf)
        //        .Count();
        //    var xmls = files.Where(x =>
        //        x.FileTypeId == (int)BillingFileType.BillXml)
        //        .Count();
        //    var final = string.Empty;
        //    if (billing.StatusId == (int)PaymentStatus.Paid)
        //        final = "green";
        //    else if (billing.StatusId == (int)PaymentStatus.Pending &&
        //        pdfs > 0 && xmls > 0)
        //        final = "red";
        //    else if (billing.StatusId == (int)PaymentStatus.Pending &&
        //        pdfs < 1 && xmls < 1)
        //        final = "yellow";
        //    return final;
        //}

        public string GetFullDateString(Billing billing)
        {
            string s_date = StringHelpers.FirstCharToUpper(billing.InitDate.ToString("dddd", new CultureInfo("es-MX")));
            string e_date = billing.EndDate.ToString("dddd", new CultureInfo("es-MX"));
            return $"{s_date} {billing.InitDate.ToString("dd/MM/yyyy")} a {e_date} {billing.EndDate.ToString("dd/MM/yyyy")}";
        }

        public string GetFranchiseName(int id, List<Franchise> franchises)
        {
            var final = "Sin especificar";
            var franchise = franchises.Where(x => x.Id == id).FirstOrDefault();
            if (franchise != null)
                final = franchise.Name;
            return final;
        }

        public IActionResult BillingList()
        {
            return View("~/Plugins/Teed.Plugin.Groceries/Views/Billing/BillingList.cshtml");
        }

        [HttpPost]
        public IActionResult BillingListData(DataSourceRequest command)
        {
            var query = _billingService.GetAll();
            var queryList = query.OrderByDescending(m => m.InitDate).ToList();
            var pagedList = new PagedList<Billing>(queryList, command.Page - 1, command.PageSize);
            var franchises = _franchiseService.GetAll().ToList();

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    Vehicle = VehicleUtils.GetVehicleName(x.Vehicle),
                    Franchise = x.Vehicle?.Franchise?.Name,
                    Comment = string.IsNullOrEmpty(x.Comment) ? "Sin comentario" : x.Comment,
                    Amount = StringHelpers.ToCurrency(x.Billed),
                    WeekName = GetFullDateString(x)
                }).ToList(),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        public IActionResult Create()
        {
            BillingModel model = new BillingModel();
            PrepareModel(model);
            return View("~/Plugins/Teed.Plugin.Groceries/Views/Billing/Create.cshtml", model);
        }

        //[HttpPost]
        //public IActionResult Create(BillingModel model)
        //{
        //    if (!ModelState.IsValid) return BadRequest();
        //    Billing billing = new Billing()
        //    {
        //        Comment = model.Comment,
        //        Billed = model.Billed,
        //        InitDate = model.InitDate,
        //        EndDate = model.EndDate,
        //        FranchiseId = model.FranchiseId,
        //        BonusAmount = model.BonusAmount,
        //        AmountAjustment = model.AmountAjustment,
        //        Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) creó la factura {model.Id}.\n"
        //    };
        //    _billingService.Insert(billing);

        //    return RedirectToAction("BillingList");
        //}

        //public IActionResult Edit(int id)
        //{
        //    Billing billing = _billingService.GetAll().Where(x => x.Id == id).FirstOrDefault();
        //    if (billing == null) return NotFound();

        //    BillingModel model = new BillingModel()
        //    {
        //        Id = billing.Id,
        //        Comment = billing.Comment,
        //        Billed = billing.Billed,
        //        InitDate = billing.InitDate,
        //        EndDate = billing.EndDate,
        //        FranchiseId = billing.FranchiseId,
        //        BonusAmount = billing.BonusAmount,
        //        AmountAjustment = billing.AmountAjustment,
        //        Log = billing.Log
        //    };
        //    PrepareModel(model);
        //    return View("~/Plugins/Teed.Plugin.Groceries/Views/Billing/Edit.cshtml", model);
        //}

        [HttpPost]
        public IActionResult Edit(BillingModel model)
        {
            Billing billing = _billingService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (billing == null) { return NotFound(); }

            if (!ModelState.IsValid) return BadRequest();

            string log = string.Empty;
            if (billing.Comment != model.Comment)
            {
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el comentario de la factura de {billing.Comment} por {model.Comment}.\n";
                billing.Log += log;

                billing.Comment = model.Comment;
            }

            if (billing.Billed != model.Billed)
            {
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el valor facturado de {billing.Billed} por {model.Billed}.\n";
                billing.Log += log;

                billing.Billed = model.Billed;
            }

            if (billing.InitDate != model.InitDate
                && billing.EndDate != model.EndDate)
            {
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió la fecha de la factura de {billing.InitDate.ToString("dd/MM/yyyy")}-{billing.EndDate.ToString("dd/MM/yyyy")} por {model.InitDate.ToString("dd/MM/yyyy")}-{model.EndDate.ToString("dd/MM/yyyy")}.\n";
                billing.Log += log;

                billing.InitDate = model.InitDate;
                billing.EndDate = model.EndDate;
            }

            //if (billing.FranchiseId != model.FranchiseId)
            //{
            //    log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió la franquicia de la factura de [{billing.FranchiseId}] por [{model.FranchiseId}].\n";
            //    billing.Log += log;

            //    billing.FranchiseId = model.FranchiseId;
            //}

            if (billing.AmountAjustment != model.AmountAjustment)
            {
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el monto ajustable de la factura de {billing.AmountAjustment} por {model.AmountAjustment}.\n";
                billing.Log += log;

                billing.AmountAjustment = model.AmountAjustment;
            }

            _billingService.Update(billing);

            return RedirectToAction("BillingList");
        }

        //delete
        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            var billing = _billingService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (billing == null)
                //No recurring payment found with the specified id
                return RedirectToAction("BillingList");

            _billingService.Delete(billing);

            return RedirectToAction("BillingList");
        }

        //[HttpPost]
        //public IActionResult GetBilled(BilledData data)
        //{
        //    var final = (decimal)0;
        //    if (data.FranchiseId > 0)
        //    {
        //        var franchise = _franchiseService.GetAll()
        //            .Where(x => x.Id == data.FranchiseId)
        //            .FirstOrDefault();

        //        var parsedInitDate = DateTime.ParseExact(data.InitDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
        //        var parsedEndDate = DateTime.ParseExact(data.EndDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);

        //        if (franchise != null)
        //            final = _billingService.GetBilledWithAdjustment(franchise, parsedInitDate, parsedEndDate, data.BillId, true, data.AmountAjustment);
        //    }
        //    return Ok(final);
        //}
    }

    public class BilledData
    {
        public string InitDate { get; set; }
        public string EndDate { get; set; }
        public int FranchiseId { get; set; }
        public decimal AmountAjustment { get; set; }
        public int BillId { get; set; }
    }
}
