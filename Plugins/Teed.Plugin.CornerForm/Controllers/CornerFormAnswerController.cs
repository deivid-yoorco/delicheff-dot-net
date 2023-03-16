using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Teed.Plugin.CornerForm.Domain;
using Teed.Plugin.CornerForm.Models;
using Teed.Plugin.CornerForm.Services;

namespace Teed.Plugin.CornerForm.Controllers
{
    public class CornerFormAnswerController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly CornerFormResultService _cornerFormResultService;

        public CornerFormAnswerController(IWorkContext workContext, IStoreService storeService, ISettingService settingService, CornerFormResultService cornerFormResultService)
        {
            _workContext = workContext;
            _storeService = storeService;
            _settingService = settingService;
            _cornerFormResultService = cornerFormResultService;
        }

        [HttpPost]
        public IActionResult SubmitAnswer(SubmitAnswerModel model)
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var pluginSettings = _settingService.LoadSetting<CornerFormSettings>(storeScope);

            CornerFormResult cornerFormResult = new CornerFormResult()
            {
                AnswerText = model.AnswerText,
                QuestionText = pluginSettings.Question,
                CustomerId = string.IsNullOrWhiteSpace(_workContext.CurrentCustomer.Email) ? 0 : _workContext.CurrentCustomer.Id
            };
            _cornerFormResultService.Insert(cornerFormResult);

            return NoContent();
        }
    }
}
