using Nop.Core;
using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Linq;
using Teed.Plugin.Manager.Domain.PartnerLiabilities;

namespace Teed.Plugin.Manager.Services
{
    public class PartnerLiabilityService
    {
        private readonly IRepository<PartnerLiability> _db;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkContext _workContext;

        public PartnerLiabilityService(
            IRepository<PartnerLiability> db,
            IEventPublisher eventPublisher,
            IWorkContext workContext)
        {
            _db = db;
            _eventPublisher = eventPublisher;
            _workContext = workContext;
        }

        public IQueryable<PartnerLiability> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public IPagedList<PartnerLiability> ListAsNoTracking(int pageIndex = 0, int pageSize = 20, bool canViewAll = false)
        {
            var query = _db.TableNoTracking;
            if (!canViewAll)
            {
                query = query.Where(x => x.CreatedByUserId == _workContext.CurrentCustomer.Id);
            }
            query = query.OrderByDescending(m => m.CreatedOnUtc).Where(x => !x.Deleted);
            return new PagedList<PartnerLiability>(query, pageIndex, pageSize);
        }

        public void Insert(PartnerLiability entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(PartnerLiability entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(PartnerLiability entity)
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
