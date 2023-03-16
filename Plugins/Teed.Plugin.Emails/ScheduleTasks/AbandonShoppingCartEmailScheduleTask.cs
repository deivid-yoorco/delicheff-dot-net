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
using Teed.Plugin.Emails.Helpers;
using Nop.Services.Logging;
using System.Diagnostics;
using System.Net.Sockets;

namespace Teed.Plugin.Emails.ScheduleTasks
{
    public class AbandonShoppingCartEmailScheduleTask : IScheduleTask
    {

        private readonly ICustomerService _customerService;
        private readonly ISettingService _settingService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IStoreContext _storeContext;
        private readonly IPictureService _pictureService;
        private readonly ICurrencyService _currencyService;
        private readonly ILanguageService _languageService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ILogger _logger;

        public AbandonShoppingCartEmailScheduleTask(ICustomerService customerService,
            ISettingService settingService,
            IMessageTemplateService messageTemplateService,
            IEmailAccountService emailAccountService,
            IMessageTokenProvider messageTokenProvider,
            IStoreContext storeContext,
            IPictureService pictureService,
            IWorkflowMessageService workflowMessageService,
            ICurrencyService currencyService,
            ILanguageService languageService,
            ILogger logger)
        {
            _customerService = customerService;
            _settingService = settingService;
            _messageTemplateService = messageTemplateService;
            _emailAccountService = emailAccountService;
            _messageTokenProvider = messageTokenProvider;
            _storeContext = storeContext;
            _pictureService = pictureService;
            _currencyService = currencyService;
            _workflowMessageService = workflowMessageService;
            _languageService = languageService;
            _logger = logger;
        }

        public void Execute()
        {
            TeedEmailsPluginSettings emailSettings = _settingService.LoadSetting<TeedEmailsPluginSettings>();
            if (emailSettings != null && emailSettings.AbandonShoppingCartEmailIsActive && emailSettings.HoursToSendAbandonShoppingCartEmail > 0)
            {
                DateTime initDate = DateTime.UtcNow.AddHours((emailSettings.HoursToSendAbandonShoppingCartEmail * -1) - 1);
                DateTime endDate = DateTime.UtcNow.AddHours(emailSettings.HoursToSendAbandonShoppingCartEmail * -1);

                var filteredCustomers = _customerService.GetAllCustomersQuery()
                    .Where(x => x.ShoppingCartItems.Where(y => !y.Product.GiftProductEnable && y.ShoppingCartTypeId == 1 && y.UpdatedOnUtc > initDate && y.UpdatedOnUtc < endDate).Any() && x.Email != null)
                    .ToList();

                var messageTemplate = _messageTemplateService.GetMessageTemplateByName(EmailHelper.ABANDONED_SHOPPING_CART_TEMPLATE_NAME, 0);
                if (messageTemplate != null && messageTemplate.IsActive)
                {
                    SendPushNotification(filteredCustomers.ToList());

                    var emailAccount = _emailAccountService.GetEmailAccountById(messageTemplate.EmailAccountId);
                    var store = _storeContext.CurrentStore;
                    foreach (var customer in filteredCustomers)
                    {
                        var tokens = new List<Token>();
                        _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
                        _messageTokenProvider.AddCustomerTokens(tokens, customer);
                        tokens.Add(new Token("Order.Product(s)", ProductListToHtmlTable(customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList()), true));
                        var toEmail = customer.Email;
                        var toName = customer.GetFullName();

                        _workflowMessageService.SendNotification(messageTemplate, emailAccount, _languageService.GetAllLanguages().Select(x => x.Id).FirstOrDefault(), tokens, toEmail, toName);
                    }
                }
            }
        }

        private void SendPushNotification(List<Customer> customers)
        {
            using (HttpClient client = new HttpClient())
            {
                var body = new
                {
                    CustomerIds = customers.Select(x => x.Id).ToList()
                };
                var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                var result = client.PostAsync(_storeContext.CurrentStore.SecureUrl + "/Api/Notification/CreateShoppingCartNotification", content).Result;
                if (!result.IsSuccessStatusCode)
                {
                    var resultJson = result.Content.ReadAsStringAsync().Result;
                    _logger.Error("[SHOPPING CART EMAIL] Error creating shopping cart push notification.", new Exception(resultJson));
                }
            }
        }

        protected virtual string ProductListToHtmlTable(List<ShoppingCartItem> items)
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
                        value = Math.Round((value / 1000), 2);
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
                        value = Math.Round((value / 1000), 2);
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
