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
    public class PayrollEmployeeService
    {
        private readonly IRepository<PayrollEmployee> _db;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICustomerService _customerService;
        private readonly PayrollEmployeeJobService _payrollEmployeeJobService;

        public PayrollEmployeeService(
            IRepository<PayrollEmployee> db,
            IEventPublisher eventPublisher,
            ICustomerService customerService,
            PayrollEmployeeJobService payrollEmployeeJobService)
        {
            _db = db;
            _eventPublisher = eventPublisher;
            _customerService = customerService;
        }

        public IQueryable<PayrollEmployee> GetAll(bool showDeleted = false, bool onlyEmployees = true, bool includeExEmployees = false)
        {
            if (onlyEmployees)
            {
                var customerIds = new List<int>();
                if (!showDeleted)
                    customerIds = _db.Table.Where(x => !x.Deleted).Select(y => y.CustomerId).ToList();
                else
                    customerIds = _db.Table.Select(y => y.CustomerId).ToList();
                if (!includeExEmployees)
                    customerIds = _customerService.GetAllCustomersQuery()
                        .Where(x => customerIds.Contains(x.Id))
                        .ToList()
                        .Where(x => x.IsInCustomerRole("employee"))
                        .Select(x => x.Id).ToList();
                else
                    customerIds = _customerService.GetAllCustomersQuery()
                    .Where(x => customerIds.Contains(x.Id))
                    .ToList()
                    .Where(x => x.IsInCustomerRole("employee") || x.IsInCustomerRole("exemployee"))
                    .Select(x => x.Id).ToList();
                if (!showDeleted)
                    return _db.Table.Where(x => customerIds.Contains(x.CustomerId) && !x.Deleted);
                else
                    return _db.Table.Where(x => customerIds.Contains(x.CustomerId));
            }
            else
            {
                if (showDeleted)
                {
                    return _db.Table;
                }
                else
                {
                    return _db.Table.Where(x => !x.Deleted);
                }
            }
        }

        public PayrollEmployee GetByCustomerId(int id, bool showDeleted = false)
        {
            if (showDeleted)
                return _db.Table.Where(x => x.CustomerId == id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.CustomerId == id && !x.Deleted).FirstOrDefault();
        }

        public List<PayrollEmployee> GetAllByJobCatalogId(int id, bool showDeleted = false)
        {
            if (showDeleted)
                return _db.Table.ToList().Where(x => x.GetCurrentJob()?.Id == id).ToList();
            else
                return _db.Table.Where(x => !x.Deleted).ToList().Where(x => x.GetCurrentJob()?.Id == id).ToList();
        }

        public PayrollEmployee GetById(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.Id == Id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.Id == Id && !x.Deleted).FirstOrDefault();
        }

        public void Insert(PayrollEmployee entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(PayrollEmployee entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}