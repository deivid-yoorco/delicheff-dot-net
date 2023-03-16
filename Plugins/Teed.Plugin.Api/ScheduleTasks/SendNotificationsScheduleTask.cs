using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Domain.Identity;
using Teed.Plugin.Api.Domain.Notifications;
using Teed.Plugin.Api.Services;
using Task = System.Threading.Tasks.Task;
using Nop.Services.Logging;
using Nop.Services.Catalog;

namespace Teed.Plugin.Api.ScheduleTasks
{
    public class SendNotificationsScheduleTask : IScheduleTask
    {
        private readonly NotificationService _notificationService;
        private readonly CustomerSecurityTokenService _customerSecurityTokenService;
        private readonly QueuedNotificationService _queuedNotificationService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger _logger;
        private readonly IProductTagService _tagService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public SendNotificationsScheduleTask(NotificationService notificationService,
            CustomerSecurityTokenService customerSecurityTokenService,
            QueuedNotificationService queuedNotificationService,
            IGenericAttributeService genericAttributeService,
            ICustomerService customerService,
            ILogger logger,
            IProductTagService tagService,
            IProductService productService,
            ICategoryService categoryService)
        {
            _notificationService = notificationService;
            _customerSecurityTokenService = customerSecurityTokenService;
            _queuedNotificationService = queuedNotificationService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _logger = logger;
            _tagService = tagService;
            _productService = productService;
            _categoryService = categoryService;
        }

        public void Execute()
        {
            DateTime utcNow = DateTime.UtcNow;
            var pendingNotificationsQuery = _notificationService.GetAll()
                .Where(x => !x.IsCancelled && x.DontSendBeforeDateUtc < utcNow)
                .Where(x => x.Id > 125177)
                .Include(x => x.QueuedNotifications)
                .Where(x => x.QueuedNotifications.Count == 0);

            if (pendingNotificationsQuery.Count() == 0) return;

            List<Domain.Notifications.Notification> pendingNotifications = pendingNotificationsQuery.ToList();
            var allTokens = _customerSecurityTokenService.GetAll()
                .Where(x => x.FirebaseToken != null && x.FirebaseToken != "")
                .ToList();
            var firebaseTokens = allTokens
                .GroupBy(x => x.Uuid)
                .Select(x => x.OrderByDescending(y => y.UpdatedOnUtc).FirstOrDefault())
                .ToList();
            List<int> userIdsWithToken = firebaseTokens.GroupBy(x => x.CustomerId).Select(x => x.Key).ToList();
            List<Customer> customers = _customerService.GetAllCustomersQuery().Where(x => x.Active && userIdsWithToken.Contains(x.Id)).ToList();

            if (FirebaseApp.DefaultInstance == null) _notificationService.CreateFirebaseApp();
            foreach (var pendingNotification in pendingNotifications)
            {
                List<int> customerIds = string.IsNullOrWhiteSpace(pendingNotification.CustomerIds) ? userIdsWithToken : pendingNotification.CustomerIds.Split(',').Select(x => int.Parse(x)).ToList();
                foreach (var customerId in customerIds)
                {
                    List<CustomerSecurityToken> userTokens = firebaseTokens.Where(x => x.CustomerId == customerId).ToList();
                    Customer customer = customers.Where(x => x.Id == customerId).FirstOrDefault();
                    foreach (var userToken in userTokens)
                    {
                        var newNotification = new QueuedNotification()
                        {
                            CustomerId = customerId,
                            NotificationId = pendingNotification.Id
                        };

                        try
                        {
                            var tuple = new Tuple<string, string, string>("", "", "");
                            if (!string.IsNullOrEmpty(pendingNotification.AdditionalData))
                            {
                                if (pendingNotification.ActionTypeId == (int)ActionType.OpenCategory)
                                {
                                    var id = 0;
                                    int.TryParse(pendingNotification.AdditionalData, out id);
                                    var category = _categoryService.GetCategoryById(id);
                                    if (category != null)
                                        if (!category.Deleted && category.Published)
                                        {
                                            tuple = new Tuple<string, string, string>(category.Id.ToString(), category.Name, (category.ParentCategoryId > 0).ToString().ToLower());
                                        }
                                }
                                else if (pendingNotification.ActionTypeId == (int)ActionType.OpenProduct)
                                {
                                    var id = 0;
                                    int.TryParse(pendingNotification.AdditionalData, out id);
                                    var product = _productService.GetProductById(id);
                                    if (product != null)
                                        if (!product.Deleted && product.Published)
                                        {
                                            tuple = new Tuple<string, string, string>(product.Id.ToString(), "", "");
                                        }
                                }
                                else if (pendingNotification.ActionTypeId == (int)ActionType.SearchTagOrKeyword ||
                                    pendingNotification.ActionTypeId == (int)ActionType.OpenExternalLink)
                                    tuple = new Tuple<string, string, string>(pendingNotification.AdditionalData, "", "");
                            }
                            var message = new Message()
                            {
                                Token = userToken.FirebaseToken,
                                Data = new Dictionary<string, string>()
                                {
                                    { "title", ApplyVariables(pendingNotification.Title, customer) },
                                    { "body", ApplyVariables(pendingNotification.Body, customer) },
                                    { "actionTypeId", pendingNotification.ActionTypeId.ToString() },
                                    // used as main reference to extra data
                                    { "additionalData1", tuple.Item1 },
                                    // used for category name when applies
                                    { "additionalData2", tuple.Item2 },
                                    // used for category "is child" when applies
                                    { "additionalData3", tuple.Item3 },
                                },
                                Android = new AndroidConfig()
                                {
                                    Priority = Priority.High
                                }
                            };
                            var response = FirebaseMessaging.DefaultInstance.SendAsync(message).Result;
                            newNotification.MessageId = response;
                        }
                        catch (Exception e)
                        {
                            newNotification.ErrorMessage = e.Message + " " + e.InnerException?.Message;
                            userToken.FirebaseToken = null;
                            _customerSecurityTokenService.Update(userToken);
                        }

                        newNotification.SentAtUtc = DateTime.UtcNow;
                        _queuedNotificationService.Insert(newNotification);
                    }
                }
            }
        }

        private string ApplyVariables(string text, Customer customer)
        {
            if (text.Contains("{{NAME}}"))
                text = text.Replace("{{NAME}}", customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName));
            return text;
        }
    }
}
