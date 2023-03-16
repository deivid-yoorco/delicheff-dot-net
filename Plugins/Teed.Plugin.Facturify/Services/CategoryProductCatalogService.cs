using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Facturify.Domain;

namespace Teed.Plugin.Facturify.Services
{
    public class CategoryProductCatalogService
    {
        private readonly IRepository<CategoryProductCatalog> _db;
        private readonly IEventPublisher _eventPublisher;

        public CategoryProductCatalogService(
            IRepository<CategoryProductCatalog> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<CategoryProductCatalog> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public CategoryProductCatalog GetByCategoryId(int id)
        {
            return _db.Table.Where(x => x.CategoryId == id).FirstOrDefault();
        }

        public void Insert(CategoryProductCatalog entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(CategoryProductCatalog entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}
