using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Services.Events;
using Teed.Plugin.Medical.Domain;
using Teed.Plugin.Medical.Domain.Prescription;

namespace Teed.Plugin.Medical.Services
{
    public class PrescriptionItemService
    {
        private readonly PrescriptionService _prescriptionService;
        private readonly IRepository<PrescriptionItem> _prescriptionItemRepository;
        private readonly IEventPublisher _eventPublisher;

        public PrescriptionItemService(
            IRepository<PrescriptionItem> prescriptionItemRepository,
            PrescriptionService prescriptionService,
            IEventPublisher eventPublisher)
        {
            _prescriptionItemRepository = prescriptionItemRepository;
            _prescriptionService = prescriptionService;
            _eventPublisher = eventPublisher;
        }

        public void Delete(PrescriptionItem entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.UpdatedOnUtc = DateTime.UtcNow;
            entity.Deleted = true;

            _prescriptionItemRepository.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public IEnumerable<PrescriptionItem> GetAll()
        {
            return _prescriptionItemRepository.TableNoTracking.Where(x => !x.Deleted).AsEnumerable();
        }

        public PrescriptionItem GetById(int id)
        {
            return _prescriptionItemRepository.GetById(id);
        }

        public void Insert(PrescriptionItem entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _prescriptionItemRepository.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(PrescriptionItem entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.UpdatedOnUtc = DateTime.UtcNow;

            _prescriptionItemRepository.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public IEnumerable<PrescriptionItem> GetAllByPrescriptionId(int prescriptionId)
        {
            var query = _prescriptionItemRepository.TableNoTracking;
            return query.OrderByDescending(m => m.CreatedOnUtc).Where(x => !x.Deleted && x.PrescriptionId == prescriptionId);
        }

        public IEnumerable<PrescriptionItem> GetAllByPrescriptionId2(int prescriptionId)
        {
            var query = _prescriptionItemRepository.Table;
            return query.OrderByDescending(m => m.CreatedOnUtc).Where(x => !x.Deleted && x.PrescriptionId == prescriptionId);
        }

        public IList<PrescriptionItem> ListByPrescriptionId(int prescriptionId, int pageIndex = 0, int pageSize = 20)
        {
            var query = _prescriptionItemRepository.Table;
            query =  query.OrderByDescending(m => m.CreatedOnUtc).Where(x => !x.Deleted && x.PrescriptionId == prescriptionId);
            return query.ToList();
        }

        public int GetElementsCount(int id)
        {
            return _prescriptionItemRepository.TableNoTracking.Where(x => x.PrescriptionId == id && !x.Deleted).Count();
        }

       
    }
}