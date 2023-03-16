using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Messages;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using Teed.Plugin.Emails.Helpers;

namespace Teed.Plugin.Emails
{
    public class TeedEmailsPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IRepository<EmailAccount> _emailAccountRepository;

        public TeedEmailsPlugin(ISettingService settingService,
            IWebHelper webHelper,
            IMessageTemplateService messageTemplateService,
            IRepository<EmailAccount> emailAccountRepository)
        {
            this._settingService = settingService;
            this._webHelper = webHelper;
            _messageTemplateService = messageTemplateService;
            _emailAccountRepository = emailAccountRepository;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/EmailsPlugin/Configure";
        }
        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            var abandonedCartMessageTemplate = _messageTemplateService.GetMessageTemplateByName(EmailHelper.ABANDONED_SHOPPING_CART_TEMPLATE_NAME, 0);
            if (abandonedCartMessageTemplate == null)
            {
                _messageTemplateService.InsertMessageTemplate(new MessageTemplate()
                {
                    Name = EmailHelper.ABANDONED_SHOPPING_CART_TEMPLATE_NAME,
                    Subject = "%Store.Name%. Olvidaste algo en tu carrito.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}¡Olvidaste algunas cosas en tu carrito!.{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = _emailAccountRepository.Table.Select(x => x.Id).FirstOrDefault(),
                });
            }

            var reorderMessageTemplate = _messageTemplateService.GetMessageTemplateByName(EmailHelper.REORDER_TEMPLATE_NAME, 0);
            if (reorderMessageTemplate == null)
            {
                _messageTemplateService.InsertMessageTemplate(new MessageTemplate()
                {
                    Name = EmailHelper.REORDER_TEMPLATE_NAME,
                    Subject = "%Store.Name%. ¡Es hora de pedir tu despensa!",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Te sugerimos estos productos.{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = _emailAccountRepository.Table.Select(x => x.Id).FirstOrDefault(),
                });
            }

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            //_settingService.DeleteSetting<TeedEmailsPluginSettings>();

            base.Uninstall();
        }

        public void GetPublicViewComponent(string widgetZone, out string viewComponentName)
        {
            throw new NotImplementedException();
        }

        IList<string> IWidgetPlugin.GetWidgetZones()
        {
            return new List<string> { "NonExistant" };
        }
    }
}
