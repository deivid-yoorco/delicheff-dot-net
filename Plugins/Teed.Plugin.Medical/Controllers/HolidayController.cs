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
using Teed.Plugin.Medical.Helpers;
using Teed.Plugin.Medical.Models.Holiday;
using Teed.Plugin.Medical.Security;
using Teed.Plugin.Medical.Services;

namespace Teed.Plugin.Medical.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class HolidayController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly HolidayService _holidayService;
        private readonly BranchService _branchService;
        private readonly HolidayBranchService _holidayBranchService;

        public HolidayController(IPermissionService permissionService,
            HolidayService holidayService,
            BranchService branchService,
            HolidayBranchService holidayBranchService)
        {
            _permissionService = permissionService;
            _holidayService = holidayService;
            _branchService = branchService;
            _holidayBranchService = holidayBranchService;
        }

        public IActionResult List()
        {
            return View("~/Plugins/Teed.Plugin.Medical/Views/Holiday/List.cshtml");
        }

        public IActionResult Create()
        {
            ViewData["Branches"] = SelectListHelper.GetBranchesList(_branchService);
            return View("~/Plugins/Teed.Plugin.Medical/Views/Holiday/Create.cshtml");
        }

        public IActionResult Details(int id)
        {
            Domain.Branches.Holiday holiday = _holidayService.GetById(id);
            if (holiday == null) return NotFound();

            DetailsViewModel model = new DetailsViewModel()
            {
                Id = holiday.Id,
                HolidayDate = holiday.Date,
                Name = holiday.Name,
                Branches = _holidayBranchService.GetBranchesByHolidayId(holiday.Id).ToList()
            };

            return View("~/Plugins/Teed.Plugin.Medical/Views/Holiday/Details.cshtml", model);
        }

        public IActionResult Edit(int id)
        {
            Holiday holiday = _holidayService.GetById(id);
            if (holiday == null) return NotFound();

            HolidayModel model = new HolidayModel()
            {
                Id = holiday.Id,
                HolidayDate = holiday.Date,
                Name = holiday.Name,
                SelectedBranchesIds = _holidayBranchService.GetBranchesByHolidayId(holiday.Id).Select(x => x.Id).ToList()
            };

            ViewData["Branches"] = SelectListHelper.GetBranchesList(_branchService);
            return View("~/Plugins/Teed.Plugin.Medical/Views/Holiday/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult ListData()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedBranches))
                return AccessDeniedView();

            var holidays = _holidayService.ListAsNoTracking().Select(x => new ListViewModel()
            {
                Id = x.Id,
                Name = x.Name,
                Date = x.Date.ToString("dd MMMM")
            });
            var gridModel = new DataSourceResult
            {
                Data = holidays,
                Total = holidays.Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(HolidayModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedBranches))
                return AccessDeniedView();

            if (!ModelState.IsValid) return BadRequest();

            if (model.SelectedBranchesIds.Count <= 0)
            {
                ViewData["Branches"] = SelectListHelper.GetBranchesList(_branchService);

                ModelState.AddModelError("", "Debes seleccionar por lo menos una sucursal.");
                return View("~/Plugins/Teed.Plugin.Medical/Views/Holiday/Create.cshtml", model);
            };

            Holiday holiday = new Holiday()
            {
                Date = model.HolidayDate,
                Name = model.Name
            };
            _holidayService.Insert(holiday);

            foreach (var item in model.SelectedBranchesIds)
            {
                _holidayBranchService.Insert(new HolidayBranch()
                {
                    HolidayId = holiday.Id,
                    BranchId = item
                });
            }
            return RedirectToAction("List");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(HolidayModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedBranches))
                return AccessDeniedView();

            if (!ModelState.IsValid) return BadRequest();

            if (model.SelectedBranchesIds.Count <= 0)
            {
                ViewData["Branches"] = SelectListHelper.GetBranchesList(_branchService);

                ModelState.AddModelError("", "Debes seleccionar por lo menos una sucursal.");
                return View("~/Plugins/Teed.Plugin.Medical/Views/Holiday/Edit.cshtml", model);
            };

            Holiday holiday = _holidayService.GetById(model.Id);
            if (holiday == null) return NotFound();

            holiday.Name = model.Name;
            holiday.Date = model.HolidayDate;
            _holidayService.Update(holiday);

            var existingBranches = _holidayBranchService.GetByHolidayId(model.Id).ToList();
            foreach (var branch in existingBranches)
            {
                if (!model.SelectedBranchesIds.Contains(branch.Id))
                {
                    _holidayBranchService.Delete(branch);
                }
            }
            foreach (var branchId in model.SelectedBranchesIds)
            {
                if (_holidayBranchService.GetByHolidayAndBranchId(holiday.Id, branchId).FirstOrDefault() == null)
                {
                    _holidayBranchService.Insert(new HolidayBranch()
                    {
                        HolidayId = holiday.Id,
                        BranchId = branchId
                    });
                }
            }
            return RedirectToAction("List");
        }
    }
}
