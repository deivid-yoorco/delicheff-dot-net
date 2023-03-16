using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Controllers;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;

namespace Teed.Plugin.Payroll.Models.PayrollEmployee
{
    public class CreateOrUpdateModel
    {
        public CreateOrUpdateModel()
        {
            AddEmployeeFile = new AddEmployeeFile();
            AddBenefit = new AddBenefit();
            AddEmployeeSalary = new AddEmployeeSalary();
            JobCatalogs = new List<SelectListItem>();
            SubordinateIds = new List<int>();
            PendingDataCount = new PendingModel();
            AddEmployeeVacation = new AddEmployeeVacation();
            AddEmployeeJob = new AddEmployeeJob();
        }

        public virtual int Id { get; set; }

        [NopResourceDisplayName("Nombre(s)")]
        public virtual string FirstNames { get; set; }

        [NopResourceDisplayName("Apellido Paterno")]
        public virtual string MiddleName { get; set; }

        [NopResourceDisplayName("Apellido Materno")]
        public virtual string LastName { get; set; }

        [NopResourceDisplayName("Fecha de nacimiento")]
        [UIHint("DateNullable")]
        public virtual DateTime? DateOfBirth { get; set; }

        [NopResourceDisplayName("Fecha de ingreso")]
        [UIHint("DateNullable")]
        public virtual DateTime? DateOfAdmission { get; set; }
        public virtual int YearsCompleted { get; set; }
        public virtual int LiberatedVacationDaysAmount { get; set; }
        public virtual int PendingVacationDaysAmount { get; set; }

        [NopResourceDisplayName("Fecha de fin de periodo de prueba")]
        [UIHint("DateNullable")]
        public virtual DateTime? TrialPeriodEndDate { get; set; }

        [UIHint("DateNullable")]
        [NopResourceDisplayName("Fecha de salida")]
        public virtual DateTime? DateOfDeparture { get; set; }

        [NopResourceDisplayName("Proceso de baja satisfactorio")]
        public virtual bool SatisfactoryDepartureProcess { get; set; }

        [NopResourceDisplayName("Dirección")]
        public virtual string Address { get; set; }
        public virtual string Latitude { get; set; }
        public virtual string Longitude { get; set; }

        [NopResourceDisplayName("Teléfono celular")]
        public virtual string Cellphone { get; set; }

        [NopResourceDisplayName("Teléfono fijo")]
        public virtual string Landline { get; set; }

        [NopResourceDisplayName("Nombre")]
        public virtual string ReferenceOneName { get; set; }
        [NopResourceDisplayName("Medio de contacto")]
        public virtual string ReferenceOneContact { get; set; }
        [NopResourceDisplayName("Nombre")]
        public virtual string ReferenceTwoName { get; set; }
        [NopResourceDisplayName("Medio de contacto")]
        public virtual string ReferenceTwoContact { get; set; }
        [NopResourceDisplayName("Nombre")]
        public virtual string ReferenceThreeName { get; set; }
        [NopResourceDisplayName("Medio de contacto")]
        public virtual string ReferenceThreeContact { get; set; }

        [NopResourceDisplayName("Puesto")]
        public virtual int JobCatalogId { get; set; }

        [NopResourceDisplayName("Usuario vinculado al empleado")]
        public virtual int CustomerId { get; set; }
        public virtual string IMSS { get; set; }
        public virtual string RFC { get; set; }
        public virtual string CURP { get; set; }

        [NopResourceDisplayName("Número de empleado")]
        public virtual int EmployeeNumber { get; set; }

        [NopResourceDisplayName("Banco")]
        public virtual int BankType { get; set; }
        public virtual SelectList BankTypes { get; set; }

        [NopResourceDisplayName("CLABE Interbancaria")]
        public virtual string Clabe { get; set; }

        [NopResourceDisplayName("Numero de cuenta")]
        public virtual string AccountNumber { get; set; }

        public virtual AddEmployeeFile AddEmployeeFile { get; set; }
        public virtual AddBenefit AddBenefit { get; set; }

        [NopResourceDisplayName("Tipo de archivo")]
        public virtual int FileType { get; set; }
        public virtual SelectList FileTypes { get; set; }
        public virtual AddEmployeeSalary AddEmployeeSalary { get; set; }

        public virtual AddEmployeeVacation AddEmployeeVacation { get; set; }

        public virtual AddEmployeeJob AddEmployeeJob { get; set; }

