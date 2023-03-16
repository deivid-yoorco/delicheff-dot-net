using System;
using System.Collections.Generic;
using System.Linq;
using Teed.Plugin.Manager.Domain.ExpensesCategories;
using Teed.Plugin.Manager.Services;

namespace Teed.Plugin.Manager.Helpers
{
    public static class ExpenseCategoryHelper
    {
        public static string GetFormattedBreadCrumb(ExpensesCategories expenseCategory, ExpensesCategoriesService expenseCategoryService, string separator = ">>")
        {
            var result = string.Empty;

            var breadcrumb = GetExpenseCategoryBreadCrumb(expenseCategory, expenseCategoryService);
            for (var i = 0; i <= breadcrumb.Count - 1; i++)
            {
                var categoryName = breadcrumb[i].Value;
                result = string.IsNullOrEmpty(result)
                    ? categoryName
                    : $"{result} {separator} {categoryName}";
            }

            return result;
        }

        private static IList<ExpensesCategories> GetExpenseCategoryBreadCrumb(ExpensesCategories expenseCategory, ExpensesCategoriesService expenseCategoryService)
        {
            if (expenseCategory == null)
                throw new ArgumentNullException(nameof(expenseCategory));

            var result = new List<ExpensesCategories>();

            //used to prevent circular references
            var alreadyProcessedExpenseCategoryIds = new List<int>();

            while (expenseCategory != null && //not null
                !expenseCategory.Deleted && //not deleted
                !alreadyProcessedExpenseCategoryIds.Contains(expenseCategory.Id)) //prevent circular references
            {
                result.Add(expenseCategory);
                alreadyProcessedExpenseCategoryIds.Add(expenseCategory.Id);
                expenseCategory = expenseCategoryService.GetAll().Where(x => x.ExpenseCategoryId == expenseCategory.ParentId).FirstOrDefault();
            }
            result.Reverse();
            return result;
        }
    }
}
