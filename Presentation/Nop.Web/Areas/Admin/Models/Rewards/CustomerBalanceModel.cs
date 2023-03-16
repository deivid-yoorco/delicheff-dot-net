using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation.Attributes;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Validators.Catalog;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Web.Areas.Admin.Models.Rewards
{
    public class CustomerBalanceModel
    {
        public int Id { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }

        public bool Deleted { get; set; }

        public int CustomerId { get; set; }

        public decimal Amount { get; set; }

        public int? OrderId { get; set; }

        public int GivenByCustomerId { get; set; }

        public string Comment { get; set; }
    }
}