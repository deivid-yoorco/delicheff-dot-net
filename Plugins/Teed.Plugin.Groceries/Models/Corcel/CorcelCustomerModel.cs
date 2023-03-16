using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.Corcel;

namespace Teed.Plugin.Groceries.Models.Corcel
{
    public class CorcelCustomerModel
    {
        public CorcelCustomerModel()
        {
            CorcelRulesThatApply = new List<CorcelRuleType>();
        }

        public DateTime? CorcelifiedDate { get; set; }
        public List<CorcelRuleType> CorcelRulesThatApply { get; set; }
    }
}
