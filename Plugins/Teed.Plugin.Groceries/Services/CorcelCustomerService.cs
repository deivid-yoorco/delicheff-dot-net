using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Linq;
using Teed.Plugin.Groceries.Domain.Corcel;

namespace Teed.Plugin.Groceries.Services
{
    public class CorcelCustomerService
    {
        private readonly IRepository<CorcelCustomer> _db;
        private readonly IEventPublisher _eventPublisher;

        public CorcelCustomerService(
            IRepository<CorcelCustomer> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<CorcelCustomer> GetAll(bool showDeleted = false)
        {
            if (!showDeleted)
                return _db.Table.Where(x => !x.Deleted);
            else
                return _db.Table;
        }

        public void Insert(CorcelCustomer entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(CorcelCustomer entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}
