using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.Address;
using Teed.Plugin.Api.Dtos.Customer;

namespace Teed.Plugin.Api.Controllers
{
    public class AddressController : ApiBaseController
    {
        #region Fields

        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICustomerService _customerService;
        private readonly IAddressService _addressService;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressAttributeParser _addressAttributeParser;

        #endregion

        #region Ctor

        public AddressController(ICountryService countryService,
            IStateProvinceService stateProvinceService,
            ICustomerService customerService,
            IAddressService addressService,
            IAddressAttributeService addressAttributeService,
            IAddressAttributeParser addressAttributeParser)
        {
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _customerService = customerService;
            _addressService = addressService;
            _addressAttributeService = addressAttributeService;
            _addressAttributeParser = addressAttributeParser;
        }

        #endregion

        #region Methods

        [HttpGet]
        public IActionResult GetCountries()
        {
            var dto = _countryService.GetAllCountries().Select(x => new GetCountriesDto()
            {
                CountryId = x.Id,
                CountryName = x.Name
            }).ToList();

            return Ok(dto);
        }

        [HttpGet]
        public IActionResult GetStates(int countryId)
        {
            var dto = _stateProvinceService.GetStateProvincesByCountryId(countryId).Select(x => new GetStatesDto()
            {
                StateId = x.Id,
                StateName = x.Name
            }).ToList();

            return Ok(dto);
        }

        [HttpGet]
        public IActionResult GetAddressAttributes()
        {
            var attributes = _addressAttributeService.GetAllAddressAttributes()
                .Select(x => new AddressAttributeDto()
                {
                    Name = x.Name,
                    IsRequired = x.IsRequired,
                    Id = x.Id
                }).ToList();

            return Ok(attributes);
        }

        [HttpGet]
        public virtual IActionResult UserAddressCount()
        {
            Customer customer = _customerService.GetCustomerById(int.Parse(UserId));
            return Ok(customer.Addresses.Count);
        }

        [HttpDelete]
        public virtual IActionResult DeleteAddress(int addressId)
        {
            Customer customer = _customerService.GetCustomerById(int.Parse(UserId));
            var address = customer.Addresses.FirstOrDefault(a => a.Id == addressId);
            if (address != null)
            {
                customer.RemoveAddress(address);
                _customerService.UpdateCustomer(customer);
                _addressService.DeleteAddress(address);
            }

            return NoContent();
        }

        [HttpPost]
        public IActionResult SaveNewAddress([FromBody] UserAddressDto dto)
        {
            Customer customer = _customerService.GetCustomerById(int.Parse(UserId));
            if (customer == null) return NotFound();

            if (dto.CountryId == null || dto.StateProvinceId == null)
            {
                dto.CountryId = _countryService.GetCountryByTwoLetterIsoCode("MX").Id;
                if (dto.Address1.Contains("EDMX") || dto.Address1.Contains("State of Mexico") || dto.Address1.Contains("Estado de México"))
                    dto.StateProvinceId = _stateProvinceService.GetStateProvinceByAbbreviation("MEX", dto.CountryId).Id;
                else
                    dto.StateProvinceId = _stateProvinceService.GetStateProvinceByAbbreviation("CMX", dto.CountryId).Id;
            }

            string customAttributes = string.Empty;
            if (dto.AddressAttributes != null && dto.AddressAttributes.Count > 0) 
            {
                customAttributes += "<Attributes>";
                foreach (var attribute in dto.AddressAttributes)
                {
                    if (!string.IsNullOrWhiteSpace(attribute.Value))
                    {
                        customAttributes += $"<AddressAttribute ID=\"{attribute.Id}\"><AddressAttributeValue><Value>{attribute.Value}</Value></AddressAttributeValue></AddressAttribute>";
                    }
                }
                customAttributes += "</Attributes>";
            }

            var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
            if (customAttributeWarnings.Count > 0) return BadRequest(string.Join(". ", customAttributeWarnings));

            Address address = new Address()
            {
                Address1 = dto.Address1,
                Address2 = dto.Address2,
                City = dto.City,
                CountryId = dto.CountryId,
                CreatedOnUtc = DateTime.UtcNow,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                StateProvinceId = dto.StateProvinceId,
                ZipPostalCode = dto.ZipPostalCode,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                CustomAttributes = customAttributes
            };
            customer.Addresses.Add(address);
            _customerService.UpdateCustomer(customer);

            dto.Id = address.Id;
            dto.StateProvince = _stateProvinceService.GetStateProvinceById(dto.StateProvinceId.Value).Name;
            dto.Country = _countryService.GetCountryById(dto.CountryId.Value).Name;

            foreach (var attribute in dto.AddressAttributes)
            {
                attribute.Name = attribute.Name + ": " + attribute.Value;
            }

            return Ok(dto);
        }

        //DEPRECATED
        //[HttpPost]
        //public IActionResult SaveNewAddress([FromBody] SaveNewAddressDto dto)
        //{
        //    Customer customer = _customerService.GetCustomerById(int.Parse(dto.CustomerId));
        //    if (customer == null) return NotFound();

        //    Address address = new Address()
        //    {
        //        Address1 = dto.Address.Address1,
        //        Address2 = dto.Address.Address2,
        //        City = dto.Address.City,
        //        CountryId = dto.Address.CountryId,
        //        CreatedOnUtc = DateTime.UtcNow,
        //        FirstName = dto.Address.FirstName,
        //        LastName = dto.Address.LastName,
        //        Email = dto.Address.Email,
        //        PhoneNumber = dto.Address.PhoneNumber,
        //        StateProvinceId = dto.Address.StateProvinceId,
        //        ZipPostalCode = dto.Address.ZipPostalCode
        //    };
        //    customer.Addresses.Add(address);
        //    _customerService.UpdateCustomer(customer);

        //    dto.Address.Id = address.Id;
        //    dto.Address.StateProvince = _stateProvinceService.GetStateProvinceById(dto.Address.StateProvinceId).Name;
        //    dto.Address.Country = _countryService.GetCountryById(dto.Address.CountryId).Name;

        //    return Ok(dto.Address);
        //}

        #endregion
    }
}
