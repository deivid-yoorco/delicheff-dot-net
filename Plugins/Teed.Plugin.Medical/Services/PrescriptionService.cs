using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Data;
using Nop.Services.Events;
using Teed.Plugin.Medical.Domain;
using Teed.Plugin.Medical.Domain.Prescription;

namespace Teed.Plugin.Medical.Services
{
    public class PrescriptionService
    {
        private readonly IRepository<Prescription> _prescriptionRepository;
        private readonly IEventPublisher _eventPublisher;

        public PrescriptionService(
            IRepository<Prescription> prescriptionRepository,
            IEventPublisher eventPublisher)
        {
            _prescriptionRepository = prescriptionRepository;
            _eventPublisher = eventPublisher;
        }

        public IEnumerable<Prescription> GetAll()
        {
            return _prescriptionRepository.TableNoTracking.Where(x => !x.Deleted).AsEnumerable();
        }

        public IEnumerable<Prescription> GetAllByPatientId(int patientId)
        {
            return _prescriptionRepository.TableNoTracking.Where(x => !x.Deleted && x.PatientId == patientId).OrderByDescending(x => x.CreatedOnUtc);
        }

        public Prescription GetById(int id)
        {
            return _prescriptionRepository.GetById(id);
        }

        public void Insert(Prescription entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;
            
            _prescriptionRepository.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public IPagedList<Prescription> ListByDoctorId(int doctorId, int pageIndex = 0, int pageSize = 20)
        {
            var query = _prescriptionRepository.Table;
            query = query.OrderBy(m => m.CreatedOnUtc).ThenBy(m => m.Id).Where(x => !x.Deleted && x.DoctorId == doctorId);
            return new PagedList<Prescription>(query, pageIndex, pageSize);
        }

        public IPagedList<Prescription> ListAsNoTracking(int pageIndex = 0, int pageSize = 20)
        {
            var query = _prescriptionRepository.TableNoTracking;
            query = query.OrderByDescending(m => m.CreatedOnUtc).Where(x => !x.Deleted);
            return new PagedList<Prescription>(query, pageIndex, pageSize);
        }

        public void Update(Prescription entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _prescriptionRepository.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}
