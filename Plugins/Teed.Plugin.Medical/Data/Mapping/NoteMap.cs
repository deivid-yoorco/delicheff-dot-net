using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Note;

namespace Teed.Plugin.Medical.Data.Mapping
{
    class NoteMap : NopEntityTypeConfiguration<Note>
    {
        public NoteMap()
        {
            ToTable("Notes");
            HasKey(m => m.Id); // Primary Key

            Property(m => m.Text).HasMaxLength(1024);
        }
    }
}
