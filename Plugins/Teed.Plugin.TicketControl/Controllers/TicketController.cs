using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using Teed.Plugin.TicketControl.Services;
using QRCoder;
using System.Drawing;
using System.IO;
using Teed.Plugin.TicketControl.Helpers;
using Nop.Services.Messages;
using Nop.Core.Data;
using Nop.Core.Domain.Messages;
using System.Collections.Generic;
using Nop.Core.Domain.Orders;
using System.Text;
using System.Net;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Customers;
using System.Linq;
using Nop.Services.Localization;
using Microsoft.AspNetCore.Authorization;

namespace Teed.Plugin.TicketControl.Controllers
{
    [Area(AreaNames.Admin)]
    public class TicketController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ScheduleService _scheduleService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IRepository<EmailAccount> _emailAccountRepository;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IStoreContext _storeContext;
        private readonly IPictureService _pictureService;
        private readonly IOrderService _orderService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ILanguageService _languageService;
        private readonly TicketService _ticketService;

        public TicketController(IPermissionService permissionService, IWorkContext workContext, ScheduleService scheduleService,
            IMessageTemplateService messageTemplateService, IRepository<EmailAccount> emailAccountRepository,
            IEmailAccountService emailAccountService, IMessageTokenProvider messageTokenProvider,
            IStoreContext storeContext, IPictureService pictureService, IOrderService orderService,
            IWorkflowMessageService workflowMessageService, ILanguageService languageService,
            TicketService ticketService)
        {
            _permissionService = permissionService;
            _workContext = workContext;
            _scheduleService = scheduleService;
            _messageTemplateService = messageTemplateService;
            _emailAccountRepository = emailAccountRepository;
            _emailAccountService = emailAccountService;
            _messageTokenProvider = messageTokenProvider;
            _storeContext = storeContext;
            _pictureService = pictureService;
            _orderService = orderService;
            _workflowMessageService = workflowMessageService;
            _languageService = languageService;
            _ticketService = ticketService;
        }

        //[HttpGet]
        //[AuthorizeAdmin]
        //public IActionResult Index()
        //{
        //    var guid = new Guid();
        //    QRCodeGenerator qrGenerator = new QRCodeGenerator();
        //    QRCodeData qrCodeData = qrGenerator.CreateQrCode(guid.ToString(),
        //    QRCodeGenerator.ECCLevel.Q);
        //    QRCode qrCode = new QRCode(qrCodeData);
        //    Bitmap qrCodeImage = qrCode.GetGraphic(20);
        //    return Ok(Convert.ToBase64String(BitmapToBytes(qrCodeImage)));
        //    //return View(BitmapToBytes(qrCodeImage));
        //}

        //[AllowAnonymous]
        //public IActionResult SendQrEmail(int orderId)
        //{
        //    var order = _orderService.GetOrderById(orderId);
        //    if (order != null)
        //    {
        //        var ticket = _ticketService.GetByOrderId(orderId);
        //        if (ticket != null)
        //        {
        //            var messageTemplate = _messageTemplateService.GetMessageTemplateByName(EmailHelper.NEW_TICKET_TEMPLATE_NAME, 0);
        //            if (messageTemplate != null && messageTemplate.IsActive)
        //            {
        //                var emailAccount = _emailAccountService.GetEmailAccountById(messageTemplate.EmailAccountId);
        //                var store = _storeContext.CurrentStore;
        //                var tokens = new List<Token>();
        //                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //                //_messageTokenProvider.AddCustomerTokens(tokens, customer);
        //                tokens.Add(new Token("Order.Product(s)", ProductListToHtmlTable(order.OrderItems.ToList()), true));
        //                var toEmail = order.Customer.Email;
        //                var toName = order.Customer.GetFullName();

        //                _workflowMessageService.SendNotification(messageTemplate, emailAccount, _languageService.GetAllLanguages().Select(x => x.Id).FirstOrDefault(), tokens, toEmail, toName);
        //            }
        //        }
        //    }
        //    return Ok();
        //}

        //protected virtual Byte[] BitmapToBytes(Bitmap img)
        //{
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        //        return stream.ToArray();
        //    }
        //}

        //protected virtual string ProductListToHtmlTable(List<OrderItem> items)
        //{
        //    var sb = new StringBuilder();
        //    sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

        //    var table = items;
        //    for (var i = 0; i <= table.Count - 1; i++)
        //    {
        //        var item = table[i];
        //        var product = item.Product;
        //        if (product == null)
        //            continue;

        //        sb.AppendLine($"<tr style=\"text-align: center;\">");

        //        var pictureUrl = product.ProductPictures.Count == 0 ? "/images/default-image.png" : _pictureService.GetPictureUrl(product.ProductPictures.FirstOrDefault().Picture);

        //        //product picture 
        //        sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\"><img src=\"{pictureUrl}\" style=\"width: 100px !important;\"/>");
        //        sb.AppendLine("</td>");

        //        //product name
        //        var productName = product.Name;

        //        sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: center;\">" + WebUtility.HtmlEncode(productName));
        //        sb.AppendLine("</td>");

        //        sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{item.Quantity}</td>");

        //        sb.AppendLine("</tr>");
        //    }

        //    sb.AppendLine("</table>");
        //    var result = sb.ToString();
        //    return result;
        //}
    }
}