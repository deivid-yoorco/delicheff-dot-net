using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Domain.Branches
{
    public class Branch : BaseEntity
    {
        public virtual Guid GuidId { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual string Name { get; set; }
        public virtual int UserId { get; set; }
        public virtual string Phone { get; set; }
        public virtual string Phone2 { get; set; }
        public virtual string StreetAddress { get; set; }
        public virtual string StreetAddress2 { get; set; }
        public virtual string City { get; set; }
        public virtual int CountryId { get; set; }
        public virtual int StateProvinceId { get; set; }
        public virtual string ZipPostalCode { get; set; }

        public virtual TimeSpan WeekOpenTime { get; set; }
        public virtual TimeSpan WeekCloseTime { get; set; }
        public virtual TimeSpan? SaturdayOpenTime { get; set; }
        public virtual TimeSpan? SaturdayCloseTime { get; set; }
        public virtual TimeSpan? SundayOpenTime { get; set; }
        public virtual TimeSpan? SundayCloseTime { get; set; }
        public virtual bool WorksOnSaturday { get; set; }
        public virtual bool WorksOnSunday { get; set; }
    }
}
