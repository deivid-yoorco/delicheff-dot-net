using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Suppliers.Domain;

namespace Teed.Plugin.Suppliers.Services
{
    public class SupplierService
    {
        private readonly IRepository<Supplier> _db;
        private readonly IEventPublisher _eventPublisher;

        public SupplierService(
            IRepository<Supplier> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<Supplier> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public Supplier GetById(int id)
        {
            return _db.Table.Where(x => x.Id == id).FirstOrDefault();
        }

        public void Insert(Supplier entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(Supplier entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(Supplier entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            
            entity.Deleted = true;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}