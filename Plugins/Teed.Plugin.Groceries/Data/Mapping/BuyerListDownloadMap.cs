using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.OrderItemBuyers;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class ManufacturerListDownloadMap : NopEntityTypeConfiguration<ManufacturerListDownload>
    {
        public ManufacturerListDownloadMap()
        {
            ToTable(nameof(ManufacturerListDownload));
            HasKey(x => x.Id);
        }
    }
}
