using Nop.Core;
using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Linq;
using Teed.Plugin.CustomPages.Domain.CustomPages;

namespace Teed.Plugin.CustomPages.Services
{
    public class CustomPagesService
    {
        private readonly IRepository<CustomPage> _db;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkContext _workContext;

        public CustomPagesService(
            IRepository<CustomPage> db,
            IEventPublisher eventPublisher,
            IWorkContext workContext)
        {
            _db = db;
            _eventPublisher = eventPublisher;
            _workContext = workContext;
        }

        public IQueryable<CustomPage> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public CustomPage GetById(int id)
        {
            return _db.Table.Where(x => x.Id == id && !x.Deleted).FirstOrDefault();
        }

        public PagedList<CustomPage> GetAllPaged(int pageIndex, int pageSize)
        {
            var query = _db.Table.Where(x => !x.Deleted).OrderBy(x => x.CreatedOnUtc);
            return new PagedList<CustomPage>(query, pageIndex, pageSize);
        }

        public void Insert(CustomPage entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(CustomPage entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}
