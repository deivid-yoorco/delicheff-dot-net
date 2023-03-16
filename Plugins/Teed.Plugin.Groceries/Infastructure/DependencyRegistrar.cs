using Autofac;
using Autofac.Core;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Web.Framework.Infrastructure;
using Teed.Plugin.Groceries.Data;
using Teed.Plugin.Groceries.Domain.HeatMaps;
using Teed.Plugin.Groceries.Domain.Franchise;
using Teed.Plugin.Groceries.Domain.MarkedBuyerItems;
using Teed.Plugin.Groceries.Domain.OrderItemBuyers;
using Teed.Plugin.Groceries.Domain.OrderReports;
using Teed.Plugin.Groceries.Domain.PenaltiesCatalog;
using Teed.Plugin.Groceries.Domain.RescheduledOrderLogs;
using Teed.Plugin.Groceries.Domain.ShippingAreas;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;
using Teed.Plugin.Groceries.Domain.ShippingZones;
using Teed.Plugin.Groceries.Domain.ShippingRegions;
using Teed.Plugin.Groceries.Domain.TipsWithCards;
using Teed.Plugin.Groceries.Domain.WebScrapingUnitProduct;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Domain.ShippingVehicles;
using Teed.Plugin.Groceries.Domain.Product;
using Teed.Plugin.Groceries.Domain.FestiveDates;
using Teed.Plugin.Groceries.Domain.SubstituteBuyers;
using Teed.Plugin.Groceries.Domain.UrbanPromoters;
using Teed.Plugin.Groceries.Domain.BuyerPayments;
using Teed.Plugin.Groceries.Domain.Manufacturer;
using Teed.Plugin.Groceries.Domain.Corcel;
using Teed.Plugin.Groceries.Domain.PrintedCouponBooks;

