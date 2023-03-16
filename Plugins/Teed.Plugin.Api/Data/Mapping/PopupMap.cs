using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Domain.Popups;

namespace Teed.Plugin.Api.Data.Mapping
{
    public class PopupMap : NopEntityTypeConfiguration<Popup>
    {
        public PopupMap()
        {
            ToTable("Popups");
            HasKey(p => p.Id);
        }
    }
}
