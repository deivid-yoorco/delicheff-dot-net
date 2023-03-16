using Nop.Data.Mapping;
using Teed.Plugin.MessageBird.Domain;

namespace Teed.Plugin.MessageBird.Data.Mapping
{
    public class MessageBirdLogMap : NopEntityTypeConfiguration<MessageBirdLog>
    {
        public MessageBirdLogMap()
        {
            ToTable("MessageBirdLogs");
            HasKey(x => x.Id);
        }
    }
}