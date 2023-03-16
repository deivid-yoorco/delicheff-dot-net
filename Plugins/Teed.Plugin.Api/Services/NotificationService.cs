using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.IO;
using System.Linq;
using Teed.Plugin.Api.Domain.Notifications;
using Teed.Plugin.Api.Domain.Payment;

namespace Teed.Plugin.Api.Services
{
    public class NotificationService
    {
        private readonly IRepository<Notification> _db;
        private readonly IEventPublisher _eventPublisher;

        public NotificationService(IRepository<Notification> db, IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<Notification> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public void Insert(Notification entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(Notification entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(Notification entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;
            entity.Deleted = true;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public FirebaseApp CreateFirebaseApp()
        {
            FileStream serviceAccount = new FileStream("Plugins/Teed.Plugin.Api/src/files/cental-en-linea-firebase-adminsdk-iqtij-787895ec3d.json", FileMode.Open);
            return FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromStream(serviceAccount)
            });
        }
    }
}
