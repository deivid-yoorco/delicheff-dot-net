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

namespace Teed.Plugin.MessageBird.Controllers
{
    public class MessageBirdPublicController : BasePluginController
    {
        private readonly ISettingService _settingService;

        public MessageBirdPublicController(ISettingService settingService)
        {
            _settingService = settingService;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("MessageBird/SetCurrentKeysForNopWeb")]
        public IActionResult SetCurrentKeysForNopWeb()
        {
            var messageBirdSettings = _settingService.LoadSetting<MessageBirdSettings>();
            var smsSettings = _settingService.LoadSetting<SmsVerificationSettings>();

            smsSettings.IsSandbox = messageBirdSettings.IsSandbox;
            smsSettings.SandboxApiKey = messageBirdSettings.SandboxApiKey;
            smsSettings.ApiKey = messageBirdSettings.ApiKey;
            _settingService.SaveSetting(smsSettings);

            return Ok();
        }
    }
}