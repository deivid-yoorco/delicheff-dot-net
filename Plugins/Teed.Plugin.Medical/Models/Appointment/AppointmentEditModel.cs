using System;
using System.Collections.Generic;
using Teed.Plugin.Medical.Domain.Appointment;

namespace Teed.Plugin.Medical.Models.Appointment
{
    public class AppointmentEditModel
    {
        public AppointmentEditModel()
        {
            SelectedProductsIds = new List<int>();
            SelectedUsersIds = new List<int>();
            AppointmentTime = new List<TimeSpan>();
        }

        public int Id { get; set; }

        public int PatientId { get; set; }

        public DateTime AppointmentDate { get; set; }

        public IList<TimeSpan> AppointmentTime { get; set; }

        public int AppointmentHour { get; set; }

        public int AppointmentMinute { get; set; }

        public string SelectedDate { get; set; }

        public string Comments { get; set; }

        public VisitType VisitType { get; set; }

        public IList<int> SelectedProductsIds { get; set; }

        public IList<int> SelectedUsersIds { get; set; }

        public int BranchId { get; set; }

        public int DoctorUserId { get; set; }

        public AppointmentStatus Status { get; set; }

        public bool IsManualAppointment { get; set; }

        public string StatusNote { get; set; }

        public string Note { get; set; }
    }
}