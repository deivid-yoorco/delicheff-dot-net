using Nop.Core;
using System;
using System.Collections.Generic;

namespace Teed.Plugin.ShoppingCartUrlGenerator.Domain
{
    public class ShoppingCartUrl : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public string Code { get; set; }
        public bool IsActive { get; set; }
        public string Body { get; set; }
        public string Log { get; set; }

        private ICollection<ShoppingCartUrlProduct> _shoppingCartUrlProducts;
        public virtual ICollection<ShoppingCartUrlProduct> ShoppingCartUrlProducts
        {
            get { return _shoppingCartUrlProducts ?? (_shoppingCartUrlProducts = new List<ShoppingCartUrlProduct>()); }
            protected set { _shoppingCartUrlProducts = value; }
        }
    }
}
