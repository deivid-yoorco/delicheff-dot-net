using Autofac;
using Autofac.Core;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Web.Framework.Infrastructure;
using Teed.Plugin.MarketingDashboard.Data;
using Teed.Plugin.MarketingDashboard.Domain.MarketingExpenses;
using Teed.Plugin.MarketingDashboard.Services;

namespace Teed.Plugin.MarketingDashboard.Infastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_marketingdashboardplugin";
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<MarketingExpenseService>().InstancePerLifetimeScope();
            builder.RegisterType<MarketingExpenseTypeService>().InstancePerLifetimeScope();
            builder.RegisterType<MarketingGrossMarginService>().InstancePerLifetimeScope();
            builder.RegisterType<MarketingDashboardDataService>().InstancePerLifetimeScope();
            builder.RegisterType<MarketingRetentionRateService>().InstancePerLifetimeScope();
            builder.RegisterType<MarketingAutomaticExpenseService>().InstancePerLifetimeScope(); // NEW
            builder.RegisterType<MarketingDiscountExpenseService>().InstancePerLifetimeScope(); // NEW

            this.RegisterPluginDataContext<PluginObjectContext>(builder, CONTEXT_NAME);

            Register<MarketingExpense>(builder);
            Register<MarketingExpenseType>(builder);
            Register<MarketingGrossMargin>(builder);
            Register<MarketingDashboardData>(builder);
            Register<MarketingRetentionRate>(builder);
            Register<MarketingAutomaticExpense>(builder); // NEW
            Register<MarketingDiscountExpense>(builder); // NEW
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
