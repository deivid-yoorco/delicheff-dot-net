using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.IO;
using System.Linq;
using Teed.Plugin.Api.Domain.Popups;

namespace Teed.Plugin.Api.Services
{
    public class PopupService
    {
        private readonly IRepository<Popup> _db;
        private readonly IEventPublisher _eventPublisher;

        public PopupService(IRepository<Popup> db, IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<Popup> GetAll(bool includeDeleted = false)
        {
            if (!includeDeleted)
                return _db.Table.Where(x => !x.Deleted);
            else
                return _db.Table;
        }

        public Popup GetById(int id)
        {
            if (id < 1)
                return null;
            return _db.Table.Where(x => !x.Deleted && x.Id == id).FirstOrDefault();
        }

        public void Insert(Popup entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(Popup entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(Popup entity)
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
