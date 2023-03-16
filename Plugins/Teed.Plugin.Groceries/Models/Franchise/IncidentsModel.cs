using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.Franchise
{
   
    public class IncidentsModel
    {
        public IncidentsModel()
        {
            OrderIdsList = new List<int>();
            OrderItemIdsList = new List<int>();
            Penalties = new List<SelectListItem>();
        }

        public int Id { get; set; }

        public string IncidentCode { get; set; }

        public DateTime Date { get; set; }
        public string DateString { get; set; }
        public int FranchiseId { get; set; }
        public string FranchiseName { get; set; }
        public int VehicleId { get; set; }
        public string VehicleName { get; set; }
        public string Comments { get; set; }
        public string Log { get; set; }

        public string OrderIds { get; set; }
        public string OrderItemIds { get; set; }
        public decimal? ReportedAmount { get; set; }
        public decimal Amount { get; set; }
        public int AuthorizedStatusId { get; set; }

        public IList<int> OrderIdsList { get; set; }
        public IList<int> OrderItemIdsList { get; set; }
        public int SelectedQuantity { get; set; }

        public IList<SelectListItem> Penalties { get; set; }
        public IList<SelectListItem> Status { get; set; }

        public AddIncidentFile AddIncidentFile { get; set; }

        public List<string> Orders { get; set; }
        public List<string> Products { get; set; }
    }

    public class AddIncidentFile
    {
        public virtual int Id { get; set; }
        public virtual int IncidentId { get; set; }
        public virtual int Size { get; set; }
        public virtual string Extension { get; set; }
        public virtual string FileMimeType { get; set; }
        public virtual IFormFile File { get; set; }
        public byte[] FileArray { get; set; }
        public virtual string Title { get; set; }
        public virtual string Description { get; set; }
    }

    public class IncidentsListModel
    {
        public string Date { get; set; }
        public int FranchiseId { get; set; }
        public int VehicleId { get; set; }
    }
}