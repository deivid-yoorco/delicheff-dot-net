using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using System.Collections.Generic;
using Nop.Core;

namespace Nop.Services.Catalog
{
    public class StockLogService : IStockLogService
    {
        private readonly IRepository<StockLog> _stockLogRepository;

        public StockLogService(IRepository<StockLog> stockLogRepository)
        {
            this._stockLogRepository = stockLogRepository;
        }

        public void InsertStockLog(StockLog stockLog)
        {
            _stockLogRepository.Insert(stockLog);
        }

        public IList<StockLog> GetStockLogById(int Id)
        {
            var query = from p in _stockLogRepository.Table
                        where p.ProductId == Id
                        orderby p.CreatedOnUtc descending
                        select p;
            var products = query.ToList();
            return products;
        }

        public IList<StockLog> GetAll()
        {
            return _stockLogRepository.Table.ToList();
        }

        public IQueryable<StockLog> GetAllQuery()
        {
            return _stockLogRepository.TableNoTracking;
        }

        public IPagedList<StockLog> GetAllPages(int pageIndex, int pageSize)
        {
            var query = from stockLog in _stockLogRepository.Table
                        orderby stockLog.CreatedOnUtc descending
                        select stockLog;

            var result = new PagedList<StockLog>(query, pageIndex, pageSize);
            return result;
        }
    }
}
