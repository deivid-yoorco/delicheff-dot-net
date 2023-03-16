﻿using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingAreas;

namespace Teed.Plugin.Groceries.Services
{
    public class PostalCodeNotificationRequestService
    {
        private readonly IRepository<PostalCodeNotificationRequest> _db;
        private readonly IEventPublisher _eventPublisher;

        public PostalCodeNotificationRequestService(
            IRepository<PostalCodeNotificationRequest> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<PostalCodeNotificationRequest> GetAll()
        {
            return _db.TableNoTracking;
        }

        public void Insert(PostalCodeNotificationRequest entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Delete(PostalCodeNotificationRequest entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;
            entity.Deleted = true;

            _db.Update(entity);
            _eventPublisher.EntityDeleted(entity);
        }
    }
}
