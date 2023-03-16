﻿using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.UrbanPromoters;

namespace Teed.Plugin.Groceries.Services
{
    public class UrbanPromoterService
    {
        private readonly IRepository<UrbanPromoter> _db;
        private readonly IEventPublisher _eventPublisher;

        public UrbanPromoterService(
            IRepository<UrbanPromoter> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<UrbanPromoter> GetAll(bool showDeleted = false)
        {
            if (showDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public UrbanPromoter GetById(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.Id == Id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.Id == Id && !x.Deleted).FirstOrDefault();
        }

        public UrbanPromoter GetByCustomerId(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.CustomerId == Id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.CustomerId == Id && !x.Deleted).FirstOrDefault();
        }

        public void Insert(UrbanPromoter entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(UrbanPromoter entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(UrbanPromoter entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.Deleted = true;
            Update(entity);
        }
    }
}
