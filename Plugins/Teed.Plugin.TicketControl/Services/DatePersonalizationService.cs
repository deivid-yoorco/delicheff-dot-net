using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.TicketControl.Domain.DatePersonalizations;

namespace Teed.Plugin.TicketControl.Services
{
    public class DatePersonalizationService
    {
        private readonly IRepository<DatePersonalization> _db;
        private readonly IEventPublisher _eventPublisher;

        public DatePersonalizationService(
            IRepository<DatePersonalization> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<DatePersonalization> GetAll(bool showDeleted = false)
        {
            if(showDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public DatePersonalization GetById(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.Id == Id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.Id == Id && !x.Deleted).FirstOrDefault();
        }

        public IQueryable<DatePersonalization> GetAllByDate(DateTime date, bool showDeleted = false)
        {
            if (date == DateTime.MinValue)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.Date == date);
            else
                return _db.Table.Where(x => x.Date == date && !x.Deleted);
        }

        public void Insert(DatePersonalization entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(DatePersonalization entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}