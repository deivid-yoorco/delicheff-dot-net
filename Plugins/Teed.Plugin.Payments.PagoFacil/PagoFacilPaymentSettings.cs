using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.PagoFacil
{
    public class PagoFacilPaymentSettings : ISettings
    {
        public string IdSucursal { get; set; }

        public string IdUsuario { get; set; }

        public int IdServicio { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }
    }
}