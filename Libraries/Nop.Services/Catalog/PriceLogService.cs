using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using System.Collections.Generic;
using Nop.Core;

namespace Nop.Services.Catalog
{
    public class PriceLogService : IPriceLogService
    {
        private readonly IRepository<PriceLog> _priceLogRepository;

        public PriceLogService(IRepository<PriceLog> priceLogRepository)
        {
            this._priceLogRepository = priceLogRepository;
        }

        public void InsertPriceLog(PriceLog priceLog)
        {
            _priceLogRepository.Insert(priceLog);
        }

        public IList<PriceLog> GetPriceLogById(int Id)
        {
            var query = from p in _priceLogRepository.Table
                        where p.ProductId == Id
                        orderby p.CreatedOnUtc descending
                        select p;
            var products = query.ToList();
            return products;
        }

        public IQueryable<PriceLog> GetAllQuery()
        {
            return _priceLogRepository.Table;
        }

        public IList<PriceLog> GetAll()
        {
            return _priceLogRepository.Table.ToList();
        }

        public IPagedList<PriceLog> GetAllPages(int pageIndex, int pageSize)
        {
            var query = from priceLog in _priceLogRepository.Table
                        orderby priceLog.CreatedOnUtc descending
                        select priceLog;

            var result = new PagedList<PriceLog>(query, pageIndex, pageSize);
            return result;
        }
    }
}
