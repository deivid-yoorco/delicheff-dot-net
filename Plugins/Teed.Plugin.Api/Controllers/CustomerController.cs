using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Web.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.Customer;

namespace Teed.Plugin.Api.Controllers
{
    public class CustomerController : ApiBaseController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly IPictureService _pictureService;

        #endregion

        #region Ctor

        public CustomerController(ICustomerService customerService, ICustomerModelFactory customerModelFactory,
            IPictureService pictureService)
        {
            _customerService = customerService;
            _customerModelFactory = customerModelFactory;
            _pictureService = pictureService;
        }

        #endregion

        #region Methods

        [HttpGet]
        public IActionResult GetCustomerAddresses()
        {
            Customer customer = _customerService.GetCustomerById(int.Parse(UserId));
            if (customer == null) return NotFound();

            var model = _customerModelFactory.PrepareCustomerAddressListModel(customer);
            var dto = model.Addresses.Select(x => new UserAddressDto()
            {
                Id = x.Id,
                Address1 = x.Address1,
                Address2 = x.Address2,
                City = x.City,
                Country = x.CountryName,
                CountryId = x.CountryId,
                Email = x.Email,
                FirstName = x.FirstName,
                LastName = x.LastName,
                PhoneNumber = x.PhoneNumber,
                StateProvince = x.StateProvinceName,
                StateProvinceId = x.StateProvinceId,
                ZipPostalCode = x.ZipPostalCode,
                AddressAttributes = x.CustomAddressAttributes.Select(y => new Dtos.Address.AddressAttributeDto() 
                { 
                    Id = y.Id,
                    IsRequired = y.IsRequired,
                    Name = System.Net.WebUtility.HtmlDecode(x.FormattedCustomAddressAttributes)
                }).ToList()

            }).ToList();

            return Ok(dto);
        }

        #endregion
    }
}
