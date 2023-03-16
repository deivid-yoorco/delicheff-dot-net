using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.Franchise
{
    public class FranchiseModel
    {
        public virtual IList<FranchiseData> Franchises { get; set; }
    }

    public class FranchiseData
    {
        public FranchiseData()
        {
            CanEdit = true;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int UserInChargeId { get; set; }
        public bool IsActive { get; set; }

        public string BuyersIds { get; set; }
        public IList<int> SelectedBuyersIds { get; set; }

        public IList<SelectListItem> AvailablesBuyers { get; set; }
        public IList<SelectListItem> AvailablesFranchisee { get; set; }
        public List<string> ExistigDeliveries { get; set; }

        public bool CanEdit { get; set; }
        public string Log { get; set; }
    }
}