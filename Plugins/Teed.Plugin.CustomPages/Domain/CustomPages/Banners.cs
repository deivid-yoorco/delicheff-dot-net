using Nop.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.CustomPages.Domain.CustomPages
{
    public class Banners : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }
        
        public virtual int CustomPageId { get; set; }

        [ForeignKey("CustomPageId")]
        public virtual CustomPage CustomPage { get; set; }

        public int BannerPicture5Id { get; set; }
        public string BannerTitle5 { get; set; }
        public string BannerSubTitle5 { get; set; }
        public string BannerLink5 { get; set; }
        public string BannerTitleColor5 { get; set; }
        public string BannerSubTitleColor5 { get; set; }

        public int BannerPicture6Id { get; set; }
        public string BannerTitle6 { get; set; }
        public string BannerSubTitle6 { get; set; }
        public string BannerLink6 { get; set; }
        public string BannerTitleColor6 { get; set; }
        public string BannerSubTitleColor6 { get; set; }
    }
}
