using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Linq;
using Teed.Plugin.Manager.Domain.PurchaseOrders;

namespace Teed.Plugin.Manager.Services
{
    public class PurchaseOrderProductService
    {
        private readonly IRepository<PurchaseOrderProduct> _db;
        private readonly IEventPublisher _eventPublisher;

        public PurchaseOrderProductService(IRepository<PurchaseOrderProduct> db, IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<PurchaseOrderProduct> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public PurchaseOrderProduct GetById(int id)
        {
            return _db.Table.Where(x => !x.Deleted && x.Id == id).FirstOrDefault();
        }

        public void Insert(PurchaseOrderProduct entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(PurchaseOrderProduct entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(PurchaseOrderProduct entity)
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
