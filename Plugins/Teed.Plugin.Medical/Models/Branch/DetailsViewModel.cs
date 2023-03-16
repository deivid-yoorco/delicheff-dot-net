using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Models.Branch
{
    public class DetailsViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Customer Customer { get; set; }

        public string Phone { get; set; }

        public string Phone2 { get; set; }

        public string StreetAddress { get; set; }

        public string StreetAddress2 { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public string StateProvince { get; set; }

        public string ZipPostalCode { get; set; }

        public TimeSpan WeekOpenTime { get; set; }

        public TimeSpan WeekCloseTime { get; set; }

        public TimeSpan? SaturdayOpenTime { get; set; }

        public TimeSpan? SaturdayCloseTime { get; set; }

        public TimeSpan? SundayOpenTime { get; set; }

        public TimeSpan? SundayCloseTime { get; set; }

        public bool WorksOnSaturday { get; set; }

        public bool WorksOnSunday { get; set; }
    }
}
