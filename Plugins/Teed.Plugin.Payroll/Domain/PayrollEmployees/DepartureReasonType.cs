using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payroll.Domain.PayrollEmployees
{
    public enum DepartureReasonType
    {
        [Display(Name = "Ninguna")]
        None = 0,

        [Display(Name = "Renuncia voluntaria")]
        Quitting = 1,

        [Display(Name = "Despido justificado")]
        Fired = 2,

        [Display(Name = "Periodo de prueba")]
        TrialEnd = 3,

        [Display(Name = "Otro")]
        Other = 4
    }
}
