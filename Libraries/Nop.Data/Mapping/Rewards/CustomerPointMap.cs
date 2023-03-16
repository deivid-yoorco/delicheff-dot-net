using Nop.Core.Domain.Rewards;

namespace Nop.Data.Mapping.Rewards
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class CustomerPointMap : NopEntityTypeConfiguration<CustomerPoint>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public CustomerPointMap()
        {
            this.ToTable("CustomerPoints");
            this.HasKey(o => o.Id);
            this.Property(o => o.Points).HasPrecision(18, 2).IsRequired();
            this.Property(o => o.Description).IsRequired();

            this.HasRequired(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId);
        }
    }
}