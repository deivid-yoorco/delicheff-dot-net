using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.OrderDeliveryReports
{
    public class OrderReportStatusViewModel
    {
        public virtual DateTime Date {get; set;}
        public virtual string DayOfWeek { get; set; }
        public virtual IList<BuyerData> Buyers { get; set; }
        public virtual string UserNameLastUpdateOnReportNoBuyed { get; set; }
        public virtual string ManagerNameConfirmExpensesReport { get; set; }
        public virtual string LastUpdateSupermarketReportByUser { get; set; }
        public List<int> SubstitutedBuyerIds { get; set; }

    }

    public class BuyerData
    {
        public virtual int BuyerId { get; set; }
        public virtual string BuyerName { get; set; }
        public virtual int Status { get; set; }
        public virtual bool HasReport { get; set; }
        public virtual string NameConfirmReport { get; set; }

    }
}