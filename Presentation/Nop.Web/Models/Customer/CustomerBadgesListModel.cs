using System.Collections.Generic;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;
using Nop.Web.Models.Common;

namespace Nop.Web.Models.Customer
{
    public partial class CustomerBadgesListModel : BaseNopModel
    {
        public CustomerBadgesListModel()
        {
            Badges = new List<BadgeModel>();
        }

        public IList<BadgeModel> Badges { get; set; }
    }

    public class BadgeModel
    {
        [NopResourceDisplayName("Ya se tiene")]
        public bool Owned { get; set; }

        [NopResourceDisplayName("Imagen")]
        public string Image { get; set; }

        [NopResourceDisplayName("Nombre")]
        public string Name { get; set; }

        [NopResourceDisplayName("Descripción")]
        public string Description { get; set; }
    }
}