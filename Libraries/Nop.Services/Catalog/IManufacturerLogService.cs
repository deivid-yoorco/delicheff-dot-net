using Nop.Core.Domain.Catalog;
using System.Collections.Generic;

namespace Nop.Services.Catalog
{
    public partial interface IManufacturerLogService
    {
        void InsertManufacturerLog(ManufacturerLog productLog);

        IList<ManufacturerLog> GetManufacturerLogById(int Id);
    }
}
