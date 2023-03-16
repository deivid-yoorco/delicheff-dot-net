using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.ShippingByAddress.Domain.ShippingBranch;

namespace Teed.Plugin.ShippingByAddress.Services
{
    public class ShippingBranchService
    {
        private readonly IRepository<ShippingBranch> _db;
        private readonly IEventPublisher _eventPublisher;

        public ShippingBranchService(
            IRepository<ShippingBranch> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<ShippingBranch> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }
        public IQueryable<ShippingBranch> GetById(int id)
        {
            return _db.Table.Where(x => !x.Deleted).Where(x => x.Id == id);
        }

        public void Insert(ShippingBranch entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(ShippingBranch entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(ShippingBranch entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;
            entity.Deleted = true;

            _db.Update(entity);
            _eventPublisher.EntityDeleted(entity);
        }
    }
}