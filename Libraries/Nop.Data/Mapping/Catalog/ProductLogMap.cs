using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class ProductLogMap : NopEntityTypeConfiguration<ProductLog>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public ProductLogMap()
        {
            this.ToTable("ProductLog");
            this.HasKey(p => p.Id);
        }
    }
}