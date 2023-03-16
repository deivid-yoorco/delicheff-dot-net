using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Doctor;

namespace Teed.Plugin.Medical.Services
{
    public class DoctorLockedDatesServicecs
    {
        private readonly IRepository<DoctorLockedDates> _doctorLockedDatesRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICustomerService _customer;

        public DoctorLockedDatesServicecs(
            IRepository<DoctorLockedDates> doctorLockedDatesRepository,
            IEventPublisher eventPublisher)
        {
            _doctorLockedDatesRepository = doctorLockedDatesRepository;
            _eventPublisher = eventPublisher;
        }

        public void Insert(DoctorLockedDates entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _doctorLockedDatesRepository.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public IQueryable<DoctorLockedDates> GetAllByDoctorId(int id)
        {
            return _doctorLockedDatesRepository.TableNoTracking.Where(x => !x.Deleted && x.DoctorId == id);
        }


        public IQueryable<int> GetAllByDate(string date)
        {
            DateTime d = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            return _doctorLockedDatesRepository.TableNoTracking.Where(x => x.InitDate <= d && x.EndDate >= d).Select(x => x.DoctorId);
        }

        public bool DateDoctor(string date, int doctorId)
        {
            DateTime d = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return _doctorLockedDatesRepository.TableNoTracking.Where(x => x.InitDate <= d && x.EndDate >= d && x.DoctorId == doctorId).Any();
        }

        public DoctorLockedDates GetById(int id)
        {
            return _doctorLockedDatesRepository.GetById(id);
        }

        public void Delete(DoctorLockedDates entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;
            entity.Deleted = true;

            _doctorLockedDatesRepository.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}
