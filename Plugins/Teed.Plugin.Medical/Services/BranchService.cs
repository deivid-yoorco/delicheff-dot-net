using Nop.Core;
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
    public class BranchService
    {
        private readonly IRepository<Branch> _branchRepository;
        private readonly IEventPublisher _eventPublisher;

        public BranchService(IRepository<Branch> branchRepository, IEventPublisher eventPublisher)
        {
            _branchRepository = branchRepository;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<Branch> GetAll()
        {
            return _branchRepository.TableNoTracking.Where(x => !x.Deleted);
        }

        public IPagedList<Branch> ListAsNoTracking(int pageIndex = 0, int pageSize = 20)
        {
            var query = _branchRepository.TableNoTracking;
            query = query.OrderByDescending(m => m.CreatedOnUtc).ThenBy(m => m.Id).Where(x => !x.Deleted);
            return new PagedList<Branch>(query, pageIndex, pageSize);
        }

        public Branch GetById(int id)
        {
            return _branchRepository.GetById(id);
        }

        public void Update(Branch entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _branchRepository.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Insert(Branch entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _branchRepository.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }
    }
}
