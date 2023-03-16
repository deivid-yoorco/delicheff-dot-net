using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.PrintedCouponBooks;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class PrintedCouponBookMap : NopEntityTypeConfiguration<PrintedCouponBook>
    {
        public PrintedCouponBookMap()
        {
            ToTable("PrintedCouponBooks");
            HasKey(x => x.Id);
        }
    }
}