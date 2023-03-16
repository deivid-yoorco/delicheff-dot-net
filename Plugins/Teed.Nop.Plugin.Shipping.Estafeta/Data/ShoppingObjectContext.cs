﻿using Nop.Core;
using Nop.Data;
using Nop.Plugin.Shipping.Estafeta.Data.Mapping;
using Nop.Plugin.Shipping.Estafeta.Domain.Shopping;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Nop.Plugin.Shipping.Estafeta.Data
{
    public class ShoppingObjectContext : DbContext, IDbContext
    {
        public ShoppingObjectContext(string nameOrConnectionString) : base(nameOrConnectionString) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new ShoppingMap());

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

            DropTable<Shopping>();
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