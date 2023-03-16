using Nop.Core.Configuration;
using System;

namespace Teed.Plugin.FavoriteProducts
{
    public class FavoriteProductsSettings : ISettings
    {
        public string Body { get; set; }
        public string TextMenu { get; set; }
        public int ProductsPerPage { get; set; }
        public bool Active { get; set; }
    }
}
