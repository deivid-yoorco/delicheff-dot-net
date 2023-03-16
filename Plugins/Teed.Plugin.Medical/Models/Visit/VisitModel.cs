using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain;
using Teed.Plugin.Medical.Domain.Prescription;
using Teed.Plugin.Medical.Models.Prescription;

namespace Teed.Plugin.Medical.Models.Visit
{
    public class VisitModel : BaseNopModel
    {
        public VisitModel()
        {
            SelectedUsersIds = new List<int>();
            SelectedProductsIds = new List<int>();
            PrescriptionProducts = new List<PrescriptionItem>();
        }
        public int Id { get; set; }
        public int PatientId { get; set; }
        public Patient Patient { get; set; }
        public int DoctorId { get; set; }
        public int BranchId { get; set; }
        public string BranchTitle { get; set; }
        public int ProductId { get; set; }
        public int AppointmentId { get; set; }
        public string Appoiment { get; set; }
        public string CurrentCondition { get; set; }
        public string Evolution { get; set; }
        public string PreviousTx { get; set; }
        public string ImportantRecord { get; set; }
        public string Diagnosis { get; set; }
        public string Prognostic { get; set; }
        public string Treatment { get; set; }
        public string Comment { get; set; }

        [Required(ErrorMessage = "Debes indicar el precio de la consulta.")]
        public decimal Price { get; set; }
        public DateTime LastUpdate { get; set; }
        public int ProductsCount { get; set; }
        public IList<int> SelectedUsersIds { get; set; }
        public IList<int> SelectedProductsIds { get; set; }
        public List<Customer> DoctorsAndNurses { get; set; }
        public List<Product> Products { get; set; }
        public List<PrescriptionItem> PrescriptionProducts { get; set; }
        public int DoctorUserId { get; set; }


        public string ItemName { get; set; }
        public string Dosage { get; set; }
        public int PrescriptionId { get; set; }
        public string Comments { get; set; }
        public int ItemsCount { get; set; }
    }
}