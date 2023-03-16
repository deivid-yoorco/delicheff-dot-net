using Nop.Core.Domain.Rewards;

namespace Nop.Data.Mapping.Rewards
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class LevelHistoryMap : NopEntityTypeConfiguration<LevelHistory>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public LevelHistoryMap()
        {
            this.ToTable("LevelHistories");
            this.HasKey(o => o.Id);
            this.Property(o => o.ActionId).IsRequired();

            this.HasRequired(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId);
            this.HasRequired(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId);
        }
    }
}