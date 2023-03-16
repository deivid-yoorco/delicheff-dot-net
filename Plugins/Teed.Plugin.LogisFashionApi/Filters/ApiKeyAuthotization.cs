using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Services.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Logisfashion.Filters
{
    public class ApiKeyAuthotization : TypeFilterAttribute
    {
        public ApiKeyAuthotization() : base(typeof(ApiKeyAuthFilter))
        {
        }

        private class ApiKeyAuthFilter : IActionFilter
        {
            public readonly ICustomerService _customerService;
            public readonly ICustomerRegistrationService _customerRegistrationService;

            public ApiKeyAuthFilter(ICustomerService customerService, ICustomerRegistrationService customerRegistrationService)
            {
                _customerService = customerService;
                _customerRegistrationService = customerRegistrationService;
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
                // Do nothing
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                string apiKey = context.HttpContext.Request.Headers["ApiKey"].ToString();
                string clientId = context.HttpContext.Request.Headers["ClientId"].ToString();
                
                try
                {
                    var customer = _customerService.GetCustomerByGuid(new Guid(clientId));
                    ((Controller)context.Controller).TempData["CustomerId"] = customer.Id;

                    if (customer != null)
                    {
                        var result = _customerRegistrationService.ValidateCustomer(customer.Email, apiKey);
                        if (result == Nop.Core.Domain.Customers.CustomerLoginResults.Successful)
                        {
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                }

                context.Result = new UnauthorizedResult();
            }
        }
    }
}
