using Autofac;
using Autofac.Core;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Web.Framework.Infrastructure;
using Teed.Plugin.ShoppingCartUrlGenerator.Data;
using Teed.Plugin.ShoppingCartUrlGenerator.Domain;
using Teed.Plugin.ShoppingCartUrlGenerator.Services;

namespace Teed.Plugin.ShoppingCartUrlGenerator.Infastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_ShoppingCartUrlGenerator";
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<ShoppingCartUrlService>().InstancePerLifetimeScope();
            builder.RegisterType<ShoppingCartUrlProductService>().InstancePerLifetimeScope();
            builder.RegisterType<ShoppingCartUrlUserService>().InstancePerLifetimeScope();

            this.RegisterPluginDataContext<PluginObjectContext>(builder, CONTEXT_NAME);

            Register<ShoppingCartUrl>(builder);
            Register<ShoppingCartUrlProduct>(builder);
            Register<ShoppingCartUrlUser>(builder);
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
