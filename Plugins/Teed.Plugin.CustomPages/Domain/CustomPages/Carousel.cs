using Nop.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.CustomPages.Domain.CustomPages
{
    public class Carousel : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }
        
        public virtual int CustomPageId { get; set; }

        [ForeignKey("CustomPageId")]
        public virtual CustomPage CustomPage { get; set; }

        public string CarouselText { get; set; }
        public string CarouselTextColor { get; set; }

        public int CarouselPicture7Id { get; set; }
        public string CarouselLink7 { get; set; }

        public int CarouselPicture8Id { get; set; }
        public string CarouselLink8 { get; set; }

        public int CarouselPicture9Id { get; set; }
        public string CarouselLink9 { get; set; }

        public int CarouselPicture10Id { get; set; }
        public string CarouselLink10 { get; set; }

        public int CarouselPicture11Id { get; set; }
        public string CarouselLink11 { get; set; }

        public int CarouselPicture12Id { get; set; }
        public string CarouselLink12 { get; set; }

        public int CarouselPicture13Id { get; set; }
        public string CarouselLink13 { get; set; }

        public int CarouselPicture14Id { get; set; }
        public string CarouselLink14 { get; set; }

        public int CarouselArrowId { get; set; }
    }
}
