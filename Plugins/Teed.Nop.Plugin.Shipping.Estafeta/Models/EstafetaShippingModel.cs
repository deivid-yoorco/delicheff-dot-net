using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Shipping.Estafeta.Models
{
    public class EstafetaShippingModel : BaseNopModel
    {
        public EstafetaShippingModel()
        {

        }

        //Datos generales
        public string Password { get; set; }
        public string Login { get; set; }

        //Frecuencia Cotizador
        [Required(ErrorMessage = "Debes de ingresar la Url")]
        public string UrlCotizador { get; set; }
        public int CotizadorId { get; set; }

        //Guías
        [Required(ErrorMessage = "Debes de ingresar la Url")]
        public string UrlGuias { get; set; }
        public string GuiasId { get; set; }
        public string ServiceTypeId { get; set; }
        public string OfficeNum { get; set; }

        //Consulta de envíos
        [Required(ErrorMessage = "Debes de ingresar la Url")]
        public string UrlEnvios { get; set; }
        public string EnviosId { get; set; }
    }
}
