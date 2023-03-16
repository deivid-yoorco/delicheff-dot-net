using Nop.Core;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Teed.Plugin.Groceries.Data.Mapping;

namespace Teed.Plugin.Groceries.Data
{
    public class PluginObjectContext : DbContext, IDbContext
    {
        public PluginObjectContext(string nameOrConnectionString) : base(nameOrConnectionString) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new WebScrapingUnitProductMap());
            modelBuilder.Configurations.Add(new PostalCodesMap());
            modelBuilder.Configurations.Add(new ShippingAreaMap());
            modelBuilder.Configurations.Add(new OrderReportLogMap());
            modelBuilder.Configurations.Add(new OrderReportMap());
            modelBuilder.Configurations.Add(new OrderReportFileMap());
            modelBuilder.Configurations.Add(new ShippingRouteMap());
            modelBuilder.Configurations.Add(new ShippingRouteUserMap());
            modelBuilder.Configurations.Add(new AditionalCostMap());
            modelBuilder.Configurations.Add(new PostalCodeSearchMap());
            modelBuilder.Configurations.Add(new OrderItemBuyerMap());
            modelBuilder.Configurations.Add(new OrderReportStatusMap());
            modelBuilder.Configurations.Add(new ShippingZoneMap());
            modelBuilder.Configurations.Add(new OrderTypeMap());
            modelBuilder.Configurations.Add(new NotDeliveredOrderItemMap());
            modelBuilder.Configurations.Add(new OrderItemLogMap());
            modelBuilder.Configurations.Add(new TipsWithCardMap());
            modelBuilder.Configurations.Add(new OrderReportTransferMap());
            modelBuilder.Configurations.Add(new BuyerListDownloadMap());
            modelBuilder.Configurations.Add(new RescheduledOrderLogMap());
            modelBuilder.Configurations.Add(new SupermarketBuyerMap());
            modelBuilder.Configurations.Add(new ManagerExpensesMap());
            modelBuilder.Configurations.Add(new ManagerExpensesStatusMap());
            modelBuilder.Configurations.Add(new ManagerQuantitiesMap());
            modelBuilder.Configurations.Add(new MarkedBuyerItemMap());
            modelBuilder.Configurations.Add(new HeatMapDataMap());
            modelBuilder.Configurations.Add(new ShippingRegionMap());
            modelBuilder.Configurations.Add(new ShippingRegionZoneMap());
            modelBuilder.Configurations.Add(new FranchiseMap());
            modelBuilder.Configurations.Add(new PenaltiesCatalogMap());
            modelBuilder.Configurations.Add(new IncidentMap());
            modelBuilder.Configurations.Add(new IncidentFileMap());
            modelBuilder.Configurations.Add(new BillingMap());
            modelBuilder.Configurations.Add(new PaymentFileMap());
            modelBuilder.Configurations.Add(new ShippingVehicleMap());
            modelBuilder.Configurations.Add(new ShippingVehicleRouteMap());
            modelBuilder.Configurations.Add(new PaymentMap());
            modelBuilder.Configurations.Add(new FranchiseBonusMap());
            modelBuilder.Configurations.Add(new FranchiseMonthlyChargeMap());
            modelBuilder.Configurations.Add(new ManufacturerBuyerMap());
            modelBuilder.Configurations.Add(new ProductMainManufacturerMap());
            modelBuilder.Configurations.Add(new PostalCodeNotificationRequestMap());
            modelBuilder.Configurations.Add(new PenaltiesVariablesMap());
            modelBuilder.Configurations.Add(new ManufacturerListDownloadMap());
            modelBuilder.Configurations.Add(new OrderOptimizationRequestMap());
            modelBuilder.Configurations.Add(new ProductPricePendingUpdateMap());
            modelBuilder.Configurations.Add(new FestiveDateMap());
            modelBuilder.Configurations.Add(new SubstituteBuyerMap());
            modelBuilder.Configurations.Add(new CostsIncreaseWarningMap());
            modelBuilder.Configurations.Add(new RelatedProductGeneratedMap());
            modelBuilder.Configurations.Add(new UrbanPromoterMap());
            modelBuilder.Configurations.Add(new UrbanPromoterCouponMap());
            modelBuilder.Configurations.Add(new UrbanPromoterFeeMap());
            modelBuilder.Configurations.Add(new UrbanPromoterPaymentMap());
            modelBuilder.Configurations.Add(new BuyerPaymentMap());
            modelBuilder.Configurations.Add(new BuyerPaymentTicketFileMap());
            modelBuilder.Configurations.Add(new BuyerPaymentByteFileMap());
            modelBuilder.Configurations.Add(new ManufacturerBankAccountMap());
            modelBuilder.Configurations.Add(new CostsDecreaseWarningMap());
            modelBuilder.Configurations.Add(new CorporateCardMap());
            modelBuilder.Configurations.Add(new CorcelProductMap());
            modelBuilder.Configurations.Add(new CorcelCustomerMap());
            modelBuilder.Configurations.Add(new PrintedCouponBookMap()); // NEW

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
            //DropTable<CustomerSecurityToken>();
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
