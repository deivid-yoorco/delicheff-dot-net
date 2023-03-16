using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Nop.Services.Customers;
using Teed.Plugin.Api.Services;
using System.Data.Entity;
using Teed.Plugin.Api.Dtos.Notifications;
using Nop.Web.Framework.Kendoui;
using Teed.Plugin.Api.Models;
using System.Globalization;
using Nop.Services.Security;
using Nop.Core;
using Teed.Plugin.Api.Domain.Notifications;
using Notification = Teed.Plugin.Api.Domain.Notifications.Notification;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Controllers;
using System.Net.Http;
using Nop.Services.Logging;
using Newtonsoft.Json.Linq;
using Teed.Plugin.Api.Domain.Identity;
using Nop.Services.Stores;
using Nop.Services.Configuration;
using Nop.Data;
using System.Data.SqlClient;

namespace Teed.Plugin.Api.Controllers
{
    public class NotificationController : ApiBaseController
    {
        #region Properties

        private readonly ICustomerService _customerService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly NotificationService _notificationService;
        private readonly QueuedNotificationService _queuedNotificationService;
        private readonly CustomerSecurityTokenService _customerSecurityTokenService;
        private readonly IDbContext _dbContext;

        #endregion

        #region Ctor

        public NotificationController(ICustomerService customerService,
            NotificationService notificationService,
            QueuedNotificationService queuedNotificationService,
            CustomerSecurityTokenService customerSecurityTokenService,
            IPermissionService permissionService,
            IStoreService storeService,
            ISettingService settingService,
            IWorkContext workContext,
            ILogger logger,
            IDbContext dbContext)
        {
            _customerService = customerService;
            _notificationService = notificationService;
            _queuedNotificationService = queuedNotificationService;
            _customerSecurityTokenService = customerSecurityTokenService;
            _permissionService = permissionService;
            _workContext = workContext;
            _logger = logger;
            _storeService = storeService;
            _settingService = settingService;
            _dbContext = dbContext;
        }

        #endregion

        #region Methods

