using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Linq;
using Teed.Plugin.Groceries.Domain.TipsWithCards;

namespace Teed.Plugin.Groceries.Services
{
    public class TipsWithCardService
    {
        private readonly IRepository<TipsWithCard> _db;
        private readonly IEventPublisher _eventPublisher;

        public TipsWithCardService(
            IRepository<TipsWithCard> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<TipsWithCard> GetAll()
        {
            return _db.Table;
        }

        public void Insert(TipsWithCard entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(TipsWithCard entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}
