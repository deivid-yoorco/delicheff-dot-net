using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Models.External;

namespace Teed.Plugin.Groceries.Helpers
{
    public static class ExternalHelper
    {
        public static List<PayrollEmployeeModel> GetExternalPayrollEmployeesByIds(List<int> employeeIds,
            IStoreContext _storeContext)
        {
            var employees = new List<PayrollEmployeeModel>();
            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(employeeIds), Encoding.UTF8, "application/json");
                var url =
                    _storeContext.CurrentStore.SecureUrl
                    ;
                var employeesResult = client.PostAsync(url + "/Admin/PayrollEmployee/GetEmployeesByIdsExternal", content).Result;
                if (employeesResult.IsSuccessStatusCode)
                {
                    var resultJson = employeesResult.Content.ReadAsStringAsync().Result;
                    employees = JsonConvert.DeserializeObject<List<PayrollEmployeeModel>>(resultJson);
                }
            }
            return employees;
        }
    }
}
