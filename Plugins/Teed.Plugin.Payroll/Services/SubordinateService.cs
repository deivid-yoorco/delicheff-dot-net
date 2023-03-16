using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;

namespace Teed.Plugin.Payroll.Services
{
    public class SubordinateService
    {
        private readonly IRepository<Subordinate> _db;
        private readonly IEventPublisher _eventPublisher;

        public SubordinateService(
            IRepository<Subordinate> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<Subordinate> GetAll(bool showDeleted = false)
        {
            if(showDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public List<Subordinate> GetAllByBossId(int id, bool showDeleted = false)
        {
            if (showDeleted)
                return _db.Table.Where(x => x.BossId == id).ToList();
            else
                return _db.Table.Where(x => x.BossId == id && !x.Deleted).ToList();
        }

        public List<Subordinate> GetAllBySubordinateId(int id, bool showDeleted = false)
        {
            if (showDeleted)
                return _db.Table.Where(x => x.PayrollSubordinateId == id).ToList();
            else
                return _db.Table.Where(x => x.PayrollSubordinateId == id && !x.Deleted).ToList();
        }

        public Subordinate GetById(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.Id == Id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.Id == Id && !x.Deleted).FirstOrDefault();
        }

        public void Insert(Subordinate entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(Subordinate entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void SetAsDeleted(Subordinate entity)
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