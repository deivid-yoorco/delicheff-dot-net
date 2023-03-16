using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Models.Appointment
{
    public class ReservedTimeModel
    {
        public DateTime SelectedDate { get; set; }
        public DateTime ReservationDate { get; set; }
        public int BranchId { get; set; }
        public int DoctorId { get; set; }
    }
}
