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

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class PaymentController : BasePluginController
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
        private readonly PaymentFileService _paymentFileService;
        private readonly IncidentsService _incidentsService;
        private readonly PaymentService _paymentService;

        public PaymentController(IPermissionService permissionService, ShippingRouteService shippingRouteService,
            ShippingRouteUserService shippingRouteUserService, IWorkContext workContext, ICustomerService customerService,
            IOrderService orderService, FranchiseService franchiseService, ISettingService settingService,
            BillingService billingService, PaymentFileService paymentFileService, IncidentsService incidentsService,
            PaymentService paymentService)
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
            _paymentFileService = paymentFileService;
            _incidentsService = incidentsService;
            _paymentService = paymentService;
        }

        public IActionResult List(int franchiseId = 0)
        {
            var franchises = _franchiseService.GetAll().OrderBy(x => x.Name);
            var model = new PaymentListModel()
            {
                SelectedFranchiseId = franchiseId,
                Franchises = franchises.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList()
            };

            model.Franchises.Insert(0, new SelectListItem() { Text = "Todas las franquicias", Value = "0" });

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Payments/List.cshtml", model);
        }

        public IActionResult Create()
        {
            var model = new PaymentModel();
            PrepareModel(model);
            return View("~/Plugins/Teed.Plugin.Groceries/Views/Payments/Create.cshtml", model);
        }

        public IActionResult Edit(int id)
        {
            var payment = _paymentService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (payment == null) return NotFound();

            var model = new PaymentModel()
            {
                PaymentAmount = payment.PaymentAmount,
                Comment = payment.Comment,
                FranchiseId = payment.FranchiseId,
                Log = payment.Log,
                StatusId = payment.StatusId,
                Id = payment.Id,
                PaymentDate = payment.PaymentDate,
                PaymentDateString = payment.PaymentDate.HasValue ? payment.PaymentDate.Value.ToString("dd-MM-yyyy") : ""
            };

            PrepareModel(model);

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Payments/Edit.cshtml", model);
        }

        public IActionResult PaymentFileList(int paymentId, DataSourceRequest command)
        {
            var query = _paymentFileService.GetAll().Where(x => x.PaymentId == paymentId);
            var queryList = query.OrderByDescending(m => m.CreatedOnUtc).ToList();
            var pagedList = new PagedList<PaymentFile>(queryList, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    Title = ((PaymentFileType)x.FileTypeId).GetDisplayName()
                }).ToList(),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        [HttpGet]
        public IActionResult PaymentFileDownload(int id, bool? isPdf = null)
        {
            var file = _paymentFileService.GetAll().Where(x => x.Id == id);
            if (isPdf != null)
            {
                var pdf = isPdf ?? false;
                file.Where(x => x.FileTypeId == (pdf ? (int)PaymentFileType.BillPdf :
                (int)PaymentFileType.BillXml)).FirstOrDefault();
                if (file == null)
                    return Ok();
            }
            else
            {
                file.FirstOrDefault();
                if (file == null)
                    return Ok();
            }
            var finalFile = file.FirstOrDefault();
            var model = new AddPaymentFile
            {
                Title = ((PaymentFileType)finalFile.FileTypeId).GetDisplayName(),
                Extension = finalFile.Extension,
                FileArray = finalFile.File
            };
            return Json(model);
        }

        [HttpPost]
        public IActionResult PaymentFileAdd(AddPaymentFile model)
        {
            try
            {
                if (model.IsPdf != null)
                    model.FileTypeId = (model.IsPdf ?? false) ? (int)PaymentFileType.BillPdf :
                        (int)PaymentFileType.BillXml;
                byte[] bytes = new byte[0];
                using (var ms = new MemoryStream())
                {
                    model.File.CopyTo(ms);
                    bytes = ms.ToArray();
                }
                var file = new PaymentFile
                {
                    File = bytes,
                    Extension = model.File.FileName.Substring(model.File.FileName.LastIndexOf(".") + 1),
                    FileMimeType = model.File.ContentType,
                    FileTypeId = model.FileTypeId,
                    Size = (int)model.File.Length,
                    PaymentId = model.PaymentId,
                };
                _paymentFileService.Insert(file);

                var payment = _paymentService.GetAll().Where(x => x.Id == model.PaymentId).FirstOrDefault();
                if (payment != null)
                {
                    payment.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) subío un archivo de tipo {((PaymentFileType)model.FileTypeId).GetDisplayName()} al pago.\n";
                    _paymentService.Update(payment);
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        public IActionResult PaymentFileUpdate(AddPaymentFile model)
        {
            var file = _paymentFileService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (file == null) return NotFound();

            file.Title = model.Title;
            file.Description = model.Description;
            _paymentFileService.Update(file);

            var payment = _paymentService.GetAll().Where(x => x.Id == model.PaymentId).FirstOrDefault();
            if (payment != null)
            {
                payment.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) actualizó un archivo de tipo {((PaymentFileType)model.FileTypeId).GetDisplayName()} ({file.Id}) del pago.\n";
                _paymentService.Update(payment);
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult PaymentFileDelete(AddPaymentFile model)
        {
            var file = _paymentFileService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (file == null) return BadRequest();

            file.Deleted = true;
            _paymentFileService.Update(file);

            var payment = _paymentService.GetAll().Where(x => x.Id == model.PaymentId).FirstOrDefault();
            if (payment != null)
            {
                payment.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) eliminó un archivo de tipo {((PaymentFileType)model.FileTypeId).GetDisplayName()} ({file.Id}) del pago.\n";
                _paymentService.Update(payment);
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command, int franchiseId = 0)
        {
            var query = _paymentService.GetAll();
            if (franchiseId > 0)
                query = query.Where(x => x.FranchiseId == franchiseId);

            var queryList = query.OrderByDescending(m => m.CreatedOnUtc).ToList();
            var pagedList = new PagedList<Payment>(queryList, command.Page - 1, command.PageSize);
            var franchises = _franchiseService.GetAll().ToList();

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    CreationDate = x.CreatedOnUtc.ToLocalTime().ToString("dd-MM-yyyy"),
                    Franchise = x.Franchise.Name,
                    PaymentDate = x.PaymentDate.HasValue ? x.PaymentDate.Value.ToString("dd-MM-yyyy") : "No pagado",
                    PaymentDateValue = x.PaymentDate,
                    PaymentAmount = x.PaymentAmount.ToString("C"),
                    Status = !x.PaymentFiles.Where(y => y.FileTypeId == 0 && !y.Deleted).Any() || !x.PaymentFiles.Where(y => y.FileTypeId == 1 && !y.Deleted).Any() ? "red" : x.StatusId == 1 && x.PaymentFiles.Where(y => y.FileTypeId != 2 && !y.Deleted).Any() ? "green" : "yellow"
                }).OrderByDescending(x => !x.PaymentDateValue.HasValue).ThenByDescending(x => x.PaymentDateValue).ToList(),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult Create(PaymentModel model)
        {
            if (!ModelState.IsValid || (string.IsNullOrEmpty(model.PaymentDateString) && model.StatusId == 1))
            {
                PrepareModel(model);
                if (string.IsNullOrEmpty(model.PaymentDateString) && model.StatusId == 1)
                    ModelState.AddModelError("", "Debes indicar la fecha de pago cuando el pago está como 'Pagado'");
                else
                    ModelState.AddModelError("", "Revisa la información ingresada e inténtalo de nuevo");
                return View("~/Plugins/Teed.Plugin.Groceries/Views/Payments/Create.cshtml", model);
            }

            if (!string.IsNullOrEmpty(model.PaymentDateString))
                model.PaymentDate = DateTime.ParseExact(model.PaymentDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            Payment payment = new Payment()
            {
                PaymentAmount = model.PaymentAmount,
                PaymentDate = model.PaymentDate,
                Comment = model.Comment,
                FranchiseId = model.FranchiseId,
                StatusId = model.StatusId,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.Email + $" ({_workContext.CurrentCustomer.Id}) creó la factura {model.Id}.\n"
            };
            _paymentService.Insert(payment);

            return RedirectToAction("Edit", new { id = payment.Id });
        }

        [HttpPost]
        public IActionResult Edit(PaymentModel model)
        {
            if (!ModelState.IsValid || (string.IsNullOrEmpty(model.PaymentDateString) && model.StatusId == 1))
            {
                PrepareModel(model);
                if (!model.PaymentDate.HasValue && model.StatusId == 1)
                    ModelState.AddModelError("", "Debes indicar la fecha de pago cuando el pago está como 'Pagado'");

                else
                    ModelState.AddModelError("", "Revisa la información ingresada e inténtalo de nuevo");
                return View("~/Plugins/Teed.Plugin.Groceries/Views/Payments/Edit.cshtml", model);
            }

            var payment = _paymentService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (payment == null) return NotFound();

            if (!string.IsNullOrEmpty(model.PaymentDateString))
                model.PaymentDate = DateTime.ParseExact(model.PaymentDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var log = UpdateLog(model, payment);

            payment.PaymentAmount = model.PaymentAmount;
            payment.Comment = model.Comment;
            payment.FranchiseId = model.FranchiseId;
            payment.StatusId = model.StatusId;
            payment.PaymentDate = model.PaymentDate;
            payment.Log += log;

            _paymentService.Update(payment);

            return RedirectToAction("Edit", new { model.Id });
        }

        private void PrepareModel(PaymentModel model)
        {
            var franchises = _franchiseService.GetAll();
            model.Franchises = franchises.Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            if (model.PaymentDate.HasValue)
            {
                model.PaymentDateString = model.PaymentDate.Value.ToString("dd-MM-yyyy");
            }
        }

        private string UpdateLog(PaymentModel newValue, Payment currentValue)
        {
            var log = string.Empty;

            if (currentValue.FranchiseId != newValue.FranchiseId)
            {
                log += $" Cambió la franquicia con ID {currentValue.FranchiseId} a la franquicia con ID {newValue.FranchiseId}.";
            }

            if (currentValue.Comment != newValue.Comment)
            {
                log += $" Cambió el comentario de {currentValue.Comment} a {newValue.Comment}.";
            }

            if (currentValue.PaymentAmount != newValue.PaymentAmount)
            {
                log += $" Cambió el monto de {currentValue.PaymentAmount.ToString("C")} a {newValue.PaymentAmount.ToString("C")}.";
            }

            if (currentValue.PaymentDate != newValue.PaymentDate)
            {
                log += $" Cambió la fecha de pago de {currentValue.PaymentDate} a {newValue.PaymentDate}.";
            }

            if (currentValue.StatusId != newValue.StatusId)
            {
                log += $" Cambió el estado de {EnumHelper.GetDisplayName((PaymentStatus)currentValue.StatusId)} a {EnumHelper.GetDisplayName((PaymentStatus)newValue.StatusId)}.";
            }

            if (!string.IsNullOrWhiteSpace(log))
            {
                log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - El usuario " + _workContext.CurrentCustomer.Email + $" ({_workContext.CurrentCustomer.Id}) modificó el pago." + log + "\n";
            }

            return log;
        }
    }
}
