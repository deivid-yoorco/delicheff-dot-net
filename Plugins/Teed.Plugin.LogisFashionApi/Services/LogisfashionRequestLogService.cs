using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Logisfashion.Domain;

namespace Teed.Plugin.Logisfashion.Services
{
    public class LogisfashionRequestLogService
    {
        private readonly IRepository<LogisfashionRequestLog> _db;
        private readonly IEventPublisher _eventPublisher;

        public LogisfashionRequestLogService(
            IRepository<LogisfashionRequestLog> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<LogisfashionRequestLog> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public LogisfashionRequestLog GetById(int id)
        {
            return _db.Table.Where(x => x.Id == id).FirstOrDefault();
        }

        public void Delete(LogisfashionRequestLog entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.Deleted = true;
            entity.UpdatedOnUtc = DateTime.UtcNow;
            _db.Update(entity);
            _eventPublisher.EntityDeleted(entity);
        }

        public void Insert(LogisfashionRequestLog entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(LogisfashionRequestLog entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}
