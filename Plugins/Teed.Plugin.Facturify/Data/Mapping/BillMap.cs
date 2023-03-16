using Nop.Data.Mapping;
using Teed.Plugin.Facturify.Domain;

namespace Teed.Plugin.Facturify.Data.Mapping
{
    public class BillMap : NopEntityTypeConfiguration<Bill>
    {
        public BillMap()
        {
            ToTable("Bills");
            HasKey(x => x.Id);
        }
    }
}
