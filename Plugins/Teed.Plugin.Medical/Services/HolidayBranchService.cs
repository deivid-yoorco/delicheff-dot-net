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
    public class HolidayBranchService
    {
        private readonly IRepository<HolidayBranch> _db;
        private readonly BranchService _branchService;
        private readonly IEventPublisher _eventPublisher;

        public HolidayBranchService(IRepository<HolidayBranch> db, IEventPublisher eventPublisher, BranchService branchService)
        {
            _db = db;
            _branchService = branchService;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<Branch> GetBranchesByHolidayId(int id)
        {
            var branchesIds = _db.TableNoTracking.Where(x => x.HolidayId == id && !x.Deleted).Select(x => x.BranchId);
            return _branchService.GetAll().Where(x => branchesIds.Contains(x.Id));
        }

        public IQueryable<HolidayBranch> GetByHolidayId(int id)
        {
            return _db.TableNoTracking.Where(x => x.HolidayId == id && !x.Deleted);
        }

        public IQueryable<HolidayBranch> GetByHolidayAndBranchId(int holidayId, int branchId)
        {
            return _db.Table.Where(x => x.HolidayId == holidayId && x.BranchId == branchId && !x.Deleted);
        }

        public IQueryable<int> GetHolidayIdsByBranchId(int branchId)
        {
            return _db.TableNoTracking.Where(x => x.BranchId == branchId && !x.Deleted).Select(x => x.HolidayId);
        }

        public void Insert(HolidayBranch entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Delete(HolidayBranch entity)
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
