using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain;
using Teed.Plugin.Medical.Domain.Prescription;

namespace Teed.Plugin.Medical.Data.Mapping
{
    public class PrescriptionItemMap : NopEntityTypeConfiguration<PrescriptionItem>
    {
        public PrescriptionItemMap()
        {
            ToTable("PrescriptionItem");
            HasKey(m => m.Id); // Primary Key

            Property(m => m.Dosage).HasMaxLength(1000);
        }
    }
}