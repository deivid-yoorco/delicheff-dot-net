using Nop.Core.Domain.Rewards;

namespace Nop.Data.Mapping.Rewards
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class LevelMap : NopEntityTypeConfiguration<Level>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public LevelMap()
        {
            this.ToTable("Levels");
            this.HasKey(o => o.Id);
            this.Property(o => o.Name).IsRequired();
            this.Property(o => o.Description).IsRequired();
            this.Property(o => o.RequiredAmount).HasPrecision(18, 2).IsRequired();

            this.HasRequired(o => o.CustomerRole)
                .WithMany()
                .HasForeignKey(o => o.CustomerRoleId);
        }
    }
}