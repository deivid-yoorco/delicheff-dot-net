using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.MarkedBuyerItems;

namespace Teed.Plugin.Groceries.Services
{
    public class MarkedBuyerItemService
    {
        private readonly IRepository<MarkedBuyerItem> _db;
        private readonly IEventPublisher _eventPublisher;

        public MarkedBuyerItemService(
            IRepository<MarkedBuyerItem> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }
        public IQueryable<MarkedBuyerItem> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public void Insert(MarkedBuyerItem entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(MarkedBuyerItem entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(MarkedBuyerItem entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;
            entity.Deleted = true;

            _db.Update(entity);
            _eventPublisher.EntityDeleted(entity);
        }
    }
}
