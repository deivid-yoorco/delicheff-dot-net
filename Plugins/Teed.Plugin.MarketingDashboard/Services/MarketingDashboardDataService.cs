using Nop.Core.Data;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Rewards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.MarketingDashboard.Domain.MarketingExpenses;
using Teed.Plugin.MarketingDashboard.Models.MarketingExpenses;
using Teed.Plugin.MarketingDashboard.Utils;

namespace Teed.Plugin.MarketingDashboard.Services
{
    public class MarketingDashboardDataService
    {
        private readonly IRepository<MarketingDashboardData> _db;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICustomerService _customerService;
        private readonly MarketingExpenseService _marketingExpenseService;
        private readonly MarketingGrossMarginService _marketingGrossMarginService;
        private readonly MarketingRetentionRateService _marketingRetentionRateService;
        private readonly ICustomerBalanceService _customerBalanceService;
        private readonly IOrderService _orderService;
        private readonly MarketingAutomaticExpenseService _marketingAutomaticExpense;
        private readonly MarketingDiscountExpenseService _marketingDiscountExpenseService;
        private readonly IDiscountService _discountService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger _logger;

        public MarketingDashboardDataService(
            IRepository<MarketingDashboardData> db,
            IEventPublisher eventPublisher,
            ICustomerService customerService,
            MarketingExpenseService marketingExpenseService,
            MarketingGrossMarginService marketingGrossMarginService,
            MarketingRetentionRateService marketingRetentionRateService,
            ICustomerBalanceService customerBalanceService,
            IOrderService orderService,
            MarketingAutomaticExpenseService marketingAutomaticExpense,
            MarketingDiscountExpenseService marketingDiscountExpenseService,
            IDiscountService discountService,
            IProductService productService,
            ICategoryService categoryService,
            ILogger logger)
        {
            _db = db;
            _eventPublisher = eventPublisher;
            _marketingExpenseService = marketingExpenseService;
            _customerService = customerService;
            _marketingGrossMarginService = marketingGrossMarginService;
            _marketingRetentionRateService = marketingRetentionRateService;
            _customerBalanceService = customerBalanceService;
            _orderService = orderService;
            _marketingAutomaticExpense = marketingAutomaticExpense;
            _marketingDiscountExpenseService = marketingDiscountExpenseService;
            _discountService = discountService;
            _productService = productService;
            _categoryService = categoryService;
            _logger = logger;
        }

