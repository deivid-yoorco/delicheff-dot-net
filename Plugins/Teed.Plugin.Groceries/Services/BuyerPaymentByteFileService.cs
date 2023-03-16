using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.BuyerPayments;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Services
{
    public class BuyerPaymentByteFileService
    {
        private readonly IRepository<BuyerPaymentByteFile> _db;
        private readonly IEventPublisher _eventPublisher;

        public BuyerPaymentByteFileService(
            IRepository<BuyerPaymentByteFile> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<BuyerPaymentByteFile> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public BuyerPaymentByteFile GetById(int id)
        {
            if (id <= 0)
                return null;

            return _db.Table.Where(x => x.Id == id && !x.Deleted).FirstOrDefault();
        }

        public void Insert(BuyerPaymentByteFile entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(BuyerPaymentByteFile entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(BuyerPaymentByteFile entity)
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