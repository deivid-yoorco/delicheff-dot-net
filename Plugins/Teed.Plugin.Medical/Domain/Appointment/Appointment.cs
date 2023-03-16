using Nop.Core;
using System;

namespace Teed.Plugin.Medical.Domain.Appointment
{
    public class Appointment : BaseEntity
    {
        public virtual Guid GuidId { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; }
        public DateTime AppointmentDate { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public AppointmentStatus Status { get; set; }
        public VisitType VisitType { get; set; }
        public string Comments { get; set; }
        public int DoctorId { get; set; }
        public int BranchId { get; set; }
        public string Note { get; set; }
        public bool IsManualAppointment { get; set; }
        public int ParentAppointmentId { get; set; }
    }
}