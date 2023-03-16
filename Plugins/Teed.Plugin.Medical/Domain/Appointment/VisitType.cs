using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Domain.Appointment
{
    public enum VisitType
    {
        [Display(Name = "Primera vez")]
        FirstTime = 1,
        [Display(Name = "Subsecuente")]
        Subsequent = 2,
        [Display(Name = "Revisión")]
        Checkup = 3,
        [Display(Name = "Retoque")]
        Tweak = 4,
        [Display(Name = "Relleno")]
        Fill = 5,
        [Display(Name = "Botox")]
        Botox = 6,
        [Display(Name = "RDP")]
        RDP = 7,
        [Display(Name = "Procedimiento")]
        Procedure = 8
    }
}