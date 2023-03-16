using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Emails.Helpers;

namespace Teed.Plugin.Emails.ScheduleTasks
{
    public class ReorderEmailScheduleTask : IScheduleTask
    {
        private readonly ICustomerService _customerService;
        private readonly ISettingService _settingService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IStoreContext _storeContext;
        private readonly IPictureService _pictureService;
        private readonly ILanguageService _languageService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IOrderService _orderService;

        public ReorderEmailScheduleTask(ICustomerService customerService,
            ISettingService settingService,
            IMessageTemplateService messageTemplateService,
            IEmailAccountService emailAccountService,
            IMessageTokenProvider messageTokenProvider,
            IStoreContext storeContext,
            IPictureService pictureService,
            IWorkflowMessageService workflowMessageService,
            ILanguageService languageService,
            IOrderService orderService)
        {
            _customerService = customerService;
            _settingService = settingService;
            _messageTemplateService = messageTemplateService;
            _emailAccountService = emailAccountService;
            _messageTokenProvider = messageTokenProvider;
            _storeContext = storeContext;
            _pictureService = pictureService;
            _workflowMessageService = workflowMessageService;
            _languageService = languageService;
            _orderService = orderService;
        }

        public void Execute()
        {
            TeedEmailsPluginSettings emailSettings = _settingService.LoadSetting<TeedEmailsPluginSettings>();
            if (emailSettings != null && emailSettings.ReorderEmailIsActive && emailSettings.DaysToSendReorderEmail > 0)
            {
                DateTime initDate = DateTime.UtcNow.AddDays((emailSettings.DaysToSendReorderEmail * -1) - 1);
                DateTime endDate = DateTime.UtcNow.AddDays(emailSettings.DaysToSendReorderEmail * -1);
                var orders = GetFilteredOrders();
                if (emailSettings.ReorderOnlyCompletedOrders)
                    orders = orders.Where(x => x.OrderStatusId == 30);

                var filteredOrders = orders;
                if (Nop.Services.TeedCommerceStores.CurrentStore == Nop.Services.TeedStores.CentralEnLinea)
                    filteredOrders = orders.Where(x => x.SelectedShippingDate.Value > initDate && x.SelectedShippingDate.Value < endDate);
                else
                    filteredOrders = orders.Where(x => x.CreatedOnUtc > initDate && x.CreatedOnUtc < endDate);

                //var testFilteredOrders = new List<Order>() { GetFilteredOrders().Where(x => x.CustomerId == 1).OrderByDescending(x => x.SelectedShippingDate).FirstOrDefault() }; // FOR TESTING

                var groupedOrders = filteredOrders.Select(x => new { x.CustomerId, x.OrderItems, x.Id }).GroupBy(x => x.CustomerId).ToList();
                var customerIds = groupedOrders.Select(x => x.Key).Distinct().ToList();
                var customers = _customerService.GetAllCustomersQuery().Where(x => customerIds.Contains(x.Id)).ToList();
                var allOrders = orders.Where(x => customerIds.Contains(x.CustomerId))
                    .Select(x => new { x.CustomerId, x.SelectedShippingDate, x.CreatedOnUtc })
                    .ToList();

                var messageTemplate = _messageTemplateService.GetMessageTemplateByName(EmailHelper.REORDER_TEMPLATE_NAME, 0);
                if (messageTemplate != null && messageTemplate.IsActive)
                {
                    var emailAccount = _emailAccountService.GetEmailAccountById(messageTemplate.EmailAccountId);
                    var store = _storeContext.CurrentStore;
                    foreach (var customerOrder in groupedOrders)
                    {
                        var customer = customers.Where(x => x.Id == customerOrder.Key).FirstOrDefault();
                        var newestOrder = allOrders.Where(x => x.CustomerId == customerOrder.Key);
                        if (Nop.Services.TeedCommerceStores.CurrentStore == Nop.Services.TeedStores.CentralEnLinea)
                            newestOrder = newestOrder.Where(x => x.SelectedShippingDate.Value > endDate);
                        else
                            newestOrder = newestOrder.Where(x => x.CreatedOnUtc > endDate);

                        if (newestOrder.Any()) continue;

                        var orderItems = customerOrder
                            .SelectMany(x => x.OrderItems)
                            .GroupBy(x => x.ProductId)
                            .Select(x => x.OrderByDescending(y => y.Quantity).FirstOrDefault())
                            .ToList();

                        var tokens = new List<Token>();
                        _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
                        _messageTokenProvider.AddCustomerTokens(tokens, customer);
                        tokens.Add(new Token("Order.OrderNumber", string.Join(",", customerOrder.Select(x => x.Id)), true));
                        tokens.Add(new Token("Order.Product(s)", ProductListToHtmlTable(orderItems), true));
                        var toEmail = customer.Email;
                        var toName = customer.GetFullName();

                        _workflowMessageService.SendNotification(messageTemplate, emailAccount, _languageService.GetAllLanguages().Select(x => x.Id).FirstOrDefault(), tokens, toEmail, toName);
                    }
                }
            }
        }

        private IQueryable<Order> GetFilteredOrders()
        {
            return _orderService.GetAllOrdersQuery().Where(x => x.OrderStatusId != 40);
        }

        protected virtual string ProductListToHtmlTable(List<OrderItem> items)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            var table = items;
            for (var i = 0; i <= table.Count - 1; i++)
            {
                var item = table[i];
                var product = item.Product;
                if (product == null)
                    continue;

                sb.AppendLine($"<tr style=\"text-align: center;\">");

                var pictureUrl = product.ProductPictures.Count == 0 ? "/images/default-image.png" : _pictureService.GetPictureUrl(product.ProductPictures.FirstOrDefault().Picture);

                //product picture 
                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\"><img src=\"{pictureUrl}\" style=\"width: 100px !important;\"/>");
                sb.AppendLine("</td>");

                //product name
                var productName = product.Name;

                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: center;\">" + WebUtility.HtmlEncode(productName));
                sb.AppendLine("</td>");

                if (item.Product.EquivalenceCoefficient > 0 && item.BuyingBySecondary)
                {
                    var type = "gr";
                    var value = (item.Quantity * 1000) / item.Product.EquivalenceCoefficient;
                    if (value >= 1000)
                    {
                        value = value / 1000;
                        type = "kg";
                    }
                    sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{value}{type}</td>");
                }
                else if (item.Product.WeightInterval > 0)
                {
                    var type = "gr";
                    var value = item.Quantity * item.Product.WeightInterval;
                    if (value >= 1000)
                    {
                        value = value / 1000;
                        type = "kg";
                    }
                    sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{value}{type}</td>");
                }
                else
                {
                    var unidades = item.Quantity != 1 ? " unidades" : " unidad";
                    sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{item.Quantity}{unidades}</td>");
                }

                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
            var result = sb.ToString();
            return result;
        }
    }
}
