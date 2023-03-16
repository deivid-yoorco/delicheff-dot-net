using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.PayrollEmployeeJobs;

namespace Teed.Plugin.Payroll.Services
{
    public class PayrollEmployeeJobService
    {
        private readonly IRepository<PayrollEmployeeJob> _db;
        private readonly IEventPublisher _eventPublisher;

        public PayrollEmployeeJobService(
            IRepository<PayrollEmployeeJob> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<PayrollEmployeeJob> GetAll(bool showDeleted = false)
        {
            if(showDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public PayrollEmployeeJob GetById(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.Id == Id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.Id == Id && !x.Deleted).FirstOrDefault();
        }

        public IQueryable<PayrollEmployeeJob> GetByJobId(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.JobCatalogId == Id);
            else
                return _db.Table.Where(x => x.JobCatalogId == Id && !x.Deleted);
        }

        public IQueryable<PayrollEmployeeJob> GetByPayrollEmployeeId(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.PayrollEmployeeId == Id).OrderByDescending(x => x.ApplyDate);
            else
                return _db.Table.Where(x => x.PayrollEmployeeId == Id && !x.Deleted).OrderByDescending(x => x.ApplyDate);
        }

        public void Insert(PayrollEmployeeJob entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(PayrollEmployeeJob entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(PayrollEmployeeJob entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.Deleted = true;
            Update(entity);
        }
    }
}