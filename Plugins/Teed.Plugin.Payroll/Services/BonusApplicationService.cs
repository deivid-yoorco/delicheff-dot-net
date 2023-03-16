using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.Bonuses;

namespace Teed.Plugin.Payroll.Services
{
    public class BonusApplicationService
    {
        private readonly IRepository<BonusApplication> _db;
        private readonly IEventPublisher _eventPublisher;

        public BonusApplicationService(
            IRepository<BonusApplication> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<BonusApplication> GetAll(bool showDeleted = false)
        {
            if (showDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public BonusApplication GetById(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.Id == Id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.Id == Id && !x.Deleted).FirstOrDefault();
        }

        public List<BonusApplication> GetAllApplicationsByBonusId(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.BonusId == Id).ToList();
            else
                return _db.Table.Where(x => x.BonusId == Id && !x.Deleted).ToList();
        }

        public List<BonusApplication> GetAllByEntityTypeAndEntityId(int EntityTypeId, int EntityId, bool showDeleted = false)
        {
            if (EntityId <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.EntityId == EntityId && x.EntityTypeId == EntityTypeId).ToList();
            else
                return _db.Table.Where(x => x.EntityId == EntityId && x.EntityTypeId == EntityTypeId && !x.Deleted).ToList();
        }

        public BonusApplication GetExact(int EntityTypeId, int EntityId, int BonusId, DateTime date, bool showDeleted = false)
        {
            if (EntityId <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.EntityId == EntityId && x.BonusId == BonusId && x.EntityTypeId == EntityTypeId && x.Date == date).FirstOrDefault();
            else
                return _db.Table.Where(x => x.EntityId == EntityId && x.BonusId == BonusId && x.EntityTypeId == EntityTypeId && x.Date == date && !x.Deleted).FirstOrDefault();
        }

        public void Insert(BonusApplication entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(BonusApplication entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(BonusApplication entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.Deleted = true;
            Update(entity);
        }
    }
}