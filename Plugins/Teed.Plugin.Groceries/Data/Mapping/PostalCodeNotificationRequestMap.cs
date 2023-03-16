using Nop.Data.Mapping;
using Teed.Plugin.Groceries.Domain.ShippingAreas;

namespace Teed.Plugin.Groceries.Data.Mapping
{
    public class PostalCodeNotificationRequestMap : NopEntityTypeConfiguration<PostalCodeNotificationRequest>
    {
        public PostalCodeNotificationRequestMap()
        {
            ToTable(nameof(PostalCodeNotificationRequest));
            HasKey(x => x.Id);
            Property(x => x.PostalCode).IsRequired();
            Property(x => x.Email).IsRequired();
        }
    }
}
