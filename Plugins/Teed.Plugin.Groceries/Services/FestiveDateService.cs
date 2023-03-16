using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.FestiveDates;

namespace Teed.Plugin.Groceries.Services
{
    public class FestiveDateService
    {
        private readonly IRepository<FestiveDate> _db;
        private readonly IEventPublisher _eventPublisher;

        public FestiveDateService(
            IRepository<FestiveDate> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<FestiveDate> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public FestiveDate GetById(int id)
        {
            return GetAll().Where(x => x.Id == id).FirstOrDefault();
        }

        public void Insert(FestiveDate entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(FestiveDate entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(FestiveDate entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;
            entity.Deleted = true;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}