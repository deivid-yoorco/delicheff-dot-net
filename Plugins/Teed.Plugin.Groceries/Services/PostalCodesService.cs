using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingAreas;

namespace Teed.Plugin.Groceries.Services
{
    public class PostalCodesService
    {
        private readonly IRepository<PostalCodes> _db;
        private readonly IEventPublisher _eventPublisher;

        public PostalCodesService(
            IRepository<PostalCodes> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<PostalCodes> GetAll()
        {
            return _db.TableNoTracking;
        }
    }
}
