using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Directory;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Suppliers.Domain;
using Teed.Plugin.Suppliers.Models;
using Teed.Plugin.Suppliers.Services;

namespace Teed.Plugin.Suppliers.Controllers
{
    [Area(AreaNames.Admin)]
    public class SupplierController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly SupplierService _supplierService;

        public SupplierController(IPermissionService permissionService, SupplierService supplierService,
            IStateProvinceService stateProvinceService, ICountryService countryService)
        {
            _permissionService = permissionService;
            _supplierService = supplierService;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
        }

        public IActionResult Index()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Supplier/Views/Supplier/Index.cshtml");
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            SupplierModel model = new SupplierModel();
            PrepareModel(model);

            return View("~/Plugins/Teed.Plugin.Supplier/Views/Supplier/Create.cshtml", model);
        }

        [HttpPost]
        public IActionResult Create(SupplierModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var country = _countryService.GetCountryByTwoLetterIsoCode("MX");
            Supplier supplier = new Supplier()
            {
                Address = model.Address,
                City = model.City,
                Comment = model.Comment,
                CountryId = country.Id,
                Email = model.Email,
                Name = model.Name,
                Phone1 = model.Phone1,
                Phone2 = model.Phone2,
                PostalCode = model.PostalCode,
                StateProvinceId = model.StateProvinceId,
                Contact = model.Contact
            };
            _supplierService.Insert(supplier);

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            Supplier supplier = _supplierService.GetById(id);
            if (supplier == null) return NotFound();

            SupplierModel model = new SupplierModel()
            {
                Address = supplier.Address,
                City = supplier.City,
                Comment = supplier.Comment,
                CountryId = supplier.CountryId,
                Email = supplier.Email,
                Name = supplier.Name,
                Phone1 = supplier.Phone1,
                Phone2 = supplier.Phone2,
                PostalCode = supplier.PostalCode,
                StateProvinceId = supplier.StateProvinceId,
                Id = supplier.Id,
                Contact = supplier.Contact
            };

            PrepareModel(model);

            return View("~/Plugins/Teed.Plugin.Supplier/Views/Supplier/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult Edit(SupplierModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            Supplier supplier = _supplierService.GetById(model.Id);
            if (supplier == null) return NotFound();

            supplier.Address = model.Address;
            supplier.City = model.City;
            supplier.Comment = model.Comment;
            supplier.Email = model.Email;
            supplier.Name = model.Name;
            supplier.Phone1 = model.Phone1;
            supplier.Phone2 = model.Phone2;
            supplier.PostalCode = model.PostalCode;
            supplier.StateProvinceId = model.StateProvinceId;
            supplier.Contact = model.Contact;

            _supplierService.Update(supplier);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            Supplier supplier = _supplierService.GetById(id);
            if (supplier == null) return NotFound();

            _supplierService.Delete(supplier);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var query = _supplierService.GetAll();
            var pagedList = new PagedList<Supplier>(query.OrderByDescending(m => m.CreatedOnUtc), command.Page - 1, command.PageSize);

            DataSourceResult gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    x.Name
                }),
                Total = query.Count()
            };

            return Json(gridModel);
        }

        private void PrepareModel(SupplierModel model)
        {
            var country = _countryService.GetCountryByTwoLetterIsoCode("MX");
            var states = _stateProvinceService.GetStateProvincesByCountryId(country.Id);
            model.States = new SelectList(states, "Id", "Name");
        }
    }
}
