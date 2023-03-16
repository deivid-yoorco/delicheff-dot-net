using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.BiweeklyPayments;

namespace Teed.Plugin.Payroll.Services
{
    public class BiweeklyPaymentService
    {
        private readonly IRepository<BiweeklyPayment> _db;
        private readonly IEventPublisher _eventPublisher;

        public BiweeklyPaymentService(
            IRepository<BiweeklyPayment> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<BiweeklyPayment> GetAll(bool showDeleted = false)
        {
            if(showDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public BiweeklyPayment GetBySalaryId(int id, bool showDeleted = false)
        {
            if (showDeleted)
                return _db.Table.Where(x => x.PayrollSalaryId == id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.PayrollSalaryId == id && !x.Deleted).FirstOrDefault();
        }

        public BiweeklyPayment GetByPayrollEmployeeId(int id, bool showDeleted = false)
        {
            if (showDeleted)
                return _db.Table.Where(x => x.PayrollEmployeeId == id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.PayrollEmployeeId == id && !x.Deleted).FirstOrDefault();
        }

        public List<BiweeklyPayment> GetAllByPayrollEmployeeId(int id, bool showDeleted = false)
        {
            if (showDeleted)
                return _db.Table.Where(x => x.PayrollEmployeeId == id).ToList();
            else
                return _db.Table.Where(x => x.PayrollEmployeeId == id && !x.Deleted).ToList();
        }

        public BiweeklyPayment GetById(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.Id == Id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.Id == Id && !x.Deleted).FirstOrDefault();
        }

        public void Insert(BiweeklyPayment entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(BiweeklyPayment entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}