using Nop.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.CustomPages.Domain.CustomPages
{
    public class Boxes : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }
        
        public virtual int CustomPageId { get; set; }

        [ForeignKey("CustomPageId")]
        public virtual CustomPage CustomPage { get; set; }

        public int BoxPicture15Id { get; set; }
        public string BoxLink15 { get; set; }

        public int BoxPicture16Id { get; set; }
        public string BoxLink16 { get; set; }

        public int BoxPicture17Id { get; set; }
        public string BoxLink17 { get; set; }

        public int BoxPicture18Id { get; set; }
        public string BoxLink18 { get; set; }

        public int BoxPicture40Id { get; set; }
        public string BoxLink40 { get; set; }

        public int BoxPicture41Id { get; set; }
        public string BoxLink41 { get; set; }
    }
}
