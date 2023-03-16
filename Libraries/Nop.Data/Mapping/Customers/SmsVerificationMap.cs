using Nop.Core.Domain.Customers;

namespace Nop.Data.Mapping.Customers
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class SmsVerificationMap : NopEntityTypeConfiguration<SmsVerification>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public SmsVerificationMap()
        {
            this.ToTable("SmsVerifications");
            this.HasKey(x => x.Id);
        }
    }
}