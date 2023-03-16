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
    public class MarketingExpenseService
    {
        private readonly IRepository<MarketingExpense> _db;
        private readonly IEventPublisher _eventPublisher;

        public MarketingExpenseService(
            IRepository<MarketingExpense> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<MarketingExpense> GetAll(bool includeDeleted = false)
        {
            if (includeDeleted)
                return _db.Table.Include(x => x.ExpenseType);
            else
                return _db.Table.Where(x => !x.Deleted).Include(x => x.ExpenseType);
        }

        public void Insert(MarketingExpense entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(MarketingExpense entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(MarketingExpense entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.Deleted = true;
            Update(entity);

            //event notification
            _eventPublisher.EntityDeleted(entity);
        }

        public List<DailyExpenseModel> GetDailyExpense(List<MarketingExpense> marketingEspenses, DateTime endDate)
        {
            var expenses = marketingEspenses.Where(x => endDate >= x.InitDate).ToList();
            var days = new List<DailyExpenseModel>();
            foreach (var expense in expenses)
            {
                int daysCount = (expense.EndDate - expense.InitDate).Days + 1;
                if (daysCount == 0) continue;
                for (DateTime i = expense.InitDate; i <= expense.EndDate; i = i.AddDays(1))
                {
                    days.Add(new DailyExpenseModel()
                    {
                        Amount = expense.Amount / daysCount,
                        Date = i
                    });
                }
            }
            return days;
        }
    }
}