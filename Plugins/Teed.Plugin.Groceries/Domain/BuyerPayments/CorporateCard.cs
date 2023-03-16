using Nop.Core;
using System;

namespace Teed.Plugin.Groceries.Domain.BuyerPayments
{
    public class CorporateCard : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public int EmployeeId { get; set; }
        public string CardNumber { get; set; }
        public int StatusId { get; set; }
        public int RuleId { get; set; }

        public string Log { get; set; }
    }
}
