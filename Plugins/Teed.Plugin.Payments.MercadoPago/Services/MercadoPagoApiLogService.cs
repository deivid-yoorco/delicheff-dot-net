using Nop.Core;
using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payments.MercadoPago.Domain;

namespace Teed.Plugin.Payments.MercadoPago.Services
{
    public class MercadoPagoApiLogService
    {
        private readonly IRepository<MercadoPagoApiLog> _db;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkContext _workContext;

        public MercadoPagoApiLogService(
            IRepository<MercadoPagoApiLog> db,
            IEventPublisher eventPublisher,
            IWorkContext workContext)
        {
            _db = db;
            _eventPublisher = eventPublisher;
            _workContext = workContext;
        }

        public IQueryable<MercadoPagoApiLog> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public void Insert(MercadoPagoApiLog entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }
    }
}
