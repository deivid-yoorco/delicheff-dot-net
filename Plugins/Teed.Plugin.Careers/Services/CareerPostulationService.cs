using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Careers.Domain;
using Teed.Plugin.Careers.Models;

namespace Teed.Plugin.Careers.Services
{
    public class CareerPostulationService
    {
        private readonly IRepository<CareerPostulations> _db;
        private readonly IEventPublisher _eventPublisher;

        public CareerPostulationService(
            IRepository<CareerPostulations> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<CareerPostulations> GetAll(bool includeDeleted = false)
        {
            if (includeDeleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }
        public void Insert(CareerPostulations entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;
            entity.CreatedOnUtc = now;
            entity.Deleted = false;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(CareerPostulations entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(CareerPostulations entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.Deleted = true;
            Update(entity);

            //event notification
            _eventPublisher.EntityDeleted(entity);
        }
    }
}