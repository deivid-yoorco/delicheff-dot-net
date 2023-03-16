using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;
using System;
using Nop.Web.Areas.Admin.Models.Settings;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Core.Domain.Catalog;

namespace Nop.Web.Models.Catalog
{
    public partial class RewardItemModel : BaseNopModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public bool IsActive { get; set; }
        public string Log { get; set; }
        public DateTime CreateOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Delete { get; set; }
    }
}