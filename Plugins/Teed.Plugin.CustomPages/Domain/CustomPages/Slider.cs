using Nop.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.CustomPages.Domain.CustomPages
{
    public class Slider : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }
        
        public virtual int CustomPageId { get; set; }

        [ForeignKey("CustomPageId")]
        public virtual CustomPage CustomPage { get; set; }

        public string SliderVideoId { get; set; }

        public int SliderBannerPicture1Id { get; set; }
        public string SliderBannerText1 { get; set; }
        public string SliderBannerLink1 { get; set; }

        public int SliderBannerPicture2Id { get; set; }
        public string SliderBannerText2 { get; set; }
        public string SliderBannerLink2 { get; set; }

        public int SliderBannerPicture3Id { get; set; }
        public string SliderBannerText3 { get; set; }
        public string SliderBannerLink3 { get; set; }

        public int SliderBannerPicture4Id { get; set; }
        public string SliderBannerText4 { get; set; }
        public string SliderBannerLink4 { get; set; }

        public int SliderArrowId { get; set; }
    }
}
