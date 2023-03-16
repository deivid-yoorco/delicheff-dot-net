using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using System.Collections.Generic;

namespace Nop.Services.Catalog
{
    public class ManufacturerLogService : IManufacturerLogService
    {
        private readonly IRepository<ManufacturerLog> _manufacturerLogRepository;

        public ManufacturerLogService(IRepository<ManufacturerLog> manufacturerLogRepository)
        {
            this._manufacturerLogRepository = manufacturerLogRepository;
        }

        public void InsertManufacturerLog(ManufacturerLog manufacturerLog)
        {
            _manufacturerLogRepository.Insert(manufacturerLog);
        }

        public virtual IList<ManufacturerLog> GetManufacturerLogById(int Id)
        {
            var query = from p in _manufacturerLogRepository.Table
                        where p.ManufacturerId == Id
                        orderby p.CreatedOnUtc descending
                        select p;
            var manufacturers = query.ToList();
            return manufacturers;
        }
    }
}
