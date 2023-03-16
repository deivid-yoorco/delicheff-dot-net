using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Domain.Onboardings
{
    public class Onboarding : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public string Name { get; set; }
        public int? ImageId { get; set; }
        public string BackgroundColor { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public bool Active { get; set; }

        public string Log { get; set; }
    }
}
