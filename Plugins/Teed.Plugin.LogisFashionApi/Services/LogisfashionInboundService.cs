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
    public class LogisfashionInboundService
    {
        private readonly IRepository<LogisfashionInbound> _db;
        private readonly IEventPublisher _eventPublisher;

        public LogisfashionInboundService(
            IRepository<LogisfashionInbound> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<LogisfashionInbound> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public LogisfashionInbound GetById(int id)
        {
            return _db.Table.Where(x => x.Id == id).FirstOrDefault();
        }

        public LogisfashionInbound GetByPONumber(string poNumber)
        {
            return _db.Table.Where(x => x.PONumber == poNumber).FirstOrDefault();
        }

        public void Insert(LogisfashionInbound entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;
            entity.GuidId = Guid.NewGuid();

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(LogisfashionInbound entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}
