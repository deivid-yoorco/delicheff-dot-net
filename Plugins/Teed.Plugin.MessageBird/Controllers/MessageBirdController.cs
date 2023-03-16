using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Teed.Plugin.MessageBird.Domain;
using Teed.Plugin.MessageBird.Models;
using Teed.Plugin.MessageBird.Security;
using Teed.Plugin.MessageBird.Services;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Services.Orders;
using Nop.Services.Common;
using Nop.Core.Domain.Customers;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace Teed.Plugin.MessageBird.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class MessageBirdController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly MessageBirdLogService _messageBirdLogService;

        public MessageBirdController(IPermissionService permissionService, IWorkContext workContext,
            IStoreService storeService, ISettingService settingService,
            ILocalizationService localizationService, ICustomerService customerService,
            MessageBirdLogService messageBirdLogService, IOrderService orderService)
        {
            _permissionService = permissionService;
            _workContext = workContext;
            _storeService = storeService;
            _settingService = settingService;
            _localizationService = localizationService;
            _customerService = customerService;
            _messageBirdLogService = messageBirdLogService;
            _orderService = orderService;
        }

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(TeedMessageBirdPermissionProvider.MessageBirdAdmin))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var messageBirdSettings = _settingService.LoadSetting<MessageBirdSettings>(storeScope);

            var model = new ConfigurationModel
            {
                IsSandbox = messageBirdSettings.IsSandbox,
                ApiKey = messageBirdSettings.ApiKey,
                WhatsAppChannelId = messageBirdSettings.WhatsAppChannelId,
                NameSpace = messageBirdSettings.Namespace,
                SandboxApiKey = messageBirdSettings.SandboxApiKey,
                SandboxWhatsAppChannelId = messageBirdSettings.SandboxWhatsAppChannelId,
                SandboxNameSpace = messageBirdSettings.SandboxNamespace
            };
            model.IsSandbox_OverrideForStore = _settingService.SettingExists(messageBirdSettings, x => x.IsSandbox, storeScope);
            model.ApiKey_OverrideForStore = _settingService.SettingExists(messageBirdSettings, x => x.ApiKey, storeScope);
            model.WhatsAppChannelId_OverrideForStore = _settingService.SettingExists(messageBirdSettings, x => x.WhatsAppChannelId, storeScope);
            model.NameSpace_OverrideForStore = _settingService.SettingExists(messageBirdSettings, x => x.Namespace, storeScope);
            model.SandboxApiKey_OverrideForStore = _settingService.SettingExists(messageBirdSettings, x => x.SandboxApiKey, storeScope);
            model.SandboxWhatsAppChannelId_OverrideForStore = _settingService.SettingExists(messageBirdSettings, x => x.SandboxWhatsAppChannelId, storeScope);
            model.SandboxNameSpace_OverrideForStore = _settingService.SettingExists(messageBirdSettings, x => x.SandboxNamespace, storeScope);

            return View("~/Plugins/Teed.Plugin.MessageBird/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(TeedMessageBirdPermissionProvider.MessageBirdAdmin))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var messageBirdSettings = _settingService.LoadSetting<MessageBirdSettings>(storeScope);

            //save settings
            messageBirdSettings.IsSandbox = model.IsSandbox;
            messageBirdSettings.ApiKey = model.ApiKey;
            messageBirdSettings.WhatsAppChannelId = model.WhatsAppChannelId;
            messageBirdSettings.Namespace = model.NameSpace;
            messageBirdSettings.SandboxApiKey = model.SandboxApiKey;
            messageBirdSettings.SandboxWhatsAppChannelId = model.SandboxWhatsAppChannelId;
            messageBirdSettings.SandboxNamespace = model.SandboxNameSpace;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(messageBirdSettings, x => x.IsSandbox, model.IsSandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(messageBirdSettings, x => x.ApiKey, model.ApiKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(messageBirdSettings, x => x.WhatsAppChannelId, model.WhatsAppChannelId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(messageBirdSettings, x => x.Namespace, model.NameSpace_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(messageBirdSettings, x => x.SandboxApiKey, model.SandboxApiKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(messageBirdSettings, x => x.SandboxWhatsAppChannelId, model.SandboxWhatsAppChannelId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(messageBirdSettings, x => x.SandboxNamespace, model.SandboxNameSpace_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        public IActionResult Log()
        {
            if (!_permissionService.Authorize(TeedMessageBirdPermissionProvider.MessageBirdAdmin))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.MessageBird/Views/MessageBird/Log.cshtml");
        }

        [HttpPost]
        public IActionResult LogData(int customerId = 0)
        {
            if (!_permissionService.Authorize(TeedMessageBirdPermissionProvider.MessageBirdAdmin))
                return AccessDeniedView();

            var logsQuery = _messageBirdLogService.GetAll();
            if (customerId > 0)
                logsQuery = logsQuery.Where(x => x.CustomerId == customerId);
            var logsData = logsQuery
                .OrderByDescending(x => x.CreatedOnUtc)
                .ToList();

            var adminIds = logsData.Select(x => x.AdminId).ToList();
            var customerIds = logsData.Select(x => x.AdminId).ToList();
            var admins = _customerService.GetAllCustomersQuery()
                .Where(x => adminIds.Contains(x.Id))
                .ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => customerIds.Contains(x.Id))
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = logsData
                .Select(x => new
                {
                    x.Id,
                    x.ToNumber,
                    x.Status,
                    StatusColor = GetStatusColor(x.Status),
                    Date = x.CreatedOnUtc.ToLocalTime().ToString("dd/MM/yyyy hh:mm:ss tt"),
                    AdminThatSent = admins.Where(y => y.Id == x.AdminId).FirstOrDefault()?.Email,
                    AdminIdThatSent = admins.Where(y => y.Id == x.AdminId).FirstOrDefault()?.Id,
                    Customer = customers.Where(y => y.Id == x.CustomerId).FirstOrDefault()?.Email,
                    CustomerId = customers.Where(y => y.Id == x.CustomerId).FirstOrDefault()?.Id,
                    IsStatusUpdatable = !IsStatusFinalStep(x.Status),
                    Template = JsonConvert.DeserializeObject<dynamic>(x.TextSent)?.templateName
                }).ToList(),
                Total = logsData.Count()
            };

            return Json(gridModel);
        }

        public async Task<IActionResult> LogInfo(int Id)
        {
            if (!_permissionService.Authorize(TeedMessageBirdPermissionProvider.MessageBirdAdmin))
                return AccessDeniedView();

            await UpdateMessageStatus(Id);
            var log = _messageBirdLogService.GetAll()
                .Where(x => x.Id == Id)
                .FirstOrDefault();
            if (log == null) return NotFound();
            var admin = _customerService.GetAllCustomersQuery()
                .Where(x => x.Id == log.AdminId)
                .FirstOrDefault();
            var customer = _customerService.GetAllCustomersQuery()
                .Where(x => x.Id == log.CustomerId)
                .FirstOrDefault();
            var model = new MessageBirdLogModel
            {
                Id = log.Id,
                AdminId = admin.Id,
                Admin = admin.Email,
                CustomerId = admin.Id,
                Customer = admin.Email,
                CreatedOn = log.CreatedOnUtc.ToLocalTime(),
                WhatsAppChannelId = log.WhatsAppChannelId,
                JsonRequest = log.JsonRequest,
                JsonResponse = log.JsonResponse,
                Status = log.Status,
                TextSent = log.TextSent,
                ToNumber = log.ToNumber,
            };

            return View("~/Plugins/Teed.Plugin.MessageBird/Views/MessageBird/LogInfo.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> SendWhatsApp(WhatsAppSendModel model)
        {
            if (!_permissionService.Authorize(TeedMessageBirdPermissionProvider.MessageBirdAdmin) &&
                !_permissionService.Authorize(TeedMessageBirdPermissionProvider.MessageBird))
                return BadRequest();

            var customer = _customerService.GetCustomerById(model.CustomerId);
            if (string.IsNullOrEmpty(model.Template) || string.IsNullOrEmpty(model.ToNumber) || customer == null)
                return BadRequest();

            int storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            MessageBirdSettings messageBirdSettings = _settingService.LoadSetting<MessageBirdSettings>(storeScope);
            string apiKey = messageBirdSettings.IsSandbox ? messageBirdSettings.SandboxApiKey : messageBirdSettings.ApiKey;
            string channelId = messageBirdSettings.IsSandbox ? messageBirdSettings.SandboxWhatsAppChannelId : messageBirdSettings.WhatsAppChannelId;
            string nameSpace = messageBirdSettings.IsSandbox ? messageBirdSettings.SandboxNamespace : messageBirdSettings.Namespace;

            string greetings = GetGreetings();

            var varsForInsert = new List<ComponentParameter>
            {
                new ComponentParameter
                {
                    type = nameof(ParameterType.text),
                    text = model.ToName
                },
                new ComponentParameter
                {
                    type = nameof(ParameterType.text),
                    text = greetings
                }
            };

            #region Disabled
            //if (!string.IsNullOrEmpty(model.Variables))
            //{
            //    var variables = model.Variables.Split(',').ToList();
            //    foreach (var variable in variables)
            //    {
            //        var varText = variable;
            //        if (variable == "<nombre>")
            //            varText = model.ToName;
            //        else if (variable == "<saludo>")
            //            varText = greetings;
            //        varsForInsert.Add(new ComponentParameter
            //        {
            //            type = nameof(ParameterType.text),
            //            text = varText
            //        });
            //    }
            //}
            #endregion

            string phoneNumber = ParsePhoneNumber(model.ToNumber);
            WhatsAppBody body = new WhatsAppBody
            {
                to = phoneNumber,
                type = nameof(BodyType.hsm),
                from = channelId,
                content = new BodyContent
                {
                    hsm = new ContentHsm
                    {
                        nameSpace = nameSpace,
                        templateName = model.Template,
                        language = new Models.HsmLanguage
                        {
                            code = nameof(LanguageType.es_MX),
                            policy = "deterministic"
                        },
                        components = new List<HsmComponent>
                        {
                            new HsmComponent
                            {
                                type = nameof(ComponentType.body),
                                parameters = varsForInsert,
                            }
                        }
                    }
                }
            };

            string data = JsonConvert.SerializeObject(body);
            List<string> successTypes = Enum.GetValues(typeof(SuccessType))
                .Cast<SuccessType>()
                .Select(v => v.ToString())
                .ToList();
            List<string> failureTypes = Enum.GetValues(typeof(FailureType))
                .Cast<FailureType>()
                .Select(v => v.ToString())
                .ToList();

            string url = "https://conversations.messagebird.com/v1/send";
            try
            {
                using (HttpClient messageClient = new HttpClient())
                {
                    //send message
                    messageClient.DefaultRequestHeaders.Add("Authorization", "AccessKey " + apiKey);
                    var content = new StringContent(data, Encoding.UTF8, "application/json");
                    var messageResult = await messageClient.PostAsync(url, content);
                    string messageJson = await messageResult.Content.ReadAsStringAsync();
                    dynamic id = JsonConvert.DeserializeObject<dynamic>(messageJson).id;

                    url = "https://conversations.messagebird.com/v1/messages/" + id;
                    using (HttpClient client = new HttpClient())
                    {
                        //check new message status
                        client.DefaultRequestHeaders.Add("Authorization", "AccessKey " + apiKey);
                        messageResult = await client.GetAsync(url);
                        messageJson = await messageResult.Content.ReadAsStringAsync();
                        if (messageResult.IsSuccessStatusCode)
                        {
                            ReceiveMessageInfo messageInfo = JsonConvert.DeserializeObject<ReceiveMessageInfo>(messageJson);
                            if (successTypes.Contains(messageInfo.status) || failureTypes.Contains(messageInfo.status))
                            {
                                MessageBirdLog log = new MessageBirdLog
                                {
                                    ToNumber = messageInfo.to,
                                    AdminId = _workContext.CurrentCustomer.Id,
                                    CustomerId = model.CustomerId,
                                    WhatsAppChannelId = channelId,
                                    Status = messageInfo.status,
                                    TextSent = JsonConvert.SerializeObject(messageInfo.content.hsm),
                                    JsonRequest = data,
                                    JsonResponse = messageJson,
                                };
                                _messageBirdLogService.Insert(log);
                                if (failureTypes.Contains(messageInfo.status))
                                    return BadRequest(messageInfo.status);
                                else if (successTypes.Contains(messageInfo.status))
                                    return Ok();
                            }
                            return BadRequest("Unknown error with response.");
                        }
                        else
                        {
                            var log = new MessageBirdLog
                            {
                                ToNumber = phoneNumber,
                                AdminId = _workContext.CurrentCustomer.Id,
                                CustomerId = model.CustomerId,
                                WhatsAppChannelId = channelId,
                                Status = "unknown",
                                TextSent = "",
                                JsonRequest = data,
                                JsonResponse = messageJson,
                            };
                            _messageBirdLogService.Insert(log);
                            return BadRequest();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var log = new MessageBirdLog
                {
                    ToNumber = phoneNumber,
                    AdminId = _workContext.CurrentCustomer.Id,
                    CustomerId = model.CustomerId,
                    WhatsAppChannelId = channelId,
                    Status = "unknown",
                    TextSent = "",
                    JsonRequest = data,
                    JsonResponse = e.Message,
                };
                _messageBirdLogService.Insert(log);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> UpdateMessageStatus(int Id)
        {
            try
            {
                int storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
                MessageBirdSettings messageBirdSettings = _settingService.LoadSetting<MessageBirdSettings>(storeScope);
                var log = _messageBirdLogService.GetById(Id);
                if (log != null && !IsStatusFinalStep(log.Status))
                {
                    ReceiveMessageInfo messageInfo = JsonConvert.DeserializeObject<ReceiveMessageInfo>(log.JsonResponse);

                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", "AccessKey " +
                                (messageBirdSettings.IsSandbox ? messageBirdSettings.SandboxApiKey : messageBirdSettings.ApiKey));
                        var messageResult = await client.GetAsync($"https://conversations.messagebird.com/v1/messages/{messageInfo.id}");
                        string messageJson = await messageResult.Content.ReadAsStringAsync();
                        if (messageResult.IsSuccessStatusCode)
                        {
                            ReceiveMessageInfo newMessageInfo = JsonConvert.DeserializeObject<ReceiveMessageInfo>(messageJson);
                            if (log.Status != newMessageInfo.status)
                            {
                                log.Status = newMessageInfo.status;
                                _messageBirdLogService.Update(log);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> SendTestWhatsApp(string templateId, string phoneNumber, int customerId)
        {
            if (!_permissionService.Authorize(TeedMessageBirdPermissionProvider.MessageBirdAdmin) &&
                !_permissionService.Authorize(TeedMessageBirdPermissionProvider.MessageBird))
                return BadRequest();

            var customer = _customerService.GetCustomerById(customerId);
            if (string.IsNullOrEmpty(templateId) || string.IsNullOrEmpty(phoneNumber) || customer == null)
                return BadRequest();

            int storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            MessageBirdSettings messageBirdSettings = _settingService.LoadSetting<MessageBirdSettings>(storeScope);
            string apiKey = messageBirdSettings.IsSandbox ? messageBirdSettings.SandboxApiKey : messageBirdSettings.ApiKey;
            string channelId = messageBirdSettings.IsSandbox ? messageBirdSettings.SandboxWhatsAppChannelId : messageBirdSettings.WhatsAppChannelId;
            string nameSpace = messageBirdSettings.IsSandbox ? messageBirdSettings.SandboxNamespace : messageBirdSettings.Namespace;

            string greetings = GetGreetings();

            var varsForInsert = new List<ComponentParameter>
            {
                new ComponentParameter
                {
                    type = nameof(ParameterType.text),
                    text = customer.GetFullName().Split(' ').FirstOrDefault()
                }
            };

            string parsedPhoneNumber = ParsePhoneNumber(phoneNumber);
            WhatsAppBody body = new WhatsAppBody
            {
                to = parsedPhoneNumber,
                type = nameof(BodyType.hsm),
                from = channelId,
                content = new BodyContent
                {
                    hsm = new ContentHsm
                    {
                        nameSpace = nameSpace,
                        templateName = templateId,
                        language = new Models.HsmLanguage
                        {
                            code = nameof(LanguageType.es_MX)
                        },
                        components = new List<HsmComponent>
                        {
                            new HsmComponent
                            {
                                type = nameof(ComponentType.body),
                                parameters = varsForInsert,
                            }
                        }
                    }
                }
            };

            string data = JsonConvert.SerializeObject(body);
            List<string> successTypes = Enum.GetValues(typeof(SuccessType))
                .Cast<SuccessType>()
                .Select(v => v.ToString())
                .ToList();
            List<string> failureTypes = Enum.GetValues(typeof(FailureType))
                .Cast<FailureType>()
                .Select(v => v.ToString())
                .ToList();

            string url = "https://conversations.messagebird.com/v1/send";
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";
            httpRequest.Headers["Authorization"] = "AccessKey " + apiKey;
            httpRequest.ContentType = "application/json";
            try
            {
                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    streamWriter.Write(data);
                }

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    dynamic id = JsonConvert.DeserializeObject<dynamic>(result).id;
                    url = "https://conversations.messagebird.com/v1/messages/" + id;
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", "AccessKey " + apiKey);
                        var messageResult = await client.GetAsync(url);
                        string messageJson = await messageResult.Content.ReadAsStringAsync();
                        if (messageResult.IsSuccessStatusCode)
                        {
                            ReceiveMessageInfo messageInfo = JsonConvert.DeserializeObject<ReceiveMessageInfo>(messageJson);
                            if (successTypes.Contains(messageInfo.status) || failureTypes.Contains(messageInfo.status))
                            {
                                MessageBirdLog log = new MessageBirdLog
                                {
                                    ToNumber = messageInfo.to,
                                    AdminId = _workContext.CurrentCustomer.Id,
                                    CustomerId = customerId,
                                    WhatsAppChannelId = channelId,
                                    Status = messageInfo.status,
                                    TextSent = JsonConvert.SerializeObject(messageInfo.content.hsm),
                                    JsonRequest = data,
                                    JsonResponse = messageJson,
                                };
                                _messageBirdLogService.Insert(log);
                                if (failureTypes.Contains(messageInfo.status))
                                    return BadRequest(messageInfo.status);
                                else if (successTypes.Contains(messageInfo.status))
                                    return Ok();
                            }
                            return BadRequest("Unknown error with response.");
                        }
                        else
                        {
                            var log = new MessageBirdLog
                            {
                                ToNumber = phoneNumber,
                                AdminId = _workContext.CurrentCustomer.Id,
                                CustomerId = customerId,
                                WhatsAppChannelId = channelId,
                                Status = "unknown",
                                TextSent = "",
                                JsonRequest = data,
                                JsonResponse = messageJson,
                            };
                            _messageBirdLogService.Insert(log);
                            return BadRequest();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var log = new MessageBirdLog
                {
                    ToNumber = phoneNumber,
                    AdminId = _workContext.CurrentCustomer.Id,
                    CustomerId = customerId,
                    WhatsAppChannelId = channelId,
                    Status = "unknown",
                    TextSent = "",
                    JsonRequest = data,
                    JsonResponse = e.Message,
                };
                _messageBirdLogService.Insert(log);
                return BadRequest(e.Message);
            }
        }

        private string GetGreetings()
        {
            DateTime today = DateTime.Now;
            if (today.Hour >= 5 && today.Hour < 12)
                return "Buenos días";
            else if (today.Hour >= 12 && today.Hour < 19)
                return "Buenas tardes";
            else
                return "Buenas noches";
        }

        private string GetStatusColor(string status)
        {
            var final = string.Empty;
            switch (status)
            {
                case "pending":
                    final = "yellow";
                    break;
                case "sent":
                    final = "green";
                    break;
                case "read":
                    final = "green";
                    break;
                case "received":
                    final = "green";
                    break;
                case "delivered":
                    final = "green";
                    break;
                case "rejected":
                    final = "red";
                    break;
                case "failed":
                    final = "red";
                    break;
                case "deleted":
                    final = "red";
                    break;
                case "unknown":
                    final = "red";
                    break;
                default:
                    break;
            }
            return final;
        }

        private string ParsePhoneNumber(string phoneNumber)
        {
            string parsedNumber = phoneNumber.Replace(" ", "");
            if (!parsedNumber.StartsWith("52"))
                parsedNumber = "52" + parsedNumber;
            parsedNumber = "+" + parsedNumber;
            return parsedNumber;
        }

        private bool IsStatusFinalStep(string currentStatus)
        {
            var finalSteps = new List<string> { nameof(SuccessType.read), nameof(FailureType.rejected), nameof(FailureType.failed), nameof(FailureType.deleted) };
            return finalSteps.Contains(currentStatus);
        }
    }
}