using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Linq;
using Teed.Plugin.Groceries.Domain.RescheduledOrderLogs;

namespace Teed.Plugin.Groceries.Services
{
    public class RescheduledOrderLogService
    {
        private readonly IRepository<RescheduledOrderLog> _db;
        private readonly IEventPublisher _eventPublisher;

        public RescheduledOrderLogService(
            IRepository<RescheduledOrderLog> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<RescheduledOrderLog> GetAll()
        {
            return _db.Table;
        }

        public void Insert(RescheduledOrderLog entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(RescheduledOrderLog entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}
