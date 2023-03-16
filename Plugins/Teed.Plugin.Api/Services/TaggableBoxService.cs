using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Teed.Plugin.Api.Domain.TaggableBoxes;

namespace Teed.Plugin.Api.Services
{
    public class TaggableBoxService
    {
        private readonly IRepository<TaggableBox> _db;
        private readonly IEventPublisher _eventPublisher;

        public TaggableBoxService(IRepository<TaggableBox> db, IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<TaggableBox> GetAll(bool includeDeleted = false)
        {
            if (!includeDeleted)
                return _db.Table.Where(x => !x.Deleted);
            else
                return _db.Table;
        }

        public TaggableBox GetById(int id)
        {
            if (id < 1)
                return null;
            return _db.Table.Where(x => !x.Deleted && x.Id == id).FirstOrDefault();
        }

        public List<TaggableBox> GetAllByType(ElementType type)
        {
            if ((int)type < 0)
                return null;
            return _db.Table.Where(x => !x.Deleted && x.Type == (int)type).ToList();
        }

        public List<TaggableBox> GetAllByPosition(BoxPosition position)
        {
            if ((int)position < 0)
                return null;
            return _db.Table.Where(x => !x.Deleted && x.Position == (int)position).ToList();
        }

        public void Insert(TaggableBox entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(TaggableBox entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(TaggableBox entity)
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
