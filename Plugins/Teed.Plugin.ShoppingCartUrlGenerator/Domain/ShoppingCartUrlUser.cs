using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.ShoppingCartUrlGenerator.Domain
{
    public class ShoppingCartUrlUser : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public string UserId { get; set; }
        public int ShoppingCartUrlId { get; set; }
    }
}