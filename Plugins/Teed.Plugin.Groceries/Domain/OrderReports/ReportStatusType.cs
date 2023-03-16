using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Groceries.Domain.OrderReports
{
    public enum ReportStatusType
    {
        [Display(Name = "Pendiente de aprobación")]
        Pending = 1,
        [Display(Name = "Cerrado")]
        Closed = 2,
    }
}
