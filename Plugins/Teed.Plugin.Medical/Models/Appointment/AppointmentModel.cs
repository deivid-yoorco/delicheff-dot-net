using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using Teed.Plugin.Medical.Domain.Appointment;

namespace Teed.Plugin.Medical.Models.Appointment
{
    public class AppointmentModel : BaseNopModel
    {
        public AppointmentModel()
        {
            SelectedUserIds = new List<int>();
            SelectedProductsIds = new List<int>();
        }

        public int PatientId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public AppointmentStatus Status { get; set; }
        public VisitType VisitType { get; set; }
        public IList<int> SelectedUserIds { get; set; }
        public IList<int> SelectedProductsIds { get; set; }
    }
}