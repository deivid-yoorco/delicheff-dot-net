﻿using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Domain.Note
{
    public class Note : BaseEntity
    {
        public virtual Guid GuidId { get; set; }
        public virtual string Text { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }
        public virtual int PatientId { get; set; }
    }
}
