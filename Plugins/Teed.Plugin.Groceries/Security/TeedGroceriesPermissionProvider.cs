using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;
using System.Linq;

namespace Teed.Plugin.Groceries.Security
{
    public partial class TeedGroceriesPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord CategoryPrice = new PermissionRecord
        {
            Name = "CEL - Precios por categoría",
            SystemName = "CategoryPrice",
            Category = "Plugin"
        };

        public static readonly PermissionRecord UpdatePrices = new PermissionRecord
        {
            Name = "CEL - Actualizar precios",
            SystemName = "UpdatePrices",
            Category = "Plugin"
        };

        public static readonly PermissionRecord ShippingArea = new PermissionRecord
        {
            Name = "CEL - Área de cobertura",
            SystemName = "ShippingArea",
            Category = "Plugin"
        };

        public static readonly PermissionRecord ShippingRoute = new PermissionRecord
        {
            Name = "CEL - Rutas de entrega",
            SystemName = "ShippingRoute",
            Category = "Plugin"
        };

        public static readonly PermissionRecord ShippingRouteUser = new PermissionRecord
        {
            Name = "CEL - Responsables rutas de entregas",
            SystemName = "ShippingRouteUser",
            Category = "Plugin"
        };

        public static readonly PermissionRecord Warnings = new PermissionRecord
        {
            Name = "CEL - Advertencias",
            SystemName = "Warnings",
            Category = "Plugin"
        };

        public static readonly PermissionRecord ShippingAreaPostalCodes = new PermissionRecord
        {
            Name = "CEL - Búsqueda de códigos postales",
            SystemName = "ShippingAreaPostalCodes",
            Category = "Plugin"
        };

        public static readonly PermissionRecord OrderItemBuyer = new PermissionRecord
        {
            Name = "CEL - Asignar compradores",
            SystemName = "OrderItemBuyer",
            Category = "Plugin"
        };

        public static readonly PermissionRecord ShippingRouteAssign = new PermissionRecord
        {
            Name = "CEL - Asignar rutas",
            SystemName = "ShippingRouteAssign",
            Category = "Plugin"
        };

        public static readonly PermissionRecord ShippingRouteMonitor = new PermissionRecord
        {
            Name = "CEL - Monitoreo de rutas",
            SystemName = "ShippingRouteMonitor",
            Category = "Plugin"
        };

        public static readonly PermissionRecord ShippingRegion = new PermissionRecord
        {
            Name = "CEL - Regiones",
            SystemName = "ShippingRegion",
            Category = "Plugin"
        };

        public static readonly PermissionRecord ShippingZone = new PermissionRecord
        {
            Name = "CEL - Zonas de entrega",
            SystemName = "ShippingZone",
            Category = "Plugin"
        };

        public static readonly PermissionRecord OrderType = new PermissionRecord
        {
            Name = "CEL - Tipos de pedido",
            SystemName = "OrderType",
            Category = "Plugin"
        };

        public static readonly PermissionRecord OrderDeliveryReports = new PermissionRecord
        {
            Name = "CEL - Reporte de compradores",
            SystemName = "OrderDeliveryReports",
            Category = "Plugin"
        };

        public static readonly PermissionRecord FinalRouteReport = new PermissionRecord
        {
            Name = "CEL - Liquidación de reparto",
            SystemName = "FinalRouteReport",
            Category = "Plugin"
        };

        public static readonly PermissionRecord HeatMap = new PermissionRecord
        {
            Name = "CEL - Mapa de calor",
            SystemName = "HeatMap",
            Category = "Plugin"
        };

        public static readonly PermissionRecord ShippingVehicles = new PermissionRecord
        {
            Name = "CEL - Vehiculos",
            SystemName = "ShippingVehicle",
            Category = "Plugin"
        };

        public static readonly PermissionRecord RouteRescue = new PermissionRecord
        {
            Name = "CEL - Rescate de ruta",
            SystemName = "RouteRescue",
            Category = "Plugin"
        };

        public static readonly PermissionRecord Franchise = new PermissionRecord
        {
            Name = "CEL - Administrar franquicias",
            SystemName = "Franchise",
            Category = "Plugin"
        };

        public static readonly PermissionRecord ManufacturerBuyer = new PermissionRecord
        {
            Name = "CEL - Asignar compradores de fabricantes",
            SystemName = "ManufacturerBuyer",
            Category = "Plugin"
        };

        public static readonly PermissionRecord DashboardHq = new PermissionRecord
        {
            Name = "CEL - Dashboard HQ",
            SystemName = "DashboardHq",
            Category = "Plugin"
        };

        public static readonly PermissionRecord DashboardHqConfig = new PermissionRecord
        {
            Name = "CEL - Configuración Dashboard HQ",
            SystemName = "DashboardHqConfig",
            Category = "Plugin"
        };

