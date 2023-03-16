using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class RewardItemMap : NopEntityTypeConfiguration<RewardItem>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public RewardItemMap()
        {
            this.ToTable("RewardItem");
            this.HasKey(p => p.Id);
            this.Property(p => p.ProductId);
            this.Property(p => p.IsActive);
            this.Property(p => p.Log).HasMaxLength(400);
            this.Property(p => p.CreatedOnUtc);
            this.Property(p => p.UpdatedOnUtc);
            this.Property(p => p.Deleted);
        }
    }
}