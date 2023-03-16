using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.TicketControl.Models.Schedule
{
    public class CreateOrEditModel
    {
        public int Id { get; set; }

        [NopResourceDisplayName("Nombre")]
        public string Name { get; set; }

        [NopResourceDisplayName("Cantidad")]
        public int Quantity { get; set; }

        [NopResourceDisplayName("Orden para mostrar")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Hora")]
        public int Hour { get; set; }

        public string Log { get; set; }
    }
}
