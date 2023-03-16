using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Patients;

namespace Teed.Plugin.Medical.Data.Mapping
{
    public class PatientFileMap : NopEntityTypeConfiguration<PatientFile>
    {
        public PatientFileMap()
        {
            ToTable("PatientFiles");
            HasKey(m => m.Id); // Primary Key
        }
    }
}