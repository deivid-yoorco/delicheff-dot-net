using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Linq;
using Teed.Plugin.ShoppingCartUrlGenerator.Domain;

namespace Teed.Plugin.ShoppingCartUrlGenerator.Services
{
    public class ShoppingCartUrlProductService
    {
        private readonly IRepository<ShoppingCartUrlProduct> _db;
        private readonly IEventPublisher _eventPublisher;

        public ShoppingCartUrlProductService(
            IRepository<ShoppingCartUrlProduct> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<ShoppingCartUrlProduct> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public ShoppingCartUrlProduct GetById(int id)
        {
            return _db.Table.Where(x => x.Id == id).FirstOrDefault();
        }

        public void Insert(ShoppingCartUrlProduct entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(ShoppingCartUrlProduct entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(ShoppingCartUrlProduct entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.Deleted = true;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}
