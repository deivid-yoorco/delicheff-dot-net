using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Patients;

namespace Teed.Plugin.Medical.Services
{
    public class PatientFileService
    {
        private readonly IRepository<PatientFile> _db;
        private readonly IEventPublisher _eventPublisher;

        public PatientFileService(IRepository<PatientFile> db, IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<PatientFile> GetAll()
        {
            return _db.TableNoTracking.Where(x => !x.Deleted);
        }

        public void Insert(PatientFile entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }
    }
}
