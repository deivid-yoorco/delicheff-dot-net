using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Linq;
using Teed.Plugin.Api.Domain.Notifications;

namespace Teed.Plugin.Api.Services
{
    public class QueuedNotificationService
    {
        private readonly IRepository<QueuedNotification> _db;
        private readonly IEventPublisher _eventPublisher;

        public QueuedNotificationService(IRepository<QueuedNotification> db, IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<QueuedNotification> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public void Insert(QueuedNotification entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(QueuedNotification entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(QueuedNotification entity)
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
