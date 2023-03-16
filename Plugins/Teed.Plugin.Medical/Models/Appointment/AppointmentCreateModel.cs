using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Appointment;

namespace Teed.Plugin.Medical.Models.Appointment
{
    public class AppointmentCreateModel
    {
        public AppointmentCreateModel()
        {
            SelectedProductsIds = new List<int>();
            SelectedUsersIds = new List<int>();
            AppointmentTime = new List<TimeSpan>();
        }

        public int PatientId { get; set; }

        public DateTime AppointmentDate { get; set; }

        public IList<TimeSpan> AppointmentTime { get; set; }

        public string SelectedDate { get; set; }

        public string Comments { get; set; }

        public VisitType VisitType { get; set; }

        public IList<int> SelectedProductsIds { get; set; }

        public IList<int> SelectedUsersIds { get; set; }

        public int BranchId { get; set; }

        public int DoctorUserId { get; set; }
    }
}