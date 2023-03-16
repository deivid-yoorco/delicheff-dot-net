using Nop.Core.Domain.Discounts;

namespace Nop.Data.Mapping.Discounts
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class SpecialDiscountTakeXPayYMap : NopEntityTypeConfiguration<SpecialDiscountTakeXPayY>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public SpecialDiscountTakeXPayYMap()
        {
            this.ToTable("SpecialDiscountTakeXPayY");
            this.HasKey(d => d.Id);
        }
    }
}