using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.ShippingByAddress.Domain.ShippingByAddressD;
using Teed.Plugin.ShippingByAddress.Models;
using Teed.Plugin.ShippingByAddress.Services;

namespace Teed.Plugin.ShippingByAddress.Controllers
{
    [Area(AreaNames.Admin)]
    public class ShippingByAddressController : BasePluginController
    {
        private const string GOOGLE_MAPS_API_KEY = "AIzaSyAUvh0c7WBIqAeMnpGnx09MKsYQOlHkJJw";

        private readonly IPermissionService _permissionService;
        private readonly ShippingByAddressService _shippingByAddressService;
        private readonly ShippingBranchService _shippingBranchService;
        private readonly ShippingBranchOrderService _shippingBranchOrderService;
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;

        public ShippingByAddressController(IPermissionService permissionService,
            ShippingByAddressService shippingByAddressService,
            ShippingBranchService shippingBranchService,
            ShippingBranchOrderService shippingBranchOrderService,
            IOrderService orderService,
            IWorkContext workContext)
        {
            _permissionService = permissionService;
            _shippingByAddressService = shippingByAddressService;
            _shippingBranchService = shippingBranchService;
            _shippingBranchOrderService = shippingBranchOrderService;
            _orderService = orderService;
            _workContext = workContext;
        }

        public SelectList PrepareBranchesSelect()
        {
            var branchesDb = _shippingBranchService.GetAll().ToList();
            var branches = branchesDb.Select(x => new
            {
                Id = x.Id.ToString(),
                Name = x.BranchName
            }).ToList();
            branches.Insert(0, new
            {
                Id = 0.ToString(),
                Name = "Selecciona una sucursal..."
            });
            return new SelectList(branches, "Id", "Name");
        }

        [AuthorizeAdmin]
        public IActionResult Index()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.ShippingByAddress/Views/ShippingByAddress/Index.cshtml");
        }

