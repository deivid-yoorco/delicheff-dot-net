using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.MinimumWagesCatalogs;

namespace Teed.Plugin.Payroll.Services
{
    public class MinimumWagesCatalogService
    {
        private readonly IRepository<MinimumWagesCatalog> _db;
        private readonly IEventPublisher _eventPublisher;

        public MinimumWagesCatalogService(
            IRepository<MinimumWagesCatalog> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<MinimumWagesCatalog> GetAll(bool showDeleted = false)
        {
            if(showDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public MinimumWagesCatalog GetById(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.Id == Id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.Id == Id && !x.Deleted).FirstOrDefault();
        }

        public MinimumWagesCatalog GetYearWage(int year, bool showDeleted = false)
        {
            if (year <= 0)
                return null;
            var yearDate = new DateTime(year, 1, 1).Date;
            if (showDeleted)
                return _db.Table.Where(x => x.Year >= yearDate).OrderByDescending(x => x.CreatedOnUtc).FirstOrDefault();
            else
                return _db.Table.Where(x => x.Year >= yearDate && !x.Deleted).OrderByDescending(x => x.CreatedOnUtc).FirstOrDefault();
        }

        public void Insert(MinimumWagesCatalog entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(MinimumWagesCatalog entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}