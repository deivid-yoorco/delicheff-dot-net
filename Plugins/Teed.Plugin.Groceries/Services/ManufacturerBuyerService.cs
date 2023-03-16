using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Linq;
using Teed.Plugin.Groceries.Domain.OrderItemBuyers;

namespace Teed.Plugin.Groceries.Services
{
    public class ManufacturerBuyerService
    {
        private readonly IRepository<ManufacturerBuyer> _db;
        private readonly IEventPublisher _eventPublisher;

        public ManufacturerBuyerService(
            IRepository<ManufacturerBuyer> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<ManufacturerBuyer> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public void Insert(ManufacturerBuyer entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(ManufacturerBuyer entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(ManufacturerBuyer entity)
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