using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Manager.Models.ExpensesCategories
{
    public class ExpensesCategoriesModel
    {
        public List<ExpenseCategory> ExpensesCategories { get; set; }
    }

    public class ExpenseCategory
    {
        public int CategoryId { get; set; }
        public int ParentCategoryId { get; set; }
        public string Value { get; set; }
        public string ValueTitle { get; set; }
    }
}
