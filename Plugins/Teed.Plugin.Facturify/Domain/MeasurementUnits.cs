using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Facturify.Domain
{
    public enum MeasurementUnits
    {
        /// <summary>
        /// Gramos
        /// </summary>
        [Display(Name = "Gramos")]
        GRM = 1,

        /// <summary>
        /// Pieza
        /// </summary>
        [Display(Name = "Pieza")]
        H87 = 2,

        /// <summary>
        /// Kilogramo
        /// </summary>
        [Display(Name = "Kilogramo")]
        KGM = 3,

        /// <summary>
        /// Litro
        /// </summary>
        [Display(Name = "Litro")]
        LTR = 4
    }
}
