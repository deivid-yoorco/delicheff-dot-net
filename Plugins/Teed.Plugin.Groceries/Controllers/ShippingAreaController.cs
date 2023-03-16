using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.ExportImport.Help;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingAreas;
using Teed.Plugin.Groceries.Models.ShippingArea;
using Teed.Plugin.Groceries.Security;
using Teed.Plugin.Groceries.Services;

namespace Teed.Plugin.Groceries.Controllers
{
    [Area(AreaNames.Admin)]
    public class ShippingAreaController : BasePluginController
    {
        private const string GOOGLE_MAPS_API_KEY = "AIzaSyAUvh0c7WBIqAeMnpGnx09MKsYQOlHkJJw";
        private readonly ShippingAreaService _shippingAreaService;
        private readonly PostalCodesService _postalCodesService;
        private readonly ShippingRouteService _shippingRouteService;
        private readonly PostalCodeSearchService _postalCodeSearchService;
        private readonly IPermissionService _permissionService;

        public ShippingAreaController(ShippingAreaService shippingAreaService, PostalCodesService postalCodesService,
            ShippingRouteService shippingRouteService, PostalCodeSearchService postalCodeSearchService,
            IPermissionService permissionService)
        {
            _shippingAreaService = shippingAreaService;
            _postalCodesService = postalCodesService;
            _shippingRouteService = shippingRouteService;
            _postalCodeSearchService = postalCodeSearchService;
            _permissionService = permissionService;
        }

