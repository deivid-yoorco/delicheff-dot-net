using Nop.Data.Mapping;
using Teed.Plugin.CustomerComments.Domain.CustomerComment;

namespace Teed.Plugin.CustomerComments.Data.Mapping
{
    public class CustomerCommentMap : NopEntityTypeConfiguration<CustomerComment>
    {
        public CustomerCommentMap()
        {
            ToTable("CustomerComments");
            HasKey(x => x.Id);
        }
    }
}