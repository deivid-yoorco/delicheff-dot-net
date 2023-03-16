using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Linq;
using Teed.Plugin.Groceries.Domain.Corcel;

namespace Teed.Plugin.Groceries.Services
{
    public class CorcelProductService
    {
        private readonly IRepository<CorcelProduct> _db;
        private readonly IEventPublisher _eventPublisher;

        public CorcelProductService(
            IRepository<CorcelProduct> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<CorcelProduct> GetAll(bool showDeleted = false)
        {
            if (!showDeleted)
                return _db.Table.Where(x => !x.Deleted);
            else
                return _db.Table;
        }

        public void Insert(CorcelProduct entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(CorcelProduct entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}
