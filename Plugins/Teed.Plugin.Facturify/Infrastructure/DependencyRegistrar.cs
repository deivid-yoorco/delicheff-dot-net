using Autofac;
using Autofac.Core;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Web.Framework.Infrastructure;
using Teed.Plugin.Facturify.Data;
using Teed.Plugin.Facturify.Domain;
using Teed.Plugin.Facturify.Services;

namespace Teed.Plugin.Facturify.Infastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_facturifyplugin";
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<CustomerBillingAddressService>().InstancePerLifetimeScope();
            builder.RegisterType<BillService>().InstancePerLifetimeScope();
            builder.RegisterType<CategoryProductCatalogService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductSatCodeService>().InstancePerLifetimeScope();

            this.RegisterPluginDataContext<PluginObjectContext>(builder, CONTEXT_NAME);

            Register<CustomerBillingAddress>(builder);
            Register<Bill>(builder);
            Register<CategoryProductCatalog>(builder);
            Register<ProductSatCode>(builder);
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
