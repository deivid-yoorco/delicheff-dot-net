using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Domain.Patients
{
    public class PatientFile : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public int PatientId { get; set; }
        public string FileUrl { get; set; }
        public string FileType { get; set; }
        public string Extension { get; set; }
        public long Size { get; set; }
        public DateTime UploadedDateUtc { get; set; }
        public int UploadedByUserId { get; set; }
        public string Description { get; set; }
    }
}