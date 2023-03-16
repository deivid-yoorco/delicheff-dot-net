using Nop.Core;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Teed.Plugin.Payroll.Data.Mapping;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;

namespace Teed.Plugin.Payroll.Data
{
    public class PluginObjectContext : DbContext, IDbContext
    {
        public PluginObjectContext(string nameOrConnectionString) : base(nameOrConnectionString) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new PayrollEmployeeMap());
            modelBuilder.Configurations.Add(new PayrollEmployeeFileMap());
            modelBuilder.Configurations.Add(new PayrollSalaryMap());
            modelBuilder.Configurations.Add(new JobCatalogMap());
            modelBuilder.Configurations.Add(new IncidentMap());
            modelBuilder.Configurations.Add(new SubordinateMap());
            modelBuilder.Configurations.Add(new BiweeklyPaymentMap());
            modelBuilder.Configurations.Add(new BiweeklyPaymentFileMap());
            modelBuilder.Configurations.Add(new MinimumWagesCatalogMap());
            modelBuilder.Configurations.Add(new BenefitMap());
            modelBuilder.Configurations.Add(new BonusMap());
            modelBuilder.Configurations.Add(new BonusAmountMap());
            modelBuilder.Configurations.Add(new BonusApplicationMap());
            modelBuilder.Configurations.Add(new TakenVacationDayMap());
            modelBuilder.Configurations.Add(new PayrollEmployeeJobMap());
            modelBuilder.Configurations.Add(new AssistanceOverrideMap());

            // NEW
            modelBuilder.Configurations.Add(new OperationalIncidentMap());

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
            //DropTable<PayrollEmployee>();
            //DropTable<PayrollEmployeeFile>();
            //DropTable<PayrollSalary>();
            //DropTable<Incident>();
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
