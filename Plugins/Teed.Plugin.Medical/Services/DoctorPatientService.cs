using System;
using System.Linq;
using Nop.Core.Data;
using Nop.Services.Events;
using Teed.Plugin.Medical.Domain;

namespace Teed.Plugin.Medical.Services
{
    public class DoctorPatientService
    {
        private readonly IRepository<DoctorPatient> _doctorPatientRepository;
        private readonly IEventPublisher _eventPublisher;

        public DoctorPatientService(
            IRepository<DoctorPatient> doctorPatientRepository,
            IEventPublisher eventPublisher)
        {
            _doctorPatientRepository = doctorPatientRepository;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<DoctorPatient> GetAll()
        {
            return _doctorPatientRepository.Table;
        }

        public void Insert(DoctorPatient entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _doctorPatientRepository.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }
    }
}
