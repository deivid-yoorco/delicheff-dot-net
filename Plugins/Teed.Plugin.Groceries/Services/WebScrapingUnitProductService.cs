using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.WebScrapingUnitProduct;

namespace Teed.Plugin.Groceries.Services
{
    public class WebScrapingUnitProductService
    {
        private readonly IRepository<WebScrapingUnitProduct> _db;
        private readonly IEventPublisher _eventPublisher;

        public WebScrapingUnitProductService(
            IRepository<WebScrapingUnitProduct> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public void Insert(WebScrapingUnitProduct entity)
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
