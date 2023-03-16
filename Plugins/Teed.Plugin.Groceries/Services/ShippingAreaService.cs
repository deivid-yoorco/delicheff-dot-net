using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingAreas;
using Teed.Plugin.Groceries.Domain.WebScrapingUnitProduct;

namespace Teed.Plugin.Groceries.Services
{
    public class ShippingAreaService
    {
        private readonly IRepository<ShippingArea> _db;
        private readonly IEventPublisher _eventPublisher;

        public ShippingAreaService(
            IRepository<ShippingArea> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<ShippingArea> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public void Insert(ShippingArea entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Delete(ShippingArea entity)
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