        public virtual IList<SelectListItem> JobCatalogs { get; set; }
        public virtual SelectList Customers { get; set; }

        [NopResourceDisplayName("Salarios")]
        public virtual SelectList Salaries { get; set; }

        [NopResourceDisplayName("Tipo de prestación")]
        public virtual SelectList BenefitTypes { get; set; }

        public virtual List<int> SubordinateIds { get; set; }

        [NopResourceDisplayName("Estatus del empleado")]
        public virtual EmployeeStatus EmployeeStatus { get; set; }
        public virtual int EmployeeStatusId { get; set; }

        [NopResourceDisplayName("Motivo de baja")]
        public virtual DepartureReasonType DepartureReasonType { get; set; }
        public virtual int DepartureReasonTypeId { get; set; }

        [NopResourceDisplayName("Comentarios de la baja")]
        public virtual string DepartureComment { get; set; }

        [NopResourceDisplayName("Tipo de contrato")]
        public virtual PayrollContractType PayrollContractType { get; set; }
        public virtual int PayrollContractTypeId { get; set; }

        [NopResourceDisplayName("Imagen de perfil")]
        [UIHint("Picture")]
        public int? ProfilePictureId { get; set; }
        public string ProfilePicture { get; set; }

        [NopResourceDisplayName("Imagen de perfil")]
        [UIHint("Picture")]
        public int? PayrollProfilePictureId { get; set; }
        public string PayrollProfilePicture { get; set; }

        public string DefaultPicture { get; set; }

        public PendingModel PendingDataCount { get; set; }

        public bool IsParnter { get; set; }


        [NopResourceDisplayName("Franquicia a la que pertenece el empleado")]
        public int? FranchiseId { get; set; }
        public virtual SelectList Franchises { get; set; }

        public string Log { get; set; }
    }

    public partial class AddEmployeeFile
    {
        public virtual int Size { get; set; }
        public virtual string Extension { get; set; }
        public virtual string FileMimeType { get; set; }

        [NopResourceDisplayName("Tipo de archivo")]
        public virtual FileType FileType { get; set; }
        public virtual string File { get; set; }

        [NopResourceDisplayName("Titulo")]
        public virtual string Title { get; set; }

        [NopResourceDisplayName("Descripción")]
        public virtual string Description { get; set; }
    }

    public partial class AddEmployeeSalary
    {
        [NopResourceDisplayName("Fecha de aplicación")]
        [UIHint("DateNullable")]
        public virtual DateTime? ApplyDate { get; set; }

        [NopResourceDisplayName("Sueldo neto")]
        public virtual decimal NetIncome { get; set; }

        [NopResourceDisplayName("ISR Retenido")]
        public virtual decimal WithheldISR { get; set; }

        [NopResourceDisplayName("Aportaciones de Seguridad Social")]
        public virtual decimal SocialSecurityContributions { get; set; }

        [NopResourceDisplayName("Bonos")]
        public virtual decimal Bonds { get; set; }

        [NopResourceDisplayName("Prestaciones")]
        public virtual decimal Benefits { get; set; }

        [NopResourceDisplayName("Salario diario")]
        public virtual decimal IntegratedDailyWage { get; set; }
    }

    public partial class AddBenefit
    {
        public virtual int Id { get; set; }
        public virtual int LinkId { get; set; }

        [NopResourceDisplayName("Nombre")]
        public virtual string Name { get; set; }

        [NopResourceDisplayName("Descripción")]
        public virtual string Description { get; set; }

        [NopResourceDisplayName("Monto")]
        public virtual decimal Amount { get; set; }
        public virtual bool isForSalary { get; set; }
    }

    public partial class AddEmployeeJob
    {
        public virtual int Id { get; set; }
        public virtual int PayrollEmployeeId { get; set; }

        [NopResourceDisplayName("Fecha de aplicación")]
        [UIHint("DateNullable")]
        public virtual DateTime? ApplyDate { get; set; }

        [NopResourceDisplayName("Empleo")]
        public virtual int JobCatalogId { get; set; }
    }

    public partial class JobSelection
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Parent { get; set; }
    }

    public partial class AddEmployeeVacation
    {
        public virtual int Id { get; set; }
        public virtual int PayrollEmployeeId { get; set; }

        [NopResourceDisplayName("Fecha de vacacion tomada")]
        [UIHint("DateNullable")]
        public virtual DateTime? VacationDate { get; set; }

        [NopResourceDisplayName("Comentario")]
        public virtual string Comment { get; set; }
    }
}
