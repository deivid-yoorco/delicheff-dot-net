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
    public class ProductSatCodeService
    {
        private readonly IRepository<ProductSatCode> _db;
        private readonly IEventPublisher _eventPublisher;

        public ProductSatCodeService(
            IRepository<ProductSatCode> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<ProductSatCode> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public IQueryable<ProductSatCode> GetByProductId(int id)
        {
            return _db.Table.Where(x => !x.Deleted && x.ProductId == id);
        }

        public IQueryable<ProductSatCode> GetByProductCatalogId(string id)
        {
            return _db.Table.Where(x => !x.Deleted && x.ProductCatalogId == id);
        }

        public ProductSatCode GetById(int id)
        {
            return _db.Table.Where(x => x.Id == id).FirstOrDefault();
        }

        public void Insert(ProductSatCode entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(ProductSatCode entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}
