using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.TicketControl.Models.DatePersonalization
{
    public class CreateOrEditModel
    {
        public CreateOrEditModel()
        {
            Schedules = new List<Schedules>();
            TakenDates = new List<DateTime>();
        }

        public int Id { get; set; }

        [NopResourceDisplayName("Fecha")]
        //[UIHint("DateNullable")]
        public DateTime Date { get; set; }

        [NopResourceDisplayName("Fecha original")]
        //[UIHint("DateNullable")]
        public DateTime OriginalDate { get; set; }

        public string Log { get; set; }

        public List<Schedules> Schedules { get; set; }

        [NopResourceDisplayName("Horarios")]
        public string ScheduleSplit { get; set; }

        public List<DateTime> TakenDates { get; set; }
    }

    public class Schedules
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OriginalQuantity { get; set; }
    }
}
