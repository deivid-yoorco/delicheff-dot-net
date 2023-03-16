using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.Corcel;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class CorcelCustomerMap : NopEntityTypeConfiguration<CorcelCustomer>
    {
        public CorcelCustomerMap()
        {
            ToTable("CorcelCustomers");
            HasKey(x => x.Id);
        }
    }
}