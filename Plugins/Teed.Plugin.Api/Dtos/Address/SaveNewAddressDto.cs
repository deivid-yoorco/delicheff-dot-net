using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Address
{
    public class SaveNewAddressDto
    {
        public string CustomerId { get; set; }
        public NewAddress Address { get; set; }
    }

    public class NewAddress
    {
        public int Id { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ZipPostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Country { get; set; }
        public string StateProvince { get; set; }
        public int CountryId { get; set; }
        public int StateProvinceId { get; set; }
    }
}
