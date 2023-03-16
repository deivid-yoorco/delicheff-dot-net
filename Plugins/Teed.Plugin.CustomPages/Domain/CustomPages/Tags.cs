using Nop.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.CustomPages.Domain.CustomPages
{
    public class Tags : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }
        
        public virtual int CustomPageId { get; set; }

        [ForeignKey("CustomPageId")]
        public virtual CustomPage CustomPage { get; set; }

        public int TagsQty { get; set; }
        public bool TagsEnable { get; set; }
    }
}
