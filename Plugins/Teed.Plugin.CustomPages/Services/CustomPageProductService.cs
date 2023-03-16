using Nop.Core;
using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Teed.Plugin.CustomPages.Domain.CustomPages;

namespace Teed.Plugin.CustomPages.Services
{
    public class CustomPageProductService
    {
        private readonly IRepository<CustomPageProduct> _db;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkContext _workContext;

        public CustomPageProductService(
            IRepository<CustomPageProduct> db,
            IEventPublisher eventPublisher,
            IWorkContext workContext)
        {
            _db = db;
            _eventPublisher = eventPublisher;
            _workContext = workContext;
        }

        public IQueryable<CustomPageProduct> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public IQueryable<CustomPageProduct> GetAllByCustomPageId(int id)
        {
            return _db.Table.Where(x => x.CustomPageId == id && !x.Deleted);
        }

        public CustomPageProduct GetByCustomPageIdAndProductId(int customPageId, int productId)
        {
            return _db.Table.Where(x => x.CustomPageId == customPageId && x.ProductId == productId && !x.Deleted).FirstOrDefault();
        }

        public void Insert(CustomPageProduct entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(CustomPageProduct entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(CustomPageProduct entity)
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
