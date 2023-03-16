using Nop.Core.Domain.Catalog;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Services.Catalog
{
    public partial interface IProductLogService
    {
        void InsertProductLog(ProductLog productLog);

        IList<ProductLog> GetProductLogById(int Id);

        IQueryable<ProductLog> GetAll();
    }
}
