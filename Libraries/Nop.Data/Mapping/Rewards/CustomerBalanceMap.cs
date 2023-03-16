using Nop.Core.Domain.Rewards;

namespace Nop.Data.Mapping.Rewards
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class CustomerBalanceMap : NopEntityTypeConfiguration<CustomerBalance>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public CustomerBalanceMap()
        {
            this.ToTable("CustomerBalances");
            this.HasKey(o => o.Id);
            this.Property(o => o.Amount).HasPrecision(18, 4);
            this.Property(o => o.CustomerId).IsRequired();
            this.Property(o => o.GivenByCustomerId).IsRequired();
        }
    }
}