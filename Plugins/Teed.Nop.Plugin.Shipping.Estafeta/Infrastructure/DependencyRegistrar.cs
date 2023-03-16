using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Shipping.Estafeta.Data;
using Nop.Plugin.Shipping.Estafeta.Domain.Shopping;
using Nop.Plugin.Shipping.Estafeta.Services;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Shipping.Estafeta.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_shoppingteed";

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<ShoppingService>().InstancePerLifetimeScope();
            //data context
            this.RegisterPluginDataContext<ShoppingObjectContext>(builder, CONTEXT_NAME);
            //override required repository with our custom context
            builder.RegisterType<EfRepository<Shopping>>()
            .As<IRepository<Shopping>>()
            .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME))
            .InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
