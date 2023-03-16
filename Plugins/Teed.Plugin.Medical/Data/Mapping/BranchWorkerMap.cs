using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Branches;

namespace Teed.Plugin.Medical.Data.Mapping
{
    public class BranchWorkerMap : NopEntityTypeConfiguration<BranchWorker>
    {
        public BranchWorkerMap()
        {
            ToTable("BranchWorkers");

            HasKey(m => m.Id); // Primary Key
        }
    }
}
