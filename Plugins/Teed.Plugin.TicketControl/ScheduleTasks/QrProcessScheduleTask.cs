using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Tasks;
using Nop.Services.Logging;
using Nop.Web.Framework.Controllers;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Core.Domain;
using Teed.Plugin.TicketControl.Services;
using Nop.Core.Domain.Payments;
using QRCoder;
using System.Drawing;
using System.IO;
using Teed.Plugin.TicketControl.Helpers;
using Teed.Plugin.TicketControl.Domain.Tickets;
using Nop.Core.Domain.Media;
using System.Text.RegularExpressions;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;

namespace Teed.Plugin.TicketControl.ScheduleTasks
{
    public class QrProcessScheduleTask : BasePluginController, IScheduleTask
    {

        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IStoreContext _storeContext;
        private readonly IPictureService _pictureService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly ILanguageService _languageService;
        private readonly IDownloadService _downloadService;
        private readonly TicketService _ticketService;

        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IWebHelper _webHelper;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;

        public QrProcessScheduleTask(ISettingService settingService,
            IPermissionService permissionService,
            IWorkContext workContext,
            IStoreService storeService,
            ILocalizationService localizationService,
            IWorkflowMessageService workflowMessageService,
            ILogger logger,
            IOrderService orderService,
            IEmailAccountService emailAccountService,
            IMessageTokenProvider messageTokenProvider,
            IStoreContext storeContext,
            IPictureService pictureService,
            IMessageTemplateService messageTemplateService,
            ILanguageService languageService,
            IDownloadService downloadService,
            TicketService ticketService,
            IProductAttributeParser productAttributeParser,
            IWebHelper webHelper,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter)
        {
            _permissionService = permissionService;
            _workContext = workContext;
            _storeService = storeService;
            _localizationService = localizationService;
            _settingService = settingService;
            _workflowMessageService = workflowMessageService;
            _logger = logger;
            _orderService = orderService;
            _ticketService = ticketService;
            _emailAccountService = emailAccountService;
            _messageTokenProvider = messageTokenProvider;
            _storeContext = storeContext;
            _pictureService = pictureService;
            _messageTemplateService = messageTemplateService;
            _languageService = languageService;
            _downloadService = downloadService;
            _productAttributeParser = productAttributeParser;
            _webHelper = webHelper;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
        }

