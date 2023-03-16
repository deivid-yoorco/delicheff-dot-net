using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Dtos
{
    public class PostalCodeNotificationRequestDto
    {
        public string PostalCode { get; set; }
        public string Email { get; set; }
    }
}
