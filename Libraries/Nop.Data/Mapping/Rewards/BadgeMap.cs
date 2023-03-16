using Nop.Core.Domain.Rewards;

namespace Nop.Data.Mapping.Rewards
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class BadgeMap : NopEntityTypeConfiguration<Badge>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public BadgeMap()
        {
            this.ToTable("Badges");
            this.HasKey(o => o.Id);
            this.Property(o => o.Name).IsRequired();
            this.Property(o => o.Description).IsRequired();
            this.Property(o => o.BronzeAmount).HasPrecision(18, 2);
            this.Property(o => o.SilverAmount).HasPrecision(18, 2);
            this.Property(o => o.GoldAmount).HasPrecision(18, 2);
            this.Property(o => o.ElementTypeId).IsRequired();
        }
    }
}