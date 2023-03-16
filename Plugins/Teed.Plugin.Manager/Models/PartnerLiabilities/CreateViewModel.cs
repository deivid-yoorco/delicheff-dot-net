using System;

namespace Teed.Plugin.Manager.Models.PartnerLiabilities
{
    public class CreateViewModel
    {
        public string SelectedDate { get; set; }
        public int CreatedByUserId { get; set; }
        public int PartnerId { get; set; }
        public string Comments { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public string Log { get; set; }
    }
}