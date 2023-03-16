using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Models.Doctors
{
    public class DoctorLockDateModel
    {
        public int Id { get; set; }
        public string InitDate { get; set; } 
        public string EndDate { get; set; }
    }
}
