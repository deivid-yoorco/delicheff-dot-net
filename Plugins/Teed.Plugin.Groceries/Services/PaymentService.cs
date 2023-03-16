using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.Franchise;
using Teed.Plugin.Groceries.Domain.OrderReports;
using Teed.Plugin.Groceries.Domain.PenaltiesCatalog;
using Teed.Plugin.Groceries.Models.Franchise;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Services
{
    public class PaymentService
    {
        private readonly IRepository<Payment> _db;
        private readonly IEventPublisher _eventPublisher;
        private readonly IOrderService _orderService;
        private readonly IncidentsService _incidentsService;
        private readonly ISettingService _settingService;
        private readonly ShippingRouteService _shippingRouteService;
        private readonly PenaltiesCatalogService _penaltiesCatalogService;
        private readonly FranchiseService _franchiseService;
        private readonly ShippingVehicleRouteService _shippingVehicleRouteService;
        private readonly ShippingVehicleService _shippingVehicleService;
        private readonly IWorkContext _workContext;

        public PaymentService(
            IRepository<Payment> db,
            IEventPublisher eventPublisher,
            IOrderService orderService,
            IncidentsService incidentsService,
            ISettingService settingService,
            ShippingRouteService shippingRouteService,
            PenaltiesCatalogService penaltiesCatalogService,
            FranchiseService franchiseService,
            ShippingVehicleRouteService shippingVehicleRouteService,
            ShippingVehicleService shippingVehicleService,
            IWorkContext workContext)
        {
            _db = db;
            _eventPublisher = eventPublisher;
            _orderService = orderService;
            _incidentsService = incidentsService;
            _settingService = settingService;
            _shippingRouteService = shippingRouteService;
            _penaltiesCatalogService = penaltiesCatalogService;
            _franchiseService = franchiseService;
            _workContext = workContext;
            _shippingVehicleRouteService = shippingVehicleRouteService;
            _shippingVehicleService = shippingVehicleService;
        }

        public IQueryable<Payment> GetAll(bool deleted = false)
        {
            if (deleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public void Insert(Payment entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(Payment entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(Payment entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.Deleted = true;
            Update(entity);

            //event notification
            _eventPublisher.EntityDeleted(entity);
        }
    }
}