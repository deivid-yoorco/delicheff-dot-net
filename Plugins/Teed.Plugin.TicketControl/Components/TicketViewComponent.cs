using Microsoft.AspNetCore.Mvc;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.TicketControl.Services;

namespace Teed.Plugin.TicketControl.Components
{
    [ViewComponent(Name = "TicketView")]
    public class TicketViewComponent : NopViewComponent
    {
        private readonly IOrderService _orderService;
        private readonly IDownloadService _downloadService;
        private readonly TicketService _ticketService;

        public TicketViewComponent(IOrderService orderService,
            IDownloadService downloadService,
            TicketService ticketService)
        {
            _orderService = orderService;
            _downloadService = downloadService;
            _ticketService = ticketService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var qrBase64 = string.Empty;
            var orderId = (int)additionalData;
            var order = _orderService.GetOrderById(orderId);
            if (order != null)
            {
                var ticket = _ticketService.GetByOrderId(order.Id);
                if (ticket != null)
                {
                    var download = _downloadService.GetDownloadById(ticket.QrPictureId);
                    if (download != null)
                    {
                        qrBase64 = Convert.ToBase64String(download.DownloadBinary);
                    }
                }
            }
            return View("~/Plugins/Teed.Plugin.TicketControl/Views/TicketView.cshtml", qrBase64);
        }
    }
}
