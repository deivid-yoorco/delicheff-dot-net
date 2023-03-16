using System.Collections.Generic;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;
using Nop.Web.Models.Common;

namespace Nop.Web.Models.Customer
{
    public partial class CustomerPointsListModel : BaseNopModel
    {
        public CustomerPointsListModel()
        {
            Points = new List<PointModel>();
        }

        public int CurrentMonthsFilteringAmount { get; set; }
        public IList<PointModel> Points { get; set; }
    }

    public class PointModel
    {
        [NopResourceDisplayName("Orden")]
        public int? OrderId { get; set; }

        [NopResourceDisplayName("Descripción")]
        public string Description { get; set; }

        [NopResourceDisplayName("Puntos")]
        public decimal Points { get; set; }

        [NopResourceDisplayName("Fecha en local")]
        public string Date { get; set; }
    }
}