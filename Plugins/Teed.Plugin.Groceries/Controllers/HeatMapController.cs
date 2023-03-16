using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Models.HeatMap;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    public class HeatMapController : BasePluginController
    {
        public const string GOOGLE_API_KEY = "AIzaSyAUvh0c7WBIqAeMnpGnx09MKsYQOlHkJJw";

        private readonly HeatMapDataService _heatMapDataService;
        private readonly IPermissionService _permissionService;

        public HeatMapController(HeatMapDataService heatMapDataService,
            IPermissionService permissionService)
        {
            _heatMapDataService = heatMapDataService;
            _permissionService = permissionService;
        }

        public IActionResult Index()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.HeatMap))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/HeatMap/Index.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile file)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.HeatMap))
                return Unauthorized();

            var model = new HeatMapModel();
            model.Data = new List<HeatMapData>();
            if (file != null && file.Length > 0)
            {
                using (var xlPackage = new ExcelPackage(file.OpenReadStream()))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                        throw new Exception("No worksheet found");

                    var postalCodes = new List<HeatMapXlsData>();
                    var row = 1;
                    while (true)
                    {
                        try
                        {
                            var code = worksheet.Cells[row, 1];
                            var value = worksheet.Cells[row, 2];
                            if (string.IsNullOrEmpty(code?.Value?.ToString())
                                || string.IsNullOrEmpty(value?.Value?.ToString()))
                                break;
                            row += 1;

                            if (!isOnlyDigits(code.Value.ToString())
                                || !isOnlyDigits(value.Value.ToString())) continue;
                            postalCodes.Add(new HeatMapXlsData
                            {
                                PostalCode = string.Format("{0:00000}", code.Value),
                                Value = string.IsNullOrEmpty(value.Value.ToString()) ? 0 : int.Parse(value.Value.ToString())
                            });
                        }
                        catch
                        {
                            break;
                        }
                    }

                    var groupedCodes = postalCodes.GroupBy(x => x.PostalCode);
                    var singleCodes = groupedCodes.Select(x => x.Key).ToList();
                    var heatMapData = _heatMapDataService.GetAll()
                        .Where(x => singleCodes.Contains(x.PostalCode))
                        .ToList();

                    foreach (var group in groupedCodes)
                    {
                        if (string.IsNullOrWhiteSpace(group.Key)) continue;
                        var dbData = heatMapData.Where(x => x.PostalCode == group.Key).FirstOrDefault();

                        if (dbData == null)
                        {
                            string latLongUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={group.Key}&components=country:MX&key={GOOGLE_API_KEY}";
                            using (HttpClient client = new HttpClient())
                            {
                                var result = await client.GetAsync(latLongUrl);
                                if (result.IsSuccessStatusCode)
                                {
                                    var json = await result.Content.ReadAsStringAsync();
                                    var objectResult = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
                                    try
                                    {
                                        string lat = objectResult.results[0].geometry.location.lat;
                                        string lng = objectResult.results[0].geometry.location.lng;

                                        if (!string.IsNullOrEmpty(lat) && !string.IsNullOrEmpty(lng))
                                        {
                                            model.Data.Add(new HeatMapData()
                                            {
                                                Latitude = lat,
                                                Longitude = lng,
                                                Weight = group.Select(x => x.Value).DefaultIfEmpty().Sum(),
                                                PostalCode = group.Key
                                            });

                                            _heatMapDataService.Insert(new Domain.HeatMaps.HeatMapData()
                                            {
                                                Latitude = lat,
                                                Longitude = lng,
                                                PostalCode = group.Key
                                            });
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Debugger.Break();
                                    }
                                }
                            }
                        }
                        else
                        {
                            model.Data.Add(new HeatMapData()
                            {
                                Latitude = dbData.Latitude,
                                Longitude = dbData.Longitude,
                                Weight = group.Select(x => x.Value).DefaultIfEmpty().Sum(),
                                PostalCode = dbData.PostalCode
                            });
                        }
                    }
                }
            }

            return View("~/Plugins/Teed.Plugin.Groceries/Views/HeatMap/Index.cshtml", model);
        }

        private bool isOnlyDigits(string str)
        {
            foreach (char c in str.Trim())
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }
    }

    public class HeatMapXlsData
    {
        public virtual string PostalCode { get; set; }
        public virtual int Value { get; set; }
    }
}
