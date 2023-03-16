using System;

namespace Teed.Plugin.Manager.Models.PartnerLiabilities
{
    public class ListViewModel
    {
        public int Id { get; set; }
        public string DateString { get; set; }
        public DateTime DateObject { get; set; }
        public string PartnerName { get; set; }
        public int PartnerId { get; set; }
        public string AmountFormatted { get; set; }
        public decimal Amount { get; set; }
        public string Balance { get; set; }
    }
}
