using Autofac;
using Autofac.Core;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Web.Framework.Infrastructure;
using Teed.Plugin.CustomPages.Data;
using Teed.Plugin.CustomPages.Domain.CustomPages;
using Teed.Plugin.CustomPages.Services;

namespace Teed.Plugin.CustomPages.Infrasctructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_teedcustompagesplugin";
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<CustomPagesService>().InstancePerLifetimeScope();
            builder.RegisterType<BannersService>().InstancePerLifetimeScope();
            builder.RegisterType<BoxesService>().InstancePerLifetimeScope();
            builder.RegisterType<CarouselService>().InstancePerLifetimeScope();
            builder.RegisterType<CategoryDropdownService>().InstancePerLifetimeScope();
            builder.RegisterType<CollageService>().InstancePerLifetimeScope();
            builder.RegisterType<ParallaxService>().InstancePerLifetimeScope();
            builder.RegisterType<PopUpService>().InstancePerLifetimeScope();
            builder.RegisterType<SliderService>().InstancePerLifetimeScope();
            builder.RegisterType<TagsService>().InstancePerLifetimeScope();
            builder.RegisterType<TopThreeService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomPageProductService>().InstancePerLifetimeScope();

            this.RegisterPluginDataContext<PluginObjectContext>(builder, CONTEXT_NAME);

            Register<CustomPage>(builder);
            Register<Banners>(builder);
            Register<Boxes>(builder);
            Register<Carousel>(builder);
            Register<CategoryDropdown>(builder);
            Register<Collage>(builder);
            Register<Parallax>(builder);
            Register<PopUp>(builder);
            Register<Slider>(builder);
            Register<Tags>(builder);
            Register<TopThree>(builder);
            Register<CustomPageProduct>(builder);
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
