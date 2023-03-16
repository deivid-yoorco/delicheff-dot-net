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
    public class UrbanPromoterFeeService
    {
        private readonly IRepository<UrbanPromoterFee> _db;
        private readonly IEventPublisher _eventPublisher;

        public UrbanPromoterFeeService(
            IRepository<UrbanPromoterFee> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<UrbanPromoterFee> GetAll(bool showDeleted = false)
        {
            if (showDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public UrbanPromoterFee GetById(int Id, bool showDeleted = false)
        {
            if (Id <= 0)
                return null;

            if (showDeleted)
                return _db.Table.Where(x => x.Id == Id).FirstOrDefault();
            else
                return _db.Table.Where(x => x.Id == Id && !x.Deleted).FirstOrDefault();
        }

        public void Insert(UrbanPromoterFee entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(UrbanPromoterFee entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(UrbanPromoterFee entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.Deleted = true;
            Update(entity);
        }

        public List<UrbanPromoterFee> GetByUrbanPromoterId(int Id, bool showDeleted = false)
        {
            if (Id == 0) throw new ArgumentNullException();

            if (showDeleted)
                return _db.Table.Where(x => x.UrbanPromoterId == Id).ToList();
            else
                return _db.Table.Where(x => x.UrbanPromoterId == Id && !x.Deleted).ToList();
        }
    }
}
