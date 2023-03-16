using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Domain.Doctor
{
    public class DoctorSchedule : BaseEntity
    {
        public virtual Guid GuidId { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int BranchId { get; set; }
        public virtual int CustomerId { get; set; }
        public virtual WeekDays DayOfTheWeek { get; set; }
        public virtual TimeSpan StartTime { get; set; }
        public virtual TimeSpan EndTime { get; set; }
    }
}