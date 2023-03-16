using Nop.Services.Customers;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Services;

namespace Teed.Plugin.Groceries.ScheduleTasks
{
    public class BuyerScheduleTask : IScheduleTask
    {
        private readonly ManufacturerBuyerService _manufacturerBuyerService;
        private readonly ICustomerService _customerService;

        public BuyerScheduleTask(ManufacturerBuyerService manufacturerBuyerService,
            ICustomerService customerService)
        {
            _manufacturerBuyerService = manufacturerBuyerService;
            _customerService = customerService;
        }

        public void Execute()
        {
            var role = _customerService.GetCustomerRoleBySystemName("buyer");
            var buyerIds = _customerService.GetAllCustomers(customerRoleIds: new int[] { role.Id }).Select(x => x.Id).ToList();
            var manufacturerBuyerWithNotValidUsers = _manufacturerBuyerService.GetAll().Where(x => !buyerIds.Contains(x.BuyerId)).ToList();
            foreach (var manufacturerBuyerWithNotValidUser in manufacturerBuyerWithNotValidUsers)
            {
                _manufacturerBuyerService.Delete(manufacturerBuyerWithNotValidUser);
            }
        }
    }
}
