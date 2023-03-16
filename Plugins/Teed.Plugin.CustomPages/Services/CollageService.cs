using Nop.Core;
using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Linq;
using Teed.Plugin.CustomPages.Domain.CustomPages;

namespace Teed.Plugin.CustomPages.Services
{
    public class CollageService
    {
        private readonly IRepository<Collage> _db;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkContext _workContext;

        public CollageService(
            IRepository<Collage> db,
            IEventPublisher eventPublisher,
            IWorkContext workContext)
        {
            _db = db;
            _eventPublisher = eventPublisher;
            _workContext = workContext;
        }

        public IQueryable<Collage> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public Collage GetByCustomPageId(int id)
        {
            return _db.Table.Where(x => x.CustomPageId == id && !x.Deleted).FirstOrDefault();
        }

        public void Insert(Collage entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(Collage entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}
