using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.Assistances;

namespace Teed.Plugin.Payroll.Services
{
    public class AssistanceOverrideService
    {
        private readonly IRepository<AssistanceOverride> _db;
        private readonly IEventPublisher _eventPublisher;

        public AssistanceOverrideService(
            IRepository<AssistanceOverride> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<AssistanceOverride> GetAll(bool showDeleted = false)
        {
            if (showDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public List<AssistanceOverride> GetAllByEmployeeId(int id, bool showDeleted = false)
        {
            if (showDeleted)
                return _db.Table.Where(x => x.PayrollEmployeeId == id).ToList();
            else
                return _db.Table.Where(x => x.PayrollEmployeeId == id && !x.Deleted).ToList();
        }

        public AssistanceOverride GetByDateAndEmployeeId(int id, DateTime date, bool showDeleted = false)
        {
            if (showDeleted)
                return _db.Table.Where(x => x.PayrollEmployeeId == id && x.OverriddenDate == date).FirstOrDefault();
            else
                return _db.Table.Where(x => x.PayrollEmployeeId == id && x.OverriddenDate == date && !x.Deleted).FirstOrDefault();
        }

        public AssistanceOverride GetById(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.Id == Id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.Id == Id && !x.Deleted).FirstOrDefault();
        }

        public void Insert(AssistanceOverride entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(AssistanceOverride entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(AssistanceOverride entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.Deleted = true;
            Update(entity);
        }
    }
}