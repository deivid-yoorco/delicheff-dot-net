using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using System;
using System.IO;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.SaleRecord.Domain.SaleRecords;
using Teed.Plugin.SaleRecord.Models.SaleRecords;
using Teed.Plugin.SaleRecord.Security;
using Teed.Plugin.SaleRecord.Services;
using System.Net.Http;

namespace Teed.Plugin.SaleRecord.Controllers
{
    [Area(AreaNames.Admin)]
    public class SaleRecordController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly SaleRecordService _saleRecordService;

        public SaleRecordController(IPermissionService permissionService, SaleRecordService saleRecordService,
            IWorkContext workContext)
        {
            _permissionService = permissionService;
            _saleRecordService = saleRecordService;
            _workContext = workContext;

        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(SaleRecordPermissionProvider.SaleRecords))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.SaleRecord/Views/SaleRecord/List.cshtml");
        }

        public async Task<IActionResult> Create()
        {
            if (!_permissionService.Authorize(SaleRecordPermissionProvider.SaleRecords))
                return AccessDeniedView();

            var model = new SaleRecordModel();
            model.SaleDateString = DateTime.Now.ToString("dd-MM-yyyy");

            model.ListTimes = await GetTimeListGlobal();

            return View("~/Plugins/Teed.Plugin.SaleRecord/Views/SaleRecord/Create.cshtml", model);
        }

        [HttpPost]
        public IActionResult Create(SaleRecordModel model)
        {
            if (!_permissionService.Authorize(SaleRecordPermissionProvider.SaleRecords))
                return AccessDeniedView();

            model.SaleDate = DateTime.ParseExact(model.SaleDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            SaleRecords saleRecordD = new SaleRecords()
            {
                SaleDate = model.SaleDate,
                Quantity = model.Quantity,
                CustomerFullName = model.CustomerFullName,
                CustomerEmail = model.CustomerEmail,
                CustomerPhoneNumber = model.CustomerPhoneNumber,
                PaymentMethod = model.PaymentMethod,
                Total = model.Total,
                Time = model.Time,
                CardTypeId = model.CardTypeId,
                CardLast4Numbers = model.CardLast4Numbers,
                SaleDateString = model.SaleDateString,
                Comment = model.Comment,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) se creó el registro de ventas.\n"
            };
            _saleRecordService.Insert(saleRecordD);

            return RedirectToAction("List");
        }

        public async Task<List<SelectListItem>> GetTimeListGlobal()
        {
            var url = (Request.IsHttps ? "https" : "http") + $"://{Request.Host}/Schedule/GetAllTimesSelectList";
            using (var client = new HttpClient())
            {
                var selectList = new List<SelectListItem>();
                var result = await client.GetAsync(url);
                if (result.IsSuccessStatusCode)
                {
                    var resultJson = await result.Content.ReadAsStringAsync();
                    selectList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SelectListItem>>(resultJson);
                }
                return selectList;
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!_permissionService.Authorize(SaleRecordPermissionProvider.SaleRecords))
                return AccessDeniedView();

            var saleRecord = _saleRecordService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (saleRecord == null) return NotFound();

            var ListTimesList = await GetTimeListGlobal();
            var model = new SaleRecordModel()
            {
                Id = saleRecord.Id,
                SaleDate = saleRecord.SaleDate,
                Quantity = saleRecord.Quantity,
                CustomerFullName = saleRecord.CustomerFullName,
                CustomerEmail = saleRecord.CustomerEmail,
                CustomerPhoneNumber = saleRecord.CustomerPhoneNumber,
                PaymentMethod = saleRecord.PaymentMethod,
                Total = saleRecord.Total,
                Time = saleRecord.Time,
                CardTypeId = saleRecord.CardTypeId,
                CardLast4Numbers = saleRecord.CardLast4Numbers,
                SaleDateString = saleRecord.SaleDateString,
                Comment = saleRecord.Comment,
                Log = saleRecord.Log,
                ListTimes = ListTimesList
            };
            return View("~/Plugins/Teed.Plugin.SaleRecord/Views/SaleRecord/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult Edit(SaleRecordModel model)
        {
            if (!_permissionService.Authorize(SaleRecordPermissionProvider.SaleRecords))
                return AccessDeniedView();

            var saleRecord = _saleRecordService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (saleRecord == null) return NotFound();

            model.SaleDate = DateTime.ParseExact(model.SaleDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            string log = PrepareLog(model, saleRecord);
            saleRecord.UpdatedOnUtc = DateTime.UtcNow;
            saleRecord.SaleDate = model.SaleDate;
            saleRecord.Quantity = model.Quantity;
            saleRecord.CustomerFullName = model.CustomerFullName;
            saleRecord.CustomerEmail = model.CustomerEmail;
            saleRecord.CustomerPhoneNumber = model.CustomerPhoneNumber;
            saleRecord.PaymentMethod = model.PaymentMethod;
            saleRecord.Total = model.Total;
            saleRecord.Time = model.Time;
            saleRecord.CardTypeId = model.CardTypeId;
            saleRecord.CardLast4Numbers = model.CardLast4Numbers;
            saleRecord.SaleDateString = model.SaleDateString;
            saleRecord.Comment = model.Comment;
            saleRecord.Log += log;

            _saleRecordService.Update(saleRecord);

            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(SaleRecordPermissionProvider.SaleRecords))
                return AccessDeniedView();

            var SaleRecord = _saleRecordService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (SaleRecord == null) return NotFound();

            SaleRecord.Log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - El elemento fue eliminado por " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}).\n";

            _saleRecordService.Delete(SaleRecord);

            return RedirectToAction("List");
        }

        [HttpPost]
        public async Task<IActionResult> ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(SaleRecordPermissionProvider.SaleRecords))
                return AccessDeniedView();

            var query = _saleRecordService.GetAll();
            var pagedList = new PagedList<SaleRecords>(query.OrderByDescending(m => m.CreatedOnUtc), command.Page - 1, command.PageSize);
            var ListTimes = await GetTimeListGlobal();

            DataSourceResult gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    x.SaleDateString,
                    Total = x.Total.ToString("C"),
                    x.Quantity,
                    x.CustomerFullName,
                    x.CustomerEmail,
                    x.CustomerPhoneNumber,
                    x.PaymentMethod,
                    Time = GetTextFromTime(x, ListTimes),
                    x.CardTypeId,
                    x.CardLast4Numbers,
                    x.Comment,
                }),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        public string GetTextFromTime(SaleRecords x, List<SelectListItem> ListTimes)
        {
            var time = ListTimes.Where(y => y.Value == x.Time).FirstOrDefault();
            if(time == null)
                return ("No se encontro el horario");

            return time.Text;
        }

        [HttpGet]
        public async Task<IActionResult> ImportaExcelSaleDay()
        {
            var RecordList = _saleRecordService.GetAll().Where(x => !x.Deleted).ToList();
            var ListTimes = await GetTimeListGlobal();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Id";
                    worksheet.Cells[row, 2].Value = "Día de la visita";
                    worksheet.Cells[row, 3].Value = "Horario de visita";
                    worksheet.Cells[row, 4].Value = "Cantidad de tickets";
                    worksheet.Cells[row, 5].Value = "Total";
                    worksheet.Cells[row, 6].Value = "Nombre del cliente";
                    worksheet.Cells[row, 7].Value = "Correo electrónico";
                    worksheet.Cells[row, 8].Value = "Número telefónico";
                    worksheet.Cells[row, 9].Value = "Metodo de pago";
                    worksheet.Cells[row, 10].Value = "Tipo de tarjeta";
                    worksheet.Cells[row, 11].Value = "4 últimos números de la tarjeta";
                    worksheet.Cells[row, 12].Value = "Comentarios";

                    foreach (var record in RecordList)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = record.Id;
                        worksheet.Cells[row, 2].Value = record.SaleDateString;
                        worksheet.Cells[row, 3].Value = GetTextFromTime(record, ListTimes);
                        worksheet.Cells[row, 4].Value = record.Quantity;
                        worksheet.Cells[row, 5].Value = record.Total;
                        worksheet.Cells[row, 6].Value = record.CustomerFullName;
                        worksheet.Cells[row, 7].Value = record.CustomerEmail;
                        worksheet.Cells[row, 8].Value = record.CustomerPhoneNumber;
                        worksheet.Cells[row, 9].Value = record.PaymentMethod;
                        worksheet.Cells[row, 10].Value = Helpers.EnumHelper.GetDisplayName((CardType)record.CardTypeId);
                        worksheet.Cells[row, 11].Value = record.CardLast4Numbers;
                        worksheet.Cells[row, 12].Value = record.Comment;

                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"Lista_de_ventas.xlsx");
            }
        }

        private string PrepareLog(SaleRecordModel newValue, SaleRecords currentValue)
        {
            string log = string.Empty;
            if (newValue.SaleDate != currentValue.SaleDate)
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el dia de visita, de {currentValue.SaleDate} a {newValue.SaleDate}.\n";
            }
            if (newValue.Time != currentValue.Time)
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el horario de visita, de '{currentValue.Time}' a '{newValue.Time}'.\n";
            }
            if (newValue.Quantity != currentValue.Quantity)
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió la cantidad de boletos, de '{currentValue.Quantity}' a '{newValue.Quantity}'.\n";
            }
            if (newValue.Total != currentValue.Total)
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el monto total, de '{currentValue.Total}' a '{newValue.Total}'.\n";
            }
            if (newValue.Total != currentValue.Total)
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el monto total, de '{currentValue.Total}' a '{newValue.Total}'.\n";
            }
            if (newValue.CustomerFullName != currentValue.CustomerFullName)
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el nombre del cliente, de '{currentValue.CustomerFullName}' a '{newValue.CustomerFullName}'.\n";
            }
            if (newValue.CustomerEmail != currentValue.CustomerEmail)
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el correo electronico, de '{currentValue.CustomerEmail}' a '{newValue.CustomerEmail}'.\n";
            }
            if (newValue.CustomerPhoneNumber != currentValue.CustomerPhoneNumber)
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el número de teléfono, de '{currentValue.CustomerPhoneNumber}' a '{newValue.CustomerPhoneNumber}'.\n";
            }
            if (newValue.PaymentMethod != currentValue.PaymentMethod)
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el metodo de pago, de '{currentValue.PaymentMethod}' a '{newValue.PaymentMethod}'.\n";
            }
            if (newValue.CardTypeId != currentValue.CardTypeId)
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el tipo de tarjeta, de '{currentValue.CardTypeId}' a '{newValue.CardTypeId}'.\n";
            }
            if (newValue.CardLast4Numbers != currentValue.CardLast4Numbers)
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió los 4 ultimos numeros de la tarjeta, de '{currentValue.CardLast4Numbers}' a '{newValue.CardLast4Numbers}'.\n";
            }
            if (newValue.Comment != currentValue.Comment)
            {
                log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) cambió el comentario, de '{currentValue.Comment}' a '{newValue.Comment}'.\n";
            }
            return log;
        }
    }
}
