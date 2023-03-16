using Nop.Services.Tasks;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;
using Teed.Plugin.Groceries.Services;

namespace Teed.Plugin.Groceries.ScheduleTasks
{
    public class ShippingRouteScheduleTask : IScheduleTask
    {
        private readonly ShippingRouteUserService _shippingRouteUserService;
        private readonly ShippingRouteService _shippingRouteService;

        public ShippingRouteScheduleTask(ShippingRouteUserService shippingRouteUserService,
            ShippingRouteService shippingRouteService)
        {
            _shippingRouteUserService = shippingRouteUserService;
            _shippingRouteService = shippingRouteService;
        }

        public void Execute()
        {
            var routes = _shippingRouteService.GetAll().ToList();
            foreach (var route in routes)
            {
                var lastShippingUserRegister = _shippingRouteUserService.GetAll()
                    .Where(x => x.ShippingRouteId == route.Id)
                    .OrderByDescending(x => x.ResponsableDateUtc)
                    .FirstOrDefault();

                if (lastShippingUserRegister != null)
                {
                    ShippingRouteUser shippingRouteUser = new ShippingRouteUser()
                    {
                        ResponsableDateUtc = DateTime.UtcNow,
                        ShippingRouteId = route.Id,
                        UserInChargeId = lastShippingUserRegister.UserInChargeId,
                        Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + $" - Se asignó automáticamente al usuario a la ruta {route.RouteName} ({route.Id})"
                    };
                    _shippingRouteUserService.Insert(shippingRouteUser);
                }
            }
        }
    }
}
