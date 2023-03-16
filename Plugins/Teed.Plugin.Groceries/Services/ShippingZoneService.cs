﻿using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.OrderReports;
using Teed.Plugin.Groceries.Domain.ShippingZones;

namespace Teed.Plugin.Groceries.Services
{
    public class ShippingZoneService
    {
        private readonly IRepository<ShippingZone> _db;
        private readonly IEventPublisher _eventPublisher;

        public ShippingZoneService(
            IRepository<ShippingZone> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<ShippingZone> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public void Insert(ShippingZone entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(ShippingZone entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(ShippingZone entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;
            entity.Deleted = true;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}