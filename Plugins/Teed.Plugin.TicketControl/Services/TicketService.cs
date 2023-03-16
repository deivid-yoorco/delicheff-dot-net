using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.TicketControl.Domain.Tickets;

namespace Teed.Plugin.TicketControl.Services
{
    public class TicketService
    {
        private readonly IRepository<Ticket> _db;
        private readonly IEventPublisher _eventPublisher;

        public TicketService(
            IRepository<Ticket> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<Ticket> GetAll(bool showDeleted = false)
        {
            if(showDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public Ticket GetById(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.Id == Id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.Id == Id && !x.Deleted).FirstOrDefault();
        }

        public Ticket GetByTicketId(Guid Id, bool showDeleted = false)
        {
            if (Id == Guid.Empty)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.TicketId == Id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.TicketId == Id && !x.Deleted).FirstOrDefault();
        }

        public Ticket GetByOrderId(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.OrderId == Id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.OrderId == Id && !x.Deleted).FirstOrDefault();
        }

        public void Insert(Ticket entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(Ticket entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}