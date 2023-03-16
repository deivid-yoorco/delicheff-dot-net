using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using System.Collections.Generic;
using Teed.Plugin.Medical.Domain;
using Teed.Plugin.Medical.Domain.Appointment;

namespace Teed.Plugin.Medical.Models.Appointment
{
    public class AppointmentDetailsModel
    {
        public int Id { get; set; }
        public Patient Patient { get; set; }
        public string Doctor { get; set; }
        public Domain.Branches.Branch Branch { get; set; }
        public string AppointmentDate { get; set; }
        public AppointmentStatus Status { get; set; }
        public string Comments { get; set; }
        public VisitType VisitType { get; set; }
        public List<Customer> DoctorsAndNurses { get; set; }
        public List<Product> Products { get; set; }
        public List<string> Customers { get; set; }
    }
}