        [AuthorizeAdmin]
        public IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = new CreateDatesViewModel();
            model.Branches = PrepareBranchesSelect();
            return View("~/Plugins/Teed.Plugin.ShippingByAddress/Views/ShippingByAddress/Create.cshtml", model);
        }

        [AuthorizeAdmin]
        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var ShippingDates = _shippingByAddressService.GetById(id).FirstOrDefault();
            EditDatesViewModel model = new EditDatesViewModel
            {
                DaysToShip = ShippingDates.DaysToShip,
                PostalCode = ShippingDates.PostalCode,
                ShippingBranchId = ShippingDates.ShippingBranchId,
                Id = ShippingDates.Id
            };

            model.Branches = PrepareBranchesSelect();
            return View("~/Plugins/Teed.Plugin.ShippingByAddress/Views/ShippingByAddress/Edit.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        public async Task<IActionResult> Create(CreateDatesViewModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!string.IsNullOrWhiteSpace(model.PostalCode) && !string.IsNullOrWhiteSpace(model.DaysToShip))
            {
                ShippingByAddressD shippingByAddress = new ShippingByAddressD
                {
                    DaysToShip = model.DaysToShip,
                    PostalCode = model.PostalCode.Replace(" ", "").Trim(),
                    ShippingBranchId = model.ShippingBranchId > 0 ? model.ShippingBranchId : null
                };

                bool result = await UpdateShippingByAddressData(shippingByAddress, model.PostalCode);
                if (result)
                {
                    _shippingByAddressService.Insert(shippingByAddress);
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", $"El código postal {model.PostalCode} no es válido.");
            }
            else
            {
                ModelState.AddModelError("", "El código postal y los días de entrega son requeridos.");
            }

            model.Branches = PrepareBranchesSelect();
            // If we get here something went wrong
            return View("~/Plugins/Teed.Plugin.ShippingByAddress/Views/ShippingByAddress/Create.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        public async Task<IActionResult> Edit(EditDatesViewModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            ShippingByAddressD shipping = _shippingByAddressService.GetById(model.Id).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(model.PostalCode) && !string.IsNullOrWhiteSpace(model.DaysToShip))
            {
                string currentPostalCode = shipping.PostalCode;
                shipping.DaysToShip = model.DaysToShip;
                shipping.PostalCode = model.PostalCode.Replace(" ", "").Trim();
                if (model.ShippingBranchId > 0)
                    shipping.ShippingBranchId = model.ShippingBranchId;
                else
                    shipping.ShippingBranchId = null;

                if (!string.Equals(currentPostalCode, model.PostalCode))
                {
                    bool result = await UpdateShippingByAddressData(shipping, model.PostalCode);
                    if (!result)
                    {
                        ModelState.AddModelError("", $"El código postal {model.PostalCode} no es válido.");
                    }
                }

                _shippingByAddressService.Update(shipping);
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "El código postal y los días de entrega son requeridos.");
            }

            model.Branches = PrepareBranchesSelect();
            // If we get here something went wrong
            return View("~/Plugins/Teed.Plugin.ShippingByAddress/Views/ShippingByAddress/Edit.cshtml", model);
        }

        [HttpGet]
        [AuthorizeAdmin]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            ShippingByAddressD shippingByAddress = _shippingByAddressService.GetById(id).FirstOrDefault();
            if (shippingByAddress == null) return NotFound();
            _shippingByAddressService.Delete(shippingByAddress);

            return RedirectToAction("Index");
        }

        [AuthorizeAdmin]
        public IActionResult ListData()
        {
            var postalCs = _shippingByAddressService.GetAll().OrderBy(x => x.PostalCode).ToList();
            var branches = _shippingBranchService.GetAll().ToList();
            foreach (var postalC in postalCs)
            {
                postalC.DaysToShip = GetDatesByIndex(postalC.DaysToShip);
            }
            var gridModel = new DataSourceResult
            {
                Data = postalCs.Select(x => new
                {
                    x.Id,
                    x.DaysToShip,
                    x.PostalCode,
                    UpdatedOnUtc = x.UpdatedOnUtc.ToString("dd/MM/yyyy"),
                    Branch = (branches.Where(y => y.Id == x.ShippingBranchId).FirstOrDefault() == null ?
                    "No asignado" : branches.Where(y => y.Id == x.ShippingBranchId).FirstOrDefault().BranchName)
                }),
                Total = postalCs.Count()
            };
            return Json(gridModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public string CheckDatesOfPc(string postalCode)
        {
            var dates = _shippingByAddressService.GetByPostalCode(postalCode).Select(x => x.DaysToShip).FirstOrDefault();
            return (dates);
        }

        public string GetDatesByIndex(string dates)
        {
            var final = "";
            foreach (var date in dates.Split(','))
            {
                switch (date)
                {
                    case "1":
                        final += "Lunes, ";
                        break;
                    case "2":
                        final += "Martes, ";
                        break;
                    case "3":
                        final += "Miércoles, ";
                        break;
                    case "4":
                        final += "Jueves, ";
                        break;
                    case "5":
                        final += "Viernes, ";
                        break;
                    case "6":
                        final += "Sábado, ";
                        break;
                    case "0":
                        final += "Domingo, ";
                        break;
                    default:
                        break;
                }
            }
            if (final.Length > 2)
                final = final.Substring(0, final.Length - 2);
            return (final);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("[controller]/[action]")]
        public IActionResult GetShippingAreas()
        {
            var shippingAreas = _shippingByAddressService.GetAll().Select(x => new
            {
                x.State,
                x.City,
                x.Suburbs,
                x.PostalCode
            }).ToList();

            return Json(shippingAreas);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("[controller]/[action]")]
        public string GetValidPostalCodes()
        {
            return string.Join(",", _shippingByAddressService.GetAll().Select(x => x.PostalCode));
        }

        /// <summary>
        /// Update ShippingByAdress object with address data. Returns if the request was successfull.
        /// </summary>
        /// <param name="shippingByAddress"></param>
        /// <param name="postalCode"></param>
        /// <returns></returns>
        private async Task<bool> UpdateShippingByAddressData(ShippingByAddressD shippingByAddress, string postalCode)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"https://maps.googleapis.com/maps/api/geocode/json?address={postalCode}&language=es&region=MX&components=country:MX&key={GOOGLE_MAPS_API_KEY}";
                var result = await client.GetAsync(url);
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    GoogleApiAddressComponents apiResult = Newtonsoft.Json.JsonConvert.DeserializeObject<GoogleApiResult>(json).results.FirstOrDefault();
                    if (apiResult != null)
                    {
                        shippingByAddress.City = apiResult.address_components.Where(x => x.types.Contains("administrative_area_level_1")).Select(x => x.long_name).FirstOrDefault();
                        shippingByAddress.State = apiResult.address_components.Where(x => x.types.Contains("locality")).Select(x => x.long_name).FirstOrDefault();
                        shippingByAddress.Suburbs = apiResult.address_components.Where(x => x.types.Contains("sublocality") || x.types.Contains("neighborhood")).Select(x => x.long_name).FirstOrDefault();
                        return true;
                    }
                }
            }
            return false;
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