        public void Execute()
        {
            // Search for paid orders with at least one product with the specification attribute "Boleto" (Currently of id: 1)
            // and not in the Tickets table to generate and send QR code

            try
            {
                var today = DateTime.UtcNow.Date;
                var sevenDaysAgo = today.AddDays(-7);
                var allPaidOrders = _orderService.GetAllOrdersQuery()
                   .Where(x => x.PaymentStatusId == (int)PaymentStatus.Paid)
                   .ToList();
                var tickets = _ticketService.GetAll().ToList();
                var ticketOrderIds = new List<int>();
                if (tickets.Any())
                    ticketOrderIds = tickets.Select(x => x.OrderId).ToList();

                allPaidOrders = allPaidOrders.Where(x => !ticketOrderIds.Contains(x.Id)
                //&& sevenDaysAgo <= x.PaidDateUtc && x.PaidDateUtc <= today
                ).ToList();
                foreach (var order in allPaidOrders)
                {
                    var anyProductHasSpecificationOptionId = order.OrderItems
                        .Select(x => x.Product)
                        .SelectMany(x => x.ProductSpecificationAttributes)
                        .Select(x => x.SpecificationAttributeOptionId)
                        .Where(x => x == 1)
                        .Any();
                    if (anyProductHasSpecificationOptionId)
                    {
                        // QR
                        var qrGuid = Guid.NewGuid();
                        QRCodeGenerator qrGenerator = new QRCodeGenerator();
                        QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrGuid.ToString(),
                        QRCodeGenerator.ECCLevel.Q);
                        QRCode qrCode = new QRCode(qrCodeData);
                        Bitmap qrCodeImage = qrCode.GetGraphic(20);
                        var qrBase64 = Convert.ToBase64String(BitmapToBytes(qrCodeImage));
                        //

                        // Email
                        var messageTemplate = _messageTemplateService.GetMessageTemplateByName(EmailHelper.NEW_TICKET_TEMPLATE_NAME, 0);
                        if (messageTemplate != null && messageTemplate.IsActive && !string.IsNullOrEmpty(qrBase64))
                        {
                            var emailAccount = _emailAccountService.GetEmailAccountById(messageTemplate.EmailAccountId);
                            var store = _storeContext.CurrentStore;
                            var tokens = new List<Token>();
                            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
                            _messageTokenProvider.AddCustomerTokens(tokens, order.Customer);
                            _messageTokenProvider.AddOrderTokens(tokens, order, 1);
                            tokens.Add(new Token("Order.Product(s)", ProductListToHtmlTable(order, order.CustomerLanguageId, 0), true));
                            tokens.Add(new Token("Ticket.URL", $"data:image/png;base64, {qrBase64}", true));
                            var toEmail = order.Customer.Email;
                            var toName = order.Customer.GetFullName();
                            if (string.IsNullOrEmpty(toEmail))
                                toEmail = order.BillingAddress.Email;
                            if (string.IsNullOrEmpty(toName))
                                toName = string.Join(" ", new List<string> { order.BillingAddress.FirstName, order.BillingAddress.LastName });
                            if (string.IsNullOrEmpty(toEmail) || string.IsNullOrEmpty(toName))
                                break;

                            _workflowMessageService.SendNotification(messageTemplate, emailAccount, _languageService.GetAllLanguages().Select(x => x.Id).FirstOrDefault(), tokens, toEmail, toName);

                            // Ticket and download insert
                            byte[] fileBinary = Convert.FromBase64String(qrBase64);

                            var download = new Download
                            {
                                DownloadGuid = Guid.NewGuid(),
                                UseDownloadUrl = false,
                                DownloadUrl = "",
                                DownloadBinary = fileBinary,
                                ContentType = "image/png",
                                //we store filename without extension for downloads
                                Filename = "qr_" + order.Id + "_" + DateTime.Now.ToString("ddMMyyyy_hhmmss"),
                                Extension = ".png",
                                IsNew = true
                            };
                            _downloadService.InsertDownload(download);

                            order.OrderNotes.Add(new OrderNote()
                            {
                                CreatedOnUtc = DateTime.UtcNow,
                                DisplayToCustomer = true,
                                Note = "Qr de la orden",
                                OrderId = order.Id,
                                DownloadId = download.Id
                            });
                            _orderService.UpdateOrder(order);

                            var ticket = new Ticket
                            {
                                OrderId = order.Id,
                                QrPictureId = download.Id,
                                TicketId = qrGuid
                            };
                            _ticketService.Insert(ticket);
                            //
                        }
                        //
                    }
                }
            }
            catch (Exception e)
            {
                var err = e;
            }
        }

