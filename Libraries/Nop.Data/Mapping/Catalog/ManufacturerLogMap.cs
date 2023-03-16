using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class ManufacturerLogMap : NopEntityTypeConfiguration<ManufacturerLog>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public ManufacturerLogMap()
        {
            this.ToTable("ManufacturerLog");
            this.HasKey(p => p.Id);
        }
    }
}