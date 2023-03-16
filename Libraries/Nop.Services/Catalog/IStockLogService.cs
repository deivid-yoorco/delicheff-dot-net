using Nop.Core;
using Nop.Core.Domain.Catalog;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Services.Catalog
{
    public partial interface IStockLogService
    {
        void InsertStockLog(StockLog priceLog);

        IList<StockLog> GetStockLogById(int Id);
        
        IList<StockLog> GetAll();

        IQueryable<StockLog> GetAllQuery();

        IPagedList<StockLog> GetAllPages(int pageIndex, int pageSize);
    }
}
