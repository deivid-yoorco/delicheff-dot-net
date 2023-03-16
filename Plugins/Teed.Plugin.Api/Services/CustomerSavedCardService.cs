using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Linq;
using Teed.Plugin.Api.Domain.Payment;

namespace Teed.Plugin.Api.Services
{
    public class CustomerSavedCardService
    {
        private readonly IRepository<CustomerSavedCard> _db;
        private readonly IEventPublisher _eventPublisher;

        public CustomerSavedCardService(IRepository<CustomerSavedCard> db, IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<CustomerSavedCard> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public void Insert(CustomerSavedCard entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(CustomerSavedCard entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(CustomerSavedCard entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;
            entity.Deleted = true;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public string GetCardLogoName(string brand)
        {
            switch (brand.ToLower())
            {
                case "american express":
                    return "t_amex";
                case "visa":
                    return "t_visa";
                case "mastercard":
                    return "t_mascard";
                default:
                    return "t_gen";
            }
        }
    }
}