        protected virtual Byte[] BitmapToBytes(Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
        protected virtual string ProductListToHtmlTable(Order order, int languageId, int vendorId = 0)
        {
            var language = _languageService.GetLanguageById(languageId);

            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");
            #region last table
            //sb.AppendLine($"<tr style=\"background-color:{_templatesSettings.Color1};text-align:center;\">");
            //sb.AppendLine($"<th>{_localizationService.GetResource("Messages.Order.Product(s).Name", languageId)}</th>");
            //sb.AppendLine($"<th>{_localizationService.GetResource("Messages.Order.Product(s).Price", languageId)}</th>");
            //sb.AppendLine($"<th>{_localizationService.GetResource("Messages.Order.Product(s).Quantity", languageId)}</th>");
            //sb.AppendLine($"<th>{_localizationService.GetResource("Messages.Order.Product(s).Total", languageId)}</th>");
            //sb.AppendLine("</tr>");

            //var table = order.OrderItems.ToList();
            //for (var i = 0; i <= table.Count - 1; i++)
            //{
            //    var orderItem = table[i];
            //    var product = orderItem.Product;
            //    if (product == null)
            //        continue;

            //    if (vendorId > 0 && product.VendorId != vendorId)
            //        continue;

            //    sb.AppendLine($"<tr style=\"background-color: {_templatesSettings.Color2};text-align: center;\">");
            //    //product name
            //    var productName = product.GetLocalized(x => x.Name, languageId);

            //    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));

            //    //add download link
            //    if (_downloadService.IsDownloadAllowed(orderItem))
            //    {
            //        var downloadUrl = $"{GetStoreUrl(order.StoreId)}{GetUrlHelper().RouteUrl("GetDownload", new { orderItemId = orderItem.OrderItemGuid })}";
            //        var downloadLink = $"<a class=\"link\" href=\"{downloadUrl}\">{_localizationService.GetResource("Messages.Order.Product(s).Download", languageId)}</a>";
            //        sb.AppendLine("<br />");
            //        sb.AppendLine(downloadLink);
            //    }
            //    //add download link
            //    if (_downloadService.IsLicenseDownloadAllowed(orderItem))
            //    {
            //        var licenseUrl = $"{GetStoreUrl(order.StoreId)}{GetUrlHelper().RouteUrl("GetLicense", new { orderItemId = orderItem.OrderItemGuid })}";
            //        var licenseLink = $"<a class=\"link\" href=\"{licenseUrl}\">{_localizationService.GetResource("Messages.Order.Product(s).License", languageId)}</a>";
            //        sb.AppendLine("<br />");
            //        sb.AppendLine(licenseLink);
            //    }
            //    //attributes
            //    if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
            //    {
            //        sb.AppendLine("<br />");
            //        sb.AppendLine(orderItem.AttributeDescription);
            //    }
            //    //rental info
            //    if (orderItem.Product.IsRental)
            //    {
            //        var rentalStartDate = orderItem.RentalStartDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalStartDateUtc.Value) : string.Empty;
            //        var rentalEndDate = orderItem.RentalEndDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalEndDateUtc.Value) : string.Empty;
            //        var rentalInfo = string.Format(_localizationService.GetResource("Order.Rental.FormattedDate"),
            //            rentalStartDate, rentalEndDate);
            //        sb.AppendLine("<br />");
            //        sb.AppendLine(rentalInfo);
            //    }
            //    //SKU
            //    if (_catalogSettings.ShowSkuOnProductDetailsPage)
            //    {
            //        var sku = product.FormatSku(orderItem.AttributesXml, _productAttributeParser);
            //        if (!string.IsNullOrEmpty(sku))
            //        {
            //            sb.AppendLine("<br />");
            //            sb.AppendLine(string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), WebUtility.HtmlEncode(sku)));
            //        }
            //    }
            //    sb.AppendLine("</td>");

            //    string unitPriceStr;
            //    if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            //    {
            //        //including tax
            //        var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
            //        unitPriceStr = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);
            //    }
            //    else
            //    {
            //        //excluding tax
            //        var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
            //        unitPriceStr = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);
            //    }
            //    sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: right;\">{unitPriceStr}</td>");

            //    sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{orderItem.Quantity}</td>");

            //    string priceStr;
            //    if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            //    {
            //        //including tax
            //        var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
            //        priceStr = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);
            //    }
            //    else
            //    {
            //        //excluding tax
            //        var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
            //        priceStr = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);
            //    }
            //    sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: right;\">{priceStr}</td>");

            //    sb.AppendLine("</tr>");
            //}

            //if (vendorId == 0)
            //{
            //    //we render checkout attributes and totals only for store owners (hide for vendors)

            //    if (!string.IsNullOrEmpty(order.CheckoutAttributeDescription))
            //    {
            //        sb.AppendLine("<tr><td style=\"text-align:right;\" colspan=\"1\">&nbsp;</td><td colspan=\"3\" style=\"text-align:right\">");
            //        sb.AppendLine(order.CheckoutAttributeDescription);
            //        sb.AppendLine("</td></tr>");
            //    }

            //    //totals
            //    WriteTotals(order, language, sb);
            //}
            #endregion
            var table = order.OrderItems.ToList();
            for (var i = 0; i <= table.Count - 1; i++)
            {
                var orderItem = table[i];
                var product = orderItem.Product;
                if (product == null)
                    continue;

                if (vendorId > 0 && product.VendorId != vendorId)
                    continue;

                sb.AppendLine($"<tr style=\"text-align: center;\">");

                // picture with/without attributes
                var pictureUrl = product.ProductPictures.Count == 0 ? "/images/default-image.png" : _pictureService.GetPictureUrl(product.ProductPictures.FirstOrDefault().Picture);
                if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    var attributeValues = _productAttributeParser.ParseProductAttributeValues(orderItem.AttributesXml);
                    var attributeValueWithPicture = attributeValues.FirstOrDefault(x => x.PictureId > 0);
                    if (attributeValueWithPicture != null)
                    {
                        var productAttributePictureCacheKey = string.Format("Nop.pres.productattribute.picture-{0}-{1}-{2}",
                                        attributeValueWithPicture.PictureId,
                                        _webHelper.IsCurrentConnectionSecured(),
                                        _storeContext.CurrentStore.Id);
                        var valuePicture = _pictureService.GetPictureById(attributeValueWithPicture.PictureId);
                        var attributePicture = _pictureService.GetPictureUrl(valuePicture);
                        if (!string.IsNullOrEmpty(attributePicture))
                            pictureUrl = attributePicture;
                    }
                }

                //product picture 
                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\"><img src=\"{pictureUrl}\" style=\"width: 100px !important;\"/>");
                sb.AppendLine("</td>");

                //product name and attributes
                var finalAttributes = string.Empty;
                var productName = product.GetLocalized(x => x.Name, languageId);
                var attributes = orderItem.AttributeDescription;
                if (!string.IsNullOrEmpty(attributes))
                {
                    var pattern = @" \[(.*?)\]";
                    finalAttributes = Regex.Replace(attributes, pattern, string.Empty);
                    finalAttributes = finalAttributes.Replace("<br />", " | ");
                }
                var finalName =
                    productName + (string.IsNullOrEmpty(finalAttributes) ? "" : ("<br />" + finalAttributes));

                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: center;\">" + WebUtility.HtmlEncode(finalName));
                sb.AppendLine("</td>");

                if (orderItem.EquivalenceCoefficient > 0 && orderItem.BuyingBySecondary)
                {
                    var type = "gr";
                    var value = (orderItem.Quantity * 1000) / orderItem.EquivalenceCoefficient;
                    if (value >= 1000)
                    {
                        value = value / 1000;
                        type = "kg";
                    }
                    sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{value}{type}</td>");
                }
                else if (orderItem.WeightInterval > 0)
                {
                    var type = "gr";
                    var value = orderItem.Quantity * orderItem.WeightInterval;
                    if (value >= 1000)
                    {
                        value = value / 1000;
                        type = "kg";
                    }
                    sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{value}{type}</td>");
                }
                else
                {
                    var unidades = orderItem.Quantity != 1 ? " unidades" : " unidad";
                    sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{orderItem.Quantity}{unidades}</td>");
                }

                string priceStr;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                    priceStr = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);
                }
                else
                {
                    //excluding tax
                    var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                    priceStr = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);
                }
                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{priceStr}</td>");

                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
            var result = sb.ToString();
            return result;
        }
    }
}
