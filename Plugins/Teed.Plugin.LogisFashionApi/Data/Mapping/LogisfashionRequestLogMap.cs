using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Logisfashion.Domain;

namespace Teed.Plugin.Logisfashion.Data.Mapping
{
    public class LogisfashionRequestLogMap : NopEntityTypeConfiguration<LogisfashionRequestLog>
    {
        public LogisfashionRequestLogMap()
        {
            ToTable("LogisfashionRequestLogs");
            HasKey(x => x.Id);
        }
    }
}
