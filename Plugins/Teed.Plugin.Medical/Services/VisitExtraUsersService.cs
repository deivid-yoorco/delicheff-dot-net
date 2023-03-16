using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Appointment;
using Teed.Plugin.Medical.Domain.Visit;

namespace Teed.Plugin.Medical.Services
{
    public class VisitExtraUsersService
    {
        private readonly IRepository<VisitExtraUsers> _db;
        private readonly IEventPublisher _eventPublisher;

        public VisitExtraUsersService(IRepository<VisitExtraUsers> db, IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<VisitExtraUsers> GetAll()
        {
            return _db.TableNoTracking.Where(x => !x.Deleted);
        }

        public VisitExtraUsers GetById(int id)
        {
            return _db.Table.Where(x => !x.Deleted && x.Id == id).FirstOrDefault();
        }

        public IQueryable<VisitExtraUsers> GetByAppointmentId(int appointmentId)
        {
            return _db.Table.Where(x => !x.Deleted && x.VisitId == appointmentId);
        }

        public void Insert(VisitExtraUsers entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Delete(VisitExtraUsers entity)
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
