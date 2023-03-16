using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Controllers;
using Teed.Plugin.Groceries.Domain.BuyerPayments;
using Teed.Plugin.Groceries.Services;

namespace Teed.Plugin.Groceries.Components
{
    [ViewComponent(Name = "BuyerPaymentLog")]
    public class BuyerPaymentManufacturerViewComponent : NopViewComponent
    {
        private readonly BuyerPaymentService _buyerPaymentService;
        private readonly BuyerPaymentTicketFileService _buyerPaymentTicketFileService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICustomerService _customerService;

        public BuyerPaymentManufacturerViewComponent(BuyerPaymentService buyerPaymentService,
            BuyerPaymentTicketFileService buyerPaymentTicketFileService,
            IManufacturerService manufacturerService,
            ICustomerService customerService)
        {
            _buyerPaymentService = buyerPaymentService;
            _buyerPaymentTicketFileService = buyerPaymentTicketFileService;
            _manufacturerService = manufacturerService;
            _customerService = customerService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var manufacturerId = (int)additionalData;
            var manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);
            var paymentRequest = _buyerPaymentService.GetAll().Where(x => x.ManufacturerId == manufacturerId).ToList();
            var buyerPaymentIds = paymentRequest.Select(x => x.Id).ToList();
            var customerIds = paymentRequest.Select(x => x.BuyerId).Distinct().ToList();
            var ticketBuyerFiles = _buyerPaymentTicketFileService.GetAll()
                .Where(x => buyerPaymentIds.Contains(x.BuyerPaymentId))
                .Select(x => new { x.FileId, x.BuyerPaymentId })
                .ToList();
            var customers = _customerService.GetAllCustomersQuery().Where(x => customerIds.Contains(x.Id)).ToList();

            var model = new List<BuyerPaymentModel>();
            foreach (var item in paymentRequest)
            {
                model.Add(new BuyerPaymentModel()
                {
                    CreationDate = item.CreatedOnUtc.ToLocalTime(),
                    RequestedAmount = item.RequestedAmount,
                    BuyerId = item.BuyerId,
                    BuyerPaymentId = item.Id,
                    Date = item.ShippingDate,
                    InvoiceFilePdfId = item.InvoiceFilePdfId,
                    InvoiceFileXmlId = item.InvoiceFileXmlId,
                    ManufacturerId = item.ManufacturerId,
                    ManufacturerName = manufacturer?.Name,
                    PaymentFileId = item.PaymentFileId,
                    PaymentStatus = (BuyerPaymentStatus)item.PaymentStatusId,
                    BuyerName = customers.Where(x => x.Id == item.BuyerId).FirstOrDefault()?.GetFullName(),
                    TicketBuyerFileIds = ticketBuyerFiles
                    .Where(x => x.BuyerPaymentId == item.Id)
                    .Select(x => x.FileId)
                    .ToList()
                });
            }

            return View("~/Plugins/Teed.Plugin.Groceries/Views/Shared/Components/BuyerPaymentManufacturer/Default.cshtml", model.OrderByDescending(x => x.Date).ToList());
        }
    }
}
