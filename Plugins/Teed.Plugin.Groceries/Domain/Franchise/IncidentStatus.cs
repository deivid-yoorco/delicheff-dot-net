using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.Franchise
{
    public enum IncidentStatus
    {
        [Display(Name = "No autorizar")]
        NotAuthorized = 0,

        [Display(Name = "Autorizar")]
        Authorized = 1,
    }
}
