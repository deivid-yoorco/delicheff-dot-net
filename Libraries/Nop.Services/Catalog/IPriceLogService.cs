using Nop.Core;
using Nop.Core.Domain.Catalog;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Services.Catalog
{
    public partial interface IPriceLogService
    {
        void InsertPriceLog(PriceLog priceLog);

        IList<PriceLog> GetPriceLogById(int Id);
        
        IList<PriceLog> GetAll();

        IQueryable<PriceLog> GetAllQuery();

        IPagedList<PriceLog> GetAllPages(int pageIndex, int pageSize);
    }
}
