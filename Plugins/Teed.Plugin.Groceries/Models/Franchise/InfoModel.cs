using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.Franchise
{
    public class InfoModel
    {
        public InfoModel()
        {
            Vehicles = new List<VehicleData>();
            Customers = new List<CustomerData>();
            VehiclesSelect = new List<SelectListItem>();
        }

        public string FranchiseName { get; set; }
        public int FranchiseId { get; set; }
        public bool IsAdmin { get; set; }
        public decimal BalanceDue { get; set; }
        public decimal BalanceToBeReleased { get; set; }
        public decimal Capital { get; set; }
        public decimal TotalPayments { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public int ActiveVehicles { get; set; }
        public int TotalIncidentsLast30Days { get; set; }

        public DateTime InitDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<SelectListItem> VehiclesSelect { get; set; }

        public IList<VehicleData> Vehicles { get; set; }
        public IList<CustomerData> Customers { get; set; }
    }

    public class VehicleData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
    }

    public class CustomerData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
    }
}