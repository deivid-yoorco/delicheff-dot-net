using Nop.Web.Framework.Mvc.Models;

namespace Teed.Plugin.FavoriteProducts.Models
{
    public class ConfigurationModel : BaseNopEntityModel
    {
        public string Body { get; set; }
        public string TextMenu { get; set; }
        public int ProductsPerPage { get; set; }
        public bool Active { get; set; }
        public string DivElementClassString { get; set; }
    }
}