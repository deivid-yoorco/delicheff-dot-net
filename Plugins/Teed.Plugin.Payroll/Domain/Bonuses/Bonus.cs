using Nop.Core;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;

namespace Teed.Plugin.Payroll.Domain.Bonuses
{
    public class Bonus : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual string Name { get; set; }
        public virtual string JobCatalogIdsFormat { get; set; }
        public virtual int ConditionTypeId { get; set; }
        public virtual int BonusTypeId { get; set; }
        public virtual int FrequencyTypeId { get; set; }
        public virtual int AuthorizationEmployeeId { get; set; }
        public virtual bool IsActive { get; set; }

        public virtual string Log { get; set; }

        public virtual ICollection<BonusAmount> Amounts { get; set; }
        public virtual ICollection<BonusApplication> Applications { get; set; }

        public virtual List<int> GetJobCatalogIds()
        {
            var Ids = new List<int>();
            try
            {
                if (!string.IsNullOrEmpty(JobCatalogIdsFormat))
                {
                    var splitting = JobCatalogIdsFormat.Split('|');
                    if (splitting.Any())
                        Ids = splitting.Select(x => int.Parse(x)).ToList();
                }
            }
            catch (Exception e)
            {
                var err = e;
            }
            return Ids;
        }

        public virtual BonusAmount GetAppliableAmount(DateTime? specifiedDate = null)
        {
            if (specifiedDate == null)
                return Amounts.Where(x => x.ApplyDate <= DateTime.Now.Date).OrderByDescending(x => x.ApplyDate).ThenByDescending(x => x.CreatedOnUtc).FirstOrDefault();
            else
                return Amounts.Where(x => x.ApplyDate <= specifiedDate.Value).OrderByDescending(x => x.ApplyDate).ThenByDescending(x => x.CreatedOnUtc).FirstOrDefault();
        }
    }
}
