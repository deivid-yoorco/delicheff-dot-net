using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Linq;
using Teed.Plugin.ShoppingCartUrlGenerator.Domain;

namespace Teed.Plugin.ShoppingCartUrlGenerator.Services
{
    public class ShoppingCartUrlUserService
    {
        private readonly IRepository<ShoppingCartUrlUser> _db;
        private readonly IEventPublisher _eventPublisher;

        public ShoppingCartUrlUserService(
            IRepository<ShoppingCartUrlUser> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<ShoppingCartUrlUser> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public ShoppingCartUrlUser GetById(int id)
        {
            return _db.Table.Where(x => x.Id == id).FirstOrDefault();
        }

        public void Insert(ShoppingCartUrlUser entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(ShoppingCartUrlUser entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(ShoppingCartUrlUser entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            
            entity.Deleted = true;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}