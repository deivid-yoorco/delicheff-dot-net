using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Domain.TaggableBoxes;

namespace Teed.Plugin.Api.Data.Mapping
{
    public class TaggableBoxMap : NopEntityTypeConfiguration<TaggableBox>
    {
        public TaggableBoxMap()
        {
            ToTable("TaggableBoxes");
            HasKey(p => p.Id);
        }
    }
}