        [HttpPost]
        public IActionResult SaveNotificationToken([FromBody] SaveNotificationTokenDto dto)
        {
            int customerId = int.Parse(UserId);
            var customerSecurityToken = _customerSecurityTokenService.GetAllByUuid(dto.DeviceUuid).Where(x => x.CustomerId == customerId).FirstOrDefault();
            if (customerSecurityToken == null) return NotFound();
            if (customerSecurityToken.FirebaseToken != dto.Token)
            {
                customerSecurityToken.FirebaseToken = dto.Token;
                _customerSecurityTokenService.Update(customerSecurityToken);
            }

            //var query = @"
            //    UPDATE [dbo].[CustomerSecurityToken] 
            //    SET FirebaseToken = @token, UpdatedOnUtc = @updatedOnUtc
            //    WHERE Uuid = @deviceUuid AND Deleted = 0 AND CustomerId = @customerId
            //";

            //_dbContext.ExecuteSqlCommand(query, false, int.MaxValue,
            //    new SqlParameter[] {
            //        new SqlParameter("@token", dto.Token),
            //        new SqlParameter("@deviceUuid", dto.DeviceUuid),
            //        new SqlParameter("@updatedOnUtc", DateTime.UtcNow),
            //        new SqlParameter("@customerId", customerId)
            //    });

            return NoContent();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SendTestNotification(int userId = 1)
        {
            if (FirebaseApp.DefaultInstance == null) _notificationService.CreateFirebaseApp();

            var token = _customerSecurityTokenService.GetAll().Where(x => x.CustomerId == userId).Select(x => x.FirebaseToken).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(token)) return BadRequest("TOKEN NO ENCONTRADO");

            var message = new Message()
            {
                Token = token,
                Data = new Dictionary<string, string>()
                {
                    { "Title", "Probadno remoto" },
                    { "Body", "Probando notificacion remota" }
                },
                Android = new AndroidConfig()
                {
                    Priority = Priority.High
                },
            };
            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            return Ok(response);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult CreateShoppingCartNotification([FromBody] CreateShoppingCartNotificationModel model)
        {
            if (model.CustomerIds.Count == 0) return NoContent();

            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var teedApiSettings = _settingService.LoadSetting<TeedApiPluginSettings>(storeScope);

            if (string.IsNullOrEmpty(teedApiSettings.AbandonedShoppingCartTitle) || string.IsNullOrEmpty(teedApiSettings.AbandonedShoppingCartBody))
                return NoContent();

            List<int> customerIdsWithToken = _customerSecurityTokenService.GetAll()
                .Where(x => x.FirebaseToken != null && model.CustomerIds.Contains(x.CustomerId))
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .ToList();

            foreach (var customerId in customerIdsWithToken)
            {
                var logText = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - Se generó una notificación automática.\n";
                Notification newNotification = new Notification()
                {
                    Title = teedApiSettings.AbandonedShoppingCartTitle,
                    Body = teedApiSettings.AbandonedShoppingCartBody,
                    DontSendBeforeDateUtc = DateTime.UtcNow,
                    Log = logText,
                    ActionTypeId = (int)ActionType.OpenShoppingCart,
                    CustomerIds = customerId.ToString(),
                    IsSystemNotification = true
                };
                _notificationService.Insert(newNotification);
            }

            return NoContent();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult CreateOrderTransactionNotification([FromBody] OrderTransactionModel model)
        {
            if (model.CustomerId == 0) return NoContent();

            bool userHasNotificationToken = _customerSecurityTokenService.GetAll()
                .Where(x => x.FirebaseToken != null && x.CustomerId == model.CustomerId)
                .Any();

            if (userHasNotificationToken)
            {
                var logText = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - Se generó una notificación automática.\n";
                var transactionData = GetTransactionNotificationData(model.TransactionType, model.OrderNumber);
                if (transactionData == null) return NoContent();
                Notification newNotification = new Notification()
                {
                    Title = transactionData?.Title,
                    Body = transactionData?.Body,
                    DontSendBeforeDateUtc = DateTime.UtcNow,
                    Log = logText,
                    ActionTypeId = (int)ActionType.OpenApp,
                    CustomerIds = model.CustomerId.ToString(),
                    IsSystemNotification = true
                };
                _notificationService.Insert(newNotification);
            }

            return NoContent();
        }

        #endregion

        #region Private methods

        private TransactionDataModel GetTransactionNotificationData(string transactionType, string orderNumber = "")
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var teedApiSettings = _settingService.LoadSetting<TeedApiPluginSettings>(storeScope);

            switch (transactionType)
            {
                case "order-paid":
                    if (string.IsNullOrEmpty(teedApiSettings.OrderPaidTitle) || string.IsNullOrEmpty(teedApiSettings.OrderPaidBody))
                        return null;
                    return new TransactionDataModel()
                    {
                        Title = teedApiSettings.OrderPaidTitle.Replace("{{ORDER_NUMBER}}", orderNumber),
                        Body = teedApiSettings.OrderPaidBody.Replace("{{ORDER_NUMBER}}", orderNumber)
                    };
                case "order-completed":
                    if (string.IsNullOrEmpty(teedApiSettings.OrderCompletedTitle) || string.IsNullOrEmpty(teedApiSettings.OrderCompletedBody))
                        return null;
                    return new TransactionDataModel()
                    {
                        Title = teedApiSettings.OrderCompletedTitle.Replace("{{ORDER_NUMBER}}", orderNumber),
                        Body = teedApiSettings.OrderCompletedBody.Replace("{{ORDER_NUMBER}}", orderNumber)
                    };
                case "order-placed":
                    if (string.IsNullOrEmpty(teedApiSettings.OrderPlacedBody) || string.IsNullOrEmpty(teedApiSettings.OrderPlacedTitle))
                        return null;
                    return new TransactionDataModel()
                    {
                        Title = teedApiSettings.OrderPlacedTitle.Replace("{{ORDER_NUMBER}}", orderNumber),
                        Body = teedApiSettings.OrderPlacedBody.Replace("{{ORDER_NUMBER}}", orderNumber)
                    };
                default:
                    return null;
            }
        }

        #endregion
    }

    public class CreateShoppingCartNotificationModel
    {
        public List<int> CustomerIds { get; set; }
    }

    public class OrderTransactionModel
    {
        public int CustomerId { get; set; }
        public string OrderNumber { get; set; }
        public string TransactionType { get; set; }
    }

    public class TransactionDataModel
    {
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
