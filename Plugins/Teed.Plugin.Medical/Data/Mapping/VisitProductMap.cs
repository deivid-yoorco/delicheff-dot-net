using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Visit;

namespace Teed.Plugin.Medical.Data.Mapping
{
    public class VisitProductMap : NopEntityTypeConfiguration<VisitProduct>
    {
        public VisitProductMap()
        {
            ToTable("VisitProduct");

            HasKey(m => m.Id); // Primary Key
        }
    }
}
