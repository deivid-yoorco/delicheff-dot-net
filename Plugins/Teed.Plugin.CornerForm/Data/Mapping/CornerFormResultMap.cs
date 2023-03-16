using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.CornerForm.Domain;

namespace Teed.Plugin.CornerForm.Data.Mapping
{
    public class CornerFormResultMap : NopEntityTypeConfiguration<CornerFormResult>
    {
        public CornerFormResultMap()
        {
            ToTable(nameof(CornerFormResult));
            HasKey(x => x.Id);
        }
    }
}
