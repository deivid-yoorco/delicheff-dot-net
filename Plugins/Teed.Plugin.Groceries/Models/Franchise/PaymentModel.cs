using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Payments;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.Franchise
{
    public class PaymentModel
    {
        public int Id { get; set; }
        public decimal PaymentAmount { get; set; }
        public string Comment { get; set; }
        public int StatusId { get; set; }
        public PaymentStatus Status { get; set; }
        public int FranchiseId { get; set; }
        public List<SelectListItem> Franchises { get; set; }
        public string Log { get; set; }
        public string PaymentDateString { get; set; }
        [UIHint("DateNullable")]
        public DateTime? PaymentDate { get; set; }

        public AddPaymentFile AddPaymentFile { get; set; }
    }

    public class AddPaymentFile
    {
        public AddPaymentFile()
        {
            IsPdf = null;
        }

        public int Id { get; set; }
        public int PaymentId { get; set; }
        public virtual int Size { get; set; }
        public virtual string Extension { get; set; }
        public virtual string FileMimeType { get; set; }
        public virtual int FileTypeId { get; set; }
        public virtual IFormFile File { get; set; }
        public byte[] FileArray { get; set; }
        public virtual string Title { get; set; }
        public virtual string Description { get; set; }

        public virtual bool? IsPdf { get; set; }
    }

    public class PaymentListModel
    {
        public List<SelectListItem> Franchises { get; set; }
        public int SelectedFranchiseId { get; set; }
    }
}
