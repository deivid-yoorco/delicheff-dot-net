using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.Checkout;
using Teed.Plugin.Api.Services;

namespace Teed.Plugin.Api.Controllers
{
    public class WebController : BasePublicController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly CustomerSavedCardService _customerSavedCardService;

        #endregion

        #region Ctor

        public WebController(CustomerSavedCardService customerSavedCardService,
            IWorkContext workContext)
        {
            _customerSavedCardService = customerSavedCardService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public IActionResult GetCustomerVisaSavedCard()
        {
            var savedCard = _customerSavedCardService.GetAll()
                .Where(x => x.CustomerId == _workContext.CurrentCustomer.Id && x.PaymentMethodSystemName == "Payments.Visa")
                .FirstOrDefault();
            if (savedCard == null) return NoContent();

            var model = new SavedCardDataDto()
            {
                Brand = savedCard.Brand,
                CardOwnerName = savedCard.CardOwnerName,
                ServiceCustomerId = savedCard.ServiceCustomerId,
                LastFourDigits = savedCard.LastFourDigits,
                CardLogoUrl = "/Plugins/Teed.Plugin.Api/src/images/card/" + _customerSavedCardService.GetCardLogoName(savedCard.Brand) + ".jpg",
                BillAddress1 = savedCard.BillAddress1,
                BillAdministrativeArea = savedCard.BillAdministrativeArea,
                BillCountry = savedCard.BillCountry,
                BillEmail = savedCard.BillEmail,
                BillFirstName = savedCard.BillFirstName,
                BillLastName = savedCard.BillLastName,
                BillLocality = savedCard.BillLocality,
                BillPhoneNumber = savedCard.BillPhoneNumber,
                BillPostalCode = savedCard.BillPostalCode
            };

            return Ok(model);
        }

        #endregion

        #region Private methods
        #endregion
    }
}