namespace Teed.Plugin.Groceries.Infastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_groceriesplugin";
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<WebScrapingUnitProductService>().InstancePerLifetimeScope();
            builder.RegisterType<ShippingAreaService>().InstancePerLifetimeScope();
            builder.RegisterType<PostalCodesService>().InstancePerLifetimeScope();
            builder.RegisterType<OrderReportService>().InstancePerLifetimeScope();
            builder.RegisterType<OrderReportLogService>().InstancePerLifetimeScope();
            builder.RegisterType<OrderReportFileService>().InstancePerLifetimeScope();
            builder.RegisterType<ShippingRouteService>().InstancePerLifetimeScope();
            builder.RegisterType<ShippingRouteUserService>().InstancePerLifetimeScope();
            builder.RegisterType<AditionalCostService>().InstancePerLifetimeScope();
            builder.RegisterType<PostalCodeSearchService>().InstancePerLifetimeScope();
            builder.RegisterType<OrderItemBuyerService>().InstancePerLifetimeScope();
            builder.RegisterType<OrderReportStatusService>().InstancePerLifetimeScope();
            builder.RegisterType<ShippingZoneService>().InstancePerLifetimeScope();
            builder.RegisterType<OrderTypeService>().InstancePerLifetimeScope();
            builder.RegisterType<NotDeliveredOrderItemService>().InstancePerLifetimeScope();
            builder.RegisterType<OrderItemLogService>().InstancePerLifetimeScope();
            builder.RegisterType<TipsWithCardService>().InstancePerLifetimeScope();
            builder.RegisterType<OrderReportTransferService>().InstancePerLifetimeScope();
            builder.RegisterType<BuyerListDownloadService>().InstancePerLifetimeScope();
            builder.RegisterType<RescheduledOrderLogService>().InstancePerLifetimeScope();
            builder.RegisterType<SupermarketBuyerService>().InstancePerLifetimeScope();
            builder.RegisterType<MarkedBuyerItemService>().InstancePerLifetimeScope();
            builder.RegisterType<ManagerExpensesService>().InstancePerLifetimeScope();
            builder.RegisterType<ManagerExpensesStatusService>().InstancePerLifetimeScope();
            builder.RegisterType<ManagerQuantitiesService>().InstancePerLifetimeScope();
            builder.RegisterType<HeatMapDataService>().InstancePerLifetimeScope();
            builder.RegisterType<HeatMapDataService>().InstancePerLifetimeScope();
            builder.RegisterType<ShippingRegionService>().InstancePerLifetimeScope();
            builder.RegisterType<ShippingRegionZoneService>().InstancePerLifetimeScope();
            builder.RegisterType<FranchiseService>().InstancePerLifetimeScope();
            builder.RegisterType<PenaltiesCatalogService>().InstancePerLifetimeScope();
            builder.RegisterType<IncidentsService>().InstancePerLifetimeScope();
            builder.RegisterType<IncidentFileService>().InstancePerLifetimeScope();
            builder.RegisterType<BillingService>().InstancePerLifetimeScope();
            builder.RegisterType<PaymentFileService>().InstancePerLifetimeScope();
            builder.RegisterType<ShippingVehicleService>().InstancePerLifetimeScope();
            builder.RegisterType<ShippingVehicleRouteService>().InstancePerLifetimeScope();
            builder.RegisterType<PaymentService>().InstancePerLifetimeScope();
            builder.RegisterType<FranchiseBonusService>().InstancePerLifetimeScope();
            builder.RegisterType<FranchiseMonthlyChargeService>().InstancePerLifetimeScope();
            builder.RegisterType<ManufacturerBuyerService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductMainManufacturerService>().InstancePerLifetimeScope();
            builder.RegisterType<PostalCodeNotificationRequestService>().InstancePerLifetimeScope();
            builder.RegisterType<PenaltiesVariablesService>().InstancePerLifetimeScope();
            builder.RegisterType<ManufacturerListDownloadService>().InstancePerLifetimeScope();
            builder.RegisterType<OrderOptimizationRequestService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductPricePendingUpdateService>().InstancePerLifetimeScope();
            builder.RegisterType<FestiveDateService>().InstancePerLifetimeScope();
            builder.RegisterType<SubstituteBuyerService>().InstancePerLifetimeScope();
            builder.RegisterType<CostsIncreaseWarningService>().InstancePerLifetimeScope();
            builder.RegisterType<RelatedProductGeneratedService>().InstancePerLifetimeScope();
            builder.RegisterType<UrbanPromoterService>().InstancePerLifetimeScope();
            builder.RegisterType<UrbanPromoterCouponService>().InstancePerLifetimeScope();
            builder.RegisterType<UrbanPromoterFeeService>().InstancePerLifetimeScope();
            builder.RegisterType<UrbanPromoterPaymentService>().InstancePerLifetimeScope();
            builder.RegisterType<BuyerPaymentService>().InstancePerLifetimeScope();
            builder.RegisterType<BuyerPaymentTicketFileService>().InstancePerLifetimeScope();
            builder.RegisterType<BuyerPaymentByteFileService>().InstancePerLifetimeScope();
            builder.RegisterType<ManufacturerBankAccountService>().InstancePerLifetimeScope();
            builder.RegisterType<CostsDecreaseWarningService>().InstancePerLifetimeScope();
            builder.RegisterType<CorporateCardService>().InstancePerLifetimeScope();
            builder.RegisterType<CorcelProductService>().InstancePerLifetimeScope();
            builder.RegisterType<CorcelCustomerService>().InstancePerLifetimeScope();
            builder.RegisterType<PrintedCouponBooksService>().InstancePerLifetimeScope(); // NEW

            this.RegisterPluginDataContext<PluginObjectContext>(builder, CONTEXT_NAME);

            Register<WebScrapingUnitProduct>(builder);
            Register<ShippingArea>(builder);
            Register<PostalCodes>(builder);
            Register<OrderReport>(builder);
            Register<OrderReportLog>(builder);
            Register<OrderReportFile>(builder);
            Register<ShippingRouteUser>(builder);
            Register<ShippingRoute>(builder);
            Register<AditionalCost>(builder);
            Register<PostalCodeSearch>(builder);
            Register<OrderItemBuyer>(builder);
            Register<OrderReportStatus>(builder);
            Register<ShippingZoneUser>(builder);
            Register<ShippingZone>(builder);
            Register<OrderType>(builder);
            Register<NotDeliveredOrderItem>(builder);
            Register<OrderItemLog>(builder);
            Register<TipsWithCard>(builder);
            Register<OrderReportTransfer>(builder);
            Register<BuyerListDownload>(builder);
            Register<RescheduledOrderLog>(builder); 
            Register<SupermarketBuyer>(builder);
            Register<ManagerExpense>(builder);
            Register<ManagerExpensesStatus>(builder);
            Register<ManagerQuantities>(builder);
            Register<MarkedBuyerItem>(builder);
            Register<HeatMapData>(builder);
            Register<ShippingRegion>(builder);
            Register<ShippingRegionZone>(builder);
            Register<Franchise>(builder);
            Register<PenaltiesCatalog>(builder);
            Register<Incidents>(builder);
            Register<IncidentFile>(builder);
            Register<Billing>(builder);
            Register<PaymentFile>(builder);
            Register<Payment>(builder);
            Register<ShippingVehicle>(builder);
            Register<ShippingVehicleRoute>(builder);
            Register<FranchiseBonus>(builder);
            Register<FranchiseMonthlyCharge>(builder);
            Register<ManufacturerBuyer>(builder);
            Register<ProductMainManufacturer>(builder);
            Register<PostalCodeNotificationRequest>(builder);
            Register<PenaltiesVariables>(builder);
            Register<ManufacturerListDownload>(builder);
            Register<OrderOptimizationRequest>(builder);
            Register<ProductPricePendingUpdate>(builder);
            Register<FestiveDate>(builder);
            Register<SubstituteBuyer>(builder);
            Register<CostsIncreaseWarning>(builder);
            Register<RelatedProductGenerated>(builder);
            Register<UrbanPromoter>(builder);
            Register<UrbanPromoterCoupon>(builder);
            Register<UrbanPromoterFee>(builder);
            Register<UrbanPromoterPayment>(builder);
            Register<BuyerPayment>(builder);
            Register<BuyerPaymentTicketFile>(builder);
            Register<BuyerPaymentByteFile>(builder);
            Register<ManufacturerBankAccount>(builder);
            Register<CostsDecreaseWarning>(builder);
            Register<CorporateCard>(builder);
            Register<CorcelProduct>(builder);
            Register<CorcelCustomer>(builder);
            Register<PrintedCouponBook>(builder); // NEW
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
