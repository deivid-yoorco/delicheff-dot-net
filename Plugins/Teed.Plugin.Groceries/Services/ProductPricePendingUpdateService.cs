using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Linq;
using Teed.Plugin.Groceries.Domain.OrderItemBuyers;
using Teed.Plugin.Groceries.Domain.Product;

namespace Teed.Plugin.Groceries.Services
{
    public class ProductPricePendingUpdateService
    {
        private readonly IRepository<ProductPricePendingUpdate> _db;
        private readonly IEventPublisher _eventPublisher;

        public ProductPricePendingUpdateService(
            IRepository<ProductPricePendingUpdate> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<ProductPricePendingUpdate> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public void Insert(ProductPricePendingUpdate entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(ProductPricePendingUpdate entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(ProductPricePendingUpdate entity)
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