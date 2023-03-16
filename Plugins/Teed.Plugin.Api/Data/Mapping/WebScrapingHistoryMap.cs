using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Domain.Groceries;

namespace Teed.Plugin.Api.Data.Mapping
{
    public class WebScrapingHistoryMap : NopEntityTypeConfiguration<WebScrapingHistory>
    {
        public WebScrapingHistoryMap()
        {
            ToTable("WebScrapingHistory");
            HasKey(m => m.Id);
        }
    }
}
