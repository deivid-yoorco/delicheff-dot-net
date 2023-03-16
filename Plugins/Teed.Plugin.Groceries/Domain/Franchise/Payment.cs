using Nop.Core;
using System;
using System.Collections.Generic;

namespace Teed.Plugin.Groceries.Domain.Franchise
{
    public class Payment : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public DateTime? PaymentDate { get; set; }
        public decimal PaymentAmount { get; set; }
        public string Comment { get; set; }
        public string Log { get; set; }
        public int StatusId { get; set; }
        public int FranchiseId { get; set; }
        public virtual Franchise Franchise { get; set; }
        public int? VerifiedByCustomerId { get; set; }
        public DateTime? VerifiedDate { get; set; }

        private ICollection<PaymentFile> _paymentFiles;
        public virtual ICollection<PaymentFile> PaymentFiles
        {
            get { return _paymentFiles ?? (_paymentFiles = new List<PaymentFile>()); }
            protected set { _paymentFiles = value; }
        }
    }
}
