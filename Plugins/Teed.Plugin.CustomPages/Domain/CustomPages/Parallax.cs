using Nop.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.CustomPages.Domain.CustomPages
{
    public class Parallax : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }
        
        public virtual int CustomPageId { get; set; }

        [ForeignKey("CustomPageId")]
        public virtual CustomPage CustomPage { get; set; }

        public int ParallaxPicture1Id { get; set; }
        public string ParallaxText1 { get; set; }
        public string ParallaxLink1 { get; set; }

        public int ParallaxPicture2Id { get; set; }
        public string ParallaxText2 { get; set; }
        public string ParallaxLink2 { get; set; }
    }
}
