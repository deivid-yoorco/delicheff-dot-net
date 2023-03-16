using Nop.Core;
using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Data.Entity;
using System.Linq;
using Teed.Plugin.Manager.Domain.PurchaseOrders;

namespace Teed.Plugin.Manager.Services
{
    public class PurchaseOrderService
    {
        private readonly IRepository<PurchaseOrder> _db;
        private readonly IEventPublisher _eventPublisher;

        public PurchaseOrderService(IRepository<PurchaseOrder> db, IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<PurchaseOrder> GetAll()
        {
            return _db.TableNoTracking.Where(x => !x.Deleted);
        }

        public PurchaseOrder GetById(int id)
        {
            return _db.Table.Where(x => x.Id == id && !x.Deleted).FirstOrDefault();
        }

        public IPagedList<PurchaseOrder> ListAsNoTracking(int pageIndex = 0, int pageSize = 20, int branchId = 0, string filterDate = null)
        {
            var query = _db.TableNoTracking;
            query = query.OrderBy(m => m.RequestedDateUtc).Where(x => !x.Deleted);
            if (branchId != 0)
            {
                query = query.Where(x => x.BranchId == branchId);
            }
            if (!string.IsNullOrWhiteSpace(filterDate))
            {
                DateTime selectedDateParsed = DateTime.ParseExact(filterDate, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                query = query.Where(x => DbFunctions.TruncateTime(x.RequestedDateUtc) == DbFunctions.TruncateTime(selectedDateParsed));
            }

            return new PagedList<PurchaseOrder>(query, pageIndex, pageSize);
        }

        public void Insert(PurchaseOrder entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(PurchaseOrder entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}
