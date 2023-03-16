using Nop.Web.Framework.Mvc.Models;

namespace Teed.Plugin.Payments.PagoFacil.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        public string IdSucursal { get; set; }
        public bool IdSucursal_OverrideForStore { get; set; }

        public string IdUsuario { get; set; }
        public bool IdUsuario_OverrideForStore { get; set; }

        public int IdServicio { get; set; }
        public bool IdServicio_OverrideForStore { get; set; }
    }
}
