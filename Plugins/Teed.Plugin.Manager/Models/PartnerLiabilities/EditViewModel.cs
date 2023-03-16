using System;

namespace Teed.Plugin.Manager.Models.PartnerLiabilities
{
    public class EditViewModel
    {
        public int Id { get; set; }
        public string SelectedDate { get; set; }
        public DateTime SelectedDateParsed { get; set; }
        public int CreatedByUserId { get; set; }
        public int PartnerId { get; set; }
        public string Comments { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public string Log { get; set; }
    }
}