using Autofac;
using Autofac.Core;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Web.Framework.Infrastructure;
using Teed.Plugin.Payroll.Data;
using Teed.Plugin.Payroll.Domain.JobCatalogs;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;
using Teed.Plugin.Payroll.Domain.Incidents;
using Teed.Plugin.Payroll.Domain.BiweeklyPayments;
using Teed.Plugin.Payroll.Services;
using Teed.Plugin.Payroll.Domain.MinimumWagesCatalogs;
using Teed.Plugin.Payroll.Domain.Benefits;
using Teed.Plugin.Payroll.Domain.Bonuses;
using Teed.Plugin.Payroll.Domain.PayrollEmployeeJobs;
using Teed.Plugin.Payroll.Domain.Assistances;

namespace Teed.Plugin.Payroll.Infastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_payrollplugin";
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<PayrollEmployeeService>().InstancePerLifetimeScope();
            builder.RegisterType<PayrollEmployeeFileService>().InstancePerLifetimeScope();
            builder.RegisterType<PayrollSalaryService>().InstancePerLifetimeScope();
            builder.RegisterType<JobCatalogService>().InstancePerLifetimeScope();
            builder.RegisterType<IncidentService>().InstancePerLifetimeScope();
            builder.RegisterType<SubordinateService>().InstancePerLifetimeScope();
            builder.RegisterType<BiweeklyPaymentService>().InstancePerLifetimeScope();
            builder.RegisterType<BiweeklyPaymentFileService>().InstancePerLifetimeScope();
            builder.RegisterType<MinimumWagesCatalogService>().InstancePerLifetimeScope();
            builder.RegisterType<BenefitService>().InstancePerLifetimeScope();
            builder.RegisterType<AssistanceService>().InstancePerLifetimeScope();
            builder.RegisterType<BonusService>().InstancePerLifetimeScope();
            builder.RegisterType<BonusAmountService>().InstancePerLifetimeScope();
            builder.RegisterType<BonusApplicationService>().InstancePerLifetimeScope();
            builder.RegisterType<TakenVacationDayService>().InstancePerLifetimeScope();
            builder.RegisterType<PayrollEmployeeJobService>().InstancePerLifetimeScope();
            builder.RegisterType<AssistanceOverrideService>().InstancePerLifetimeScope();

            // NEW
            builder.RegisterType<OperationalIncidentService>().InstancePerLifetimeScope();

            this.RegisterPluginDataContext<PluginObjectContext>(builder, CONTEXT_NAME);

            Register<PayrollEmployee>(builder);
            Register<PayrollEmployeeFile>(builder);
            Register<PayrollSalary>(builder);
            Register<JobCatalog>(builder);
            Register<Incident>(builder);
            Register<Subordinate>(builder);
            Register<BiweeklyPayment>(builder);
            Register<BiweeklyPaymentFile>(builder);
            Register<MinimumWagesCatalog>(builder);
            Register<Benefit>(builder);
            Register<Bonus>(builder);
            Register<BonusAmount>(builder);
            Register<BonusApplication>(builder);
            Register<TakenVacationDay>(builder);
            Register<PayrollEmployeeJob>(builder);
            Register<AssistanceOverride>(builder);

            // NEW
            Register<OperationalIncident>(builder);
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
