using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Domain.Popups
{
    public class Popup : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public string Name { get; set; }
        public int? ImageId { get; set; }
        public int? ImageForDesktopId { get; set; }
        public bool Mondays { get; set; }
        public bool Tuesdays { get; set; }
        public bool Wednesdays { get; set; }
        public bool Thursdays { get; set; }
        public bool Fridays { get; set; }
        public bool Saturdays { get; set; }
        public bool Sundays { get; set; }
        public bool FirstTimeOnly { get; set; }
        public bool Active { get; set; }
        public DateTime? ViewableDeadlineDate { get; set; }

        public string Log { get; set; }
    }
}