        [AuthorizeAdmin]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingArea))
                return AccessDeniedView();

            var postalCodes = string.Join(",", _shippingAreaService.GetAll().Select(x => x.PostalCode));
            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingArea/Configure.cshtml", new ConfigureModel() { PostalCodes = postalCodes });
        }

        [AuthorizeAdmin]
        public IActionResult InvalidPostalCodes()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingAreaPostalCodes))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingArea/InvalidPostalCodes.cshtml");
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult GetInvalidPostalCodes(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingAreaPostalCodes))
                return AccessDeniedView();

            var query = _postalCodeSearchService.GetAll();
            var queryList = query.OrderByDescending(m => m.CreatedOnUtc).ToList();
            var pagedList = new PagedList<PostalCodeSearch>(queryList, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    CreateDate = x.CreatedOnUtc.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt"),
                    x.PostalCode
                }).ToList(),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        [Route("[controller]/[action]")]
        [AllowAnonymous]
        public IActionResult RegisterInvalidPostalCode(string postalCode)
        {
            PostalCodeSearch postalCodeSearch = new PostalCodeSearch()
            {
                PostalCode = postalCode
            };
            _postalCodeSearchService.Insert(postalCodeSearch);

            return NoContent();
        }

        [AuthorizeAdmin]
        public IActionResult ExportExcelSearchPostalCode()
        {
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingAreaPostalCodes))
                return AccessDeniedView();

            List<PostalCodeSearch> postalCodeSearch = _postalCodeSearchService.GetAll().ToList();
            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("Códigos postales");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Fecha";
                    worksheet.Cells[row, 2].Value = "Código postal";
                    foreach (var item in postalCodeSearch)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = item.CreatedOnUtc.ToLocalTime();
                        worksheet.Cells[row, 2].Value = item.PostalCode;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"search_postal_codes_{DateTime.Now.ToString("ddMMyyyyhhmmss")}.xlsx");
            }
        }

        [HttpPost]
        [AuthorizeAdmin]
        public async Task<IActionResult> Configure(ConfigureModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (!_permissionService.Authorize(TeedGroceriesPermissionProvider.ShippingArea))
                return AccessDeniedView();

            string[] postalCodesArray = model.PostalCodes.Replace(" ", "").Split(',');
            string[] dbPostalCodes = _shippingAreaService.GetAll().Select(y => y.PostalCode).ToArray();
            List<string> newPostalCodes = postalCodesArray.Where(x => !dbPostalCodes.Contains(x)).ToList();
            List<string> postalCodesToDelete = dbPostalCodes.Where(x => !postalCodesArray.Contains(x)).ToList();
            List<string> invalidPostalCode = new List<string>();
            List<ShippingArea> shippingAreasToUpload = new List<ShippingArea>();

            foreach (var postalCode in newPostalCodes)
            {
                var postalCodeData = _postalCodesService.GetAll().Where(x => x.Cp == postalCode);
                if (postalCodeData.FirstOrDefault() == null)
                {
                    invalidPostalCode.Add(postalCode);
                    continue;
                }

                var shippingArea = new ShippingArea()
                {
                    City = postalCodeData.FirstOrDefault()?.Municipio,
                    PostalCode = postalCode,
                    State = postalCodeData.FirstOrDefault()?.Edo,
                    Suburbs = string.Join(", ", postalCodeData.Select(x => x.Colonia))
                };
                shippingAreasToUpload.Add(shippingArea);
            }

            using (HttpClient client = new HttpClient())
            {
                foreach (var postalCode in invalidPostalCode)
                {
                    string url = $"https://maps.googleapis.com/maps/api/geocode/json?address={postalCode}&language=es&region=MX&components=country:MX&key={GOOGLE_MAPS_API_KEY}";
                    var result = await client.GetAsync(url);
                    if (result.IsSuccessStatusCode)
                    {
                        string json = await result.Content.ReadAsStringAsync();
                        GoogleApiAddressComponents apiResult = Newtonsoft.Json.JsonConvert.DeserializeObject<GoogleApiResult>(json).results.FirstOrDefault();
                        if (apiResult != null)
                        {
                            var shippingArea = new ShippingArea()
                            {
                                City = apiResult.address_components.Where(x => x.types.Contains("administrative_area_level_1")).Select(x => x.long_name).FirstOrDefault(),
                                PostalCode = postalCode,
                                State = apiResult.address_components.Where(x => x.types.Contains("locality")).Select(x => x.long_name).FirstOrDefault(),
                                Suburbs = apiResult.address_components.Where(x => x.types.Contains("sublocality") || x.types.Contains("neighborhood")).Select(x => x.long_name).FirstOrDefault()
                            };

                            if (string.IsNullOrWhiteSpace(shippingArea.PostalCode))
                            {
                                ModelState.AddModelError("", $"El código postal {postalCode} no es válido.");
                                return View("~/Plugins/Teed.Plugin.Groceries/Views/ShippingArea/Configure.cshtml", model);
                            }
                            shippingAreasToUpload.Add(shippingArea);
                        }
                    }
                }
            }

            foreach (var shippingArea in shippingAreasToUpload)
            {
                //var route = _shippingRouteService.GetAll().Where(x => x.PostalCodes.Contains(shippingArea.PostalCode)).FirstOrDefault();
                //if (route == null)
                //{
                //    route = _shippingRouteService.GetAll().OrderByDescending(x => x.CreatedOnUtc).FirstOrDefault();
                //    route.PostalCodes += ("," + shippingArea.PostalCode);
                //    _shippingRouteService.Update(route);
                //}
                _shippingAreaService.Insert(shippingArea);
            }

            foreach (var postalCode in postalCodesToDelete)
            {
                var route = _shippingRouteService.GetAll().Where(x => x.PostalCodes.Contains(postalCode)).FirstOrDefault();
                if (route != null)
                {
                    route.PostalCodes = route.PostalCodes.Replace("," + postalCode, "").Replace(postalCode + ",", "").Replace(postalCode, "");
                    _shippingRouteService.Update(route);
                }

                var shippingArea = _shippingAreaService.GetAll().Where(x => x.PostalCode == postalCode).FirstOrDefault();
                if (shippingArea != null) _shippingAreaService.Delete(shippingArea);
            }

            return RedirectToAction("Configure");
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("[controller]/[action]")]
        public string GetValidPostalCodes()
        {
            return string.Join(",", _shippingAreaService.GetAll().Select(x => x.PostalCode));
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("[controller]/[action]")]
        public IActionResult GetShippingAreas()
        {
            var shippingAreas = _shippingAreaService.GetAll().Select(x => new
            {
                x.State,
                x.City,
                x.Suburbs,
                x.PostalCode
            }).ToList();

            return Json(shippingAreas);
        }
    }

    class GoogleApiResult
    {
        public List<GoogleApiAddressComponents> results { get; set; }
    }

    class GoogleApiAddressComponents
    {
        public List<GoogleApiAddressComponent> address_components { get; set; }
    }

    class GoogleApiAddressComponent
    {
        public string long_name { get; set; }
        public string short_name { get; set; }
        public string[] types { get; set; }
    }
}
