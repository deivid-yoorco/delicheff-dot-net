using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Domain.TaggableBoxes
{
    public class TaggableBox : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public string Name { get; set; }
        public int Position { get; set; }
        public int Type { get; set; }
        public int ElementId { get; set; }
        public int PictureId { get; set; }

        public string Log { get; set; }
    }
}
