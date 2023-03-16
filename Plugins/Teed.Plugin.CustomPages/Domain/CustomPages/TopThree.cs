using Nop.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.CustomPages.Domain.CustomPages
{
    public class TopThree : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }
        
        public virtual int CustomPageId { get; set; }

        [ForeignKey("CustomPageId")]
        public virtual CustomPage CustomPage { get; set; }

        public int Top3Picture3Id { get; set; }
        public string Top3Text3 { get; set; }
        public string Top3Link3 { get; set; }

        public int Top3Picture4Id { get; set; }
        public string Top3Text4 { get; set; }
        public string Top3Link4 { get; set; }

        public int Top3Picture5Id { get; set; }
        public string Top3Text5 { get; set; }
        public string Top3Link5 { get; set; }
    }
}
