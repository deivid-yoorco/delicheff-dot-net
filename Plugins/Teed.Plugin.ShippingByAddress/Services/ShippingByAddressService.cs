using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.ShippingByAddress.Domain.ShippingByAddressD;

namespace Teed.Plugin.ShippingByAddress.Services
{
    public class ShippingByAddressService
    {
        private readonly IRepository<ShippingByAddressD> _db;
        private readonly IEventPublisher _eventPublisher;

        public ShippingByAddressService(
            IRepository<ShippingByAddressD> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<ShippingByAddressD> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }
        public IQueryable<ShippingByAddressD> GetById(int id)
        {
            return _db.Table.Where(x => !x.Deleted).Where(x => x.Id == id);
        }

        public IQueryable<ShippingByAddressD> GetByPostalCode(string pc)
        {
            return _db.Table.Where(x => !x.Deleted).Where(x => x.PostalCode == pc);
        }

        public IQueryable<ShippingByAddressD> GetByDays(string days)
        {
            if (!days.Contains(','))
            {
                return _db.Table.Where(x => !x.Deleted).Where(x => x.DaysToShip.Contains(days));
            }
            else
            {
                var all = _db.Table.Where(x => !x.Deleted);
                foreach (var day in days.Split(','))
                {
                    if (!string.IsNullOrEmpty(day))
                        all = all.Where(x => x.DaysToShip.Contains(day));
                }
                return all;
            }

        }

        public void Insert(ShippingByAddressD entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(ShippingByAddressD entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(ShippingByAddressD entity)
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