using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Doctor;

namespace Teed.Plugin.Medical.Services
{
    public class DoctorAppointmentIntervalService
    {
        private readonly IRepository<DoctorAppointmentInterval> _db;
        private readonly IEventPublisher _eventPublisher;

        public DoctorAppointmentIntervalService(IRepository<DoctorAppointmentInterval> db, IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<DoctorAppointmentInterval> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public IQueryable<DoctorAppointmentInterval> GetByCustomerId(int customerId)
        {
            return _db.Table.Where(x => !x.Deleted && x.CustomerId == customerId);
        }

        public void Insert(DoctorAppointmentInterval entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(DoctorAppointmentInterval entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}
