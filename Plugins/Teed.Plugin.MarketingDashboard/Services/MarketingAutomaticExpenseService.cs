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
    public class MarketingAutomaticExpenseService
    {
        private readonly IRepository<MarketingAutomaticExpense> _db;
        private readonly IEventPublisher _eventPublisher;

        public MarketingAutomaticExpenseService(
            IRepository<MarketingAutomaticExpense> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<MarketingAutomaticExpense> GetAll(bool includeDeleted = false)
        {
            if (includeDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public MarketingAutomaticExpense GetByInitDate(DateTime initDate, bool includeDeleted = false)
        {
            if (initDate == DateTime.MinValue)
                return null;

            if (includeDeleted)
                return _db.Table.Where(x => x.InitDate == initDate).FirstOrDefault();
            else
                return _db.Table.Where(x => !x.Deleted).FirstOrDefault();
        }

        public void Insert(MarketingAutomaticExpense entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(MarketingAutomaticExpense entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(MarketingAutomaticExpense entity)
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