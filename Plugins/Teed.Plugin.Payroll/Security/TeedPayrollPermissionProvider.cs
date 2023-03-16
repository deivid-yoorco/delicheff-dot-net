using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;
using System.Linq;

namespace Teed.Plugin.Payroll.Security
{
    public partial class TeedPayrollPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord PayrollEmployee = new PermissionRecord
        {
            Name = "Teed - Expediente de empleados",
            SystemName = "PayrollEmployee",
            Category = "Plugin"
        };

        public static readonly PermissionRecord JobCatalog = new PermissionRecord
        {
            Name = "Teed - Catálogo de puestos",
            SystemName = "JobCatalog",
            Category = "Plugin"
        };

        public static readonly PermissionRecord Incident = new PermissionRecord
        {
            Name = "Teed - Reporte de incidencias",
            SystemName = "Incident",
            Category = "Plugin"
        };

        public static readonly PermissionRecord Assistance = new PermissionRecord
        {
            Name = "Teed - Bitácora de asistencia",
            SystemName = "Assistance",
            Category = "Plugin"
        };

        public static readonly PermissionRecord PayrollManager = new PermissionRecord
        {
            Name = "Teed - Administrador de Nómina",
            SystemName = "PayrollManager",
            Category = "Plugin"
        };

        public static readonly PermissionRecord BiweeklyPayment = new PermissionRecord
        {
            Name = "Teed - Nómina quincenal",
            SystemName = "BiweeklyPayment",
            Category = "Plugin"
        };

        public static readonly PermissionRecord MinimumWagesCatalog = new PermissionRecord
        {
            Name = "Teed - Catálogo de salarios mínimos",
            SystemName = "MinimumWagesCatalog",
            Category = "Plugin"
        };

        public static readonly PermissionRecord PayrollAlerts = new PermissionRecord
        {
            Name = "Teed - Alertas de nómina",
            SystemName = "PayrollAlerts",
            Category = "Plugin"
        };

        public static readonly PermissionRecord Bonuses = new PermissionRecord
        {
            Name = "Teed - Bonos",
            SystemName = "Bonuses",
            Category = "Plugin"
        };

        public static readonly PermissionRecord ManageBonuses = new PermissionRecord
        {
            Name = "Teed - Administración de bonos",
            SystemName = "ManageBonuses",
            Category = "Plugin"
        };

        public static readonly PermissionRecord OperationalIncident = new PermissionRecord
        {
            Name = "Teed - Incidencias operacionales",
            SystemName = "OperationalIncident",
            Category = "Plugin"
        };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                PayrollEmployee,
                JobCatalog,
                Incident,
                Assistance,
                PayrollManager,
                BiweeklyPayment,
                MinimumWagesCatalog,
                PayrollAlerts,
                Bonuses,
                ManageBonuses,
                OperationalIncident
            };
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return Enumerable.Empty<DefaultPermissionRecord>();
        }
    }
}