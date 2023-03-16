using System;
using System.Collections.Generic;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;
using Nop.Web.Models.Common;

namespace Nop.Web.Models.Customer
{
    public partial class CustomerBalanceListModel : BaseNopModel
    {
        public CustomerBalanceListModel()
        {
            CurrentMonthsFilteringAmount = 1;
            Balances = new List<BalanceModel>();
        }

        public int CurrentMonthsFilteringAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<BalanceModel> Balances { get; set; }
    }

    public class BalanceModel
    {
        [NopResourceDisplayName("Orden")]
        public int? OrderId { get; set; }

        [NopResourceDisplayName("Descripción")]
        public string Description { get; set; }

        [NopResourceDisplayName("Puntos")]
        public decimal Amount { get; set; }

        [NopResourceDisplayName("Fecha en local")]
        public string Date { get; set; }

        public DateTime DateUnformatted { get; set; }
    }
}