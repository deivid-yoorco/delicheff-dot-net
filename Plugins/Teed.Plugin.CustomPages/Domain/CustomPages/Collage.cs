using Nop.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.CustomPages.Domain.CustomPages
{
    public class Collage : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }
        
        public virtual int CustomPageId { get; set; }

        [ForeignKey("CustomPageId")]
        public virtual CustomPage CustomPage { get; set; }

        public bool CollageEnable { get; set; }

        
        
        public int CollagePicture19Id { get; set; }
        public string CollageLink19 { get; set; }

        
        
        public int CollagePicture20Id { get; set; }
        public string CollageLink20 { get; set; }

        
        
        public int CollagePicture21Id { get; set; }
        public string CollageLink21 { get; set; }

        
        
        public int CollagePicture22Id { get; set; }
        public string CollageLink22 { get; set; }

        
        
        public int CollagePicture23Id { get; set; }
        public string CollageLink23 { get; set; }

        
        
        public int CollagePicture24Id { get; set; }
        public string CollageLink24 { get; set; }

        
        
        public int CollagePicture25Id { get; set; }
        public string CollageLink25 { get; set; }

        
        
        public int CollagePicture26Id { get; set; }
        public string CollageLink26 { get; set; }

        
        
        public int CollagePicture27Id { get; set; }
        public string CollageLink27 { get; set; }

        
        
        public int CollagePicture28Id { get; set; }
        public string CollageLink28 { get; set; }

        
        
        public int CollagePicture29Id { get; set; }
        public string CollageLink29 { get; set; }

        
        
        public int CollagePicture30Id { get; set; }
        public string CollageLink30 { get; set; }

        
        
        public int CollagePicture31Id { get; set; }
        public string CollageLink31 { get; set; }

        
        
        public int CollagePicture32Id { get; set; }
        public string CollageLink32 { get; set; }

        
        
        public int CollagePicture33Id { get; set; }
        public string CollageLink33 { get; set; }

        
        
        public int CollagePicture34Id { get; set; }
        public string CollageLink34 { get; set; }

        
        
        public int CollagePicture35Id { get; set; }
        public string CollageLink35 { get; set; }

        
        
        public int CollagePicture36Id { get; set; }
        public string CollageLink36 { get; set; }

        
        
        public int CollagePicture37Id { get; set; }
        public string CollageLink37 { get; set; }

        
        
        public int CollagePicture38Id { get; set; }
        public string CollageLink38 { get; set; }
    }
}