        public IQueryable<MarketingDashboardData> GetAll(bool includeDeleted = false)
        {
            if (includeDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public List<DateTime> GetInitDates()
        {
            return GetAll()
                .Where(x => x.MarketingDashboardDataTypeId == 10)
                .GroupBy(x => x.GenerationDateUtc)
                .OrderByDescending(x => x.Key)
                .FirstOrDefault()
                .Select(x => x.InitDate)
                .GroupBy(x => x)
                .Select(x => x.Key)
                .OrderBy(x => x)
                .ToList();
        }

        public void Insert(MarketingDashboardData entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(MarketingDashboardData entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(MarketingDashboardData entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.Deleted = true;
            Update(entity);

            //event notification
            _eventPublisher.EntityDeleted(entity);
        }

        /// <summary>
        /// Base data and Weekly data 120 days
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="controlDateUtc"></param>
        /// <param name="versionCode">0 = CEL; 1 = OTHER</param>
        public void GenerateDashboardData10(List<Order> orders, DateTime controlDateUtc, int versionCode)
        {
            var errors = new List<string>();
            List<IGrouping<DateTime, Order>> groupedByWeek = null;
            if (versionCode == 0)
            {
                groupedByWeek = orders
               .OrderBy(x => x.SelectedShippingDate)
               .ToList()
               .GroupBy(x => x.SelectedShippingDate.Value.AddDays(-(int)x.SelectedShippingDate.Value.DayOfWeek).AddDays(1))
               .OrderByDescending(x => x.Key)
               .ToList();
            }
            else
            {
                groupedByWeek = orders
               .OrderBy(x => x.CreatedOnUtc)
               .ToList()
               .GroupBy(x => x.CreatedOnUtc.ToLocalTime().AddDays(-(int)x.CreatedOnUtc.DayOfWeek).AddDays(1).Date)
               .OrderByDescending(x => x.Key)
               .ToList();
            }

            var allMarketingExpenses = _marketingExpenseService.GetAll().ToList();
            var grossMargin = _marketingGrossMarginService.GetAll().ToList();
            var retentionRate = _marketingRetentionRateService.GetAll().ToList();

            var currentDate = groupedByWeek.FirstOrDefault().Key;
            var dataToUpload = new List<MarketingDashboardData>();

            foreach (var group in groupedByWeek)
            {
                if (errors.Any()) continue;

                try
                {
                    currentDate = group.Key;
                    var initDate = group.Key;
                    var endDate = group.Key.AddDays(6);

                    if (endDate >= DateTime.Now.Date) continue;

                    var controlDate30DaysFromEnd = endDate.AddDays(-30);
                    var controlDate60DaysFromEnd = endDate.AddDays(-60);
                    var controlDate90DaysFromEnd = endDate.AddDays(-90);
                    var controlDate120DaysFromEnd = endDate.AddDays(-120);
                    var controlDate121DaysFromEnd = endDate.AddDays(-121);
                    var controlDate150DaysFromEnd = endDate.AddDays(-150);

                    //Gasto publicitario del periodo
                    decimal marketingExpenses = _marketingExpenseService.GetDailyExpense(allMarketingExpenses, endDate)
                        .Where(x => x.Date >= initDate && x.Date <= endDate)
                        .Select(x => x.Amount)
                        .DefaultIfEmpty()
                        .Sum();

                    decimal automaticExpenses = _marketingAutomaticExpense.GetAll()
                        .Where(x => initDate == x.InitDate)
                        .Select(x => x.DiscountByCoupons + x.DiscountByProducts + x.DiscountByShipping + x.Balances)
                        .DefaultIfEmpty()
                        .Sum();

                    marketingExpenses += automaticExpenses;

                    var registeredUsers = _customerService.GetAllCustomersQuery()
                    .Where(x => x.Email != "" && x.Email != null && x.CreatedOnUtc >= initDate && x.CreatedOnUtc <= endDate).ToList();

                    //Nuevos registros en el periodo
                    int newRegisteredUsersCount = registeredUsers.Count;

                    //Nuevas cuentas activas (al menos han hecho un pedido en la vida) del periodo
                    int newActiveCount = 0;
                    foreach (var customer in registeredUsers)
                    {
                        if (group.Where(x => x.CustomerId == customer.Id).Any())
                            newActiveCount++;
                    }

                    //Total de pedidos del periodo
                    int pedidosCount = versionCode == 0 ? OrderUtils.GetPedidosGroupByList(group.Select(x => x).ToList()).Count() : group.Select(x => x).Count();

                    //Total de ventas del periodo
                    decimal salesTotal = group.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();

                    //Número de días trabajados en el periodo
                    int workingDays = 0;
                    for (DateTime i = initDate; i <= endDate; i = i.AddDays(1))
                    {
                        if (versionCode == 0)
                        {
                            if (group.Where(x => x.SelectedShippingDate == i).Any())
                                workingDays++;
                        }
                        else if (versionCode == 1)
                        {
                            if (group.Where(x => x.CreatedOnUtc.ToLocalTime().Date == i).Any())
                                workingDays++;
                        }
                    }

                    //Total de pedidos de los últimos 30 días a partir de la fecha final del periodo
                    int totalPedidos30Days = 0;
                    if (versionCode == 0)
                    {
                        totalPedidos30Days = OrderUtils.GetPedidosGroupByList(orders
                                .Where(x => x.SelectedShippingDate >= controlDate30DaysFromEnd && x.SelectedShippingDate <= endDate)
                                .ToList())
                                .Count();
                    }
                    else if (versionCode == 1)
                    {
                        totalPedidos30Days = orders
                                .Where(x => x.CreatedOnUtc.ToLocalTime() >= controlDate30DaysFromEnd && x.CreatedOnUtc.ToLocalTime() <= endDate)
                                .ToList()
                                .Count();
                    }

                    //Número de clientes que hayan hecho al menos un pedido en los últimos 60 días a partir de la fecha final del periodo
                    int customerCount60Days = 0;
                    if (versionCode == 0)
                    {
                        customerCount60Days = orders
                                .Where(x => x.SelectedShippingDate >= controlDate60DaysFromEnd && x.SelectedShippingDate <= endDate)
                                .GroupBy(x => x.CustomerId)
                                .Count();
                    }
                    else if (versionCode == 1)
                    {
                        customerCount60Days = orders
                                .Where(x => x.CreatedOnUtc.ToLocalTime() >= controlDate60DaysFromEnd && x.CreatedOnUtc.ToLocalTime() <= endDate)
                                .GroupBy(x => x.CustomerId)
                                .Count();
                    }

                    //Número de clientes que hayan hecho al menos un pedido en los últimos 30 días a partir de la fecha final del periodo
                    int customerCount30Days = 0;
                    if (versionCode == 0)
                    {
                        customerCount30Days = orders
                                .Where(x => x.SelectedShippingDate >= controlDate30DaysFromEnd && x.SelectedShippingDate <= endDate)
                                .GroupBy(x => x.CustomerId)
                                .Count();
                    }
                    else if (versionCode == 1)
                    {
                        customerCount30Days = orders
                                .Where(x => x.CreatedOnUtc.ToLocalTime() >= controlDate30DaysFromEnd && x.CreatedOnUtc.ToLocalTime() <= endDate)
                                .GroupBy(x => x.CustomerId)
                                .Count();
                    }

                    //Número de clientes que hayan hecho su primer pedido en los últimos 30 días a partir de la fecha final del periodo
                    int firstOrderCount30Days = 0;
                    List<int> customerIdsPeriodOrders = new List<int>();
                    List<Order> filteredOrders = new List<Order>();
                    if (versionCode == 0)
                    {
                        customerIdsPeriodOrders = orders
                        .Where(x => x.SelectedShippingDate >= controlDate30DaysFromEnd && x.SelectedShippingDate <= endDate)
                        .GroupBy(x => x.CustomerId)
                        .Select(x => x.Key)
                        .ToList();
                        filteredOrders = orders.Where(x => x.SelectedShippingDate < controlDate30DaysFromEnd).ToList();
                    }
                    else if (versionCode == 1)
                    {
                        customerIdsPeriodOrders = orders
                        .Where(x => x.CreatedOnUtc.ToLocalTime() >= controlDate30DaysFromEnd && x.CreatedOnUtc.ToLocalTime() <= endDate)
                        .GroupBy(x => x.CustomerId)
                        .Select(x => x.Key)
                        .ToList();
                        filteredOrders = orders.Where(x => x.CreatedOnUtc.ToLocalTime() < controlDate30DaysFromEnd).ToList();

                    }

                    foreach (var customerId in customerIdsPeriodOrders)
                    {
                        if (!filteredOrders.Where(x => x.CustomerId == customerId).Any())
                            firstOrderCount30Days++;
                    }

                    //Número de clientes que hayan hecho al menos un pedido en los últimos 120 días a partir de la fecha final del periodo
                    int customerCount120Days = 0;
                    if (versionCode == 0)
                    {
                        customerCount120Days = orders
                                .Where(x => x.SelectedShippingDate >= controlDate120DaysFromEnd && x.SelectedShippingDate <= endDate)
                                .GroupBy(x => x.CustomerId)
                                .Count();
                    }
                    else if (versionCode == 1)
                    {
                        customerCount120Days = orders
                                .Where(x => x.CreatedOnUtc.ToLocalTime() >= controlDate120DaysFromEnd && x.CreatedOnUtc.ToLocalTime() <= endDate)
                                .GroupBy(x => x.CustomerId)
                                .Count();
                    }


                    //Número de clientes que hayan hecho al menos un pedido en los últimos 90 días a partir de la fecha final del periodo
                    int customerCount90Days = 0;
                    if (versionCode == 0)
                    {
                        customerCount90Days = orders
                                .Where(x => x.SelectedShippingDate >= controlDate90DaysFromEnd && x.SelectedShippingDate <= endDate)
                                .GroupBy(x => x.CustomerId)
                                .Count();
                    }
                    else if (versionCode == 1)
                    {
                        customerCount90Days = orders
                                .Where(x => x.CreatedOnUtc.ToLocalTime() >= controlDate90DaysFromEnd && x.CreatedOnUtc.ToLocalTime() <= endDate)
                                .GroupBy(x => x.CustomerId)
                                .Count();
                    }

                    //Número de clientes que hayan hecho su primer pedido en los últimos 90 días a partir de la fecha final del periodo
                    int firstOrderCount90Days = 0;
                    if (versionCode == 0)
                    {
                        filteredOrders = orders.Where(x => x.SelectedShippingDate < controlDate90DaysFromEnd).ToList();
                        customerIdsPeriodOrders = orders
                            .Where(x => x.SelectedShippingDate >= controlDate90DaysFromEnd && x.SelectedShippingDate <= endDate)
                            .GroupBy(x => x.CustomerId)
                            .Select(x => x.Key)
                            .ToList();
                    }
                    else if (versionCode == 1)
                    {
                        filteredOrders = orders.Where(x => x.CreatedOnUtc.ToLocalTime() < controlDate90DaysFromEnd).ToList();
                        customerIdsPeriodOrders = orders
                            .Where(x => x.CreatedOnUtc.ToLocalTime() >= controlDate90DaysFromEnd && x.CreatedOnUtc.ToLocalTime() <= endDate)
                            .GroupBy(x => x.CustomerId)
                            .Select(x => x.Key)
                            .ToList();
                    }

                    foreach (var customerId in customerIdsPeriodOrders)
                    {
                        if (!filteredOrders.Where(x => x.CustomerId == customerId).Any())
                            firstOrderCount90Days++;
                    }

                    List<int> customersWithAtLeastOneOrderMoreThan120Days = new List<int>();
                    List<int> customersWithAtLeastOneOrder120Days = new List<int>();
                    List<IGrouping<dynamic, IGrouping<dynamic, Order>>> groupedPedidos = null;
                    List<IGrouping<int, Order>> groupedOrders = null;

                    if (versionCode == 0)
                    {
                        customersWithAtLeastOneOrderMoreThan120Days = orders.Where(x => x.SelectedShippingDate < controlDate120DaysFromEnd)
                        .GroupBy(x => x.CustomerId)
                        .Select(x => x.Key)
                        .ToList();
                        var orders120Days = orders.Where(x => x.SelectedShippingDate >= controlDate120DaysFromEnd && x.SelectedShippingDate <= endDate).ToList();

                        groupedPedidos = OrderUtils.GetPedidosGroupByList(orders120Days)
                            .GroupBy(x => x.Key.CustomerId).ToList();

                        customersWithAtLeastOneOrder120Days = orders.Where(x => x.SelectedShippingDate >= controlDate120DaysFromEnd && x.SelectedShippingDate <= endDate)
                            .GroupBy(x => x.CustomerId)
                            .Select(x => x.Key)
                            .ToList();
                    }
                    else if (versionCode == 1)
                    {
                        customersWithAtLeastOneOrderMoreThan120Days = orders.Where(x => x.CreatedOnUtc.ToLocalTime() < controlDate120DaysFromEnd)
                        .GroupBy(x => x.CustomerId)
                        .Select(x => x.Key)
                        .ToList();
                        var orders120Days = orders.Where(x => x.CreatedOnUtc.ToLocalTime() >= controlDate120DaysFromEnd && x.CreatedOnUtc.ToLocalTime() <= endDate).ToList();

                        groupedOrders = orders120Days.GroupBy(x => x.CustomerId).ToList();

                        customersWithAtLeastOneOrder120Days = orders.Where(x => x.CreatedOnUtc.ToLocalTime() >= controlDate120DaysFromEnd && x.CreatedOnUtc.ToLocalTime() <= endDate)
                            .GroupBy(x => x.CustomerId)
                            .Select(x => x.Key)
                            .ToList();
                    }


                    //Número de clientes que hicieron su primer pedido hace más de 120 días y que hicieron al menos 1 pedido en los últimos 120 días a partir de la fecha final del periodo
                    int customersCountAtLeastOneOrder120DaysAndMore = customersWithAtLeastOneOrderMoreThan120Days.Where(x => customersWithAtLeastOneOrder120Days.Contains(x)).Count();

                    //Número de clientes que hicieron su primer pedido hace más de 120 días y que hicieron únicamente 1 pedido en los últimos 120 días a partir de la fecha final del periodo
                    dynamic customersWithOnlyOnePedido120Days = null;
                    if (versionCode == 0)
                    {
                        customersWithOnlyOnePedido120Days = groupedPedidos
                           .Where(x => x.Count() == 1)
                           .Select(x => x.Key)
                           .ToList();
                    }
                    else if (versionCode == 1)
                    {
                        customersWithOnlyOnePedido120Days = groupedOrders
                           .Where(x => x.Count() == 1)
                           .Select(x => x.Key)
                           .ToList();
                    }
                    int customersCountOnlyOnePedido120DaysAndMore = customersWithAtLeastOneOrderMoreThan120Days.Where(x => customersWithOnlyOnePedido120Days.Contains(x)).Count();

                    //Número de clientes que hicieron su primer pedido hace más de 120 días y que hicieron 2 ó 3 pedidos en los últimos 120 días a partir de la fecha final del periodo
                    dynamic customersWith2or3Pedidos120Days = null;
                    if (versionCode == 0)
                    {
                        customersWith2or3Pedidos120Days = groupedPedidos
                       .Where(x => x.Count() >= 2 && x.Count() <= 3)
                       .Select(x => x.Key)
                       .ToList();
                    }
                    else if (versionCode == 1)
                    {
                        customersWith2or3Pedidos120Days = groupedOrders
                       .Where(x => x.Count() >= 2 && x.Count() <= 3)
                       .Select(x => x.Key)
                       .ToList();
                    }
                    int customersCount2or3Pedidos120DaysAndMore = customersWithAtLeastOneOrderMoreThan120Days.Where(x => customersWith2or3Pedidos120Days.Contains(x)).Count();

                    //Número de clientes que hicieron su primer pedido hace más de 120 días y que hicieron 4 ó 5 pedidos en los últimos 120 días a partir de la fecha final del periodo
                    dynamic customersWith4or5Pedidos120Days = null;
                    if (versionCode == 0)
                    {
                        customersWith4or5Pedidos120Days = groupedPedidos
                        .Where(x => x.Count() >= 4 && x.Count() <= 5)
                        .Select(x => x.Key)
                        .ToList();
                    }
                    else if (versionCode == 1)
                    {
                        customersWith4or5Pedidos120Days = groupedOrders
                        .Where(x => x.Count() >= 4 && x.Count() <= 5)
                        .Select(x => x.Key)
                        .ToList();
                    }
                    int customersCount4or5Pedidos120DaysAndMore = customersWithAtLeastOneOrderMoreThan120Days.Where(x => customersWith4or5Pedidos120Days.Contains(x)).Count();

                    //Número de clientes que hicieron su primer pedido hace más de 120 días y que hicieron 6 ó 7 pedidos en los últimos 120 días a partir de la fecha final del periodo
                    dynamic customersWith6or7Pedidos120Days = null;
                    if (versionCode == 0)
                    {
                        customersWith6or7Pedidos120Days = groupedPedidos
                        .Where(x => x.Count() >= 6 && x.Count() <= 7)
                        .Select(x => x.Key)
                        .ToList();
                    }
                    else if (versionCode == 1)
                    {
                        customersWith6or7Pedidos120Days = groupedOrders
                        .Where(x => x.Count() >= 6 && x.Count() <= 7)
                        .Select(x => x.Key)
                        .ToList();
                    }
                    int customersCount6or7Pedidos120DaysAndMore = customersWithAtLeastOneOrderMoreThan120Days.Where(x => customersWith6or7Pedidos120Days.Contains(x)).Count();

                    //Número de clientes que hicieron su primer pedido hace más de 120 días y que hicieron 8 ó 9 pedidos en los últimos 120 días a partir de la fecha final del periodo
                    dynamic customersWith8or9Pedidos120Days = null;
                    if (versionCode == 0)
                    {
                        customersWith8or9Pedidos120Days = groupedPedidos
                        .Where(x => x.Count() >= 8 && x.Count() <= 9)
                        .Select(x => x.Key)
                        .ToList();
                    }
                    else if (versionCode == 1)
                    {
                        customersWith8or9Pedidos120Days = groupedOrders
                        .Where(x => x.Count() >= 8 && x.Count() <= 9)
                        .Select(x => x.Key)
                        .ToList();
                    }
                    int customersCount8or9Pedidos120DaysAndMore = customersWithAtLeastOneOrderMoreThan120Days.Where(x => customersWith8or9Pedidos120Days.Contains(x)).Count();

                    //Número de clientes que hicieron su primer pedido hace más de 120 días y que hicieron 10 ó más pedidos en los últimos 120 días a partir de la fecha final del periodo
                    dynamic customersWithMoreThan10Pedidos120Days = null;
                    if (versionCode == 0)
                    {
                        customersWithMoreThan10Pedidos120Days = groupedPedidos
                        .Where(x => x.Count() >= 10)
                        .Select(x => x.Key)
                        .ToList();
                    }
                    else if (versionCode == 1)
                    {
                        customersWithMoreThan10Pedidos120Days = groupedOrders
                        .Where(x => x.Count() >= 10)
                        .Select(x => x.Key)
                        .ToList();
                    }
                    int customersCountMoreThan10Pedidos120DaysAndMore = customersWithAtLeastOneOrderMoreThan120Days.Where(x => customersWithMoreThan10Pedidos120Days.Contains(x)).Count();

                    List<int> customerIdsWithOneOrderBetween150and121days = new List<int>();
                    if (versionCode == 0)
                    {
                        customerIdsWithOneOrderBetween150and121days = orders
                        .Where(x => x.SelectedShippingDate >= controlDate150DaysFromEnd && x.SelectedShippingDate <= controlDate121DaysFromEnd)
                        .GroupBy(x => x.CustomerId)
                        .Select(x => x.Key)
                        .ToList();
                    }
                    else if (versionCode == 1)
                    {
                        customerIdsWithOneOrderBetween150and121days = orders
                        .Where(x => x.CreatedOnUtc.ToLocalTime() >= controlDate150DaysFromEnd && x.CreatedOnUtc.ToLocalTime() <= controlDate121DaysFromEnd)
                        .GroupBy(x => x.CustomerId)
                        .Select(x => x.Key)
                        .ToList();
                    }

                    //Número de clientes que hicieron al menos un pedido entre 150 y 121 días atrás a partir de la fecha final del periodo
                    int customersWithOneOrderBetween150and121days = customerIdsWithOneOrderBetween150and121days.Count;

                    //Número de clientes que hicieron al menos un pedido entre 150 y 121 días atrás y que hicieron al menos un pedido en los últimos 120 días a partir de la fecha final del periodo 
                    int customersWithOneOrderBetween150and121daysAnd120daysPedido = customerIdsWithOneOrderBetween150and121days.Where(x => customersWithAtLeastOneOrder120Days.Contains(x)).Count();

                    var firstOrders = new List<Order>();
                    var notFirstOrders = new List<Order>();
                    PrepareFirstAndLastOrders(firstOrders, notFirstOrders, orders, initDate, group.GroupBy(x => x.CustomerId).ToList(), versionCode);

                    //Ventas generadas por pedidos dentro del periodo que fueron el primer pedido del cliente
                    var salesFirstOrders = firstOrders.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();

                    //Número de pedidos dentro del periodo que fueron el primer pedido del cliente
                    var firstPedidosCount = versionCode == 0 ? OrderUtils.GetPedidosGroupByList(firstOrders).Count() : firstOrders.Count;

                    //Ventas generadas por pedidos dentro del periodo que fueron hechas por clientes con más de un pedido
                    var salesNotFirstOrders = notFirstOrders.Select(x => x.OrderTotal).DefaultIfEmpty().Sum();

                    //Número de pedidos dentro del periodo que fueron hechos por clientes con más de un pedido
                    var notFirstOrdersCount = versionCode == 0 ? OrderUtils.GetPedidosGroupByList(notFirstOrders).Count() : notFirstOrders.Count;

                    //Costo por nuevo registro
                    decimal newRegisterCost = newRegisteredUsersCount == 0 ? 0 : (decimal)marketingExpenses / (decimal)newRegisteredUsersCount;

                    //CAC (Costo por adquisición de cliente)
                    decimal adquisitionCost = newActiveCount == 0 ? 0 : (decimal)marketingExpenses / (decimal)newActiveCount;

                    //Tasa de registros a compras
                    decimal buyinRegister = newRegisteredUsersCount == 0 ? 0 : (decimal)newActiveCount / (decimal)newRegisteredUsersCount;

                    //Promedio de pedidos diarios
                    decimal dailyPedidosAverage = workingDays == 0 ? 0 : (decimal)pedidosCount / (decimal)workingDays;

                    //Promedio de venta diaria
                    decimal dailySalesAverage = workingDays == 0 ? 0 : (decimal)salesTotal / (decimal)workingDays;

                    //Ticket promedio
                    decimal averageTicket = pedidosCount == 0 ? 0 : (decimal)salesTotal / (decimal)pedidosCount;

                    //Ticket promedio clientes nuevos
                    decimal averageTicketNewCustomers = firstPedidosCount == 0 ? 0 : (decimal)salesFirstOrders / (decimal)firstPedidosCount;

                    //Ticket promedio clientes recurrentes
                    decimal averageTicketOldCustomers = notFirstOrdersCount == 0 ? 0 : (decimal)salesNotFirstOrders / (decimal)notFirstOrdersCount;

                    //Recurrencia mensual
                    decimal recurrence = (customerCount30Days - firstOrderCount30Days) == 0 ? 0 : (decimal)(totalPedidos30Days - firstOrderCount30Days) / (decimal)(customerCount30Days - firstOrderCount30Days);

                    //Recurrencia 120/1
                    decimal recurrence120days = customersCountAtLeastOneOrder120DaysAndMore == 0 ? 0 : (decimal)customersCountOnlyOnePedido120DaysAndMore / (decimal)customersCountAtLeastOneOrder120DaysAndMore;

                    //Recurrencia 120/2-3
                    decimal recurrence120days2or3 = customersCountAtLeastOneOrder120DaysAndMore == 0 ? 0 : (decimal)customersCount2or3Pedidos120DaysAndMore / (decimal)customersCountAtLeastOneOrder120DaysAndMore;

                    //Recurrencia 120/4-5
                    decimal recurrence120days4or5 = customersCountAtLeastOneOrder120DaysAndMore == 0 ? 0 : (decimal)customersCount4or5Pedidos120DaysAndMore / (decimal)customersCountAtLeastOneOrder120DaysAndMore;

                    //Recurrencia 120/6-7
                    decimal recurrence120days6or7 = customersCountAtLeastOneOrder120DaysAndMore == 0 ? 0 : (decimal)customersCount6or7Pedidos120DaysAndMore / (decimal)customersCountAtLeastOneOrder120DaysAndMore;

                    //Recurrencia 120/8-9
                    decimal recurrence120days8or9 = customersCountAtLeastOneOrder120DaysAndMore == 0 ? 0 : (decimal)customersCount8or9Pedidos120DaysAndMore / (decimal)customersCountAtLeastOneOrder120DaysAndMore;

                    //Recurrencia 120/10+
                    decimal recurrence120daysMoreThan10 = customersCountAtLeastOneOrder120DaysAndMore == 0 ? 0 : (decimal)customersCountMoreThan10Pedidos120DaysAndMore / (decimal)customersCountAtLeastOneOrder120DaysAndMore;

                    var currentRetentionRate = retentionRate.Where(x => x.ApplyDate <= endDate)
                        .OrderByDescending(x => x.ApplyDate)
                        .Select(x => x.Value)
                        .DefaultIfEmpty()
                        .FirstOrDefault();

                    //Tasa de retención estabilizada
                    decimal retentionRate120Days = currentRetentionRate;

                    //Churn Rate mensual
                    decimal monthlyChurnRate = customerCount30Days == 0 ? 0 : (decimal)(customerCount60Days - customerCount30Days - firstOrderCount30Days) / (decimal)customerCount30Days;

                    //Churn Rate trimestral
                    decimal quarterlyChurnRate = customerCount90Days == 0 ? 0 : (decimal)(customerCount120Days - customerCount90Days - firstOrderCount90Days) / (decimal)customerCount90Days;

                    //Customer Annual Value
                    decimal customerAnnualValue = averageTicket * recurrence * 12;

                    var currentGrossMargin = grossMargin.Where(x => x.ApplyDate <= endDate)
                        .OrderByDescending(x => x.ApplyDate)
                        .Select(x => x.Value)
                        .DefaultIfEmpty()
                        .FirstOrDefault() / 100;

                    //Costumer Annual Contribution
                    decimal customerAnnualContribution = customerAnnualValue * currentGrossMargin;

                    //Costumer Annual Contribution (mensual) * Tasa de Retención
                    decimal customerAnnualContributionRetention = customerAnnualContribution * retentionRate120Days;

                    //Costumer Annual Contribution (120/1) * Tasa de Retención
                    decimal customerAnnualContribution120Retention = currentGrossMargin * recurrence120days * averageTicket * 3;

                    //Costumer Annual Contribution (120/2-3) * Tasa de Retención
                    decimal customerAnnualContribution120Retention2or3 = currentGrossMargin * recurrence120days2or3 * averageTicket * 3;

                    //Costumer Annual Contribution (120/4-5) * Tasa de Retención
                    decimal customerAnnualContribution120Retention4or5 = currentGrossMargin * recurrence120days4or5 * averageTicket * 3;

                    //Costumer Annual Contribution (120/6-7) * Tasa de Retención
                    decimal customerAnnualContribution120Retention6or7 = currentGrossMargin * recurrence120days6or7 * averageTicket * 3;

                    //Costumer Annual Contribution (120/8-9) * Tasa de Retención
                    decimal customerAnnualContribution120Retention8or9 = currentGrossMargin * recurrence120days8or9 * averageTicket * 3;

                    //Costumer Annual Contribution (120/10+) * Tasa de Retención
                    decimal customerAnnualContribution120RetentionMoreThan10 = currentGrossMargin * recurrence120daysMoreThan10 * averageTicket * 3;

                    //(CAV*Margen Bruto)/CAC
                    decimal customFormula1 = adquisitionCost == 0 ? 0 : (decimal)(customerAnnualValue * currentGrossMargin) / (decimal)adquisitionCost;

                    //[Costumer Annual Contribution (mensual) * Tasa de Retención] / CAC
                    decimal customFormula2 = adquisitionCost == 0 ? 0 : (decimal)customerAnnualContributionRetention / (decimal)adquisitionCost;

                    List<int> firstOrderClientIds = firstOrders.Select(x => x.CustomerId).ToList();
                    int client30DaysAfterCount = 0;
                    int client60DaysAfterCount = 0;
                    int client90DaysAfterCount = 0;
                    int client120DaysAfterCount = 0;
                    int client150DaysAfterCount = 0;
                    int client180DaysAfterCount = 0;
                    int client210DaysAfterCount = 0;
                    int client240DaysAfterCount = 0;
                    int client270DaysAfterCount = 0;
                    int client300DaysAfterCount = 0;
                    int client330DaysAfterCount = 0;
                    int client360DaysAfterCount = 0;
                    int client390DaysAfterCount = 0;
                    int client420DaysAfterCount = 0;
                    int client450DaysAfterCount = 0;
                    int client480DaysAfterCount = 0;
                    int client510DaysAfterCount = 0;
                    int client540DaysAfterCount = 0;
                    int client570DaysAfterCount = 0;
                    int client600DaysAfterCount = 0;
                    int client630DaysAfterCount = 0;
                    int client660DaysAfterCount = 0;
                    int client690DaysAfterCount = 0;
                    int client720DaysAfterCount = 0;
                    decimal client30DaysAfterPercentage = 0;
                    decimal client60DaysAfterPercentage = 0;
                    decimal client90DaysAfterPercentage = 0;
                    decimal client120DaysAfterPercentage = 0;
                    decimal client150DaysAfterPercentage = 0;
                    decimal client180DaysAfterPercentage = 0;
                    decimal client210DaysAfterPercentage = 0;
                    decimal client240DaysAfterPercentage = 0;
                    decimal client270DaysAfterPercentage = 0;
                    decimal client300DaysAfterPercentage = 0;
                    decimal client330DaysAfterPercentage = 0;
                    decimal client360DaysAfterPercentage = 0;
                    decimal client390DaysAfterPercentage = 0;
                    decimal client420DaysAfterPercentage = 0;
                    decimal client450DaysAfterPercentage = 0;
                    decimal client480DaysAfterPercentage = 0;
                    decimal client510DaysAfterPercentage = 0;
                    decimal client540DaysAfterPercentage = 0;
                    decimal client570DaysAfterPercentage = 0;
                    decimal client600DaysAfterPercentage = 0;
                    decimal client630DaysAfterPercentage = 0;
                    decimal client660DaysAfterPercentage = 0;
                    decimal client690DaysAfterPercentage = 0;
                    decimal client720DaysAfterPercentage = 0;
                    decimal client30DaysAfterTicket = 0;
                    decimal client60DaysAfterTicket = 0;
                    decimal client90DaysAfterTicket = 0;
                    decimal client120DaysAfterTicket = 0;
                    decimal client150DaysAfterTicket = 0;
                    decimal client180DaysAfterTicket = 0;
                    decimal client210DaysAfterTicket = 0;
                    decimal client240DaysAfterTicket = 0;
                    decimal client270DaysAfterTicket = 0;
                    decimal client300DaysAfterTicket = 0;
                    decimal client330DaysAfterTicket = 0;
                    decimal client360DaysAfterTicket = 0;
                    decimal client390DaysAfterTicket = 0;
                    decimal client420DaysAfterTicket = 0;
                    decimal client450DaysAfterTicket = 0;
                    decimal client480DaysAfterTicket = 0;
                    decimal client510DaysAfterTicket = 0;
                    decimal client540DaysAfterTicket = 0;
                    decimal client570DaysAfterTicket = 0;
                    decimal client600DaysAfterTicket = 0;
                    decimal client630DaysAfterTicket = 0;
                    decimal client660DaysAfterTicket = 0;
                    decimal client690DaysAfterTicket = 0;
                    decimal client720DaysAfterTicket = 0;
                    decimal client30DaysAfterRecurrence = 0;
                    decimal client60DaysAfterRecurrence = 0;
                    decimal client90DaysAfterRecurrence = 0;
                    decimal client120DaysAfterRecurrence = 0;
                    decimal client150DaysAfterRecurrence = 0;
                    decimal client180DaysAfterRecurrence = 0;
                    decimal client210DaysAfterRecurrence = 0;
                    decimal client240DaysAfterRecurrence = 0;
                    decimal client270DaysAfterRecurrence = 0;
                    decimal client300DaysAfterRecurrence = 0;
                    decimal client330DaysAfterRecurrence = 0;
                    decimal client360DaysAfterRecurrence = 0;
                    decimal client390DaysAfterRecurrence = 0;
                    decimal client420DaysAfterRecurrence = 0;
                    decimal client450DaysAfterRecurrence = 0;
                    decimal client480DaysAfterRecurrence = 0;
                    decimal client510DaysAfterRecurrence = 0;
                    decimal client540DaysAfterRecurrence = 0;
                    decimal client570DaysAfterRecurrence = 0;
                    decimal client600DaysAfterRecurrence = 0;
                    decimal client630DaysAfterRecurrence = 0;
                    decimal client660DaysAfterRecurrence = 0;
                    decimal client690DaysAfterRecurrence = 0;
                    decimal client720DaysAfterRecurrence = 0;

                    PrepareClientDaysData(ref client30DaysAfterCount,
                        ref client60DaysAfterCount,
                        ref client90DaysAfterCount,
                        ref client120DaysAfterCount,
                        ref client150DaysAfterCount,
                        ref client180DaysAfterCount,
                        ref client210DaysAfterCount,
                        ref client240DaysAfterCount,
                        ref client270DaysAfterCount,
                        ref client300DaysAfterCount,
                        ref client330DaysAfterCount,
                        ref client360DaysAfterCount,
                        ref client390DaysAfterCount,
                        ref client420DaysAfterCount,
                        ref client450DaysAfterCount,
                        ref client480DaysAfterCount,
                        ref client510DaysAfterCount,
                        ref client540DaysAfterCount,
                        ref client570DaysAfterCount,
                        ref client600DaysAfterCount,
                        ref client630DaysAfterCount,
                        ref client660DaysAfterCount,
                        ref client690DaysAfterCount,
                        ref client720DaysAfterCount,
                        ref client30DaysAfterPercentage,
                        ref client60DaysAfterPercentage,
                        ref client90DaysAfterPercentage,
                        ref client120DaysAfterPercentage,
                        ref client150DaysAfterPercentage,
                        ref client180DaysAfterPercentage,
                        ref client210DaysAfterPercentage,
                        ref client240DaysAfterPercentage,
                        ref client270DaysAfterPercentage,
                        ref client300DaysAfterPercentage,
                        ref client330DaysAfterPercentage,
                        ref client360DaysAfterPercentage,
                        ref client390DaysAfterPercentage,
                        ref client420DaysAfterPercentage,
                        ref client450DaysAfterPercentage,
                        ref client480DaysAfterPercentage,
                        ref client510DaysAfterPercentage,
                        ref client540DaysAfterPercentage,
                        ref client570DaysAfterPercentage,
                        ref client600DaysAfterPercentage,
                        ref client630DaysAfterPercentage,
                        ref client660DaysAfterPercentage,
                        ref client690DaysAfterPercentage,
                        ref client720DaysAfterPercentage,
                        ref client30DaysAfterTicket,
                        ref client60DaysAfterTicket,
                        ref client90DaysAfterTicket,
                        ref client120DaysAfterTicket,
                        ref client150DaysAfterTicket,
                        ref client180DaysAfterTicket,
                        ref client210DaysAfterTicket,
                        ref client240DaysAfterTicket,
                        ref client270DaysAfterTicket,
                        ref client300DaysAfterTicket,
                        ref client330DaysAfterTicket,
                        ref client360DaysAfterTicket,
                        ref client390DaysAfterTicket,
                        ref client420DaysAfterTicket,
                        ref client450DaysAfterTicket,
                        ref client480DaysAfterTicket,
                        ref client510DaysAfterTicket,
                        ref client540DaysAfterTicket,
                        ref client570DaysAfterTicket,
                        ref client600DaysAfterTicket,
                        ref client630DaysAfterTicket,
                        ref client660DaysAfterTicket,
                        ref client690DaysAfterTicket,
                        ref client720DaysAfterTicket,
                        ref client30DaysAfterRecurrence,
                        ref client60DaysAfterRecurrence,
                        ref client90DaysAfterRecurrence,
                        ref client120DaysAfterRecurrence,
                        ref client150DaysAfterRecurrence,
                        ref client180DaysAfterRecurrence,
                        ref client210DaysAfterRecurrence,
                        ref client240DaysAfterRecurrence,
                        ref client270DaysAfterRecurrence,
                        ref client300DaysAfterRecurrence,
                        ref client330DaysAfterRecurrence,
                        ref client360DaysAfterRecurrence,
                        ref client390DaysAfterRecurrence,
                        ref client420DaysAfterRecurrence,
                        ref client450DaysAfterRecurrence,
                        ref client480DaysAfterRecurrence,
                        ref client510DaysAfterRecurrence,
                        ref client540DaysAfterRecurrence,
                        ref client570DaysAfterRecurrence,
                        ref client600DaysAfterRecurrence,
                        ref client630DaysAfterRecurrence,
                        ref client660DaysAfterRecurrence,
                        ref client690DaysAfterRecurrence,
                        ref client720DaysAfterRecurrence,
                        orders,
                        firstOrderClientIds,
                        firstPedidosCount,
                        endDate,
                        versionCode,
                        false);

                    dataToUpload.Add(new MarketingDashboardData()
                    {
                        AdquisitionCost = adquisitionCost,
                        AverageTicket = averageTicket,
                        CustomerAnnualValue = customerAnnualValue,
                        DailyPedidosAverage = dailyPedidosAverage,
                        DailySalesAverage = dailySalesAverage,
                        NewActiveCount = newActiveCount,
                        BuyinRegister = buyinRegister,
                        CustomerCount120Days = customerCount120Days,
                        CustomerCount30Days = customerCount30Days,
                        CustomerCount60Days = customerCount60Days,
                        CustomerCount90Days = customerCount90Days,
                        FirstOrderCount30Days = firstOrderCount30Days,
                        FirstOrderCount90Days = firstOrderCount90Days,
                        CustomFormula1 = customFormula1,
                        MarketingExpenses = marketingExpenses,
                        MonthlyChurnRate = monthlyChurnRate,
                        NewRegisterCost = newRegisterCost,
                        NewRegisteredUsersCount = newRegisteredUsersCount,
                        PedidosCount = pedidosCount,
                        QuarterlyChurnRate = quarterlyChurnRate,
                        Recurrence = recurrence,
                        SalesTotal = salesTotal,
                        TotalPedidos30Days = totalPedidos30Days,
                        WorkingDays = workingDays,
                        InitDate = initDate,
                        EndDate = endDate,
                        GenerationDateUtc = controlDateUtc,
                        CustomersWithOneOrderBetween150and121days = customersWithOneOrderBetween150and121days,
                        CustomersWithOneOrderBetween150and121daysAnd120daysPedido = customersWithOneOrderBetween150and121daysAnd120daysPedido,
                        CustomersCount2or3Pedidos120DaysAndMore = customersCount2or3Pedidos120DaysAndMore,
                        CustomersCount4or5Pedidos120DaysAndMore = customersCount4or5Pedidos120DaysAndMore,
                        CustomersCount6or7Pedidos120DaysAndMore = customersCount6or7Pedidos120DaysAndMore,
                        CustomersCount8or9Pedidos120DaysAndMore = customersCount8or9Pedidos120DaysAndMore,
                        CustomersCountAtLeastOneOrder120DaysAndMore = customersCountAtLeastOneOrder120DaysAndMore,
                        CustomersCountMoreThan10Pedidos120DaysAndMore = customersCountMoreThan10Pedidos120DaysAndMore,
                        CustomersCountOnlyOnePedido120DaysAndMore = customersCountOnlyOnePedido120DaysAndMore,
                        FirstPedidosCount = firstPedidosCount,
                        NotFirstOrdersCount = notFirstOrdersCount,
                        SalesFirstOrders = salesFirstOrders,
                        SalesNotFirstOrders = salesNotFirstOrders,
                        AverageTicketNewCustomers = averageTicketNewCustomers,
                        AverageTicketOldCustomers = averageTicketOldCustomers,
                        CustomerAnnualContribution = customerAnnualContribution,
                        CustomerAnnualContribution120Retention = customerAnnualContribution120Retention,
                        CustomerAnnualContribution120Retention2or3 = customerAnnualContribution120Retention2or3,
                        CustomerAnnualContribution120Retention4or5 = customerAnnualContribution120Retention4or5,
                        CustomerAnnualContribution120Retention6or7 = customerAnnualContribution120Retention6or7,
                        CustomerAnnualContribution120Retention8or9 = customerAnnualContribution120Retention8or9,
                        CustomerAnnualContribution120RetentionMoreThan10 = customerAnnualContribution120RetentionMoreThan10,
                        CustomerAnnualContributionRetention = customerAnnualContributionRetention,
                        CustomFormula2 = customFormula2,
                        Recurrence120days = recurrence120days,
                        Recurrence120days2or3 = recurrence120days2or3,
                        Recurrence120days4or5 = recurrence120days4or5,
                        Recurrence120days6or7 = recurrence120days6or7,
                        Recurrence120days8or9 = recurrence120days8or9,
                        Recurrence120daysMoreThan10 = recurrence120daysMoreThan10,
                        RetentionRate120Days = retentionRate120Days,
                        Client120DaysAfterCount = client120DaysAfterCount,
                        Client120DaysAfterPercentage = client120DaysAfterPercentage,
                        Client150DaysAfterCount = client150DaysAfterCount,
                        Client150DaysAfterPercentage = client150DaysAfterPercentage,
                        Client180DaysAfterCount = client180DaysAfterCount,
                        Client180DaysAfterPercentage = client180DaysAfterPercentage,
                        Client210DaysAfterCount = client210DaysAfterCount,
                        Client210DaysAfterPercentage = client210DaysAfterPercentage,
                        Client240DaysAfterCount = client240DaysAfterCount,
                        Client240DaysAfterPercentage = client240DaysAfterPercentage,
                        Client270DaysAfterCount = client270DaysAfterCount,
                        Client270DaysAfterPercentage = client270DaysAfterPercentage,
                        Client300DaysAfterCount = client300DaysAfterCount,
                        Client300DaysAfterPercentage = client300DaysAfterPercentage,
                        Client30DaysAfterCount = client30DaysAfterCount,
                        Client30DaysAfterPercentage = client30DaysAfterPercentage,
                        Client330DaysAfterCount = client330DaysAfterCount,
                        Client330DaysAfterPercentage = client330DaysAfterPercentage,
                        Client360DaysAfterCount = client360DaysAfterCount,
                        Client360DaysAfterPercentage = client360DaysAfterPercentage,
                        Client60DaysAfterCount = client60DaysAfterCount,
                        Client60DaysAfterPercentage = client60DaysAfterPercentage,
                        Client90DaysAfterCount = client90DaysAfterCount,
                        Client90DaysAfterPercentage = client90DaysAfterPercentage,
                        Client390DaysAfterCount = client390DaysAfterCount,
                        Client390DaysAfterPercentage = client390DaysAfterPercentage,
                        Client420DaysAfterCount = client420DaysAfterCount,
                        Client420DaysAfterPercentage = client420DaysAfterPercentage,
                        Client450DaysAfterCount = client450DaysAfterCount,
                        Client450DaysAfterPercentage = client450DaysAfterPercentage,
                        Client480DaysAfterCount = client480DaysAfterCount,
                        Client480DaysAfterPercentage = client480DaysAfterPercentage,
                        Client510DaysAfterCount = client510DaysAfterCount,
                        Client510DaysAfterPercentage = client510DaysAfterPercentage,
                        Client540DaysAfterCount = client540DaysAfterCount,
                        Client540DaysAfterPercentage = client540DaysAfterPercentage,
                        Client570DaysAfterCount = client570DaysAfterCount,
                        Client570DaysAfterPercentage = client570DaysAfterPercentage,
                        Client600DaysAfterCount = client600DaysAfterCount,
                        Client600DaysAfterPercentage = client600DaysAfterPercentage,
                        Client630DaysAfterCount = client630DaysAfterCount,
                        Client630DaysAfterPercentage = client630DaysAfterPercentage,
                        Client660DaysAfterCount = client660DaysAfterCount,
                        Client660DaysAfterPercentage = client660DaysAfterPercentage,
                        Client690DaysAfterCount = client690DaysAfterCount,
                        Client690DaysAfterPercentage = client690DaysAfterPercentage,
                        Client720DaysAfterCount = client720DaysAfterCount,
                        Client720DaysAfterPercentage = client720DaysAfterPercentage,
                        Client120DaysAfterRecurrence = client120DaysAfterRecurrence,
                        Client120DaysAfterTicket = client120DaysAfterTicket,
                        Client150DaysAfterRecurrence = client150DaysAfterRecurrence,
                        Client150DaysAfterTicket = client150DaysAfterTicket,
                        Client180DaysAfterRecurrence = client180DaysAfterRecurrence,
                        Client180DaysAfterTicket = client180DaysAfterTicket,
                        Client210DaysAfterRecurrence = client210DaysAfterRecurrence,
                        Client210DaysAfterTicket = client210DaysAfterTicket,
                        Client240DaysAfterRecurrence = client240DaysAfterRecurrence,
                        Client240DaysAfterTicket = client240DaysAfterTicket,
                        Client270DaysAfterRecurrence = client270DaysAfterRecurrence,
                        Client270DaysAfterTicket = client270DaysAfterTicket,
                        Client300DaysAfterRecurrence = client300DaysAfterRecurrence,
                        Client300DaysAfterTicket = client300DaysAfterTicket,
                        Client30DaysAfterRecurrence = client30DaysAfterRecurrence,
                        Client30DaysAfterTicket = client30DaysAfterTicket,
                        Client330DaysAfterRecurrence = client330DaysAfterRecurrence,
                        Client330DaysAfterTicket = client330DaysAfterTicket,
                        Client360DaysAfterRecurrence = client360DaysAfterRecurrence,
                        Client360DaysAfterTicket = client360DaysAfterTicket,
                        Client390DaysAfterRecurrence = client390DaysAfterRecurrence,
                        Client390DaysAfterTicket = client390DaysAfterTicket,
                        Client420DaysAfterRecurrence = client420DaysAfterRecurrence,
                        Client420DaysAfterTicket = client420DaysAfterTicket,
                        Client450DaysAfterRecurrence = client450DaysAfterRecurrence,
                        Client450DaysAfterTicket = client450DaysAfterTicket,
                        Client480DaysAfterRecurrence = client480DaysAfterRecurrence,
                        Client480DaysAfterTicket = client480DaysAfterTicket,
                        Client510DaysAfterRecurrence = client510DaysAfterRecurrence,
                        Client510DaysAfterTicket = client510DaysAfterTicket,
                        Client540DaysAfterRecurrence = client540DaysAfterRecurrence,
                        Client540DaysAfterTicket = client540DaysAfterTicket,
                        Client570DaysAfterRecurrence = client570DaysAfterRecurrence,
                        Client570DaysAfterTicket = client570DaysAfterTicket,
                        Client600DaysAfterRecurrence = client600DaysAfterRecurrence,
                        Client600DaysAfterTicket = client600DaysAfterTicket,
                        Client60DaysAfterRecurrence = client60DaysAfterRecurrence,
                        Client60DaysAfterTicket = client60DaysAfterTicket,
                        Client630DaysAfterRecurrence = client630DaysAfterRecurrence,
                        Client630DaysAfterTicket = client630DaysAfterTicket,
                        Client660DaysAfterRecurrence = client660DaysAfterRecurrence,
                        Client660DaysAfterTicket = client660DaysAfterTicket,
                        Client690DaysAfterRecurrence = client690DaysAfterRecurrence,
                        Client690DaysAfterTicket = client690DaysAfterTicket,
                        Client720DaysAfterRecurrence = client720DaysAfterRecurrence,
                        Client720DaysAfterTicket = client720DaysAfterTicket,
                        Client90DaysAfterRecurrence = client90DaysAfterRecurrence,
                        Client90DaysAfterTicket = client90DaysAfterTicket,
                        MarketingDashboardDataTypeId = 10
                    });
                }
                catch (Exception e)
                {
                    errors.Add(e.Message + ", " + e.InnerException?.Message);
                }
            }

            if (!errors.Any())
                foreach (var item in dataToUpload)
                {
                    Insert(item);
                }
            else
            {
                _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Error, "Error generating marketing dashboard data 10 - " + currentDate.ToString("dd/MM/yyyy"),
                    string.Join("\n\n", errors));
            }
        }

        /// <summary>
        /// Monthly data 120 days
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="controlDateUtc"></param>
        /// <param name="versionCode"></param>
        public void GenerateDashboardData20(List<Order> orders, DateTime controlDateUtc, int versionCode)
        {
            var errors = new List<string>();
            List<IGrouping<DateTime, Order>> groupedByMonth = null;
            if (versionCode == 0)
            {
                groupedByMonth = orders
               .OrderBy(x => x.SelectedShippingDate)
               .ToList()
               .GroupBy(x => new DateTime(x.SelectedShippingDate.Value.Year, x.SelectedShippingDate.Value.Month, 1))
               .OrderByDescending(x => x.Key)
               .ToList();
            }
            else
            {
               groupedByMonth = orders
              .OrderBy(x => x.CreatedOnUtc)
              .ToList()
              .GroupBy(x => new DateTime(x.CreatedOnUtc.ToLocalTime().Year, x.CreatedOnUtc.ToLocalTime().Month, 1))
              .OrderByDescending(x => x.Key)
              .ToList();
            }
            var currentDate = groupedByMonth.FirstOrDefault().Key;
            var dataToUpload = new List<MarketingDashboardData>();

            foreach (var group in groupedByMonth)
            {
                if (errors.Any()) continue;

                try
                {
                    currentDate = group.Key;
                    if (group.Key.Month > DateTime.Now.Month && group.Key.Year == DateTime.Now.Year) continue;
                    var initDate = group.Key;
                    var endDate = group.Key.AddMonths(1).AddDays(-1);

                    var firstOrders = new List<Order>();
                    var notFirstOrders = new List<Order>();
                    PrepareFirstAndLastOrders(firstOrders, notFirstOrders, orders, initDate, group.GroupBy(x => x.CustomerId).ToList(), versionCode);

                    List<int> firstOrderClientIds = firstOrders.Select(x => x.CustomerId).ToList();

                    //Clientes que hicieron su primera compra en el periodo
                    int firstPedidosCount = OrderUtils.GetPedidosGroupByList(firstOrders, versionCode).Count();

                    int client30DaysAfterCount = 0;
                    int client60DaysAfterCount = 0;
                    int client90DaysAfterCount = 0;
                    int client120DaysAfterCount = 0;
                    int client150DaysAfterCount = 0;
                    int client180DaysAfterCount = 0;
                    int client210DaysAfterCount = 0;
                    int client240DaysAfterCount = 0;
                    int client270DaysAfterCount = 0;
                    int client300DaysAfterCount = 0;
                    int client330DaysAfterCount = 0;
                    int client360DaysAfterCount = 0;
                    int client390DaysAfterCount = 0;
                    int client420DaysAfterCount = 0;
                    int client450DaysAfterCount = 0;
                    int client480DaysAfterCount = 0;
                    int client510DaysAfterCount = 0;
                    int client540DaysAfterCount = 0;
                    int client570DaysAfterCount = 0;
                    int client600DaysAfterCount = 0;
                    int client630DaysAfterCount = 0;
                    int client660DaysAfterCount = 0;
                    int client690DaysAfterCount = 0;
                    int client720DaysAfterCount = 0;
                    decimal client30DaysAfterPercentage = 0;
                    decimal client60DaysAfterPercentage = 0;
                    decimal client90DaysAfterPercentage = 0;
                    decimal client120DaysAfterPercentage = 0;
                    decimal client150DaysAfterPercentage = 0;
                    decimal client180DaysAfterPercentage = 0;
                    decimal client210DaysAfterPercentage = 0;
                    decimal client240DaysAfterPercentage = 0;
                    decimal client270DaysAfterPercentage = 0;
                    decimal client300DaysAfterPercentage = 0;
                    decimal client330DaysAfterPercentage = 0;
                    decimal client360DaysAfterPercentage = 0;
                    decimal client390DaysAfterPercentage = 0;
                    decimal client420DaysAfterPercentage = 0;
                    decimal client450DaysAfterPercentage = 0;
                    decimal client480DaysAfterPercentage = 0;
                    decimal client510DaysAfterPercentage = 0;
                    decimal client540DaysAfterPercentage = 0;
                    decimal client570DaysAfterPercentage = 0;
                    decimal client600DaysAfterPercentage = 0;
                    decimal client630DaysAfterPercentage = 0;
                    decimal client660DaysAfterPercentage = 0;
                    decimal client690DaysAfterPercentage = 0;
                    decimal client720DaysAfterPercentage = 0;
                    decimal client30DaysAfterTicket = 0;
                    decimal client60DaysAfterTicket = 0;
                    decimal client90DaysAfterTicket = 0;
                    decimal client120DaysAfterTicket = 0;
                    decimal client150DaysAfterTicket = 0;
                    decimal client180DaysAfterTicket = 0;
                    decimal client210DaysAfterTicket = 0;
                    decimal client240DaysAfterTicket = 0;
                    decimal client270DaysAfterTicket = 0;
                    decimal client300DaysAfterTicket = 0;
                    decimal client330DaysAfterTicket = 0;
                    decimal client360DaysAfterTicket = 0;
                    decimal client390DaysAfterTicket = 0;
                    decimal client420DaysAfterTicket = 0;
                    decimal client450DaysAfterTicket = 0;
                    decimal client480DaysAfterTicket = 0;
                    decimal client510DaysAfterTicket = 0;
                    decimal client540DaysAfterTicket = 0;
                    decimal client570DaysAfterTicket = 0;
                    decimal client600DaysAfterTicket = 0;
                    decimal client630DaysAfterTicket = 0;
                    decimal client660DaysAfterTicket = 0;
                    decimal client690DaysAfterTicket = 0;
                    decimal client720DaysAfterTicket = 0;
                    decimal client30DaysAfterRecurrence = 0;
                    decimal client60DaysAfterRecurrence = 0;
                    decimal client90DaysAfterRecurrence = 0;
                    decimal client120DaysAfterRecurrence = 0;
                    decimal client150DaysAfterRecurrence = 0;
                    decimal client180DaysAfterRecurrence = 0;
                    decimal client210DaysAfterRecurrence = 0;
                    decimal client240DaysAfterRecurrence = 0;
                    decimal client270DaysAfterRecurrence = 0;
                    decimal client300DaysAfterRecurrence = 0;
                    decimal client330DaysAfterRecurrence = 0;
                    decimal client360DaysAfterRecurrence = 0;
                    decimal client390DaysAfterRecurrence = 0;
                    decimal client420DaysAfterRecurrence = 0;
                    decimal client450DaysAfterRecurrence = 0;
                    decimal client480DaysAfterRecurrence = 0;
                    decimal client510DaysAfterRecurrence = 0;
                    decimal client540DaysAfterRecurrence = 0;
                    decimal client570DaysAfterRecurrence = 0;
                    decimal client600DaysAfterRecurrence = 0;
                    decimal client630DaysAfterRecurrence = 0;
                    decimal client660DaysAfterRecurrence = 0;
                    decimal client690DaysAfterRecurrence = 0;
                    decimal client720DaysAfterRecurrence = 0;

                    PrepareClientDaysData(ref client30DaysAfterCount,
                        ref client60DaysAfterCount,
                        ref client90DaysAfterCount,
                        ref client120DaysAfterCount,
                        ref client150DaysAfterCount,
                        ref client180DaysAfterCount,
                        ref client210DaysAfterCount,
                        ref client240DaysAfterCount,
                        ref client270DaysAfterCount,
                        ref client300DaysAfterCount,
                        ref client330DaysAfterCount,
                        ref client360DaysAfterCount,
                        ref client390DaysAfterCount,
                        ref client420DaysAfterCount,
                        ref client450DaysAfterCount,
                        ref client480DaysAfterCount,
                        ref client510DaysAfterCount,
                        ref client540DaysAfterCount,
                        ref client570DaysAfterCount,
                        ref client600DaysAfterCount,
                        ref client630DaysAfterCount,
                        ref client660DaysAfterCount,
                        ref client690DaysAfterCount,
                        ref client720DaysAfterCount,
                        ref client30DaysAfterPercentage,
                        ref client60DaysAfterPercentage,
                        ref client90DaysAfterPercentage,
                        ref client120DaysAfterPercentage,
                        ref client150DaysAfterPercentage,
                        ref client180DaysAfterPercentage,
                        ref client210DaysAfterPercentage,
                        ref client240DaysAfterPercentage,
                        ref client270DaysAfterPercentage,
                        ref client300DaysAfterPercentage,
                        ref client330DaysAfterPercentage,
                        ref client360DaysAfterPercentage,
                        ref client390DaysAfterPercentage,
                        ref client420DaysAfterPercentage,
                        ref client450DaysAfterPercentage,
                        ref client480DaysAfterPercentage,
                        ref client510DaysAfterPercentage,
                        ref client540DaysAfterPercentage,
                        ref client570DaysAfterPercentage,
                        ref client600DaysAfterPercentage,
                        ref client630DaysAfterPercentage,
                        ref client660DaysAfterPercentage,
                        ref client690DaysAfterPercentage,
                        ref client720DaysAfterPercentage,
                        ref client30DaysAfterTicket,
                        ref client60DaysAfterTicket,
                        ref client90DaysAfterTicket,
                        ref client120DaysAfterTicket,
                        ref client150DaysAfterTicket,
                        ref client180DaysAfterTicket,
                        ref client210DaysAfterTicket,
                        ref client240DaysAfterTicket,
                        ref client270DaysAfterTicket,
                        ref client300DaysAfterTicket,
                        ref client330DaysAfterTicket,
                        ref client360DaysAfterTicket,
                        ref client390DaysAfterTicket,
                        ref client420DaysAfterTicket,
                        ref client450DaysAfterTicket,
                        ref client480DaysAfterTicket,
                        ref client510DaysAfterTicket,
                        ref client540DaysAfterTicket,
                        ref client570DaysAfterTicket,
                        ref client600DaysAfterTicket,
                        ref client630DaysAfterTicket,
                        ref client660DaysAfterTicket,
                        ref client690DaysAfterTicket,
                        ref client720DaysAfterTicket,
                        ref client30DaysAfterRecurrence,
                        ref client60DaysAfterRecurrence,
                        ref client90DaysAfterRecurrence,
                        ref client120DaysAfterRecurrence,
                        ref client150DaysAfterRecurrence,
                        ref client180DaysAfterRecurrence,
                        ref client210DaysAfterRecurrence,
                        ref client240DaysAfterRecurrence,
                        ref client270DaysAfterRecurrence,
                        ref client300DaysAfterRecurrence,
                        ref client330DaysAfterRecurrence,
                        ref client360DaysAfterRecurrence,
                        ref client390DaysAfterRecurrence,
                        ref client420DaysAfterRecurrence,
                        ref client450DaysAfterRecurrence,
                        ref client480DaysAfterRecurrence,
                        ref client510DaysAfterRecurrence,
                        ref client540DaysAfterRecurrence,
                        ref client570DaysAfterRecurrence,
                        ref client600DaysAfterRecurrence,
                        ref client630DaysAfterRecurrence,
                        ref client660DaysAfterRecurrence,
                        ref client690DaysAfterRecurrence,
                        ref client720DaysAfterRecurrence,
                        orders,
                        firstOrderClientIds,
                        firstPedidosCount,
                        endDate,
                        versionCode,
                        false);

                    dataToUpload.Add(new MarketingDashboardData()
                    {
                        InitDate = initDate,
                        EndDate = endDate,
                        GenerationDateUtc = controlDateUtc,
                        FirstPedidosCount = firstPedidosCount,
                        Client120DaysAfterCount = client120DaysAfterCount,
                        Client120DaysAfterPercentage = client120DaysAfterPercentage,
                        Client150DaysAfterCount = client150DaysAfterCount,
                        Client150DaysAfterPercentage = client150DaysAfterPercentage,
                        Client180DaysAfterCount = client180DaysAfterCount,
                        Client180DaysAfterPercentage = client180DaysAfterPercentage,
                        Client210DaysAfterCount = client210DaysAfterCount,
                        Client210DaysAfterPercentage = client210DaysAfterPercentage,
                        Client240DaysAfterCount = client240DaysAfterCount,
                        Client240DaysAfterPercentage = client240DaysAfterPercentage,
                        Client270DaysAfterCount = client270DaysAfterCount,
                        Client270DaysAfterPercentage = client270DaysAfterPercentage,
                        Client300DaysAfterCount = client300DaysAfterCount,
                        Client300DaysAfterPercentage = client300DaysAfterPercentage,
                        Client30DaysAfterCount = client30DaysAfterCount,
                        Client30DaysAfterPercentage = client30DaysAfterPercentage,
                        Client330DaysAfterCount = client330DaysAfterCount,
                        Client330DaysAfterPercentage = client330DaysAfterPercentage,
                        Client360DaysAfterCount = client360DaysAfterCount,
                        Client360DaysAfterPercentage = client360DaysAfterPercentage,
                        Client60DaysAfterCount = client60DaysAfterCount,
                        Client60DaysAfterPercentage = client60DaysAfterPercentage,
                        Client90DaysAfterCount = client90DaysAfterCount,
                        Client90DaysAfterPercentage = client90DaysAfterPercentage,
                        Client390DaysAfterCount = client390DaysAfterCount,
                        Client390DaysAfterPercentage = client390DaysAfterPercentage,
                        Client420DaysAfterCount = client420DaysAfterCount,
                        Client420DaysAfterPercentage = client420DaysAfterPercentage,
                        Client450DaysAfterCount = client450DaysAfterCount,
                        Client450DaysAfterPercentage = client450DaysAfterPercentage,
                        Client480DaysAfterCount = client480DaysAfterCount,
                        Client480DaysAfterPercentage = client480DaysAfterPercentage,
                        Client510DaysAfterCount = client510DaysAfterCount,
                        Client510DaysAfterPercentage = client510DaysAfterPercentage,
                        Client540DaysAfterCount = client540DaysAfterCount,
                        Client540DaysAfterPercentage = client540DaysAfterPercentage,
                        Client570DaysAfterCount = client570DaysAfterCount,
                        Client570DaysAfterPercentage = client570DaysAfterPercentage,
                        Client600DaysAfterCount = client600DaysAfterCount,
                        Client600DaysAfterPercentage = client600DaysAfterPercentage,
                        Client630DaysAfterCount = client630DaysAfterCount,
                        Client630DaysAfterPercentage = client630DaysAfterPercentage,
                        Client660DaysAfterCount = client660DaysAfterCount,
                        Client660DaysAfterPercentage = client660DaysAfterPercentage,
                        Client690DaysAfterCount = client690DaysAfterCount,
                        Client690DaysAfterPercentage = client690DaysAfterPercentage,
                        Client720DaysAfterCount = client720DaysAfterCount,
                        Client720DaysAfterPercentage = client720DaysAfterPercentage,
                        Client120DaysAfterRecurrence = client120DaysAfterRecurrence,
                        Client120DaysAfterTicket = client120DaysAfterTicket,
                        Client150DaysAfterRecurrence = client150DaysAfterRecurrence,
                        Client150DaysAfterTicket = client150DaysAfterTicket,
                        Client180DaysAfterRecurrence = client180DaysAfterRecurrence,
                        Client180DaysAfterTicket = client180DaysAfterTicket,
                        Client210DaysAfterRecurrence = client210DaysAfterRecurrence,
                        Client210DaysAfterTicket = client210DaysAfterTicket,
                        Client240DaysAfterRecurrence = client240DaysAfterRecurrence,
                        Client240DaysAfterTicket = client240DaysAfterTicket,
                        Client270DaysAfterRecurrence = client270DaysAfterRecurrence,
                        Client270DaysAfterTicket = client270DaysAfterTicket,
                        Client300DaysAfterRecurrence = client300DaysAfterRecurrence,
                        Client300DaysAfterTicket = client300DaysAfterTicket,
                        Client30DaysAfterRecurrence = client30DaysAfterRecurrence,
                        Client30DaysAfterTicket = client30DaysAfterTicket,
                        Client330DaysAfterRecurrence = client330DaysAfterRecurrence,
                        Client330DaysAfterTicket = client330DaysAfterTicket,
                        Client360DaysAfterRecurrence = client360DaysAfterRecurrence,
                        Client360DaysAfterTicket = client360DaysAfterTicket,
                        Client390DaysAfterRecurrence = client390DaysAfterRecurrence,
                        Client390DaysAfterTicket = client390DaysAfterTicket,
                        Client420DaysAfterRecurrence = client420DaysAfterRecurrence,
                        Client420DaysAfterTicket = client420DaysAfterTicket,
                        Client450DaysAfterRecurrence = client450DaysAfterRecurrence,
                        Client450DaysAfterTicket = client450DaysAfterTicket,
                        Client480DaysAfterRecurrence = client480DaysAfterRecurrence,
                        Client480DaysAfterTicket = client480DaysAfterTicket,
                        Client510DaysAfterRecurrence = client510DaysAfterRecurrence,
                        Client510DaysAfterTicket = client510DaysAfterTicket,
                        Client540DaysAfterRecurrence = client540DaysAfterRecurrence,
                        Client540DaysAfterTicket = client540DaysAfterTicket,
                        Client570DaysAfterRecurrence = client570DaysAfterRecurrence,
                        Client570DaysAfterTicket = client570DaysAfterTicket,
                        Client600DaysAfterRecurrence = client600DaysAfterRecurrence,
                        Client600DaysAfterTicket = client600DaysAfterTicket,
                        Client60DaysAfterRecurrence = client60DaysAfterRecurrence,
                        Client60DaysAfterTicket = client60DaysAfterTicket,
                        Client630DaysAfterRecurrence = client630DaysAfterRecurrence,
                        Client630DaysAfterTicket = client630DaysAfterTicket,
                        Client660DaysAfterRecurrence = client660DaysAfterRecurrence,
                        Client660DaysAfterTicket = client660DaysAfterTicket,
                        Client690DaysAfterRecurrence = client690DaysAfterRecurrence,
                        Client690DaysAfterTicket = client690DaysAfterTicket,
                        Client720DaysAfterRecurrence = client720DaysAfterRecurrence,
                        Client720DaysAfterTicket = client720DaysAfterTicket,
                        Client90DaysAfterRecurrence = client90DaysAfterRecurrence,
                        Client90DaysAfterTicket = client90DaysAfterTicket,
                        MarketingDashboardDataTypeId = 20
                    });
                }
                catch (Exception e)
                {
                    errors.Add(e.Message + ", " + e.InnerException?.Message);
                }
            }

            if (!errors.Any())
                foreach (var item in dataToUpload)
                {
                    Insert(item);
                }
            else
            {
                _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Error, "Error generating marketing dashboard data 20 - " + currentDate.ToString("dd/MM/yyyy"),
                    string.Join("\n\n", errors));
            }
        }

        /// <summary>
        /// Monthly data 30 days
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="controlDateUtc"></param>
        /// <param name="versionCode"></param>
        /// <param name="shouldUpload"></param>
        /// <returns>List of generated data</returns>
        public List<MarketingDashboardData> GenerateDashboardData30(List<Order> orders,
            DateTime controlDateUtc,
            int versionCode,
            bool shouldUpload)
        {
            var errors = new List<string>();
            List<IGrouping<DateTime, Order>> groupedByMonth = null;
            if (versionCode == 0)
            {
                groupedByMonth = orders
               .OrderBy(x => x.SelectedShippingDate)
               .ToList()
               .GroupBy(x => new DateTime(x.SelectedShippingDate.Value.Year, x.SelectedShippingDate.Value.Month, 1))
               .OrderByDescending(x => x.Key)
               .ToList();
            }
            else
            {
                groupedByMonth = orders
              .OrderBy(x => x.CreatedOnUtc)
              .ToList()
              .GroupBy(x => new DateTime(x.CreatedOnUtc.ToLocalTime().Year, x.CreatedOnUtc.ToLocalTime().Month, 1))
              .OrderByDescending(x => x.Key)
              .ToList();
            }
            var currentDate = groupedByMonth.FirstOrDefault().Key;
            var dataToUpload = new List<MarketingDashboardData>();

            foreach (var group in groupedByMonth)
            {
                if (errors.Any()) continue;

                try
                {
                    currentDate = group.Key;
                    if (group.Key.Month > DateTime.Now.Month && group.Key.Year == DateTime.Now.Year) continue;
                    var initDate = group.Key;
                    var endDate = group.Key.AddMonths(1).AddDays(-1);

                    var firstOrders = new List<Order>();
                    var notFirstOrders = new List<Order>();
                    PrepareFirstAndLastOrders(firstOrders, notFirstOrders, orders, initDate, group.GroupBy(x => x.CustomerId).ToList(), versionCode);

                    List<int> firstOrderClientIds = firstOrders.Select(x => x.CustomerId).ToList();

                    //Clientes que hicieron su primera compra en el periodo
                    int firstPedidosCount = OrderUtils.GetPedidosGroupByList(firstOrders, versionCode).Count();

                    int client30DaysAfterCount = 0;
                    int client60DaysAfterCount = 0;
                    int client90DaysAfterCount = 0;
                    int client120DaysAfterCount = 0;
                    int client150DaysAfterCount = 0;
                    int client180DaysAfterCount = 0;
                    int client210DaysAfterCount = 0;
                    int client240DaysAfterCount = 0;
                    int client270DaysAfterCount = 0;
                    int client300DaysAfterCount = 0;
                    int client330DaysAfterCount = 0;
                    int client360DaysAfterCount = 0;
                    int client390DaysAfterCount = 0;
                    int client420DaysAfterCount = 0;
                    int client450DaysAfterCount = 0;
                    int client480DaysAfterCount = 0;
                    int client510DaysAfterCount = 0;
                    int client540DaysAfterCount = 0;
                    int client570DaysAfterCount = 0;
                    int client600DaysAfterCount = 0;
                    int client630DaysAfterCount = 0;
                    int client660DaysAfterCount = 0;
                    int client690DaysAfterCount = 0;
                    int client720DaysAfterCount = 0;
                    decimal client30DaysAfterPercentage = 0;
                    decimal client60DaysAfterPercentage = 0;
                    decimal client90DaysAfterPercentage = 0;
                    decimal client120DaysAfterPercentage = 0;
                    decimal client150DaysAfterPercentage = 0;
                    decimal client180DaysAfterPercentage = 0;
                    decimal client210DaysAfterPercentage = 0;
                    decimal client240DaysAfterPercentage = 0;
                    decimal client270DaysAfterPercentage = 0;
                    decimal client300DaysAfterPercentage = 0;
                    decimal client330DaysAfterPercentage = 0;
                    decimal client360DaysAfterPercentage = 0;
                    decimal client390DaysAfterPercentage = 0;
                    decimal client420DaysAfterPercentage = 0;
                    decimal client450DaysAfterPercentage = 0;
                    decimal client480DaysAfterPercentage = 0;
                    decimal client510DaysAfterPercentage = 0;
                    decimal client540DaysAfterPercentage = 0;
                    decimal client570DaysAfterPercentage = 0;
                    decimal client600DaysAfterPercentage = 0;
                    decimal client630DaysAfterPercentage = 0;
                    decimal client660DaysAfterPercentage = 0;
                    decimal client690DaysAfterPercentage = 0;
                    decimal client720DaysAfterPercentage = 0;
                    decimal client30DaysAfterTicket = 0;
                    decimal client60DaysAfterTicket = 0;
                    decimal client90DaysAfterTicket = 0;
                    decimal client120DaysAfterTicket = 0;
                    decimal client150DaysAfterTicket = 0;
                    decimal client180DaysAfterTicket = 0;
                    decimal client210DaysAfterTicket = 0;
                    decimal client240DaysAfterTicket = 0;
                    decimal client270DaysAfterTicket = 0;
                    decimal client300DaysAfterTicket = 0;
                    decimal client330DaysAfterTicket = 0;
                    decimal client360DaysAfterTicket = 0;
                    decimal client390DaysAfterTicket = 0;
                    decimal client420DaysAfterTicket = 0;
                    decimal client450DaysAfterTicket = 0;
                    decimal client480DaysAfterTicket = 0;
                    decimal client510DaysAfterTicket = 0;
                    decimal client540DaysAfterTicket = 0;
                    decimal client570DaysAfterTicket = 0;
                    decimal client600DaysAfterTicket = 0;
                    decimal client630DaysAfterTicket = 0;
                    decimal client660DaysAfterTicket = 0;
                    decimal client690DaysAfterTicket = 0;
                    decimal client720DaysAfterTicket = 0;
                    decimal client30DaysAfterRecurrence = 0;
                    decimal client60DaysAfterRecurrence = 0;
                    decimal client90DaysAfterRecurrence = 0;
                    decimal client120DaysAfterRecurrence = 0;
                    decimal client150DaysAfterRecurrence = 0;
                    decimal client180DaysAfterRecurrence = 0;
                    decimal client210DaysAfterRecurrence = 0;
                    decimal client240DaysAfterRecurrence = 0;
                    decimal client270DaysAfterRecurrence = 0;
                    decimal client300DaysAfterRecurrence = 0;
                    decimal client330DaysAfterRecurrence = 0;
                    decimal client360DaysAfterRecurrence = 0;
                    decimal client390DaysAfterRecurrence = 0;
                    decimal client420DaysAfterRecurrence = 0;
                    decimal client450DaysAfterRecurrence = 0;
                    decimal client480DaysAfterRecurrence = 0;
                    decimal client510DaysAfterRecurrence = 0;
                    decimal client540DaysAfterRecurrence = 0;
                    decimal client570DaysAfterRecurrence = 0;
                    decimal client600DaysAfterRecurrence = 0;
                    decimal client630DaysAfterRecurrence = 0;
                    decimal client660DaysAfterRecurrence = 0;
                    decimal client690DaysAfterRecurrence = 0;
                    decimal client720DaysAfterRecurrence = 0;

                    PrepareClientDaysData(ref client30DaysAfterCount,
                        ref client60DaysAfterCount,
                        ref client90DaysAfterCount,
                        ref client120DaysAfterCount,
                        ref client150DaysAfterCount,
                        ref client180DaysAfterCount,
                        ref client210DaysAfterCount,
                        ref client240DaysAfterCount,
                        ref client270DaysAfterCount,
                        ref client300DaysAfterCount,
                        ref client330DaysAfterCount,
                        ref client360DaysAfterCount,
                        ref client390DaysAfterCount,
                        ref client420DaysAfterCount,
                        ref client450DaysAfterCount,
                        ref client480DaysAfterCount,
                        ref client510DaysAfterCount,
                        ref client540DaysAfterCount,
                        ref client570DaysAfterCount,
                        ref client600DaysAfterCount,
                        ref client630DaysAfterCount,
                        ref client660DaysAfterCount,
                        ref client690DaysAfterCount,
                        ref client720DaysAfterCount,
                        ref client30DaysAfterPercentage,
                        ref client60DaysAfterPercentage,
                        ref client90DaysAfterPercentage,
                        ref client120DaysAfterPercentage,
                        ref client150DaysAfterPercentage,
                        ref client180DaysAfterPercentage,
                        ref client210DaysAfterPercentage,
                        ref client240DaysAfterPercentage,
                        ref client270DaysAfterPercentage,
                        ref client300DaysAfterPercentage,
                        ref client330DaysAfterPercentage,
                        ref client360DaysAfterPercentage,
                        ref client390DaysAfterPercentage,
                        ref client420DaysAfterPercentage,
                        ref client450DaysAfterPercentage,
                        ref client480DaysAfterPercentage,
                        ref client510DaysAfterPercentage,
                        ref client540DaysAfterPercentage,
                        ref client570DaysAfterPercentage,
                        ref client600DaysAfterPercentage,
                        ref client630DaysAfterPercentage,
                        ref client660DaysAfterPercentage,
                        ref client690DaysAfterPercentage,
                        ref client720DaysAfterPercentage,
                        ref client30DaysAfterTicket,
                        ref client60DaysAfterTicket,
                        ref client90DaysAfterTicket,
                        ref client120DaysAfterTicket,
                        ref client150DaysAfterTicket,
                        ref client180DaysAfterTicket,
                        ref client210DaysAfterTicket,
                        ref client240DaysAfterTicket,
                        ref client270DaysAfterTicket,
                        ref client300DaysAfterTicket,
                        ref client330DaysAfterTicket,
                        ref client360DaysAfterTicket,
                        ref client390DaysAfterTicket,
                        ref client420DaysAfterTicket,
                        ref client450DaysAfterTicket,
                        ref client480DaysAfterTicket,
                        ref client510DaysAfterTicket,
                        ref client540DaysAfterTicket,
                        ref client570DaysAfterTicket,
                        ref client600DaysAfterTicket,
                        ref client630DaysAfterTicket,
                        ref client660DaysAfterTicket,
                        ref client690DaysAfterTicket,
                        ref client720DaysAfterTicket,
                        ref client30DaysAfterRecurrence,
                        ref client60DaysAfterRecurrence,
                        ref client90DaysAfterRecurrence,
                        ref client120DaysAfterRecurrence,
                        ref client150DaysAfterRecurrence,
                        ref client180DaysAfterRecurrence,
                        ref client210DaysAfterRecurrence,
                        ref client240DaysAfterRecurrence,
                        ref client270DaysAfterRecurrence,
                        ref client300DaysAfterRecurrence,
                        ref client330DaysAfterRecurrence,
                        ref client360DaysAfterRecurrence,
                        ref client390DaysAfterRecurrence,
                        ref client420DaysAfterRecurrence,
                        ref client450DaysAfterRecurrence,
                        ref client480DaysAfterRecurrence,
                        ref client510DaysAfterRecurrence,
                        ref client540DaysAfterRecurrence,
                        ref client570DaysAfterRecurrence,
                        ref client600DaysAfterRecurrence,
                        ref client630DaysAfterRecurrence,
                        ref client660DaysAfterRecurrence,
                        ref client690DaysAfterRecurrence,
                        ref client720DaysAfterRecurrence,
                        orders,
                        firstOrderClientIds,
                        firstPedidosCount,
                        endDate,
                        versionCode,
                        true);

                    dataToUpload.Add(new MarketingDashboardData()
                    {
                        InitDate = initDate,
                        EndDate = endDate,
                        GenerationDateUtc = controlDateUtc,
                        FirstPedidosCount = firstPedidosCount,
                        Client120DaysAfterCount = client120DaysAfterCount,
                        Client120DaysAfterPercentage = client120DaysAfterPercentage,
                        Client150DaysAfterCount = client150DaysAfterCount,
                        Client150DaysAfterPercentage = client150DaysAfterPercentage,
                        Client180DaysAfterCount = client180DaysAfterCount,
                        Client180DaysAfterPercentage = client180DaysAfterPercentage,
                        Client210DaysAfterCount = client210DaysAfterCount,
                        Client210DaysAfterPercentage = client210DaysAfterPercentage,
                        Client240DaysAfterCount = client240DaysAfterCount,
                        Client240DaysAfterPercentage = client240DaysAfterPercentage,
                        Client270DaysAfterCount = client270DaysAfterCount,
                        Client270DaysAfterPercentage = client270DaysAfterPercentage,
                        Client300DaysAfterCount = client300DaysAfterCount,
                        Client300DaysAfterPercentage = client300DaysAfterPercentage,
                        Client30DaysAfterCount = client30DaysAfterCount,
                        Client30DaysAfterPercentage = client30DaysAfterPercentage,
                        Client330DaysAfterCount = client330DaysAfterCount,
                        Client330DaysAfterPercentage = client330DaysAfterPercentage,
                        Client360DaysAfterCount = client360DaysAfterCount,
                        Client360DaysAfterPercentage = client360DaysAfterPercentage,
                        Client60DaysAfterCount = client60DaysAfterCount,
                        Client60DaysAfterPercentage = client60DaysAfterPercentage,
                        Client90DaysAfterCount = client90DaysAfterCount,
                        Client90DaysAfterPercentage = client90DaysAfterPercentage,
                        Client390DaysAfterCount = client390DaysAfterCount,
                        Client390DaysAfterPercentage = client390DaysAfterPercentage,
                        Client420DaysAfterCount = client420DaysAfterCount,
                        Client420DaysAfterPercentage = client420DaysAfterPercentage,
                        Client450DaysAfterCount = client450DaysAfterCount,
                        Client450DaysAfterPercentage = client450DaysAfterPercentage,
                        Client480DaysAfterCount = client480DaysAfterCount,
                        Client480DaysAfterPercentage = client480DaysAfterPercentage,
                        Client510DaysAfterCount = client510DaysAfterCount,
                        Client510DaysAfterPercentage = client510DaysAfterPercentage,
                        Client540DaysAfterCount = client540DaysAfterCount,
                        Client540DaysAfterPercentage = client540DaysAfterPercentage,
                        Client570DaysAfterCount = client570DaysAfterCount,
                        Client570DaysAfterPercentage = client570DaysAfterPercentage,
                        Client600DaysAfterCount = client600DaysAfterCount,
                        Client600DaysAfterPercentage = client600DaysAfterPercentage,
                        Client630DaysAfterCount = client630DaysAfterCount,
                        Client630DaysAfterPercentage = client630DaysAfterPercentage,
                        Client660DaysAfterCount = client660DaysAfterCount,
                        Client660DaysAfterPercentage = client660DaysAfterPercentage,
                        Client690DaysAfterCount = client690DaysAfterCount,
                        Client690DaysAfterPercentage = client690DaysAfterPercentage,
                        Client720DaysAfterCount = client720DaysAfterCount,
                        Client720DaysAfterPercentage = client720DaysAfterPercentage,
                        Client120DaysAfterRecurrence = client120DaysAfterRecurrence,
                        Client120DaysAfterTicket = client120DaysAfterTicket,
                        Client150DaysAfterRecurrence = client150DaysAfterRecurrence,
                        Client150DaysAfterTicket = client150DaysAfterTicket,
                        Client180DaysAfterRecurrence = client180DaysAfterRecurrence,
                        Client180DaysAfterTicket = client180DaysAfterTicket,
                        Client210DaysAfterRecurrence = client210DaysAfterRecurrence,
                        Client210DaysAfterTicket = client210DaysAfterTicket,
                        Client240DaysAfterRecurrence = client240DaysAfterRecurrence,
                        Client240DaysAfterTicket = client240DaysAfterTicket,
                        Client270DaysAfterRecurrence = client270DaysAfterRecurrence,
                        Client270DaysAfterTicket = client270DaysAfterTicket,
                        Client300DaysAfterRecurrence = client300DaysAfterRecurrence,
                        Client300DaysAfterTicket = client300DaysAfterTicket,
                        Client30DaysAfterRecurrence = client30DaysAfterRecurrence,
                        Client30DaysAfterTicket = client30DaysAfterTicket,
                        Client330DaysAfterRecurrence = client330DaysAfterRecurrence,
                        Client330DaysAfterTicket = client330DaysAfterTicket,
                        Client360DaysAfterRecurrence = client360DaysAfterRecurrence,
                        Client360DaysAfterTicket = client360DaysAfterTicket,
                        Client390DaysAfterRecurrence = client390DaysAfterRecurrence,
                        Client390DaysAfterTicket = client390DaysAfterTicket,
                        Client420DaysAfterRecurrence = client420DaysAfterRecurrence,
                        Client420DaysAfterTicket = client420DaysAfterTicket,
                        Client450DaysAfterRecurrence = client450DaysAfterRecurrence,
                        Client450DaysAfterTicket = client450DaysAfterTicket,
                        Client480DaysAfterRecurrence = client480DaysAfterRecurrence,
                        Client480DaysAfterTicket = client480DaysAfterTicket,
                        Client510DaysAfterRecurrence = client510DaysAfterRecurrence,
                        Client510DaysAfterTicket = client510DaysAfterTicket,
                        Client540DaysAfterRecurrence = client540DaysAfterRecurrence,
                        Client540DaysAfterTicket = client540DaysAfterTicket,
                        Client570DaysAfterRecurrence = client570DaysAfterRecurrence,
                        Client570DaysAfterTicket = client570DaysAfterTicket,
                        Client600DaysAfterRecurrence = client600DaysAfterRecurrence,
                        Client600DaysAfterTicket = client600DaysAfterTicket,
                        Client60DaysAfterRecurrence = client60DaysAfterRecurrence,
                        Client60DaysAfterTicket = client60DaysAfterTicket,
                        Client630DaysAfterRecurrence = client630DaysAfterRecurrence,
                        Client630DaysAfterTicket = client630DaysAfterTicket,
                        Client660DaysAfterRecurrence = client660DaysAfterRecurrence,
                        Client660DaysAfterTicket = client660DaysAfterTicket,
                        Client690DaysAfterRecurrence = client690DaysAfterRecurrence,
                        Client690DaysAfterTicket = client690DaysAfterTicket,
                        Client720DaysAfterRecurrence = client720DaysAfterRecurrence,
                        Client720DaysAfterTicket = client720DaysAfterTicket,
                        Client90DaysAfterRecurrence = client90DaysAfterRecurrence,
                        Client90DaysAfterTicket = client90DaysAfterTicket,
                        MarketingDashboardDataTypeId = 30
                    });
                }
                catch (Exception e)
                {
                    errors.Add(e.Message + ", " + e.InnerException?.Message);
                }
            }

            if (!errors.Any())
            {
                if (shouldUpload) UploadData(dataToUpload);
            }
            else
            {
                _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Error, "Error generating marketing dashboard data 30 - " + currentDate.ToString("dd/MM/yyyy"),
                    string.Join("\n\n", errors));
                dataToUpload = new List<MarketingDashboardData>();
            }
            return dataToUpload;
        }

        private void UploadData(List<MarketingDashboardData> dataToUpload)
        {
            foreach (var item in dataToUpload)
            {
                Insert(item);
            }
        }

        /// <summary>
        /// Weekly data 30 days
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="controlDateUtc"></param>
        /// <param name="versionCode">0 = CEL; 1 = OTHER</param>
        public void GenerateDashboardData40(List<Order> orders, DateTime controlDateUtc, int versionCode)
        {
            var errors = new List<string>();
            List<IGrouping<DateTime, Order>> groupedByWeek = null;
            if (versionCode == 0)
            {
                groupedByWeek = orders
               .OrderBy(x => x.SelectedShippingDate)
               .ToList()
               .GroupBy(x => x.SelectedShippingDate.Value.AddDays(-(int)x.SelectedShippingDate.Value.DayOfWeek).AddDays(1))
               .OrderByDescending(x => x.Key)
               .ToList();
            }
            else
            {
                groupedByWeek = orders
               .OrderBy(x => x.CreatedOnUtc)
               .ToList()
               .GroupBy(x => x.CreatedOnUtc.ToLocalTime().AddDays(-(int)x.CreatedOnUtc.DayOfWeek).AddDays(1).Date)
               .OrderByDescending(x => x.Key)
               .ToList();
            }
            var currentDate = groupedByWeek.FirstOrDefault().Key;

            var customers = _customerService.GetAllCustomers()
                .Where(x => x.Email != "" && x.Email != null)
                .ToList();
            var allMarketingExpenses = _marketingExpenseService.GetAll().ToList();
            var grossMargin = _marketingGrossMarginService.GetAll().ToList();
            var retentionRate = _marketingRetentionRateService.GetAll().ToList();

            var dataToUpload = new List<MarketingDashboardData>();

            foreach (var group in groupedByWeek)
            {
                if (errors.Any()) continue;

                try
                {
                    currentDate = group.Key;
                    var initDate = group.Key;
                    var endDate = group.Key.AddDays(6);

                    if (endDate >= DateTime.Now.Date) continue;

                    var firstOrders = new List<Order>();
                    var notFirstOrders = new List<Order>();
                    PrepareFirstAndLastOrders(firstOrders, notFirstOrders, orders, initDate, group.GroupBy(x => x.CustomerId).ToList(), versionCode);

                    var firstPedidosCount = versionCode == 0 ? OrderUtils.GetPedidosGroupByList(firstOrders).Count() : firstOrders.Count;

                    List<int> firstOrderClientIds = firstOrders.Select(x => x.CustomerId).ToList();
                    int client30DaysAfterCount = 0;
                    int client60DaysAfterCount = 0;
                    int client90DaysAfterCount = 0;
                    int client120DaysAfterCount = 0;
                    int client150DaysAfterCount = 0;
                    int client180DaysAfterCount = 0;
                    int client210DaysAfterCount = 0;
                    int client240DaysAfterCount = 0;
                    int client270DaysAfterCount = 0;
                    int client300DaysAfterCount = 0;
                    int client330DaysAfterCount = 0;
                    int client360DaysAfterCount = 0;
                    int client390DaysAfterCount = 0;
                    int client420DaysAfterCount = 0;
                    int client450DaysAfterCount = 0;
                    int client480DaysAfterCount = 0;
                    int client510DaysAfterCount = 0;
                    int client540DaysAfterCount = 0;
                    int client570DaysAfterCount = 0;
                    int client600DaysAfterCount = 0;
                    int client630DaysAfterCount = 0;
                    int client660DaysAfterCount = 0;
                    int client690DaysAfterCount = 0;
                    int client720DaysAfterCount = 0;
                    decimal client30DaysAfterPercentage = 0;
                    decimal client60DaysAfterPercentage = 0;
                    decimal client90DaysAfterPercentage = 0;
                    decimal client120DaysAfterPercentage = 0;
                    decimal client150DaysAfterPercentage = 0;
                    decimal client180DaysAfterPercentage = 0;
                    decimal client210DaysAfterPercentage = 0;
                    decimal client240DaysAfterPercentage = 0;
                    decimal client270DaysAfterPercentage = 0;
                    decimal client300DaysAfterPercentage = 0;
                    decimal client330DaysAfterPercentage = 0;
                    decimal client360DaysAfterPercentage = 0;
                    decimal client390DaysAfterPercentage = 0;
                    decimal client420DaysAfterPercentage = 0;
                    decimal client450DaysAfterPercentage = 0;
                    decimal client480DaysAfterPercentage = 0;
                    decimal client510DaysAfterPercentage = 0;
                    decimal client540DaysAfterPercentage = 0;
                    decimal client570DaysAfterPercentage = 0;
                    decimal client600DaysAfterPercentage = 0;
                    decimal client630DaysAfterPercentage = 0;
                    decimal client660DaysAfterPercentage = 0;
                    decimal client690DaysAfterPercentage = 0;
                    decimal client720DaysAfterPercentage = 0;
                    decimal client30DaysAfterTicket = 0;
                    decimal client60DaysAfterTicket = 0;
                    decimal client90DaysAfterTicket = 0;
                    decimal client120DaysAfterTicket = 0;
                    decimal client150DaysAfterTicket = 0;
                    decimal client180DaysAfterTicket = 0;
                    decimal client210DaysAfterTicket = 0;
                    decimal client240DaysAfterTicket = 0;
                    decimal client270DaysAfterTicket = 0;
                    decimal client300DaysAfterTicket = 0;
                    decimal client330DaysAfterTicket = 0;
                    decimal client360DaysAfterTicket = 0;
                    decimal client390DaysAfterTicket = 0;
                    decimal client420DaysAfterTicket = 0;
                    decimal client450DaysAfterTicket = 0;
                    decimal client480DaysAfterTicket = 0;
                    decimal client510DaysAfterTicket = 0;
                    decimal client540DaysAfterTicket = 0;
                    decimal client570DaysAfterTicket = 0;
                    decimal client600DaysAfterTicket = 0;
                    decimal client630DaysAfterTicket = 0;
                    decimal client660DaysAfterTicket = 0;
                    decimal client690DaysAfterTicket = 0;
                    decimal client720DaysAfterTicket = 0;
                    decimal client30DaysAfterRecurrence = 0;
                    decimal client60DaysAfterRecurrence = 0;
                    decimal client90DaysAfterRecurrence = 0;
                    decimal client120DaysAfterRecurrence = 0;
                    decimal client150DaysAfterRecurrence = 0;
                    decimal client180DaysAfterRecurrence = 0;
                    decimal client210DaysAfterRecurrence = 0;
                    decimal client240DaysAfterRecurrence = 0;
                    decimal client270DaysAfterRecurrence = 0;
                    decimal client300DaysAfterRecurrence = 0;
                    decimal client330DaysAfterRecurrence = 0;
                    decimal client360DaysAfterRecurrence = 0;
                    decimal client390DaysAfterRecurrence = 0;
                    decimal client420DaysAfterRecurrence = 0;
                    decimal client450DaysAfterRecurrence = 0;
                    decimal client480DaysAfterRecurrence = 0;
                    decimal client510DaysAfterRecurrence = 0;
                    decimal client540DaysAfterRecurrence = 0;
                    decimal client570DaysAfterRecurrence = 0;
                    decimal client600DaysAfterRecurrence = 0;
                    decimal client630DaysAfterRecurrence = 0;
                    decimal client660DaysAfterRecurrence = 0;
                    decimal client690DaysAfterRecurrence = 0;
                    decimal client720DaysAfterRecurrence = 0;

                    PrepareClientDaysData(ref client30DaysAfterCount,
                        ref client60DaysAfterCount,
                        ref client90DaysAfterCount,
                        ref client120DaysAfterCount,
                        ref client150DaysAfterCount,
                        ref client180DaysAfterCount,
                        ref client210DaysAfterCount,
                        ref client240DaysAfterCount,
                        ref client270DaysAfterCount,
                        ref client300DaysAfterCount,
                        ref client330DaysAfterCount,
                        ref client360DaysAfterCount,
                        ref client390DaysAfterCount,
                        ref client420DaysAfterCount,
                        ref client450DaysAfterCount,
                        ref client480DaysAfterCount,
                        ref client510DaysAfterCount,
                        ref client540DaysAfterCount,
                        ref client570DaysAfterCount,
                        ref client600DaysAfterCount,
                        ref client630DaysAfterCount,
                        ref client660DaysAfterCount,
                        ref client690DaysAfterCount,
                        ref client720DaysAfterCount,
                        ref client30DaysAfterPercentage,
                        ref client60DaysAfterPercentage,
                        ref client90DaysAfterPercentage,
                        ref client120DaysAfterPercentage,
                        ref client150DaysAfterPercentage,
                        ref client180DaysAfterPercentage,
                        ref client210DaysAfterPercentage,
                        ref client240DaysAfterPercentage,
                        ref client270DaysAfterPercentage,
                        ref client300DaysAfterPercentage,
                        ref client330DaysAfterPercentage,
                        ref client360DaysAfterPercentage,
                        ref client390DaysAfterPercentage,
                        ref client420DaysAfterPercentage,
                        ref client450DaysAfterPercentage,
                        ref client480DaysAfterPercentage,
                        ref client510DaysAfterPercentage,
                        ref client540DaysAfterPercentage,
                        ref client570DaysAfterPercentage,
                        ref client600DaysAfterPercentage,
                        ref client630DaysAfterPercentage,
                        ref client660DaysAfterPercentage,
                        ref client690DaysAfterPercentage,
                        ref client720DaysAfterPercentage,
                        ref client30DaysAfterTicket,
                        ref client60DaysAfterTicket,
                        ref client90DaysAfterTicket,
                        ref client120DaysAfterTicket,
                        ref client150DaysAfterTicket,
                        ref client180DaysAfterTicket,
                        ref client210DaysAfterTicket,
                        ref client240DaysAfterTicket,
                        ref client270DaysAfterTicket,
                        ref client300DaysAfterTicket,
                        ref client330DaysAfterTicket,
                        ref client360DaysAfterTicket,
                        ref client390DaysAfterTicket,
                        ref client420DaysAfterTicket,
                        ref client450DaysAfterTicket,
                        ref client480DaysAfterTicket,
                        ref client510DaysAfterTicket,
                        ref client540DaysAfterTicket,
                        ref client570DaysAfterTicket,
                        ref client600DaysAfterTicket,
                        ref client630DaysAfterTicket,
                        ref client660DaysAfterTicket,
                        ref client690DaysAfterTicket,
                        ref client720DaysAfterTicket,
                        ref client30DaysAfterRecurrence,
                        ref client60DaysAfterRecurrence,
                        ref client90DaysAfterRecurrence,
                        ref client120DaysAfterRecurrence,
                        ref client150DaysAfterRecurrence,
                        ref client180DaysAfterRecurrence,
                        ref client210DaysAfterRecurrence,
                        ref client240DaysAfterRecurrence,
                        ref client270DaysAfterRecurrence,
                        ref client300DaysAfterRecurrence,
                        ref client330DaysAfterRecurrence,
                        ref client360DaysAfterRecurrence,
                        ref client390DaysAfterRecurrence,
                        ref client420DaysAfterRecurrence,
                        ref client450DaysAfterRecurrence,
                        ref client480DaysAfterRecurrence,
                        ref client510DaysAfterRecurrence,
                        ref client540DaysAfterRecurrence,
                        ref client570DaysAfterRecurrence,
                        ref client600DaysAfterRecurrence,
                        ref client630DaysAfterRecurrence,
                        ref client660DaysAfterRecurrence,
                        ref client690DaysAfterRecurrence,
                        ref client720DaysAfterRecurrence,
                        orders,
                        firstOrderClientIds,
                        firstPedidosCount,
                        endDate,
                        versionCode,
                        true);

                    dataToUpload.Add(new MarketingDashboardData()
                    {
                        InitDate = initDate,
                        EndDate = endDate,
                        GenerationDateUtc = controlDateUtc,
                        FirstPedidosCount = firstPedidosCount,
                        Client120DaysAfterCount = client120DaysAfterCount,
                        Client120DaysAfterPercentage = client120DaysAfterPercentage,
                        Client150DaysAfterCount = client150DaysAfterCount,
                        Client150DaysAfterPercentage = client150DaysAfterPercentage,
                        Client180DaysAfterCount = client180DaysAfterCount,
                        Client180DaysAfterPercentage = client180DaysAfterPercentage,
                        Client210DaysAfterCount = client210DaysAfterCount,
                        Client210DaysAfterPercentage = client210DaysAfterPercentage,
                        Client240DaysAfterCount = client240DaysAfterCount,
                        Client240DaysAfterPercentage = client240DaysAfterPercentage,
                        Client270DaysAfterCount = client270DaysAfterCount,
                        Client270DaysAfterPercentage = client270DaysAfterPercentage,
                        Client300DaysAfterCount = client300DaysAfterCount,
                        Client300DaysAfterPercentage = client300DaysAfterPercentage,
                        Client30DaysAfterCount = client30DaysAfterCount,
                        Client30DaysAfterPercentage = client30DaysAfterPercentage,
                        Client330DaysAfterCount = client330DaysAfterCount,
                        Client330DaysAfterPercentage = client330DaysAfterPercentage,
                        Client360DaysAfterCount = client360DaysAfterCount,
                        Client360DaysAfterPercentage = client360DaysAfterPercentage,
                        Client60DaysAfterCount = client60DaysAfterCount,
                        Client60DaysAfterPercentage = client60DaysAfterPercentage,
                        Client90DaysAfterCount = client90DaysAfterCount,
                        Client90DaysAfterPercentage = client90DaysAfterPercentage,
                        Client390DaysAfterCount = client390DaysAfterCount,
                        Client390DaysAfterPercentage = client390DaysAfterPercentage,
                        Client420DaysAfterCount = client420DaysAfterCount,
                        Client420DaysAfterPercentage = client420DaysAfterPercentage,
                        Client450DaysAfterCount = client450DaysAfterCount,
                        Client450DaysAfterPercentage = client450DaysAfterPercentage,
                        Client480DaysAfterCount = client480DaysAfterCount,
                        Client480DaysAfterPercentage = client480DaysAfterPercentage,
                        Client510DaysAfterCount = client510DaysAfterCount,
                        Client510DaysAfterPercentage = client510DaysAfterPercentage,
                        Client540DaysAfterCount = client540DaysAfterCount,
                        Client540DaysAfterPercentage = client540DaysAfterPercentage,
                        Client570DaysAfterCount = client570DaysAfterCount,
                        Client570DaysAfterPercentage = client570DaysAfterPercentage,
                        Client600DaysAfterCount = client600DaysAfterCount,
                        Client600DaysAfterPercentage = client600DaysAfterPercentage,
                        Client630DaysAfterCount = client630DaysAfterCount,
                        Client630DaysAfterPercentage = client630DaysAfterPercentage,
                        Client660DaysAfterCount = client660DaysAfterCount,
                        Client660DaysAfterPercentage = client660DaysAfterPercentage,
                        Client690DaysAfterCount = client690DaysAfterCount,
                        Client690DaysAfterPercentage = client690DaysAfterPercentage,
                        Client720DaysAfterCount = client720DaysAfterCount,
                        Client720DaysAfterPercentage = client720DaysAfterPercentage,
                        Client120DaysAfterRecurrence = client120DaysAfterRecurrence,
                        Client120DaysAfterTicket = client120DaysAfterTicket,
                        Client150DaysAfterRecurrence = client150DaysAfterRecurrence,
                        Client150DaysAfterTicket = client150DaysAfterTicket,
                        Client180DaysAfterRecurrence = client180DaysAfterRecurrence,
                        Client180DaysAfterTicket = client180DaysAfterTicket,
                        Client210DaysAfterRecurrence = client210DaysAfterRecurrence,
                        Client210DaysAfterTicket = client210DaysAfterTicket,
                        Client240DaysAfterRecurrence = client240DaysAfterRecurrence,
                        Client240DaysAfterTicket = client240DaysAfterTicket,
                        Client270DaysAfterRecurrence = client270DaysAfterRecurrence,
                        Client270DaysAfterTicket = client270DaysAfterTicket,
                        Client300DaysAfterRecurrence = client300DaysAfterRecurrence,
                        Client300DaysAfterTicket = client300DaysAfterTicket,
                        Client30DaysAfterRecurrence = client30DaysAfterRecurrence,
                        Client30DaysAfterTicket = client30DaysAfterTicket,
                        Client330DaysAfterRecurrence = client330DaysAfterRecurrence,
                        Client330DaysAfterTicket = client330DaysAfterTicket,
                        Client360DaysAfterRecurrence = client360DaysAfterRecurrence,
                        Client360DaysAfterTicket = client360DaysAfterTicket,
                        Client390DaysAfterRecurrence = client390DaysAfterRecurrence,
                        Client390DaysAfterTicket = client390DaysAfterTicket,
                        Client420DaysAfterRecurrence = client420DaysAfterRecurrence,
                        Client420DaysAfterTicket = client420DaysAfterTicket,
                        Client450DaysAfterRecurrence = client450DaysAfterRecurrence,
                        Client450DaysAfterTicket = client450DaysAfterTicket,
                        Client480DaysAfterRecurrence = client480DaysAfterRecurrence,
                        Client480DaysAfterTicket = client480DaysAfterTicket,
                        Client510DaysAfterRecurrence = client510DaysAfterRecurrence,
                        Client510DaysAfterTicket = client510DaysAfterTicket,
                        Client540DaysAfterRecurrence = client540DaysAfterRecurrence,
                        Client540DaysAfterTicket = client540DaysAfterTicket,
                        Client570DaysAfterRecurrence = client570DaysAfterRecurrence,
                        Client570DaysAfterTicket = client570DaysAfterTicket,
                        Client600DaysAfterRecurrence = client600DaysAfterRecurrence,
                        Client600DaysAfterTicket = client600DaysAfterTicket,
                        Client60DaysAfterRecurrence = client60DaysAfterRecurrence,
                        Client60DaysAfterTicket = client60DaysAfterTicket,
                        Client630DaysAfterRecurrence = client630DaysAfterRecurrence,
                        Client630DaysAfterTicket = client630DaysAfterTicket,
                        Client660DaysAfterRecurrence = client660DaysAfterRecurrence,
                        Client660DaysAfterTicket = client660DaysAfterTicket,
                        Client690DaysAfterRecurrence = client690DaysAfterRecurrence,
                        Client690DaysAfterTicket = client690DaysAfterTicket,
                        Client720DaysAfterRecurrence = client720DaysAfterRecurrence,
                        Client720DaysAfterTicket = client720DaysAfterTicket,
                        Client90DaysAfterRecurrence = client90DaysAfterRecurrence,
                        Client90DaysAfterTicket = client90DaysAfterTicket,
                        MarketingDashboardDataTypeId = 40
                    });
                }
                catch (Exception e)
                {
                    errors.Add(e.Message + ", " + e.InnerException?.Message);
                }
            }

            if (!errors.Any())
                foreach (var item in dataToUpload)
                {
                    Insert(item);
                }
            else
            {
                _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Error, "Error generating marketing dashboard data 40 - " + currentDate.ToString("dd/MM/yyyy"),
                    string.Join("\n\n", errors));
            }
        }

