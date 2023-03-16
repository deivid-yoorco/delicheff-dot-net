using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.OrderReports;
using Teed.Plugin.Groceries.Domain.SubstituteBuyers;

namespace Teed.Plugin.Groceries.Services
{
    public class SubstituteBuyerService
    {
        private readonly IRepository<SubstituteBuyer> _db;
        private readonly IEventPublisher _eventPublisher;

        public SubstituteBuyerService(
            IRepository<SubstituteBuyer> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<SubstituteBuyer> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public IQueryable<SubstituteBuyer> GetAllBySelectedShippingDate(DateTime date)
        {
            return _db.Table.Where(x => x.SelectedShippingDate == date && !x.Deleted);
        }

        public IQueryable<SubstituteBuyer> GetAllByCustomerId(int id)
        {
            return _db.Table.Where(x => x.CustomerId == id && !x.Deleted);
        }

        public IQueryable<SubstituteBuyer> GetAllBySubstituteCustomerId(int id)
        {
            return _db.Table.Where(x => x.SubstituteCustomerId == id && !x.Deleted);
        }

        public void Insert(SubstituteBuyer entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(SubstituteBuyer entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(SubstituteBuyer entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;
            entity.Deleted = true;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}