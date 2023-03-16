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
    public class OfficeService
    {
        private readonly IRepository<Office> _db;
        private readonly IEventPublisher _eventPublisher;

        public OfficeService(IRepository<Office> db, IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IPagedList<Office> ListAsNoTracking(int pageIndex = 0, int pageSize = 20)
        {
            var query = _db.TableNoTracking;
            query = query.OrderByDescending(m => m.CreatedOnUtc).ThenBy(m => m.Id).Where(x => !x.Deleted);
            return new PagedList<Office>(query, pageIndex, pageSize);
        }

        public Office GetById(int id)
        {
            return _db.GetById(id);
        }

        public IQueryable<Office> GetAll()
        {
            return _db.TableNoTracking.Where(x => !x.Deleted);
        }

        public void Insert(Office entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(Office entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}
