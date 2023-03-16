using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.ShippingRoutes
{
    public enum OrderOptimizationTypes
    {
        [Display(Name = "Solicitud inmediata")]
        Inmediate = 1,
        [Display(Name = "Solicitud en consideración")]
        ToConsider = 2,
        [Display(Name = "Solicitud en proceso de evaluación")]
        EvaluationProcess = 3,
        [Display(Name = "Solicitud realizada por el cliente")]
        RequestedByClient = 4,
        [Display(Name = "Solicitud aplicable en trayecto")]
        ApplicableOnWay = 5
    }
}
