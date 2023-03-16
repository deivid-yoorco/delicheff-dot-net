using Nop.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Groceries.Domain.BuyerPayments
{
    public enum CorporateCardStatus
    {
        [Display(Name = "Activa")]
        Active = 0,

        [Display(Name = "Inactiva")]
        Inactive = 1,

        [Display(Name = "Extraviada")]
        Lost = 2
    }

    public enum RuleType
    {
        [Display(Name = "Ninguna")]
        None = 0,
    }
}
