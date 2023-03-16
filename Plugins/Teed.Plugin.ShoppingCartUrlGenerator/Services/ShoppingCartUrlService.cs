using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Data.Entity;
using System.Linq;
using Teed.Plugin.ShoppingCartUrlGenerator.Domain;

namespace Teed.Plugin.ShoppingCartUrlGenerator.Services
{
    public class ShoppingCartUrlService
    {
        private readonly IRepository<ShoppingCartUrl> _db;
        private readonly IEventPublisher _eventPublisher;

        public ShoppingCartUrlService(
            IRepository<ShoppingCartUrl> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<ShoppingCartUrl> GetAll()
        {
            return _db.Table.Include(x => x.ShoppingCartUrlProducts).Where(x => !x.Deleted);
        }

        public ShoppingCartUrl GetById(int id)
        {
            return _db.Table.Where(x => x.Id == id).Include(x => x.ShoppingCartUrlProducts).FirstOrDefault();
        }

        public void Insert(ShoppingCartUrl entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(ShoppingCartUrl entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(ShoppingCartUrl entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            
            entity.Deleted = true;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}