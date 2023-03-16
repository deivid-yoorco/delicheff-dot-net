using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payroll.Domain.PayrollEmployees
{
    public enum EmployeeStatus
    {
        [Display(Name = "Candidato")]
        Candidate = 10,

        [Display(Name = "Activo")]
        Active = 20,

        [Display(Name = "Baja")]
        Discharged = 30
    }
}
