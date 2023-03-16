using Autofac;
using Autofac.Core;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Web.Framework.Infrastructure;
using Teed.Plugin.ShippingByAddress.Domain.ShippingByAddressD;
using Teed.Plugin.ShippingByAddress.Data;
using Teed.Plugin.ShippingByAddress.Services;
using Teed.Plugin.ShippingByAddress.Domain.ShippingBranch;

namespace Teed.Plugin.Groceries.Infastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_shippingbyaddress";
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<ShippingByAddressService>().InstancePerLifetimeScope();
            builder.RegisterType<ShippingBranchService>().InstancePerLifetimeScope();
            builder.RegisterType<ShippingBranchOrderService>().InstancePerLifetimeScope();

            this.RegisterPluginDataContext<PluginObjectContext>(builder, CONTEXT_NAME);

            Register<ShippingByAddressD>(builder);
            Register<ShippingBranch>(builder);
            Register<ShippingBranchOrder>(builder);
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
