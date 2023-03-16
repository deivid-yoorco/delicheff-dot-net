using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Logisfashion.Domain
{
    public class LogisfashionRequestLog : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public string RequestJson { get; set; }
        public string LastResponseJson { get; set; }
        public RequestType RequestType { get; set; }
        public StatusType StatusType { get; set; }
        public string StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public bool Error { get; set; }
        public string ErrorMessage { get; set; }
        public int ParentLogId { get; set; }
        public int CustomerId { get; set; }
        public string LogInternalId { get; set; }
        public string RelatedElementName { get; set; }
        public int RelatedElementId { get; set; }
    }
}