using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;

namespace Teed.Plugin.Payroll.Services
{
    public class TakenVacationDayService
    {
        private readonly IRepository<TakenVacationDay> _db;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICustomerService _customerService;

        public TakenVacationDayService(
            IRepository<TakenVacationDay> db,
            IEventPublisher eventPublisher,
            ICustomerService customerService)
        {
            _db = db;
            _eventPublisher = eventPublisher;
            _customerService = customerService;
        }

        public IQueryable<TakenVacationDay> GetAll(bool showDeleted = false)
        {
            if (showDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }
        public TakenVacationDay GetById(int id, bool showDeleted = false)
        {
            if (showDeleted)
                return _db.Table.Where(x => x.Id == id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.Id == id && !x.Deleted).FirstOrDefault();
        }

        public IQueryable<TakenVacationDay> GetAllByPayrollId(int id, bool showDeleted = false)
        {
            if (showDeleted)
                return _db.Table.Where(x => x.PayrollId == id);
            else
                return _db.Table.Where(x => x.PayrollId == id && !x.Deleted);
        }

        public void Insert(TakenVacationDay entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(TakenVacationDay entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}