using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Note;

namespace Teed.Plugin.Medical.Services
{
    public class NoteService
    {
        private readonly IRepository<Note> _noteRepository;
        private readonly IEventPublisher _eventPublisher;

        public NoteService(
            IRepository<Note> noteRepository,
            IEventPublisher eventPublisher)
        {
            _noteRepository = noteRepository;
            _eventPublisher = eventPublisher;
        }

        public void Insert(Note entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _noteRepository.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public Note GetById(int id)
        {
            return _noteRepository.GetById(id);
        }

        public IEnumerable<Note> GetAllByPatientId(int patientId)
        {
            return _noteRepository.TableNoTracking.Where(x => !x.Deleted && x.PatientId == patientId).OrderByDescending(x => x.CreatedOnUtc);
        }

        public IQueryable<Note> GetAll()
        {
            return _noteRepository.TableNoTracking.Where(x => !x.Deleted);
        }

        public Note GetLast(int id)
        {
            return _noteRepository.TableNoTracking.Where(x => x.PatientId == id).OrderByDescending(x => x.CreatedOnUtc).FirstOrDefault();
        }

        public void Update(Note entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _noteRepository.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

    }
}
