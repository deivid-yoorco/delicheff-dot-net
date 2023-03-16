using Nop.Data.Mapping;
using Teed.Plugin.TicketControl.Domain.Tickets;

namespace Teed.Plugin.TicketControl.Data.Mapping
{
    public class TicketMap : NopEntityTypeConfiguration<Ticket>
    {
        public TicketMap()
        {
            ToTable("Tickets");
            HasKey(x => x.Id);
        }
    }
}