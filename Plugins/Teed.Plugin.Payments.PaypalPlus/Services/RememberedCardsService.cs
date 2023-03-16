using Nop.Core;
using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Linq;
using Teed.Plugin.Payments.PaypalPlus.Domain;

namespace Teed.Plugin.Payments.PaypalPlus.Services
{
    public class RememberedCardsService
    {
        private readonly IRepository<RememberedCards> _db;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkContext _workContext;

        public RememberedCardsService(
            IRepository<RememberedCards> db,
            IEventPublisher eventPublisher,
            IWorkContext workContext)
        {
            _db = db;
            _eventPublisher = eventPublisher;
            _workContext = workContext;
        }

        public IQueryable<RememberedCards> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public void Insert(RememberedCards entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(RememberedCards entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}