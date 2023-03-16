using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Services
{
    public class OrderItemLogService
    {
        private readonly IRepository<OrderItemLog> _db;
        private readonly IEventPublisher _eventPublisher;

        public OrderItemLogService(
            IRepository<OrderItemLog> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<OrderItemLog> GetAll()
        {
            return _db.Table;
        }

        public void Insert(OrderItemLog entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(OrderItemLog entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}
