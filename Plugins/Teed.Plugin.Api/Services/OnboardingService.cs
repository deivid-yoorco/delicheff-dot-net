using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.IO;
using System.Linq;
using Teed.Plugin.Api.Domain.Onboardings;

namespace Teed.Plugin.Api.Services
{
    public class OnboardingService
    {
        private readonly IRepository<Onboarding> _db;
        private readonly IEventPublisher _eventPublisher;

        public OnboardingService(IRepository<Onboarding> db, IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<Onboarding> GetAll(bool includeDeleted = false)
        {
            if (!includeDeleted)
                return _db.Table.Where(x => !x.Deleted);
            else
                return _db.Table;
        }

        public Onboarding GetById(int id)
        {
            if (id < 1)
                return null;
            return _db.Table.Where(x => !x.Deleted && x.Id == id).FirstOrDefault();
        }

        public void Insert(Onboarding entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(Onboarding entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(Onboarding entity)
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
