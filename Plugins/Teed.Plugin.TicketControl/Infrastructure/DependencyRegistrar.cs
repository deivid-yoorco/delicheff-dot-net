using Autofac;
using Autofac.Core;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Web.Framework.Infrastructure;
using Teed.Plugin.TicketControl.Data;
using Teed.Plugin.TicketControl.Domain.Schedules;
using Teed.Plugin.TicketControl.Domain.DatePersonalizations;
using Teed.Plugin.TicketControl.Domain.Tickets;
using Teed.Plugin.TicketControl.Services;

namespace Teed.Plugin.TicketControl.Infastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_ticketcontrolplugin";
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<ScheduleService>().InstancePerLifetimeScope();
            builder.RegisterType<DatePersonalizationService>().InstancePerLifetimeScope();
            builder.RegisterType<TicketService>().InstancePerLifetimeScope();

            this.RegisterPluginDataContext<PluginObjectContext>(builder, CONTEXT_NAME);

            Register<Schedule>(builder);
            Register<DatePersonalization>(builder);
            Register<Ticket>(builder);
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
