using Nop.Core;
using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain;
using Teed.Plugin.Medical.Domain.Doctor;

namespace Teed.Plugin.Medical.Services
{
    public class DoctorScheduleService
    {
        private readonly IRepository<DoctorSchedule> _db;
        private readonly IEventPublisher _eventPublisher;

        public DoctorScheduleService(IRepository<DoctorSchedule> db, IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IPagedList<DoctorSchedule> ListAsNoTracking(int pageIndex = 0, int pageSize = 20)
        {
            var query = _db.TableNoTracking;
            query = query.OrderByDescending(m => m.CreatedOnUtc).ThenBy(m => m.Id).Where(x => !x.Deleted);
            return new PagedList<DoctorSchedule>(query, pageIndex, pageSize);
        }

        public DoctorSchedule GetById(int id)
        {
            return _db.GetById(id);
        }

        public IQueryable<DoctorSchedule> GetAll()
        {
            return _db.TableNoTracking.Where(x => !x.Deleted);
        }

        public IList<DoctorSchedule> ListByDoctorId(int doctorId, int pageIndex = 0, int pageSize = 20)
        {
            var query = _db.Table;
            query = query.OrderByDescending(m => m.CreatedOnUtc).Where(x => !x.Deleted && x.CustomerId == doctorId);
            return query.ToList();
        }

        public IQueryable<int> GetIdsDoctor(string date)
        {
            DateTime d = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var day = (int)d.DayOfWeek;
            WeekDays weekDay = (WeekDays)day;
            return _db.TableNoTracking.Where(x => !x.Deleted && x.DayOfTheWeek == weekDay).Select(x => x.CustomerId);
        }

        public IQueryable<int> GetIdsDoctorBYDateAndTime(string date, TimeSpan time)
        {
            DateTime d = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var day = (int)d.DayOfWeek;
            WeekDays weekDay = (WeekDays)day;
            return _db.TableNoTracking.Where(x => !x.Deleted && x.DayOfTheWeek == weekDay && x.StartTime <= time && x.EndTime >= time).Select(x => x.CustomerId);
        }

        public IQueryable<int> GetIdsDoctorBYTime(TimeSpan time)
        {
            
            return _db.TableNoTracking.Where(x => !x.Deleted  && x.StartTime <= time && x.EndTime >= time).Select(x => x.CustomerId);
        }

        public void Insert(DoctorSchedule entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(DoctorSchedule entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(DoctorSchedule entity)
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