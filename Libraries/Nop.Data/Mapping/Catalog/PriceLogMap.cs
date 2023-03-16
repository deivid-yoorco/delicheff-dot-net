using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class PriceLogMap : NopEntityTypeConfiguration<PriceLog>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public PriceLogMap()
        {
            this.ToTable("PriceLog");
            this.HasKey(p => p.Id);
        }
    }
}