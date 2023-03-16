using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Controllers
{
    public class RewardsController : BasePublicController
    {
        [HttpsRequirement(SslRequirement.No)]
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
