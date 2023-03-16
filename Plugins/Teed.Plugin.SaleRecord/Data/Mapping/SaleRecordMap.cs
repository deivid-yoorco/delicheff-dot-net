using Nop.Data.Mapping;
using Teed.Plugin.SaleRecord.Domain.SaleRecords;

namespace Teed.Plugin.SaleRecord.Data.Mapping
{
    public class SaleRecordMap : NopEntityTypeConfiguration<SaleRecords>
    {
        public SaleRecordMap()
        {
            ToTable(nameof(SaleRecords));
            HasKey(x => x.Id);
        }
    }
}