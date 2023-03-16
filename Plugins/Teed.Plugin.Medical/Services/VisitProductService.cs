using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Visit;

namespace Teed.Plugin.Medical.Services
{
    public class VisitProductService
    {
        private readonly IRepository<VisitProduct> _db;
        private readonly IEventPublisher _eventPublisher;
        private readonly IProductService _productService;

        public VisitProductService(IRepository<VisitProduct> db, IEventPublisher eventPublisher, IProductService productService)
        {
            _db = db;
            _eventPublisher = eventPublisher;
            _productService = productService;
        }

        public IQueryable<VisitProduct> GetAll()
        {
            return _db.TableNoTracking.Where(x => !x.Deleted);
        }

        public void Insert(VisitProduct entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public IQueryable<VisitProduct> GetByProductAndVisitId(int productId, int visitId)
        {
            return _db.Table.Where(x => x.ProductId == productId && x.VisitId == visitId && !x.Deleted);
        }

        public IQueryable<int> GetProductsIdsByVisitId(int id)
        {
            return _db.TableNoTracking.Where(x => x.VisitId == id && !x.Deleted).Select(x => x.ProductId);
        }

        public List<Product> GetProductsByVisitId(int id)
        {
            var productIds = _db.TableNoTracking.Where(x => x.VisitId == id && !x.Deleted).Select(x => x.ProductId);
            return _productService.GetProductsByIds(productIds.ToArray()).ToList();
        }
    }
}