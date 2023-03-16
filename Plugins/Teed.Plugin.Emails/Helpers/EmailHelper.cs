using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Emails.Helpers
{
    public static class EmailHelper
    {
        public static readonly string ABANDONED_SHOPPING_CART_TEMPLATE_NAME = "Abandoned.ShopingCart";
        public static readonly string ABANDONED_SHOPPING_CART_SCHEDULETASK_NAME = "Teed.Plugin.Emails.ScheduleTasks.AbandonShoppingCartEmailScheduleTask";

        public static readonly string REORDER_TEMPLATE_NAME = "Reorder.Email";
        public static readonly string REORDER_SCHEDULETASK_NAME = "Teed.Plugin.Emails.ScheduleTasks.ReorderEmailScheduleTask";
    }
}
