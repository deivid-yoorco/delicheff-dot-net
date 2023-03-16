using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Branches;

namespace Teed.Plugin.Medical.Services
{
    public class BranchWorkerService
    {
        private readonly IRepository<BranchWorker> _branchWorkerRepository;
        //private readonly DoctorPatientService _doctorPatientService;
        private readonly IEventPublisher _eventPublisher;

        public BranchWorkerService(
            IRepository<BranchWorker> branchWorkerRepository,
            IEventPublisher eventPublisher
            )
        {
            _branchWorkerRepository = branchWorkerRepository;
            _eventPublisher = eventPublisher;

        }

        public void Insert(BranchWorker entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _branchWorkerRepository.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public IQueryable<int> GetUsersIdsByBranchId(int id)
        {
            return _branchWorkerRepository.Table.Where(x => x.BranchId == id && !x.Deleted).Select(x => x.CustomerId);
        }

        public IQueryable<BranchWorker> GetByBranchId(int id)
        {
            return _branchWorkerRepository.Table.Where(x => x.BranchId == id && !x.Deleted);
        }

        public IQueryable<int> GetByCustomerId(int id)
        {
            return _branchWorkerRepository.Table.Where(x => x.CustomerId == id && !x.Deleted).Select(x => x.BranchId);
        }

        public IQueryable<BranchWorker> GetByCustomerIdAndBranchId(int customerId, int branchId)
        {
            return _branchWorkerRepository.Table.Where(x => x.CustomerId == customerId && x.BranchId == branchId && !x.Deleted);
        }

        public void Delete(BranchWorker entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;
            entity.Deleted = true;

            _branchWorkerRepository.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}
