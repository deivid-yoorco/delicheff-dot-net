using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.RewardItem
{
    public partial class RewardItemsModel : BaseNopModel
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public bool BuyingBySecondary { get; set; }

        public string SelectedPropertyOption { get; set; }

        public bool IsActive { get; set; }

        public string Log { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }

        public bool Deleted { get; set; }
    }
}