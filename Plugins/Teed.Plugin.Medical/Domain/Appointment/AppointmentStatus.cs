using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Medical.Domain.Appointment
{
    public enum AppointmentStatus
    {
        [Display(Name = "Confirmada")]
        Confirmed = 1,

        [Display(Name = "Agendado")]
        Scheduled = 2,
        
        [Display(Name = "Cancelada")]
        Cancelled = 3,

        [Display(Name = "Completada")]
        Complete = 4,

        [Display(Name = "Reprogramada")]
        Rescheduled = 5,

        [Display(Name = "No confirmada")]
        NotConfirmed = 7,

        [Display(Name = "Registrado")]
        Registered = 8,
    }
}