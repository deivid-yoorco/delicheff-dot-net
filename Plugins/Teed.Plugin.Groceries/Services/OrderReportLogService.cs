using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Linq;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Services
{
    public class OrderReportLogService
    {
        private readonly IRepository<OrderReportLog> _db;
        private readonly IEventPublisher _eventPublisher;

        public OrderReportLogService(
            IRepository<OrderReportLog> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<OrderReportLog> GetAll()
        {
            return _db.TableNoTracking;
        }

        public void Insert(OrderReportLog entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }
    }
}