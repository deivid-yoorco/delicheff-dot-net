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
    public class BonusAmountService
    {
        private readonly IRepository<BonusAmount> _db;
        private readonly IEventPublisher _eventPublisher;

        public BonusAmountService(
            IRepository<BonusAmount> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<BonusAmount> GetAll(bool showDeleted = false)
        {
            if (showDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public BonusAmount GetById(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.Id == Id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.Id == Id && !x.Deleted).FirstOrDefault();
        }

        public List<BonusAmount> GetAllAmountsByBonusId(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.BonusId == Id).ToList();
            else
                return _db.Table.Where(x => x.BonusId == Id && !x.Deleted).ToList();
        }

        public void Insert(BonusAmount entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(BonusAmount entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(BonusAmount entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.Deleted = true;
            Update(entity);
        }
    }
}