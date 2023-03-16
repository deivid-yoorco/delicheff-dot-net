using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.Incidents;

namespace Teed.Plugin.Payroll.Services
{
    public class OperationalIncidentService
    {
        private readonly IRepository<OperationalIncident> _db;
        private readonly IEventPublisher _eventPublisher;

        public OperationalIncidentService(
            IRepository<OperationalIncident> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<OperationalIncident> GetAll(bool showDeleted = false)
        {
            if(showDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public OperationalIncident GetById(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.Id == Id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.Id == Id && !x.Deleted).FirstOrDefault();
        }

        public void Insert(OperationalIncident entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(OperationalIncident entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(OperationalIncident entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.Deleted = true;
            Update(entity);
        }
    }
}