using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.Benefits;

namespace Teed.Plugin.Payroll.Services
{
    public class BenefitService
    {
        private readonly IRepository<Benefit> _db;
        private readonly IEventPublisher _eventPublisher;

        public BenefitService(
            IRepository<Benefit> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<Benefit> GetAll(bool showDeleted = false)
        {
            if(showDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public List<Benefit> GetAllBySalaryOrEmployeeId(int id, bool isForSalary, bool showDeleted = false)
        {
            if (showDeleted)
                return _db.Table.Where(x => x.SalaryOrEmployeeId == id && x.IsForSalary == isForSalary).ToList();
            else
                return _db.Table.Where(x => x.SalaryOrEmployeeId == id && x.IsForSalary == isForSalary && !x.Deleted).ToList();
        }

        public Benefit GetById(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.Id == Id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.Id == Id && !x.Deleted).FirstOrDefault();
        }

        public void Insert(Benefit entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(Benefit entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}