using Nop.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Domain.Identity
{
    public class CustomerSecurityToken : BaseEntity
    {
        public virtual Guid GuidId { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }
        public virtual int CustomerId { get; set; }
        public virtual string Uuid { get; set; }
        public virtual string Model { get; set; }
        public virtual string Platform { get; set; }
        public virtual string Version { get; set; }
        public virtual string Manufacturer { get; set; }
        public virtual string Serial { get; set; }
        public virtual string RefreshToken { get; set; }
        public virtual string FirebaseToken { get; set; }
    }
}