        private void PrepareClientDaysData(ref int client30DaysAfterCount,
            ref int client60DaysAfterCount,
            ref int client90DaysAfterCount,
            ref int client120DaysAfterCount,
            ref int client150DaysAfterCount,
            ref int client180DaysAfterCount,
            ref int client210DaysAfterCount,
            ref int client240DaysAfterCount,
            ref int client270DaysAfterCount,
            ref int client300DaysAfterCount,
            ref int client330DaysAfterCount,
            ref int client360DaysAfterCount,
            ref int client390DaysAfterCount,
            ref int client420DaysAfterCount,
            ref int client450DaysAfterCount,
            ref int client480DaysAfterCount,
            ref int client510DaysAfterCount,
            ref int client540DaysAfterCount,
            ref int client570DaysAfterCount,
            ref int client600DaysAfterCount,
            ref int client630DaysAfterCount,
            ref int client660DaysAfterCount,
            ref int client690DaysAfterCount,
            ref int client720DaysAfterCount,
            ref decimal client30DaysAfterPercentage,
            ref decimal client60DaysAfterPercentage,
            ref decimal client90DaysAfterPercentage,
            ref decimal client120DaysAfterPercentage,
            ref decimal client150DaysAfterPercentage,
            ref decimal client180DaysAfterPercentage,
            ref decimal client210DaysAfterPercentage,
            ref decimal client240DaysAfterPercentage,
            ref decimal client270DaysAfterPercentage,
            ref decimal client300DaysAfterPercentage,
            ref decimal client330DaysAfterPercentage,
            ref decimal client360DaysAfterPercentage,
            ref decimal client390DaysAfterPercentage,
            ref decimal client420DaysAfterPercentage,
            ref decimal client450DaysAfterPercentage,
            ref decimal client480DaysAfterPercentage,
            ref decimal client510DaysAfterPercentage,
            ref decimal client540DaysAfterPercentage,
            ref decimal client570DaysAfterPercentage,
            ref decimal client600DaysAfterPercentage,
            ref decimal client630DaysAfterPercentage,
            ref decimal client660DaysAfterPercentage,
            ref decimal client690DaysAfterPercentage,
            ref decimal client720DaysAfterPercentage,
            ref decimal client30DaysAfterTicket,
            ref decimal client60DaysAfterTicket,
            ref decimal client90DaysAfterTicket,
            ref decimal client120DaysAfterTicket,
            ref decimal client150DaysAfterTicket,
            ref decimal client180DaysAfterTicket,
            ref decimal client210DaysAfterTicket,
            ref decimal client240DaysAfterTicket,
            ref decimal client270DaysAfterTicket,
            ref decimal client300DaysAfterTicket,
            ref decimal client330DaysAfterTicket,
            ref decimal client360DaysAfterTicket,
            ref decimal client390DaysAfterTicket,
            ref decimal client420DaysAfterTicket,
            ref decimal client450DaysAfterTicket,
            ref decimal client480DaysAfterTicket,
            ref decimal client510DaysAfterTicket,
            ref decimal client540DaysAfterTicket,
            ref decimal client570DaysAfterTicket,
            ref decimal client600DaysAfterTicket,
            ref decimal client630DaysAfterTicket,
            ref decimal client660DaysAfterTicket,
            ref decimal client690DaysAfterTicket,
            ref decimal client720DaysAfterTicket,
            ref decimal client30DaysAfterRecurrence,
            ref decimal client60DaysAfterRecurrence,
            ref decimal client90DaysAfterRecurrence,
            ref decimal client120DaysAfterRecurrence,
            ref decimal client150DaysAfterRecurrence,
            ref decimal client180DaysAfterRecurrence,
            ref decimal client210DaysAfterRecurrence,
            ref decimal client240DaysAfterRecurrence,
            ref decimal client270DaysAfterRecurrence,
            ref decimal client300DaysAfterRecurrence,
            ref decimal client330DaysAfterRecurrence,
            ref decimal client360DaysAfterRecurrence,
            ref decimal client390DaysAfterRecurrence,
            ref decimal client420DaysAfterRecurrence,
            ref decimal client450DaysAfterRecurrence,
            ref decimal client480DaysAfterRecurrence,
            ref decimal client510DaysAfterRecurrence,
            ref decimal client540DaysAfterRecurrence,
            ref decimal client570DaysAfterRecurrence,
            ref decimal client600DaysAfterRecurrence,
            ref decimal client630DaysAfterRecurrence,
            ref decimal client660DaysAfterRecurrence,
            ref decimal client690DaysAfterRecurrence,
            ref decimal client720DaysAfterRecurrence,
            List<Order> orders,
            List<int> firstOrderClientIds,
            int firstPedidosCount,
            DateTime endDate,
            int versionCode,
            bool is30Days)
        {
            var controlDateInit = endDate.AddDays(1);
            var controlDateEnd = endDate.AddDays(is30Days ? 30 : 90);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 1 y 30 días posteriores al periodo.
            client30DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 31 y 60 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 31 : 1);
            controlDateEnd = endDate.AddDays(is30Days ? 60 : 120);
            client60DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 61 y 90 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 61 : 30);
            controlDateEnd = endDate.AddDays(is30Days ? 90 : 150);
            client90DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 91 y 120 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 91 : 60);
            controlDateEnd = endDate.AddDays(is30Days ? 120 : 180);
            client120DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 121 y 150 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 121 : 90);
            controlDateEnd = endDate.AddDays(is30Days ? 150 : 210);
            client150DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 151 y 180 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 151 : 120);
            controlDateEnd = endDate.AddDays(is30Days ? 180 : 240);
            client180DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 181 y 210 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 181 : 150);
            controlDateEnd = endDate.AddDays(is30Days ? 218 : 270);
            client210DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 211 y 240 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 211 : 180);
            controlDateEnd = endDate.AddDays(is30Days ? 240 : 300);
            client240DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 241 y 270 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 241 : 210);
            controlDateEnd = endDate.AddDays(is30Days ? 270 : 330);
            client270DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 271 y 300 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 271 : 240);
            controlDateEnd = endDate.AddDays(is30Days ? 300 : 360);
            client300DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 301 y 330 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 301 : 270);
            controlDateEnd = endDate.AddDays(is30Days ? 330 : 390);
            client330DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 331 y 360 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 331 : 300);
            controlDateEnd = endDate.AddDays(is30Days ? 360 : 420);
            client360DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 361 y 390 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 361 : 330);
            controlDateEnd = endDate.AddDays(is30Days ? 390 : 450);
            client390DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 391 y 420 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 391 : 360);
            controlDateEnd = endDate.AddDays(is30Days ? 420 : 480);
            client420DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 421 y 450 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 421 : 390);
            controlDateEnd = endDate.AddDays(is30Days ? 450 : 510);
            client450DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 451 y 480 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 451 : 420);
            controlDateEnd = endDate.AddDays(is30Days ? 480 : 540);
            client480DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 481 y 510 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 481 : 450);
            controlDateEnd = endDate.AddDays(is30Days ? 510 : 570);
            client510DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 511 y 540 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 511 : 480);
            controlDateEnd = endDate.AddDays(is30Days ? 540 : 600);
            client540DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 541 y 570 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 541 : 510);
            controlDateEnd = endDate.AddDays(is30Days ? 570 : 630);
            client570DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 571 y 600 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 571 : 540);
            controlDateEnd = endDate.AddDays(is30Days ? 600 : 660);
            client600DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 601 y 630 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 601 : 570);
            controlDateEnd = endDate.AddDays(is30Days ? 630 : 690);
            client630DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 631 y 660 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 631 : 600);
            controlDateEnd = endDate.AddDays(is30Days ? 660 : 720);
            client660DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 661 y 690 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 661 : 630);
            controlDateEnd = endDate.AddDays(is30Days ? 690 : 750);
            client690DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 691 y 720 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 691 : 660);
            controlDateEnd = endDate.AddDays(is30Days ? 720 : 780);
            client720DaysAfterCount = GetOrdersCount(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            client30DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client30DaysAfterCount / (decimal)firstPedidosCount;
            client60DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client60DaysAfterCount / (decimal)firstPedidosCount;
            client90DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client90DaysAfterCount / (decimal)firstPedidosCount;
            client120DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client120DaysAfterCount / (decimal)firstPedidosCount;
            client150DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client150DaysAfterCount / (decimal)firstPedidosCount;
            client180DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client180DaysAfterCount / (decimal)firstPedidosCount;
            client210DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client210DaysAfterCount / (decimal)firstPedidosCount;
            client240DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client240DaysAfterCount / (decimal)firstPedidosCount;
            client270DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client270DaysAfterCount / (decimal)firstPedidosCount;
            client300DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client300DaysAfterCount / (decimal)firstPedidosCount;
            client330DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client330DaysAfterCount / (decimal)firstPedidosCount;
            client360DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client360DaysAfterCount / (decimal)firstPedidosCount;
            client390DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client390DaysAfterCount / (decimal)firstPedidosCount;
            client420DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client420DaysAfterCount / (decimal)firstPedidosCount;
            client450DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client450DaysAfterCount / (decimal)firstPedidosCount;
            client480DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client480DaysAfterCount / (decimal)firstPedidosCount;
            client510DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client510DaysAfterCount / (decimal)firstPedidosCount;
            client540DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client540DaysAfterCount / (decimal)firstPedidosCount;
            client570DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client570DaysAfterCount / (decimal)firstPedidosCount;
            client600DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client600DaysAfterCount / (decimal)firstPedidosCount;
            client630DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client630DaysAfterCount / (decimal)firstPedidosCount;
            client660DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client660DaysAfterCount / (decimal)firstPedidosCount;
            client690DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client690DaysAfterCount / (decimal)firstPedidosCount;
            client720DaysAfterPercentage = firstPedidosCount == 0 ? 0 : (decimal)client720DaysAfterCount / (decimal)firstPedidosCount;

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 1 y 30 días posteriores al periodo.
            controlDateInit = endDate.AddDays(1);
            controlDateEnd = endDate.AddDays(is30Days ? 30 : 90);
            var pedidos30Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client30DaysAfterTicket = pedidos30Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(1);
            controlDateEnd = endDate.AddDays(is30Days ? 30 : 30);
            var pedidos30 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 31 y 60 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 31 : 1);
            controlDateEnd = endDate.AddDays(is30Days ? 60 : 120);
            var pedidos60Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client60DaysAfterTicket = pedidos60Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(31);
            controlDateEnd = endDate.AddDays(60);
            var pedidos60 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 61 y 90 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 61 : 30);
            controlDateEnd = endDate.AddDays(is30Days ? 90 : 150);
            var pedidos90Ímp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client90DaysAfterTicket = pedidos90Ímp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(61);
            controlDateEnd = endDate.AddDays(90);
            var pedidos90 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 91 y 120 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 91 : 60);
            controlDateEnd = endDate.AddDays(is30Days ? 120 : 180);
            var pedidos120Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client120DaysAfterTicket = pedidos120Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(91);
            controlDateEnd = endDate.AddDays(120);
            var pedidos120 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 121 y 150 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 121 : 90);
            controlDateEnd = endDate.AddDays(is30Days ? 150 : 210);
            var pedidos150Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client150DaysAfterTicket = pedidos150Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(121);
            controlDateEnd = endDate.AddDays(150);
            var pedidos150 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 151 y 180 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 151 : 120);
            controlDateEnd = endDate.AddDays(is30Days ? 180 : 240);
            var pedidos180Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client180DaysAfterTicket = pedidos180Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(151);
            controlDateEnd = endDate.AddDays(180);
            var pedidos180 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 181 y 210 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 181 : 150);
            controlDateEnd = endDate.AddDays(is30Days ? 210 : 270);
            var pedidos210Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client210DaysAfterTicket = pedidos210Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(181);
            controlDateEnd = endDate.AddDays(210);
            var pedidos210 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 211 y 240 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 211 : 180);
            controlDateEnd = endDate.AddDays(is30Days ? 240 : 300);
            var pedidos240Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client240DaysAfterTicket = pedidos240Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(211);
            controlDateEnd = endDate.AddDays(240);
            var pedidos240 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 241 y 270 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 241 : 210);
            controlDateEnd = endDate.AddDays(is30Days ? 270 : 330);
            var pedidos270Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client270DaysAfterTicket = pedidos270Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(241);
            controlDateEnd = endDate.AddDays(270);
            var pedidos270 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 271 y 300 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 271 : 240);
            controlDateEnd = endDate.AddDays(is30Days ? 300 : 360);
            var pedidos300Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client300DaysAfterTicket = pedidos300Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(271);
            controlDateEnd = endDate.AddDays(300);
            var pedidos300 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 301 y 330 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 301 : 270);
            controlDateEnd = endDate.AddDays(is30Days ? 330 : 390);
            var pedidos330Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client330DaysAfterTicket = pedidos330Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(301);
            controlDateEnd = endDate.AddDays(330);
            var pedidos330 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 331 y 360 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 331 : 300);
            controlDateEnd = endDate.AddDays(is30Days ? 360 : 420);
            var pedidos360Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client360DaysAfterTicket = pedidos360Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(331);
            controlDateEnd = endDate.AddDays(360);
            var pedidos360 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 361 y 390 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 361 : 330);
            controlDateEnd = endDate.AddDays(is30Days ? 390 : 450);
            var pedidos390Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client390DaysAfterTicket = pedidos390Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(361);
            controlDateEnd = endDate.AddDays(390);
            var pedidos390 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 391 y 420 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 391 : 360);
            controlDateEnd = endDate.AddDays(is30Days ? 420 : 480);
            var pedidos420Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client420DaysAfterTicket = pedidos420Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(391);
            controlDateEnd = endDate.AddDays(420);
            var pedidos420 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 421 y 450 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 421 : 390);
            controlDateEnd = endDate.AddDays(is30Days ? 450 : 510);
            var pedidos450Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client450DaysAfterTicket = pedidos450Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(421);
            controlDateEnd = endDate.AddDays(450);
            var pedidos450 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 451 y 480 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 451 : 420);
            controlDateEnd = endDate.AddDays(is30Days ? 480 : 540);
            var pedidos480Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client480DaysAfterTicket = pedidos480Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(451);
            controlDateEnd = endDate.AddDays(480);
            var pedidos480 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 481 y 510 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 481 : 450);
            controlDateEnd = endDate.AddDays(is30Days ? 510 : 570);
            var pedidos510Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client510DaysAfterTicket = pedidos510Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(481);
            controlDateEnd = endDate.AddDays(510);
            var pedidos510 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 511 y 540 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 511 : 480);
            controlDateEnd = endDate.AddDays(is30Days ? 540 : 600);
            var pedidos540Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client540DaysAfterTicket = pedidos540Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(511);
            controlDateEnd = endDate.AddDays(540);
            var pedidos540 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 541 y 570 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 541 : 510);
            controlDateEnd = endDate.AddDays(is30Days ? 570 : 630);
            var pedidos570Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client570DaysAfterTicket = pedidos570Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(541);
            controlDateEnd = endDate.AddDays(570);
            var pedidos570 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 571 y 600 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 571 : 540);
            controlDateEnd = endDate.AddDays(is30Days ? 600 : 660);
            var pedidos600Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client600DaysAfterTicket = pedidos600Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(571);
            controlDateEnd = endDate.AddDays(600);
            var pedidos600 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 601 y 630 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 601 : 570);
            controlDateEnd = endDate.AddDays(is30Days ? 630 : 690);
            var pedidos630Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client630DaysAfterTicket = pedidos630Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(601);
            controlDateEnd = endDate.AddDays(630);
            var pedidos630 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 631 y 660 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 631 : 600);
            controlDateEnd = endDate.AddDays(is30Days ? 660 : 720);
            var pedidos660Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client660DaysAfterTicket = pedidos660Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(631);
            controlDateEnd = endDate.AddDays(660);
            var pedidos660 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 661 y 690 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 661 : 630);
            controlDateEnd = endDate.AddDays(is30Days ? 690 : 750);
            var pedidos690Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client690DaysAfterTicket = pedidos690Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(661);
            controlDateEnd = endDate.AddDays(690);
            var pedidos690 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            // De los clientes que hicieron su primer pedido, cuántos hicieron al menos una compra entre 691 y 720 días posteriores al periodo.
            controlDateInit = endDate.AddDays(is30Days ? 691 : 660);
            controlDateEnd = endDate.AddDays(is30Days ? 720 : 780);
            var pedidos720Imp = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);
            client720DaysAfterTicket = pedidos720Imp
                .Select(x => x.Select(y => y.OrderTotal).DefaultIfEmpty().Sum()).DefaultIfEmpty().Average();

            controlDateInit = endDate.AddDays(691);
            controlDateEnd = endDate.AddDays(720);
            var pedidos720 = GetPedidos(orders, controlDateInit, controlDateEnd, firstOrderClientIds, versionCode);

            client30DaysAfterRecurrence = (decimal)pedidos30.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client60DaysAfterRecurrence = (decimal)pedidos60.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client90DaysAfterRecurrence = (decimal)pedidos90.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client120DaysAfterRecurrence = (decimal)pedidos120.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client150DaysAfterRecurrence = (decimal)pedidos150.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client180DaysAfterRecurrence = (decimal)pedidos180.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client210DaysAfterRecurrence = (decimal)pedidos210.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client240DaysAfterRecurrence = (decimal)pedidos240.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client270DaysAfterRecurrence = (decimal)pedidos270.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client300DaysAfterRecurrence = (decimal)pedidos300.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client330DaysAfterRecurrence = (decimal)pedidos330.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client360DaysAfterRecurrence = (decimal)pedidos360.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client390DaysAfterRecurrence = (decimal)pedidos390.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client420DaysAfterRecurrence = (decimal)pedidos420.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client450DaysAfterRecurrence = (decimal)pedidos450.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client480DaysAfterRecurrence = (decimal)pedidos480.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client510DaysAfterRecurrence = (decimal)pedidos510.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client540DaysAfterRecurrence = (decimal)pedidos540.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client570DaysAfterRecurrence = (decimal)pedidos570.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client600DaysAfterRecurrence = (decimal)pedidos600.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client630DaysAfterRecurrence = (decimal)pedidos630.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client660DaysAfterRecurrence = (decimal)pedidos660.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client690DaysAfterRecurrence = (decimal)pedidos690.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
            client720DaysAfterRecurrence = (decimal)pedidos720.GroupBy(x => x.Key.CustomerId).Select(x => x.DefaultIfEmpty().Count()).DefaultIfEmpty().Average();
        }

        /// <summary>
        /// Generate automatic expenses information models
        /// </summary>
        /// <param name="controlDate"></param>
        public void GenerateAutomaticExpensesDashboardData(DateTime controlDate, bool useAsSingle = true)
        {
            var orders = OrderUtils.GetFilteredOrders(_orderService);
            var firstDate = controlDate.AddDays(-(int)controlDate.DayOfWeek).AddDays(1);
            var createdAutomaticExpenses = _marketingAutomaticExpense.GetAll()
                .Where(x => !x.Deleted).ToList();
            if (useAsSingle)
            {
                var lastDate = controlDate.AddDays(6);
                orders = orders
                .Where(x => firstDate <= x.SelectedShippingDate && x.SelectedShippingDate <= lastDate);
            }
            else
            {
                orders = orders
                .Where(x => firstDate >= x.SelectedShippingDate);
            }

            if (orders.Count() > 0)
            {
                var automaticExpenses = new List<AutomaticExpensesModel>();
                var discountsOfOrders = orders.SelectMany(x => x.DiscountUsageHistory).ToList();
                var balances = _customerBalanceService.GetAllCustomerBalancesQuery()
                    .Where(x => !x.Deleted).ToList();

                var weeks = new List<DateTime>();
                var firstOrderDate = orders.OrderBy(x => x.SelectedShippingDate).FirstOrDefault().SelectedShippingDate.Value;
                var currentInitDate = firstOrderDate.AddDays(-(int)firstOrderDate.DayOfWeek).AddDays(1);
                weeks.Add(currentInitDate);

                if (currentInitDate < controlDate && !useAsSingle)
                {
                    do
                    {
                        currentInitDate = currentInitDate.AddDays(7);
                        weeks.Add(currentInitDate);
                    } while (currentInitDate < controlDate);
                }

                var count = 0;
                try
                {
                    for (int i = count; i < weeks.Count; i++)
                    {
                        var initDate = weeks[i];
                        var endDate = initDate.AddDays(6);
                        var ordersOfWeek = orders.Where(x => x.SelectedShippingDate.Value >= initDate && x.SelectedShippingDate.Value <= endDate)
                            .ToList();
                        var orderIdsOfWeek = ordersOfWeek.Select(x => x.Id).ToList();
                        var orderItemsOfWeek = _orderService.GetAllOrderItemsQuery()
                            .Where(x => orderIdsOfWeek.Contains(x.OrderId));
                        var discountsHistoryInfo = _discountService.GetAllDiscountUsageHistoryQuery()
                            .Where(x => orderIdsOfWeek.Contains(x.OrderId));
                        //var discountIds = discountsHistoryInfo.Select(x => x.DiscountId).ToList();
                        //var discounts = _discountService.GetAllDiscounts(showHidden: true)
                        //    .Where(x => discountIds.Contains(x.Id))
                        //    .ToList();
                        var discountsOfWeek = discountsHistoryInfo
                            .Select(x => new
                            {
                                x.Id,
                                x.Discount,
                            }).ToList();
                        var productCategories = _categoryService.GetProductCategoriesQuery();

                        //var discountsByProducts = new List<DiscountsByProduct>();
                        //var discountsByCategoryOrProduct =
                        //    discountsOfWeek.Where(x => x.Discount.DiscountType == DiscountType.AssignedToCategories ||
                        //    x.Discount.DiscountType == DiscountType.AssignedToSkus)
                        //    .ToList();
                        //foreach (var discountHistory in discountsByCategoryOrProduct)
                        //{
                        //    if (discountHistory.Discount.DiscountType == DiscountType.AssignedToSkus)
                        //    {
                        //        var productIdsOfDiscount = discountHistory.Discount.AppliedToProducts
                        //            .Where(x => !x.Deleted).Select(x => x.Id).ToList();
                        //        var productsInOrderItems = orderItemsOfWeek.Where(x => productIdsOfDiscount.Contains(x.ProductId)).ToList();
                        //        var productIdsInOrderItems = productsInOrderItems.Select(x => x.ProductId).ToList();
                        //        var categories = productsInOrderItems
                        //            .SelectMany(x => x.Product.ProductCategories)
                        //            .Select(x => x.Category)
                        //            .Where(x => x.ParentCategoryId == 0)
                        //            .GroupBy(x => x.Id)
                        //            .ToList();
                        //        foreach (var category in categories)
                        //        {
                        //            var existingModel = discountsByProducts.Where(x => x.CategoryId == category.Key).FirstOrDefault();
                        //            var productIdsOfCategory = productCategories.Where(x => productIdsInOrderItems.Contains(x.ProductId))
                        //                .Select(x => x.ProductId).ToList();
                        //            var orderItemsOfCurrentCategory = productsInOrderItems
                        //                .Where(x => productIdsOfCategory.Contains(x.ProductId))
                        //                .ToList();
                        //            if (existingModel == null)
                        //            {
                        //                discountsByProducts.Add(new DiscountsByProduct
                        //                {
                        //                    CategoryId = category.Key,
                        //                    CategoryName = category.FirstOrDefault().Name,
                        //                    Amount = orderItemsOfCurrentCategory.Select(x => x.DiscountAmountInclTax).DefaultIfEmpty().Sum()
                        //                });
                        //            }
                        //            else
                        //                existingModel.Amount += orderItemsOfCurrentCategory.Select(x => x.DiscountAmountInclTax).DefaultIfEmpty().Sum();
                        //        }

                        //    }
                        //    else if (discountHistory.Discount.DiscountType == DiscountType.AssignedToCategories)
                        //    {
                        //        var categories = discountHistory.Discount.AppliedToCategories
                        //            .Where(x => !x.Deleted && x.ParentCategoryId == 0).ToList();
                        //        foreach (var category in categories)
                        //        {
                        //            var existingModel = discountsByProducts.Where(x => x.CategoryId == category.Id).FirstOrDefault();
                        //            var productIdsOfCategory = productCategories.Where(x => category.Id == x.CategoryId)
                        //                .Select(x => x.ProductId).ToList();
                        //            var orderItemsOfCurrentCategory = orderItemsOfWeek
                        //                .Where(x => productIdsOfCategory.Contains(x.ProductId))
                        //                .ToList();
                        //            if (existingModel == null)
                        //            {
                        //                discountsByProducts.Add(new DiscountsByProduct
                        //                {
                        //                    CategoryId = category.Id,
                        //                    CategoryName = category.Name,
                        //                    Amount = decimal.Round(orderItemsOfCurrentCategory.Select(x => x.DiscountAmountInclTax).DefaultIfEmpty().Sum(), 2)
                        //                });
                        //            }
                        //            else
                        //                existingModel.Amount += orderItemsOfCurrentCategory.Select(x => x.DiscountAmountInclTax).DefaultIfEmpty().Sum();
                        //        }
                        //    }
                        //}

                        var discountsByShipping =
                            discountsOfWeek.Where(x => x.Discount.DiscountType == DiscountType.AssignedToShipping)
                            .ToList();
                        var discountsByShippingAmount = decimal.Round(discountsByShipping.Select(x => x.Discount.DiscountAmount).DefaultIfEmpty().Sum(), 2);

                        var balancesAmount = decimal.Round(balances.Where(x => initDate.ToUniversalTime() <= x.CreatedOnUtc && x.CreatedOnUtc <= endDate.ToUniversalTime())
                            .Select(x => x.Amount).DefaultIfEmpty().Sum(), 2);

                        var discountsByCoupons = decimal.Round(ordersOfWeek.Select(x => x.OrderDiscount).DefaultIfEmpty().Sum() +
                            orderItemsOfWeek.Select(x => x.DiscountAmountInclTax).DefaultIfEmpty().Sum() +
                            balancesAmount, 2);

                        var discountsByProducts = decimal.Round(orderItemsOfWeek.Select(x => x.DiscountAmountInclTax).DefaultIfEmpty().Sum(), 2);

                        var createdAutomaticExpense = createdAutomaticExpenses
                            .Where(x => x.InitDate == initDate).FirstOrDefault();
                        if (createdAutomaticExpense == null)
                        {
                            var automaticExpense = new MarketingAutomaticExpense
                            {
                                InitDate = initDate,
                                DiscountByCoupons = discountsByCoupons,
                                DiscountByShipping = discountsByShippingAmount,
                                DiscountByProducts = discountsByProducts,
                                Balances = balancesAmount,
                            };
                            _marketingAutomaticExpense.Insert(automaticExpense);

                            //foreach (var discountsByProduct in discountsByProducts)
                            //{
                            //    var discountExpense = new MarketingDiscountExpense
                            //    {
                            //        MarketingAutomaticExpenseId = automaticExpense.Id,
                            //        EntityId = discountsByProduct.CategoryId,
                            //        EntityTypeId = (int)MarketingDiscountExpenseType.Category,
                            //        Amount = discountsByProduct.Amount,
                            //    };
                            //    _marketingDiscountExpenseService.Insert(discountExpense);
                            //}
                        }
                        else
                        {
                            var doneDiscountExpenses = createdAutomaticExpense.MarketingDiscountExpenses
                                .Where(x => x.Deleted).ToList();
                            var doneDiscountExpenseIds = doneDiscountExpenses.Select(x => x.EntityId).ToList();
                            //var currentCategoryIds = discountsByProducts.Select(x => x.CategoryId).ToList();
                            //var differentCategories = doneDiscountExpenseIds.Except(currentCategoryIds);

                            if (createdAutomaticExpense.DiscountByCoupons != discountsByCoupons ||
                                createdAutomaticExpense.DiscountByShipping != discountsByShippingAmount ||
                                createdAutomaticExpense.Balances != balancesAmount
                                //|| differentCategories.Any()
                                )
                            {
                                createdAutomaticExpense.DiscountByCoupons = discountsByCoupons;
                                createdAutomaticExpense.DiscountByShipping = discountsByShippingAmount;
                                createdAutomaticExpense.Balances = balancesAmount;

                                //foreach (var doneDiscountExpense in doneDiscountExpenses)
                                //    _marketingDiscountExpenseService.Delete(doneDiscountExpense);

                                //foreach (var discountsByProduct in discountsByProducts)
                                //{
                                //    var discountExpense = new MarketingDiscountExpense
                                //    {
                                //        MarketingAutomaticExpenseId = createdAutomaticExpense.Id,
                                //        EntityId = discountsByProduct.CategoryId,
                                //        EntityTypeId = (int)MarketingDiscountExpenseType.Category,
                                //        Amount = discountsByProduct.Amount,
                                //    };
                                //    _marketingDiscountExpenseService.Insert(discountExpense);
                                //}
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _ = e;
                }
            }
        }

        private IEnumerable<IGrouping<dynamic, Order>> GetPedidos(List<Order> orders, DateTime controlDateInit, DateTime controlDateEnd, List<int> firstOrderClientIds, int versionCode)
        {
            if (versionCode == 0)
            {
                return OrderUtils.GetPedidosGroupByList(orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList());
            }
            else
            {
                return OrderUtils.GetPedidosGroupByList(orders.Where(x => x.CreatedOnUtc.ToLocalTime() >= controlDateInit && x.CreatedOnUtc.ToLocalTime() <= controlDateEnd)
                .Where(x => firstOrderClientIds.Contains(x.CustomerId)).ToList(), versionCode);
            }
        }

        private int GetOrdersCount(List<Order> orders, DateTime controlDateInit, DateTime controlDateEnd, List<int> firstOrderClientIds, int versionCode)
        {
            if (versionCode == 0)
            {
                return orders.Where(x => x.SelectedShippingDate >= controlDateInit && x.SelectedShippingDate <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();
            }
            else if (versionCode == 1)
            {
                return orders.Where(x => x.CreatedOnUtc.ToLocalTime() >= controlDateInit && x.CreatedOnUtc.ToLocalTime() <= controlDateEnd)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .Where(x => firstOrderClientIds.Contains(x))
                .Count();
            }
            return 0;
        }

        private void PrepareFirstAndLastOrders(List<Order> firstOrders,
            List<Order> notFirstOrders,
            List<Order> allOrders,
            DateTime initDate,
            List<IGrouping<int, Order>> dateOrdersGroupedByCustomer,
            int versionCode)
        {
            List<Order> previousOrders = new List<Order>();
            if (versionCode == 0)
            {
                previousOrders = allOrders.Where(x => x.SelectedShippingDate < initDate).ToList();
            }
            else if (versionCode == 1)
            {
                previousOrders = allOrders.Where(x => x.CreatedOnUtc.ToLocalTime() < initDate).ToList();
            }

            foreach (var itemGroup in dateOrdersGroupedByCustomer)
            {
                if (versionCode == 0)
                {
                    if (!previousOrders.Where(x => x.CustomerId == itemGroup.Key).Any())
                    {
                        var firstPedido = OrderUtils.GetPedidosGroupByList(itemGroup.Select(x => x).ToList())
                            .OrderBy(x => x.Key.SelectedShippingDate)
                            .FirstOrDefault();
                        firstOrders.AddRange(firstPedido.Select(x => x));
                    }
                    else
                    {
                        notFirstOrders.AddRange(itemGroup.Select(x => x));
                    }
                }
                else if (versionCode == 1)
                {
                    if (!previousOrders.Where(x => x.CustomerId == itemGroup.Key).Any())
                    {
                        var firstPedido = itemGroup.Select(x => x).ToList()
                            .OrderBy(x => x.CreatedOnUtc)
                            .FirstOrDefault();
                        firstOrders.Add(firstPedido);
                    }
                    else
                    {
                        notFirstOrders.AddRange(itemGroup.Select(x => x));
                    }
                }
            }
        }
    }
}