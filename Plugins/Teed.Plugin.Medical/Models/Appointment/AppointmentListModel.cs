using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain;

namespace Teed.Plugin.Medical.Models.Appointment
{
    public class AppointmentListModel
    {
        public int Id { get; set; }
        public Patient Patient { get; set; }
        public string Status { get; set; }

        public string AppointmentDate { get; set; }
        public string AppointmentTime { get; set; }
        public string Doctor { get; set; }
        public string Branch { get; set; }
        public string Appointment { get; set; }
        public string Comments { get; set; }
        public string VisitType { get; set; }
        public int BranchId { get; set; }
        public int DoctorId { get; set; }
    }
}