        public static readonly PermissionRecord DiscountsBreakdown = new PermissionRecord
        {
            Name = "CEL - Desglose de descuentos",
            SystemName = "DiscountsBreakdown",
            Category = "Plugin"
        };

        public static readonly PermissionRecord OperationalErrors = new PermissionRecord
        {
            Name = "CEL - Errores Operativos",
            SystemName = "OperationalErrors",
            Category = "Plugin"
        };

        public static readonly PermissionRecord PANDL = new PermissionRecord
        {
            Name = "CEL - P&L",
            SystemName = "PANDL",
            Category = "Plugin"
        };
        public static readonly PermissionRecord NewsLetter = new PermissionRecord
        {
            Name = "CEL - Exportar datos del cliente",
            SystemName = "NewsLetter",
            Category = "Plugin"
        };

        public static readonly PermissionRecord OptimizationRequest = new PermissionRecord
        {
            Name = "CEL - Solicitud de optimización",
            SystemName = "OptimizationRequest",
            Category = "Plugin"
        };

        public static readonly PermissionRecord OptimizationRequestManager = new PermissionRecord
        {
            Name = "CEL - Coordinación de solicitudes de optimización",
            SystemName = "OptimizationRequestManager",
            Category = "Plugin"
        };

        public static readonly PermissionRecord UpdateCost = new PermissionRecord
        {
            Name = "CEL - Módulo de actualización de costos",
            SystemName = "UpdateCost",
            Category = "Plugin"
        };

        public static readonly PermissionRecord FestiveDates = new PermissionRecord
        {
            Name = "CEL - Aplicación de días festivos",
            SystemName = "FestiveDates",
            Category = "Plugin"
        };

        public static readonly PermissionRecord AssignLimitedRoles = new PermissionRecord
        {
            Name = "CEL - Asignar perfil operativo",
            SystemName = "AssignLimitedRoles",
            Category = "Plugin"
        };

        public static readonly PermissionRecord AssignSubstituteBuyers = new PermissionRecord
        {
            Name = "CEL - Asignar comprador suplente",
            SystemName = "AssignSubstituteBuyers",
            Category = "Plugin"
        };

        public static readonly PermissionRecord UrbanPromoters = new PermissionRecord
        {
            Name = "CEL - Promotores urbanos",
            SystemName = "UrbanPromoters",
            Category = "Plugin"
        };

        public static readonly PermissionRecord BuyerReportPrint = new PermissionRecord
        {
            Name = "CEL - Impresiones compras",
            SystemName = "BuyerReportPrint",
            Category = "Plugin"
        };

        public static readonly PermissionRecord DeliveryReportPrint = new PermissionRecord
        {
            Name = "CEL - Impresiones logística",
            SystemName = "DeliveryReportPrint",
            Category = "Plugin"
        };

        public static readonly PermissionRecord BuyerPayment = new PermissionRecord
        {
            Name = "CEL - Solicitudes de pago",
            SystemName = "BuyerPayment",
            Category = "Plugin"
        };

        public static readonly PermissionRecord CorporateCard = new PermissionRecord
        {
            Name = "CEL - Tarjetas corporativas",
            SystemName = "CorporateCard",
            Category = "Plugin"
        };

        public static readonly PermissionRecord CorcelProduct = new PermissionRecord
        {
            Name = "CORCEL - Productos",
            SystemName = "CorcelProducts",
            Category = "Plugin"
        };

        public static readonly PermissionRecord CorcelCustomer = new PermissionRecord
        {
            Name = "CORCEL - Clientes",
            SystemName = "CorcelCustomer",
            Category = "Plugin"
        };

        public static readonly PermissionRecord PrintedCouponBook = new PermissionRecord
        {
            Name = "CEL - Cuponeras impresas",
            SystemName = "PrintedCouponBook",
            Category = "Plugin"
        };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                CategoryPrice,
                UpdatePrices,
                ShippingArea,
                ShippingRoute,
                ShippingRouteUser,
                Warnings,
                ShippingAreaPostalCodes,
                OrderItemBuyer,
                ShippingRouteAssign,
                ShippingRouteMonitor,
                ShippingZone,
                ShippingRegion,
                OrderType,
                OrderDeliveryReports,
                HeatMap,
                ShippingVehicles,
                RouteRescue,
                Franchise,
                ManufacturerBuyer,
                DashboardHq,
                DashboardHqConfig,
                DiscountsBreakdown,
                OperationalErrors,
                PANDL,
                OptimizationRequest,
                OptimizationRequestManager,
                NewsLetter,
                UpdateCost,
                FestiveDates,
                AssignLimitedRoles,
                AssignSubstituteBuyers,
                UrbanPromoters,
                BuyerReportPrint,
                DeliveryReportPrint,
                BuyerPayment,
                CorporateCard,
                FinalRouteReport,
                CorcelProduct,
                CorcelCustomer,
                PrintedCouponBook
            };
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return Enumerable.Empty<DefaultPermissionRecord>();
        }
    }
}