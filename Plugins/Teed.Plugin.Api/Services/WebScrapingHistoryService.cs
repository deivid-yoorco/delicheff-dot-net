using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Domain.Groceries;
using Teed.Plugin.Api.Domain.Identity;

namespace Teed.Plugin.Api.Services
{
    public class WebScrapingHistoryService
    {
        private readonly IRepository<WebScrapingHistory> _db;
        private readonly IEventPublisher _eventPublisher;

        public WebScrapingHistoryService(IRepository<WebScrapingHistory> db, IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<WebScrapingHistory> GetAll()
        {
            return _db.TableNoTracking.Where(x => !x.Deleted).Include(x => x.WebScrapingProduct);
        }

        public void Insert(WebScrapingHistory entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }
    }
}
