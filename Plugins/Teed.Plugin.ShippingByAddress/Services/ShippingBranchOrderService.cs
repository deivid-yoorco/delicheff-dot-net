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
    public class ShippingBranchOrderService
    {
        private readonly IRepository<ShippingBranchOrder> _db;
        private readonly IEventPublisher _eventPublisher;

        public ShippingBranchOrderService(
            IRepository<ShippingBranchOrder> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<ShippingBranchOrder> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }
        public IQueryable<ShippingBranchOrder> GetById(int id)
        {
            return _db.Table.Where(x => !x.Deleted).Where(x => x.Id == id);
        }

        public List<ShippingBranchOrder> GetByShippingBranchId(int id)
        {
            if (id > 0)
                return null;
            return _db.Table.Where(x => !x.Deleted).Where(x => x.ShippingBranchId == id).ToList();
        }

        public List<ShippingBranchOrder> GetByOrderId(int id)
        {
            if (id > 0)
                return null;
            return _db.Table.Where(x => !x.Deleted).Where(x => x.OrderId == id).ToList();
        }

        public void Insert(ShippingBranchOrder entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(ShippingBranchOrder entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(ShippingBranchOrder entity)
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