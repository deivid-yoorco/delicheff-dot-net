using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.SaleRecord.Models.SaleRecords
{
    public class SaleRecordModel
    {
        public SaleRecordModel()
        {
            ListTimes = new List<SelectListItem>();
        }

        public int Id { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal Quantity { get; set; }
        public string CustomerFullName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string PaymentMethod { get; set; }
        public decimal Total { get; set; }
        public string Time { get; set; }
        public int CardTypeId { get; set; }
        public string CardLast4Numbers { get; set; }
        public string Log { get; set; }
        public string SaleDateString { get; set; }
        public string Comment { get; set; }

        public IList<SelectListItem> ListTimes { get; set; }
    }
}
