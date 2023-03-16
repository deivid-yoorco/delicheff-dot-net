using Nop.Core.Data;
using Nop.Plugin.Shipping.Estafeta.Domain.Shopping;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Shipping.Estafeta.Services
{
    public class ShoppingService
    {
        private readonly IRepository<Shopping> _shoppingRepository;
        private readonly IEventPublisher _eventPublisher;

        public ShoppingService(IRepository<Shopping> shoppingRepository, IEventPublisher eventPublisher)
        {
            _shoppingRepository = shoppingRepository;
            _eventPublisher = eventPublisher;
        }

        public void Delete(Shopping entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.Updated = now;
            entity.Deleted = true;

            _shoppingRepository.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public IEnumerable<Shopping> GetAll()
        {
            return _shoppingRepository.TableNoTracking.Where(x => !x.Deleted).AsEnumerable();
        }

        public void Insert(Shopping entity)
        {
            try
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));

                DateTime now = DateTime.UtcNow;
                entity.GuidId = Guid.NewGuid();
                entity.Created = now;
                entity.Updated = now;

                _shoppingRepository.Insert(entity);
                _eventPublisher.EntityInserted(entity);
            }
            catch (Exception e)
            {
                var test = e.Message;
                throw;
            }

        }

        /// <summary>
        /// Logs the specified record.
        /// </summary>
        /// <param name="record">The record.</param>
        public void Log(Shopping record)
        {
            _shoppingRepository.Insert(record);
        }
    }
}
