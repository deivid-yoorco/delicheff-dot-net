using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace Teed.Plugin.Groceries.Models.ShippingUserRoute
{
    public class AssignBuyersModel
    {
        public DateTime Date { get; set; }
        public List<SelectListItem> SubstituteBuyers { get; set; }
        public List<Buyer> Buyers { get; set; }
    }

    public class Buyer
    {
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public int SubstituteCustomerId { get; set; }
    }

    public class SubmitAssignBuyersModel
    {
        public DateTime Date { get; set; }
        public List<SubmitAssignBuyersData> Data { get; set; }
    }

    public class SubmitAssignBuyersData
    {
        public int BuyerId { get; set; }
        public string SelectedCustomerId { get; set; }
    }
}