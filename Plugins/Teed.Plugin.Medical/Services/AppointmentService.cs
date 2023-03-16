using Nop.Core;
using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Appointment;

namespace Teed.Plugin.Medical.Services
{
    public class AppointmentService
    {
        private readonly IRepository<Appointment> _appointmentRepository;
        private readonly IEventPublisher _eventPublisher;

        public AppointmentService(
            IRepository<Appointment> appointmentRepository,
            IEventPublisher eventPublisher)
        {
            _appointmentRepository = appointmentRepository;
            _eventPublisher = eventPublisher;
        }

        public void Insert(Appointment entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _appointmentRepository.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public IQueryable<Appointment> GetAll()
        {
            return _appointmentRepository.Table.Where(x => !x.Deleted).Include(x => x.Patient);
        }

        public Appointment GetById(int id)
        {
            return _appointmentRepository.Table.Where(x => x.Id == id).Include(x => x.Patient).FirstOrDefault();
        }

        public bool CheckAppoinment(DateTime date , int  doctorId,int branchId)
        {
            return _appointmentRepository.Table.Where(x => x.DoctorId == doctorId && x.AppointmentDate == date && x.BranchId == branchId).Any();
        }

        public IPagedList<Appointment> ListAsNoTracking(int pageIndex = 0, int pageSize = 20, int branchId = 0, int doctorId = 0, string filterDate = null)
        {
            var query = _appointmentRepository.TableNoTracking;
            query = query.OrderBy(m => m.AppointmentDate).Where(x => !x.Deleted && x.ParentAppointmentId == 0);
            if (branchId != 0)
            {
                query = query.Where(x => x.BranchId == branchId);
            }
            if (doctorId != 0)
            {
                query = query.Where(x => x.DoctorId == doctorId);
            }
            if (!string.IsNullOrWhiteSpace(filterDate))
            {
                DateTime selectedDateParsed = DateTime.ParseExact(filterDate, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                query = query.Where(x => DbFunctions.TruncateTime(x.AppointmentDate) == DbFunctions.TruncateTime(selectedDateParsed));
            }
            else
            {
                query = query.Where(x => DbFunctions.TruncateTime(x.AppointmentDate) > DbFunctions.TruncateTime(DateTime.Now));
            }
            return new PagedList<Appointment>(query, pageIndex, pageSize);
        }

        public void Update(Appointment entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _appointmentRepository.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(Appointment entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;
            entity.Deleted = true;

            _appointmentRepository.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void SetAppointmentAsCompleted(Appointment entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;
            entity.Status = AppointmentStatus.Complete;

            _appointmentRepository.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}