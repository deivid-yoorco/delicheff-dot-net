using Nop.Core;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Teed.Plugin.CustomPages.Data.Mapping;
using Teed.Plugin.CustomPages.Domain.CustomPages;

namespace Teed.Plugin.CustomPages.Data
{
    public class PluginObjectContext : DbContext, IDbContext
    {
        public PluginObjectContext(string nameOrConnectionString) : base(nameOrConnectionString) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new CustomPageMap());
            modelBuilder.Configurations.Add(new BoxesMap());
            modelBuilder.Configurations.Add(new CarouselMap());
            modelBuilder.Configurations.Add(new BannersMap());
            modelBuilder.Configurations.Add(new CategoryDropdownMap());
            modelBuilder.Configurations.Add(new CollageMap());
            modelBuilder.Configurations.Add(new ParallaxMap());
            modelBuilder.Configurations.Add(new PopUpMap());
            modelBuilder.Configurations.Add(new SliderMap());
            modelBuilder.Configurations.Add(new TagsMap());
            modelBuilder.Configurations.Add(new TopThreeMap());
            modelBuilder.Configurations.Add(new CustomPageProductMap());

            base.OnModelCreating(modelBuilder);
        }
        public string CreateDatabaseScript()
        {
            return ((IObjectContextAdapter)this).ObjectContext.CreateDatabaseScript();
        }
        public new IDbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }
        public void Install()
        {
            var dbScript = CreateDatabaseScript();
            Database.ExecuteSqlCommand(dbScript);
            SaveChanges();
        }

        public void Uninstall()
        {
            //drop the table
            //DropTable<CustomPage>();
            //DropTable<Boxes>();
            //DropTable<Carousel>();
            //DropTable<Banners>();
            //DropTable<CategoryDropdown>();
            //DropTable<Collage>();
            //DropTable<Parallax>();
            //DropTable<PopUp>();
            //DropTable<Slider>();
            //DropTable<Tags>();
            //DropTable<TopThree>();
            //DropTable<CustomPageProduct>();
        }

        private void DropTable<T>() where T : BaseEntity
        {
            var tableName = this.GetTableName<T>();
            this.DropPluginTable(tableName);
        }

        public IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters) where TEntity : BaseEntity, new()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TElement> SqlQuery<TElement>(string sql, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public int ExecuteSqlCommand(string sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters)
        {
            throw new NotImplementedException();
        }
        public void Detach(object entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            ((IObjectContextAdapter)this).ObjectContext.Detach(entity);
        }

        public virtual bool ProxyCreationEnabled
        {
            get
            {
                return Configuration.ProxyCreationEnabled;
            }
            set
            {
                Configuration.ProxyCreationEnabled = value;
            }
        }

        public virtual bool AutoDetectChangesEnabled
        {
            get
            {
                return Configuration.AutoDetectChangesEnabled;
            }
            set
            {
                Configuration.AutoDetectChangesEnabled = value;
            }
        }
    }
}
