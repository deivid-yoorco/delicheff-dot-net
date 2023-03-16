using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Messages;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.ScheduleTasks
{
    public class RecurrenceReminderTask : IScheduleTask
    {
        private static readonly string RECURRENCE_REMINDER_BASE_TEMPLATE_NAME = "Recurrence.Reminder.";
        private readonly int DAYS_BETWEEN_EMAILS = 15;

        private readonly ICustomerService _customerService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IOrderService _orderService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IStoreContext _storeContext;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly ILanguageService _languageService;
        private readonly IRepository<EmailAccount> _emailAccountRepository;
        private readonly ISettingService _settingService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;

        public RecurrenceReminderTask(ICustomerService customerService, IMessageTemplateService messageTemplateService,
            IOrderService orderService, IWorkflowMessageService workflowMessageService,
            IQueuedEmailService queuedEmailService, IStoreContext storeContext,
            IEmailAccountService emailAccountService, IMessageTokenProvider messageTokenProvider,
            ILanguageService languageService, IRepository<EmailAccount> emailAccountRepository,
            ISettingService settingService,
            INewsLetterSubscriptionService newsLetterSubscriptionService)
        {
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _customerService = customerService;
            _messageTemplateService = messageTemplateService;
            _orderService = orderService;
            _workflowMessageService = workflowMessageService;
            _queuedEmailService = queuedEmailService;
            _storeContext = storeContext;
            _emailAccountService = emailAccountService;
            _messageTokenProvider = messageTokenProvider;
            _languageService = languageService;
            _emailAccountRepository = emailAccountRepository;
            _settingService = settingService;
        }

        public void Execute()
        {
            if (!TaskRunUtils.ShouldRunTask(_settingService, "RecurrenceReminderTask")) return;

            var store = _storeContext.CurrentStore;
            var messageTemplates = new List<MessageTemplate>();

            #region Email creation if not already created

            for (int i = 1; i <= 4; i++)
            {
                var currentTamplateName = RECURRENCE_REMINDER_BASE_TEMPLATE_NAME + i;
                messageTemplates.Add(_messageTemplateService.GetMessageTemplateByName(currentTamplateName, store.Id));
                if (messageTemplates[i - 1] == null)
                {
                    var emailTitle = i == 1 ? "Haz tu súper en un clic" : i == 2 ? "El súper que te consiente" : i == 3 ? "El súper como te lo mereces" : "Tu primera vez";
                    messageTemplates[i - 1] = new MessageTemplate()
                    {
                        Name = currentTamplateName,
                        Subject = $"%Store.Name%. {emailTitle}.",
                        Body = $@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"" style=""font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; box-sizing: border-box;  margin: 0;"">

<head>
    <meta name=""viewport"" content=""width=device-width"" />
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <title>%Store.Name% - {emailTitle} </title>
    <!-- Compiled and minified CSS -->
    <link rel=""stylesheet"" href=""https://cdnjs.cloudflare.com/ajax/libs/materialize/1.0.0/css/materialize.min.css"">

    <!-- Compiled and minified JavaScript -->
    <script src=""https://cdnjs.cloudflare.com/ajax/libs/materialize/1.0.0/js/materialize.min.js""></script>

    <style type=""text/css"">
        body {{
            -webkit-font-smoothing: antialiased;
            -webkit-text-size-adjust: none;
            width: 100% !important;
            height: 100%;
            line-height: 1.6em;
            background-color: #f9f9f9;
            font-size: 1.1em;
        }}

        a {{
            color: #ffffff;
            font-weight: bold;
            text-decoration: none;
        }}

.ii a[href] {{
            color: #ffffff !important;
            font-weight: bold;
            text-decoration: none;
        }}

        img.welcome {{
            width: 100% !important;

        }}

        ul {{
            list-style-type: none;
            margin: 0;
            padding: 0;
            overflow: hidden;

        }}

        li {{
            display: inline;
            padding-left: 1em;
            padding-right: 1em;
        }}

        table {{
            border: none;
        }}

        .tienda-text-color {{
            color: %Email.StoreColor% !important;
        }}

        .tienda-background-color {{
            background-color: %Email.StoreColor% !important;
        }}

        .link {{
            color: #9bc54c;
            text-decoration: underline;
        }}

        @media only screen and (max-width: 640px) {{
            body {{
                padding: 0 !important;
            }}

            h1 {{
                font-weight: 800 !important;
                margin: 20px 0 5px !important;
            }}

            h2 {{
                font-weight: 800 !important;
                margin: 20px 0 5px !important;
            }}

            h3 {{
                font-weight: 800 !important;
                margin: 20px 0 5px !important;
            }}

            h4 {{
                font-weight: 800 !important;
                margin: 20px 0 5px !important;
            }}

            h1 {{
                font-size: 22px !important;
            }}

            h2 {{
                font-size: 18px !important;
            }}

            h3 {{
                font-size: 16px !important;
            }}

            .container {{
                padding: 0 !important;
                width: 100% !important;
            }}

            .content {{
                padding: 0 !important;
            }}

            .content-wrap {{
                padding: 10px !important;
            }}

            .invoice {{
                width: 100% !important;
            }}


        }}
    </style>
</head>

<body itemscope=itemscope itemtype=""http://schema.org/EmailMessage"" style=""font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box;  -webkit-font-smoothing: antialiased; -webkit-text-size-adjust: none; width: 100% !important; height: 100%; line-height: 1.6em; background-color: #f6f6f6; margin: 0;""
    bgcolor=""#f6f6f6"">
    <table cellspacing=""0"" cellpadding=""0"" class=""striped body-wrap"" style=""font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box;  width: 100%; background-color: #f6f6f6; margin: 0;""
        bgcolor=""#f6f6f6"">
        <tr style=""font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box;  margin: 0;"">
            <td style=""font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box;  vertical-align: top; margin: 0;""
                valign=""top""></td>
            <td class=""container"" width=""600"" style=""font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; display: block !important; max-width: 600px !important; clear: both !important; margin: 0 auto;""
                valign=""top"">
                <div class=""content"" style=""font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box;  max-width: 600px; display: block; margin: 0 auto; padding: 20px;"">
                    <table class=""main"" width=""100%"" cellpadding=""0"" cellspacing=""0"" itemprop=""action"" itemscope=itemscope itemtype=""http://schema.org/ConfirmAction""
                        style=""font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box;  border-radius: 3px; background-color: #fff; margin: 0; border: 1px solid #e9e9e9;""
                        bgcolor=""#fff"">
                        <tr style=""font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box;  margin: 0;"">
                            <td class=""content-wrap"" style=""font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box;  vertical-align: top; margin: 0;""
                                valign=""top"">
                                <meta itemprop=""name"" content=""Confirm Email"" style=""font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box;  margin: 0;""
                                />
                                <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box;  margin: 0;"">
                                    <tr style=""font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box;  margin: 0; text-align: center"">
                                        <td class=""center content-block"" style=""font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box;  vertical-align: top; margin: 0; padding: 0 0 20px;""
                                            valign=""top"">
                                            <div style=""margin: 2%;"" class=""center"">
                                                <a class=""link"" href=""%Store.URL%/cupon-bienvenida-{i}"" target=""_blank"">Si no puedes ver el mail correctamente, da clic en este link</a>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr style=""font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box;  margin: 0; text-align: center"">
                                        <td class=""center content-block"" style=""font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box;  vertical-align: top; margin: 0; padding: 0 0 20px;""
                                            valign=""top"">
                                            <div style=""margin: 2%;"" class=""center"">
                                                <a href=""%Store.URL%"" target=""_blank"">
                                                <img class=""welcome"" src=""%Store.URL%/Themes/TeedMaterialV2/Content/images/correo_cupon{i}.png"" />
                                                </a>
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </div>
            </td>
            <td style=""font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box;  vertical-align: top; margin: 0;""
                valign=""top""></td>
        </tr>
    </table>
</body>

</html>",
                        IsActive = true,
                        EmailAccountId = _emailAccountRepository.Table.Select(x => x.Id).FirstOrDefault(),
                    };
                    _messageTemplateService.InsertMessageTemplate(messageTemplates[i - 1]);
                }
            }

            #endregion

            int languageId = _languageService.GetAllLanguages().Select(x => x.Id).FirstOrDefault();
            DateTime controlDate = new DateTime(2022, 1, 1).Date;
            List<int> customerIdsWithOrders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate >= controlDate)
                .Select(x => x.CustomerId).Distinct()
                .ToList();

            var customersWithoutOrders = _customerService.GetAllCustomersQuery()
                .Where(x => x.CreatedOnUtc >= controlDate && !customerIdsWithOrders.Contains(x.Id) && !string.IsNullOrEmpty(x.Email))
                .ToList();
            List<string> customerEmails = customersWithoutOrders.Select(x => x.Email).Distinct().ToList();

            var messageTemplateIds = messageTemplates.Select(x => x.Id).ToList();
            var recurrenceQueuedEmails = _queuedEmailService.GetAllQueuedEmailsQuery()
                .Where(x => customerEmails.Contains(x.To) && messageTemplateIds.Contains(x.UsedMessageTemplateId.Value))
                .ToList();

            var newsletterSubscriptions = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions();

            foreach (var customer in customersWithoutOrders)
            {
                bool shouldSendEmail = newsletterSubscriptions.Where(x => x.Email == customer.Email).Select(x => x.Active).FirstOrDefault();
                if (!shouldSendEmail) continue;
                var tokens = new List<Token>();
                _messageTokenProvider.AddCustomerTokens(tokens, customer);
                var toEmail = customer.Email;
                var toName = customer.GetFullName();
                for (int i = 0; i < messageTemplates.Count; i++)
                {
                    bool templateAlreadySent = recurrenceQueuedEmails.Where(x => x.UsedMessageTemplateId.Value == messageTemplates[i].Id && toEmail == x.To).Any();
                    if (templateAlreadySent) continue;
                    if (i > 0)
                    {
                        var previousTemplate = messageTemplates[i - 1];
                        var alreadySentPreviousTemplate = recurrenceQueuedEmails
                            .Where(x => x.UsedMessageTemplateId.Value == previousTemplate.Id && toEmail == x.To)
                            .FirstOrDefault();
                        if ((alreadySentPreviousTemplate.SentOnUtc.HasValue && 
                            alreadySentPreviousTemplate.SentOnUtc.Value.ToLocalTime() >= DateTime.Now.AddDays(-1 * DAYS_BETWEEN_EMAILS)) ||
                            !alreadySentPreviousTemplate.SentOnUtc.HasValue)
                        {
                            break;
                        }
                    }
                    var emailAccount = _emailAccountService.GetEmailAccountById(messageTemplates[i].EmailAccountId);
                    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
                    _workflowMessageService.SendNotification(messageTemplates[i], emailAccount, languageId, tokens, toEmail, toName);
                    break;
                }
            }
        }
    }
}
