using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.CornerForm.Domain;

namespace Teed.Plugin.CornerForm.Services
{
    public class CornerFormResultService
    {
        private readonly IRepository<CornerFormResult> _db;
        private readonly IEventPublisher _eventPublisher;

        public CornerFormResultService(
            IRepository<CornerFormResult> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<CornerFormResult> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public CornerFormResult GetById(int id)
        {
            return _db.Table.Where(x => x.Id == id).FirstOrDefault();
        }

        public void Insert(CornerFormResult entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(CornerFormResult entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(CornerFormResult entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            
            entity.Deleted = true;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}