using Nop.Core.Configuration;

namespace Nop.Plugin.Shipping.Estafeta
{
    public class EstafetaSettings : ISettings
    {
        //Datos generales

        public string Password { get; set; }

        public string Login { get; set; }

        //Frecuencia Cotizador

        public string UrlCotizador { get; set; }

        public int CotizadorId { get; set; }

        //Guías

        public string UrlGuias { get; set; }

        public string GuiasId { get; set; }

        public string ServiceTypeId { get; set; }

        public string OfficeNum { get; set; }

        //Consulta de envíos

        public string UrlEnvios { get; set; }

        public string EnviosId { get; set; }
    }
}