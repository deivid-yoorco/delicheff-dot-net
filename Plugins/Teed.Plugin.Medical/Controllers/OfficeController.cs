using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Branches;
using Teed.Plugin.Medical.Models;
using Teed.Plugin.Medical.Models.Office;
using Teed.Plugin.Medical.Security;
using Teed.Plugin.Medical.Services;

namespace Teed.Plugin.Medical.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class OfficeController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly OfficeService _officeService;
        private readonly BranchService _branchService;

        public OfficeController(IPermissionService permissionService,
            OfficeService officeService,
            BranchService branchService)
        {
            _permissionService = permissionService;
            _officeService = officeService;
            _branchService = branchService;
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedBranches))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Medical/Views/Office/List.cshtml");
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedBranches))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Medical/Views/Office/Create.cshtml");
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedBranches))
                return AccessDeniedView();

            Office office = _officeService.GetById(id);
            if (office == null) return NotFound();

            OfficeModel model = new OfficeModel()
            {
                BranchId = office.BranchId,
                Name = office.Name,
                Id = office.Id
            };

            return View("~/Plugins/Teed.Plugin.Medical/Views/Office/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult ListData()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedBranches))
                return AccessDeniedView();

            var offices = _officeService.ListAsNoTracking().Select(x => new Models.Office.ListViewModel()
            {
                Id = x.Id,
                Name = x.Name,
                BranchName = _branchService.GetById(x.BranchId).Name
            });
            var gridModel = new DataSourceResult
            {
                Data = offices,
                Total = offices.Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult BranchListData()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedBranches))
                return AccessDeniedView();

            var branches = _branchService.GetAll().AsEnumerable();
            var elements = branches.Select(x => new BranchesListModel
            {
                Id = x.Id,
                Branch = $"{x.Name}"
            }).ToList();

            return Json(elements);
        }

        [HttpGet]
        public IActionResult OfficeListData(int branchId)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedBranches))
                return AccessDeniedView();

            var offices = _officeService.GetAll().Where(x => x.BranchId == branchId).AsEnumerable();
            var elements = offices.Select(x => new OfficeListModel
            {
                Id = x.Id,
                Office = $"{x.Name}"
            }).ToList();

            return Json(elements);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(OfficeModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedBranches))
                return AccessDeniedView();

            if (model.BranchId == 0)
            {
                ModelState.AddModelError("", "Debes seleccionar la sucursal a la que pertenece.");
                return View("~/Plugins/Teed.Plugin.Medical/Views/Office/Create.cshtml", model);
            }

            Office office = new Office()
            {
                BranchId = model.BranchId,
                Name = model.Name
            };
            _officeService.Insert(office);

            return RedirectToAction("List");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(OfficeModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedBranches))
                return AccessDeniedView();

            if (model.BranchId == 0)
            {
                ModelState.AddModelError("", "Debes seleccionar la sucursal a la que pertenece.");
                return View("~/Plugins/Teed.Plugin.Medical/Views/Office/Edit.cshtml", model);
            }

            Office office = _officeService.GetById(model.Id);
            if (office == null) return NotFound();

            office.Name = model.Name;
            office.BranchId = model.BranchId;
            _officeService.Update(office);

            return RedirectToAction("List");
        }
    }
}
