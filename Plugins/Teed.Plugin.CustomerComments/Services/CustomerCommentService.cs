using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.CustomerComments.Domain.CustomerComment;

namespace Teed.Plugin.CustomerComments.Services
{
    public class CustomerCommentService
    {
        private readonly IRepository<CustomerComment> _db;
        private readonly IEventPublisher _eventPublisher;

        public CustomerCommentService(
            IRepository<CustomerComment> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<CustomerComment> GetAll(bool showDeleted = false)
        {
            if(showDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public CustomerComment GetById(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.Id == Id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.Id == Id && !x.Deleted).FirstOrDefault();
        }

        public List<CustomerComment> GetAllByCustomerId(int customerId, bool showDeleted = false)
        {
            if (customerId <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.CustomerId == customerId).ToList();
            else
                return _db.Table.Where(x => x.CustomerId == customerId && !x.Deleted).ToList();
        }

        public List<CustomerComment> GetAllByAdminId(int adminId, bool showDeleted = false)
        {
            if (adminId <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.CreatedByCustomerId == adminId).ToList();
            else
                return _db.Table.Where(x => x.CreatedByCustomerId == adminId && !x.Deleted).ToList();
        }

        public void Insert(CustomerComment entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(CustomerComment entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(CustomerComment entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.Deleted = true;
            Update(entity);
        }
    }
}