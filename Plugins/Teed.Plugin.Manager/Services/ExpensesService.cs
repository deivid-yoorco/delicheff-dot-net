using Nop.Core;
using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Manager.Domain.Expenses;
using Teed.Plugin.Manager.Domain.ExpensesCategories;

namespace Teed.Plugin.Manager.Services
{
    public class ExpensesService
    {
        private readonly IRepository<Expense> _db;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkContext _workContext;

        public ExpensesService(
            IRepository<Expense> db,
            IEventPublisher eventPublisher,
            IWorkContext workContext)
        {
            _db = db;
            _eventPublisher = eventPublisher;
            _workContext = workContext;
        }

        public IQueryable<Expense> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public IPagedList<Expense> ListAsNoTracking(int pageIndex = 0, int pageSize = 20, bool canViewAll = false)
        {
            var query = _db.TableNoTracking;
            if (!canViewAll)
            {
                query = query.Where(x => x.CreatedByUserId == _workContext.CurrentCustomer.Id);
            }
            query = query.OrderByDescending(m => m.CreatedOnUtc).Where(x => !x.Deleted);
            return new PagedList<Expense>(query, pageIndex, pageSize);
        }

        public void Insert(Expense entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(Expense entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(Expense entity)
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