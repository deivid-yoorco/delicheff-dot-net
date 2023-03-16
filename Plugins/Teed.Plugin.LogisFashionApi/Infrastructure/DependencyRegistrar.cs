using Autofac;
using Autofac.Core;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Web.Framework.Infrastructure;
using Teed.Plugin.Logisfashion.Data;
using Teed.Plugin.Logisfashion.Domain;
using Teed.Plugin.Logisfashion.Services;

namespace Teed.Plugin.Logisfashion.Infastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_logisfashionplugin";
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<LogisfashionRequestLogService>().InstancePerLifetimeScope();

            this.RegisterPluginDataContext<PluginObjectContext>(builder, CONTEXT_NAME);

            Register<LogisfashionRequestLog>(builder);
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
