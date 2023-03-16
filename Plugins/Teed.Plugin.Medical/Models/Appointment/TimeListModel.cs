using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Models.Appointment
{
    public class TimeListModel
    {
        public TimeSpan TimeValue { get; set; }
        public string TimeText { get; set; }
        public bool IsActive { get; set; }
    }
}
