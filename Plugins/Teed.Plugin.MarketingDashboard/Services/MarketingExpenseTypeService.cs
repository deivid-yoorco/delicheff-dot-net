using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.MarketingDashboard.Domain.MarketingExpenses;

namespace Teed.Plugin.MarketingDashboard.Services
{
    public class MarketingExpenseTypeService
    {
        private readonly IRepository<MarketingExpenseType> _db;
        private readonly IEventPublisher _eventPublisher;

        public MarketingExpenseTypeService(
            IRepository<MarketingExpenseType> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<MarketingExpenseType> GetAll(bool includeDeleted = false)
        {
            if (includeDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public void Insert(MarketingExpenseType entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(MarketingExpenseType entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(MarketingExpenseType entity)
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