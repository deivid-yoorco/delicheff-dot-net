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
    public class MarketingRetentionRateService
    {
        private readonly IRepository<MarketingRetentionRate> _db;
        private readonly IEventPublisher _eventPublisher;

        public MarketingRetentionRateService(
            IRepository<MarketingRetentionRate> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<MarketingRetentionRate> GetAll(bool includeDeleted = false)
        {
            if (includeDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public void Insert(MarketingRetentionRate entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(MarketingRetentionRate entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(MarketingRetentionRate entity)
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