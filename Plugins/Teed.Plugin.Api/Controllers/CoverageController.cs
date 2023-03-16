using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.Coverage;

namespace Teed.Plugin.Api.Controllers
{
    public class CoverageController : ApiBaseController
    {
        #region Properties
        #endregion

        #region Ctor

        public CoverageController()
        {

        }

        #endregion

        #region Methods

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCoverageData(string appUrl)
        {
            var url = appUrl + "/shippingarea/GetShippingAreas";

            List<CoverageDto> dto = new List<CoverageDto>();
            using (HttpClient client = new HttpClient())
            {
                var result = await client.GetAsync(url);
                if (result.IsSuccessStatusCode)
                {
                    var json = await result.Content.ReadAsStringAsync();
                    dto = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CoverageDto>>(json);
                }
            }

            return Ok(dto.OrderBy(x => x.PostalCode));
        }

        #endregion
    }
}
