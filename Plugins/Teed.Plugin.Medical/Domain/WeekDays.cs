using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Domain
{
    public enum WeekDays
    {
        [Display(Name = "Domingo")]
        Sunday = 0,

        [Display(Name = "Lunes")]
        Monday = 1,

        [Display(Name = "Martes")]
        Tuesday = 2,

        [Display(Name = "Miércoles")]
        Wednesday = 3,

        [Display(Name = "Jueves")]
        Thursday = 4,

        [Display(Name = "Viernes")]
        Friday = 5,

        [Display(Name = "Sábado")]
        Saturday = 6,
    }
}