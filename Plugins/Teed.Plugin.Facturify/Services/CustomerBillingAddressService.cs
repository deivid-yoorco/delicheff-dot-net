using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Facturify.Domain;

namespace Teed.Plugin.Facturify.Services
{
    public class CustomerBillingAddressService
    {
        private readonly IRepository<CustomerBillingAddress> _db;
        private readonly IEventPublisher _eventPublisher;

        public CustomerBillingAddressService(
            IRepository<CustomerBillingAddress> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<CustomerBillingAddress> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public IQueryable<CustomerBillingAddress> GetByCustomerId(int id)
        {
            return _db.Table.Where(x => !x.Deleted && x.CustomerId == id);
        }

        public CustomerBillingAddress GetById(int id)
        {
            return _db.Table.Where(x => x.Id == id).FirstOrDefault();
        }

        public void Insert(CustomerBillingAddress entity)
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
