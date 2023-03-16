using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.TaggableBox
{
    public class TaggableBoxDto
    {
        public int ElementId { get; set; }
        public string PictureUrl { get; set; }
        public int Position { get; set; }
        public int Type { get; set; }
        public string ElementName { get; set; }
        public bool IsChild { get; set; }
    }
}
