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
    public class MercadoPagoTransactionService
    {
        private readonly IRepository<MercadoPagoTransaction> _db;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkContext _workContext;

        public MercadoPagoTransactionService(
            IRepository<MercadoPagoTransaction> db,
            IEventPublisher eventPublisher,
            IWorkContext workContext)
        {
            _db = db;
            _eventPublisher = eventPublisher;
            _workContext = workContext;
        }

        public IQueryable<MercadoPagoTransaction> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public void Insert(MercadoPagoTransaction entity)
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
