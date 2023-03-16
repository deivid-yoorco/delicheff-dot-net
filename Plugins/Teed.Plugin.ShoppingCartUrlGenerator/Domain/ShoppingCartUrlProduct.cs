using Nop.Core;
using System;

namespace Teed.Plugin.ShoppingCartUrlGenerator.Domain
{
    public class ShoppingCartUrlProduct : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int ProductId { get; set; }
        public virtual int Quantity { get; set; }
        public virtual bool BuyingBySecondary { get; set; }
        public virtual string SelectedPropertyOption { get; set; }

        public virtual int ShoppingCartUrlId { get; set; }
        public ShoppingCartUrl ShoppingCartUrl { get; set; }
    }
}