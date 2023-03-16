using Autofac;
using Autofac.Core;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Web.Framework.Infrastructure;
using Teed.Plugin.Api.Data;
using Teed.Plugin.Api.Domain.Groceries;
using Teed.Plugin.Api.Domain.Identity;
using Teed.Plugin.Api.Domain.Notifications;
using Teed.Plugin.Api.Domain.Payment;
using Teed.Plugin.Api.Domain.Popups;
using Teed.Plugin.Api.Domain.TaggableBoxes;
using Teed.Plugin.Api.Domain.Onboardings;
using Teed.Plugin.Api.Helper;
using Teed.Plugin.Api.Services;

namespace Teed.Plugin.Api.Infastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_api";
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<CustomerSecurityTokenService>().InstancePerLifetimeScope();
            builder.RegisterType<CotizadorEstafetaService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductAttributeConverter>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerSavedCardService>().InstancePerLifetimeScope();
            builder.RegisterType<NotificationService>().InstancePerLifetimeScope();
            builder.RegisterType<QueuedNotificationService>().InstancePerLifetimeScope();
            if (Nop.Services.TeedCommerceStores.CurrentStore == Nop.Services.TeedStores.CentralEnLinea)
            {
                builder.RegisterType<WebScrapingUnitService>().InstancePerLifetimeScope();
                builder.RegisterType<WebScrapingHistoryService>().InstancePerLifetimeScope();
                builder.RegisterType<PopupService>().InstancePerLifetimeScope(); // NEW
                builder.RegisterType<OnboardingService>().InstancePerLifetimeScope(); // NEW
                builder.RegisterType<TaggableBoxService>().InstancePerLifetimeScope(); // NEW
            }

            this.RegisterPluginDataContext<ApiObjectContext>(builder, CONTEXT_NAME);

            Register<CustomerSecurityToken>(builder);
            Register<CustomerSavedCard>(builder);
            Register<Notification>(builder);
            Register<QueuedNotification>(builder);
            if (Nop.Services.TeedCommerceStores.CurrentStore == Nop.Services.TeedStores.CentralEnLinea)
            {
                Register<WebScrapingUnit>(builder);
                Register<WebScrapingHistory>(builder);
                Register<Popup>(builder); // NEW
                Register<Onboarding>(builder); // NEW
                Register<TaggableBox>(builder); // NEW
            }
        }

        private void Register<T>(ContainerBuilder builder) where T : BaseEntity
        {
            builder.RegisterType<EfRepository<T>>()
            .As<IRepository<T>>()
            .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME))
            .InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
