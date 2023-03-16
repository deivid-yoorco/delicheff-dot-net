using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Payments;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teed.Plugin.Groceries.Dtos;
using Teed.Plugin.TicketControl.Controllers;
using TicketService = Teed.Plugin.TicketControl.Services.TicketService;
using ScheduleService = Teed.Plugin.TicketControl.Services.ScheduleService;
using Nop.Core.Domain.Orders;

namespace Teed.Plugin.Groceries.Controllers
{
    public class AppController : ApiBaseController
    {
        private readonly IOrderService _orderService;
        private readonly IPictureService _pictureService;
        private readonly ICustomerService _customerService;
        private readonly TicketService _ticketService;
        private readonly ScheduleService _scheduleService;

        public AppController(IOrderService orderService,
            ICustomerService customerService,
            IPictureService pictureService,
            TicketService ticketService,
            ScheduleService scheduleService)
        {
            _orderService = orderService;
            _pictureService = pictureService;
            _ticketService = ticketService;
            _scheduleService = scheduleService;
            _customerService = customerService;
        }

        [HttpGet]
        public IActionResult GetTicketInfo(string ticketId)
        {
            int userId = int.Parse(UserId);
            if (!(UserIsInRole("Aplicación móvil") || UserIsInRole("Administrador"))) return BadRequest();
            var user = _customerService.GetCustomerById(userId);

            var ticket = _ticketService.GetByTicketId(new Guid(ticketId));
            if (ticket == null) return BadRequest();

            var order = _orderService.GetOrderById(ticket.OrderId);
            if (order == null) return BadRequest();

            var verificationUser = _customerService.GetCustomerById(ticket.VerificationUserId ?? 0);
            DateTime? verification = null;
            if (ticket.VerificationDateUtc != null)
                verification = (ticket.VerificationDateUtc ?? DateTime.Now).ToLocalTime();

            var time = order.SelectedShippingTime.Split(new string[] { " [" }, StringSplitOptions.None);
            var dto = new TicketDto
            {
                Schedule = time[0],
                OrderPaid = order.PaymentStatusId == (int)PaymentStatus.Paid,
                TicketId = ticket.TicketId.ToString(),
                VerificationDate = verification,
                VerificationUser = verificationUser == null ? string.Empty :
                $"{verificationUser.GetFullName()} ({verificationUser.Email})",
                CustomerName = order.Customer.GetFullName()
            };

            var items = order.OrderItems.Select(x => new ItemDto
            {
                Id = x.ProductId,
                Name = x.Product.Name,
                Price = x.PriceInclTax.ToString("C"),
                Quantity = $"{x.Quantity} {(x.Quantity > 1 ? "unidades" : "unidad")}",
                Img = x.Product.ProductPictures.Count == 0 ? "/images/default-image.png" :
                    _pictureService.GetPictureUrl(x.Product.ProductPictures.FirstOrDefault().Picture)
            }).ToList();

            if (ticket.VerificationDateUtc == null)
            {
                ticket.VerificationUserId = user.Id;
                ticket.VerificationDateUtc = DateTime.UtcNow;
                _ticketService.Update(ticket);

                order.OrderNotes.Add(new OrderNote()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    DisplayToCustomer = false,
                    Note = $"Boleto verificado por {user.GetFullName()} ({user.Email}, Ticket Id: {ticket.TicketId})",
                    OrderId = order.Id
                });
                order.OrderNotes.Add(new OrderNote()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    DisplayToCustomer = true,
                    Note = "Boleto verificado",
                    OrderId = order.Id
                });
                _orderService.UpdateOrder(order);
            }

            dto.Items = items;

            return Ok(dto);
        }

        protected virtual string ReturnHourString(int hour)
        {
            try
            {
                var date = new DateTime(2000, 1, 1, hour, 0, 0);
                return date.ToString("h:mm tt");
            }
            catch (Exception e)
            {
                return hour.ToString();
            }
        }
    }
}
