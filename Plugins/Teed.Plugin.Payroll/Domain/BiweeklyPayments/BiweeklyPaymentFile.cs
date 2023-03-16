﻿using Nop.Core;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payroll.Domain.BiweeklyPayments
{
    public class BiweeklyPaymentFile : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int Size { get; set; }
        public virtual string Extension { get; set; }
        public virtual string FileMimeType { get; set; }
        public virtual PaymentFileType PaymentFileType { get; set; }
        public virtual byte[] File { get; set; }

        public virtual BiweeklyPayment BiweeklyPayment { get; set; }
        public virtual int BiweeklyPaymentId { get; set; }
    }
}