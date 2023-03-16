using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.PagoFacil.Models
{
    public class JsonModel
    {
        public WebServices_Transacciones WebServices_Transacciones { get; set; }
    }

    public class WebServices_Transacciones
    {
        public Transaccion transaccion { get; set; }

    }

    public class Transaccion
    {
        public string autorizado { get; set; }
        public string autorizacion { get; set; }
        public string transaccion { get; set; }
        public string idTransaccion { get; set; }
        public string TransIni { get; set; }
        public string TransFin { get; set; }
        public DataVal dataVal { get; set; }
        public string pf_message { get; set; }
        public string status { get; set; }
    }

    public class DataVal
    {
        public string numeroTarjeta { get; set; }
        public string monto { get; set; }
    }
}
