
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain;

namespace Teed.Plugin.Medical.Models.Doctors
{
    public class DoctorScheduleModel
    {
        public int Id { get; set; }
        public string WeekDay { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string BranchName { get; set; }
        public int BranchId { get; set; }
    }
}
