using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.MarketingDashboard.Domain.MarketingExpenses;
using Teed.Plugin.MarketingDashboard.Models.MarketingExpenses;

namespace Teed.Plugin.MarketingDashboard.Services
{
    public class MarketingDiscountExpenseService
    {
        private readonly IRepository<MarketingDiscountExpense> _db;
        private readonly IEventPublisher _eventPublisher;

        public MarketingDiscountExpenseService(
            IRepository<MarketingDiscountExpense> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<MarketingDiscountExpense> GetAll(bool includeDeleted = false)
        {
            if (includeDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public List<MarketingDiscountExpense> GetByMarketingAutomaticExpenseId(int id, bool includeDeleted = false)
        {
            if (id == 0)
                return null;

            if (includeDeleted)
                return _db.Table.Where(x => x.MarketingAutomaticExpenseId == id).ToList();
            else
                return _db.Table.Where(x => !x.Deleted && x.MarketingAutomaticExpenseId == id).ToList();
        }

        public void Insert(MarketingDiscountExpense entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(MarketingDiscountExpense entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(MarketingDiscountExpense entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.Deleted = true;
            Update(entity);

            //event notification
            _eventPublisher.EntityDeleted(entity);
        }
    }
}