using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using System.Collections.Generic;

namespace Nop.Services.Catalog
{
    public class ProductLogService : IProductLogService
    {
        private readonly IRepository<ProductLog> _productLogRepository;

        public ProductLogService(IRepository<ProductLog> productLogRepository)
        {
            this._productLogRepository = productLogRepository;
        }

        public void InsertProductLog(ProductLog productLog)
        {
            _productLogRepository.Insert(productLog);
        }

        public IQueryable<ProductLog> GetAll()
        {
            return _productLogRepository.Table;
        }

        public virtual IList<ProductLog> GetProductLogById(int Id)
        {
            var query = from p in _productLogRepository.Table
                        where p.ProductId == Id
                        orderby p.CreatedOnUtc descending
                        select p;
            var products = query.ToList();
            return products;
        }
    }
}
