using System.Collections.Generic;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;
using Nop.Web.Models.Common;

namespace Nop.Web.Models.Customer
{
    public partial class CustomerLeaderboardsListModel : BaseNopModel
    {
        public CustomerLeaderboardsListModel()
        {
            Leaders = new List<LeaderModel>();
        }

        public IList<LeaderModel> Leaders { get; set; }
    }

    public class LeaderModel
    {
        [NopResourceDisplayName("Usuario")]
        public string User { get; set; }

        [NopResourceDisplayName("Puntos")]
        public decimal Points { get; set; }

        public bool IsCurrentUser { get; set; }
    }
}