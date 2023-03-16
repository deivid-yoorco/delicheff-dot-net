using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Controllers;

namespace Teed.Plugin.Payroll.Helpers
{
    public static class ExternalHelper
    {
        public static FranchiseAndRoutesExternal GetExternalFranchiseAndRoutesInfoByDates(IStoreContext _storeContext)
        {
            var franchiseAndRoutes = new FranchiseAndRoutesExternal();
            using (HttpClient client = new HttpClient())
            {
                var url =
                    //"https://localhost:44387/"
                    _storeContext.CurrentStore.SecureUrl
                    ;
                var employeesResult = client.GetAsync(url + "/Admin/ShippingRoute/GetFranchiseAndRouteExternal").Result;
                if (employeesResult.IsSuccessStatusCode)
                {
                    var resultJson = employeesResult.Content.ReadAsStringAsync().Result;
                    franchiseAndRoutes = JsonConvert.DeserializeObject<FranchiseAndRoutesExternal>(resultJson);
                }
            }
            return franchiseAndRoutes;
        }

        public static OrderInfo GetExternalFranchiseAndRouteOrderInfo(int id, IStoreContext _storeContext)
        {
            var orderInfo = new OrderInfo();
            using (HttpClient client = new HttpClient())
            {
                var url =
                    //"https://localhost:44387/"
                    _storeContext.CurrentStore.SecureUrl
                    ;
                var employeesResult = client.GetAsync(url + "/Admin/ShippingRoute/GetFranchiseAndRouteByOrderIdExternal/" + id).Result;
                if (employeesResult.IsSuccessStatusCode)
                {
                    var resultJson = employeesResult.Content.ReadAsStringAsync().Result;
                    orderInfo = JsonConvert.DeserializeObject<OrderInfo>(resultJson);
                }
            }
            return orderInfo;
        }
    }
}
