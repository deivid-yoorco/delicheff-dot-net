using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class StockLogMap : NopEntityTypeConfiguration<StockLog>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public StockLogMap()
        {
            this.ToTable("StockLog");
            this.HasKey(p => p.Id);
        }
    }
}