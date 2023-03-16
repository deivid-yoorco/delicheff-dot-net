using Nop.Core.Configuration;
using System;

namespace Teed.Plugin.RecentProducts
{
    public class RecentProductsSettings : ISettings
    {
        public string Body { get; set; }
        public string TextMenu { get; set; }
        public int ProductsBeforeDays { get; set; }
        public int ProductsPerPage { get; set; }
        public bool Active { get; set; }
    }
}
