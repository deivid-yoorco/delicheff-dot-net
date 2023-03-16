using Nop.Services.Orders;
using Nop.Services.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Teed.Plugin.MessageBird.ScheduleTasks
{
    public class AftermarketScheduleTask : IScheduleTask
    {
        private readonly IOrderService _orderService;

        public AftermarketScheduleTask(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public void Execute()
        {
            

            
        }
    }
}
