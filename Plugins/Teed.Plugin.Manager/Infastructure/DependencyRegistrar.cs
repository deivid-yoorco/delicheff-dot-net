using Autofac;
using Autofac.Core;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Web.Framework.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Manager.Data;
using Teed.Plugin.Manager.Domain.CashExpenses;
using Teed.Plugin.Manager.Domain.Expenses;
using Teed.Plugin.Manager.Domain.ExpensesCategories;
using Teed.Plugin.Manager.Domain.PartnerLiabilities;
using Teed.Plugin.Manager.Domain.PurchaseOrders;
using Teed.Plugin.Manager.Services;

namespace Teed.Plugin.Manager.Infastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_teedmanagerplugin";
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<ExpensesCategoriesService>().InstancePerLifetimeScope();
            builder.RegisterType<ExpensesService>().InstancePerLifetimeScope();
            builder.RegisterType<ExpenseFileService>().InstancePerLifetimeScope();
            builder.RegisterType<CashExpenseFileService>().InstancePerLifetimeScope();
            builder.RegisterType<CashExpenseService>().InstancePerLifetimeScope();
            builder.RegisterType<PurchaseOrderService>().InstancePerLifetimeScope();
            builder.RegisterType<PurchaseOrderProductService>().InstancePerLifetimeScope();
            builder.RegisterType<PartnerLiabilityService>().InstancePerLifetimeScope();

            this.RegisterPluginDataContext<PluginObjectContext>(builder, CONTEXT_NAME);

            Register<ExpensesCategories>(builder);
            Register<Expense>(builder);
            Register<ExpenseFile>(builder);
            Register<CashExpenseFile>(builder);
            Register<CashExpense>(builder);
            Register<PurchaseOrder>(builder);
            Register<PurchaseOrderProduct>(builder);
            Register<PartnerLiability>(builder);
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
