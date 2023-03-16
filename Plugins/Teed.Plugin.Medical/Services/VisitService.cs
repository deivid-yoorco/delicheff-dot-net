using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Visit;

namespace Teed.Plugin.Medical.Services
{
    public class VisitService
    {
        private readonly IRepository<Visit> _db;
        private readonly IEventPublisher _eventPublisher;

        public VisitService(IRepository<Visit> db, IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<Visit> GetAll()
        {
            return _db.TableNoTracking.Where(x => !x.Deleted);
        }

        public IQueryable<Visit> GetAllTracking()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public Visit GetById(int id)
        {
            return _db.Table.Where(x => !x.Deleted && x.Id == id).FirstOrDefault();
        }

        public void Insert(Visit entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(Visit entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void VisitUpdated(Visit entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;
            entity.LastUpdate = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}