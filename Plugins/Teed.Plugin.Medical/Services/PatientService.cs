using Nop.Core;
using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Teed.Plugin.Medical.Domain;

namespace Teed.Plugin.Medical.Services
{
    public class PatientService
    {
        private readonly IRepository<Patient> _patientRepository;
        private readonly DoctorPatientService _doctorPatientService;
        private readonly IEventPublisher _eventPublisher;

        public PatientService(
            IRepository<Patient> patientRepository,
            IEventPublisher eventPublisher,
            DoctorPatientService doctorPatientService)
        {
            _patientRepository = patientRepository;
            _eventPublisher = eventPublisher;
            _doctorPatientService = doctorPatientService;
        }

        public void Delete(Patient entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;
            entity.Deleted = true;

            _patientRepository.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public IEnumerable<Patient> GetAll()
        {
            return _patientRepository.TableNoTracking.Where(x => !x.Deleted).AsEnumerable();
        }

        public IEnumerable<Patient> GetAllPatients()
        {
            return _patientRepository.Table.Where(x => !x.Deleted).AsEnumerable();
        }

        public IEnumerable<Patient> GetAllByDoctorId(int doctorId)
        {
            List<int> doctorPatientIds = _doctorPatientService.GetAll().Where(x => x.DoctorId == doctorId).Select(x => x.PatientId).ToList();
            var query = _patientRepository.Table;
            return query.OrderBy(m => m.CreatedOnUtc).ThenBy(m => m.Id).Where(x => !x.Deleted && doctorPatientIds.Contains(x.Id));
        }

        public Patient GetById(int id)
        {
            return _patientRepository.GetById(id);
        }

        public void Insert(Patient entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _patientRepository.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public IPagedList<Patient> List(int pageIndex = 0, int pageSize = 20)
        {
            var query = _patientRepository.Table;
            query = query.OrderBy(m => m.CreatedOnUtc).ThenBy(m => m.Id).Where(x => !x.Deleted);
            return new PagedList<Patient>(query, pageIndex, pageSize);
        }

        public IPagedList<Patient> ListAsNoTracking(int pageIndex = 0, int pageSize = 20)
        {
            var query = _patientRepository.TableNoTracking;
            query = query.OrderBy(m => m.CreatedOnUtc).ThenBy(m => m.Id).Where(x => !x.Deleted);
            return new PagedList<Patient>(query, pageIndex, pageSize);
        }

        public IPagedList<Patient> ListByDoctorId(int doctorId, int pageIndex = 0, int pageSize = 20)
        {
            List<int> doctorPatientIds = _doctorPatientService.GetAll().Where(x => x.DoctorId == doctorId).Select(x => x.PatientId).ToList();

            var query = _patientRepository.Table;
            query = query.OrderBy(m => m.CreatedOnUtc).ThenBy(m => m.Id).Where(x => !x.Deleted && doctorPatientIds.Contains(x.Id));
            return new PagedList<Patient>(query, pageIndex, pageSize);
        }

        public void Update(Patient entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _patientRepository.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public Patient FindByEmail(string email)
        {
            return _patientRepository.TableNoTracking.Where(x => x.Email == email).FirstOrDefault();
        }

        public IQueryable<Patient> QueryGetAll()
        {
            return _patientRepository.Table.Where(x => !x.Deleted);
        }
    }
}