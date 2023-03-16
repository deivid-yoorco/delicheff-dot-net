using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Dtos
{
    public class TicketDto
    {
        public string TicketId { get; set; }
        public bool OrderPaid { get; set; }
        public string Schedule { get; set; }
        public string Time { get; set; }
        public DateTime? VerificationDate { get; set; }
        public string VerificationUser { get; set; }
        public string CustomerName { get; set; }

        public List<ItemDto> Items { get; set; }
    }

    public class ItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }
        public string Quantity { get; set; }
        public string Img { get; set; }
    }
}
