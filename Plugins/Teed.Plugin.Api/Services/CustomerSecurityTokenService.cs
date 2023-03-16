using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Domain.Identity;

namespace Teed.Plugin.Api.Services
{
    public class CustomerSecurityTokenService
    {
        private readonly IRepository<CustomerSecurityToken> _db;
        private readonly IEventPublisher _eventPublisher;

        public CustomerSecurityTokenService(IRepository<CustomerSecurityToken> db, IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<CustomerSecurityToken> GetAll()
        {
            return _db.TableNoTracking.Where(x => !x.Deleted);
        }

        public IQueryable<CustomerSecurityToken> GetAllByUuid(string uuid)
        {
            return _db.Table.Where(x => !x.Deleted && x.Uuid == uuid);
        }

        public void Insert(CustomerSecurityToken entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(CustomerSecurityToken entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void UpdateRefreshToken(CustomerSecurityToken entity, string refeshToken)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;
            entity.RefreshToken = refeshToken;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}
