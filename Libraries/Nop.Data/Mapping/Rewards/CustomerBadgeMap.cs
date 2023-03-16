using Nop.Core.Domain.Rewards;

namespace Nop.Data.Mapping.Rewards
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class CustomerBadgeMap : NopEntityTypeConfiguration<CustomerBadge>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public CustomerBadgeMap()
        {
            this.ToTable("CustomerBadges");
            this.HasKey(o => o.Id);
            this.Property(o => o.BadgeLevel).IsRequired();
            this.Property(o => o.Log).IsRequired();

            this.HasRequired(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId);
            this.HasRequired(o => o.Badge)
                .WithMany()
                .HasForeignKey(o => o.BadgeId);
        }
    }
}