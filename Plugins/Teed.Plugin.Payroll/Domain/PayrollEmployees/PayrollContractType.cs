using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Payroll.Domain.PayrollEmployees
{
    public enum PayrollContractType
    {
        [Display(Name = "Sin especificar",
            Description = "Sin especificar")]
        NotSecified = 0,

        [Display(Name = "Empleado", 
            Description = "Empleado")]
        Employee = 1,

        [Display(Name = "Prestador de servicios",
            Description = "Prestador de servicios")]
        ServiceProvider = 2,

        [Display(Name = "Esporádico",
            Description = "Esporádico")]
        Sporadic = 3,

        [Display(Name = "Accionista operativo",
            Description = "Accionista operativo")]
        OperatingShareholder = 4,
    }
}
