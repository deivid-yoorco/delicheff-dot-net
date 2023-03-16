using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.BiweeklyPayments;

namespace Teed.Plugin.Payroll.Services
{
    public class BiweeklyPaymentFileService
    {
        private readonly IRepository<BiweeklyPaymentFile> _db;
        private readonly IEventPublisher _eventPublisher;

        public BiweeklyPaymentFileService(
            IRepository<BiweeklyPaymentFile> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<BiweeklyPaymentFile> GetAll(bool showDeleted = false)
        {
            if(showDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public List<BiweeklyPaymentFile> GetAllByBiweeklyPaymentId(int id, bool showDeleted = false)
        {
            if (showDeleted)
                return _db.Table.Where(x => x.BiweeklyPaymentId == id).ToList();
            else
                return _db.Table.Where(x => x.BiweeklyPaymentId == id && !x.Deleted).ToList();
        }

        public BiweeklyPaymentFile GetById(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.Id == Id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.Id == Id && !x.Deleted).FirstOrDefault();
        }

        public void Insert(BiweeklyPaymentFile entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(BiweeklyPaymentFile entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}