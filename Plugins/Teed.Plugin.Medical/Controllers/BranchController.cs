using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
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
using Teed.Plugin.Medical.Models;
using Teed.Plugin.Medical.Models.Branch;
using Teed.Plugin.Medical.Security;
using Teed.Plugin.Medical.Services;

namespace Teed.Plugin.Medical.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class BranchController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly ICountryService _countryService;
        private readonly ILocalizationService _localizationService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICustomerService _customerService;
        private readonly BranchService _branchService;
        private readonly BranchWorkerService _branchWorkerService;

        public BranchController(IPermissionService permissionService,
            BranchService branchService,
            ICountryService countryService,
            ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            BranchWorkerService branchWorkerService,
            ICustomerService customerService)
        {
            _permissionService = permissionService;
            _branchService = branchService;
            _countryService = countryService;
            _localizationService = localizationService;
            _stateProvinceService = stateProvinceService;
            _branchWorkerService = branchWorkerService;
            _customerService = customerService;
        }

        public IActionResult List()
        {
            return View("~/Plugins/Teed.Plugin.Medical/Views/Branch/List.cshtml");
        }

        public IActionResult Create()
        {
            BranchModel model = new BranchModel();
            PrepareBranchModel(model);
            ViewData["AllCustomers"] = SelectListHelper.GetAllCustomers(_customerService);

            return View("~/Plugins/Teed.Plugin.Medical/Views/Branch/Create.cshtml", model);
        }

        public IActionResult Details(int id)
        {
            Branch branch = _branchService.GetById(id);
            if (branch == null) return NotFound();

            DetailsViewModel model = new DetailsViewModel()
            {
                City = branch.City,
                Country = _countryService.GetCountryById(branch.CountryId).Name,
                Id = branch.Id,
                Name = branch.Name,
                Phone = branch.Phone,
                Phone2 = branch.Phone2,
                StateProvince = _stateProvinceService.GetStateProvinceById(branch.StateProvinceId).Name,
                StreetAddress = branch.StreetAddress,
                StreetAddress2 = branch.StreetAddress2,
                Customer = _customerService.GetCustomerById(branch.UserId),
                ZipPostalCode = branch.ZipPostalCode,
                WeekOpenTime = branch.WeekOpenTime,
                WeekCloseTime = branch.WeekCloseTime,
                SaturdayOpenTime = branch.SaturdayOpenTime,
                SaturdayCloseTime = branch.SaturdayCloseTime,
                SundayOpenTime = branch.SundayOpenTime,
                SundayCloseTime = branch.SundayCloseTime,
                WorksOnSaturday = branch.WorksOnSaturday,
                WorksOnSunday = branch.WorksOnSunday
            };

            return View("~/Plugins/Teed.Plugin.Medical/Views/Branch/Details.cshtml", model);
        }

        public IActionResult Edit(int id)
        {
            Branch branch = _branchService.GetById(id);
            if (branch == null) return NotFound();

            BranchModel model = new BranchModel()
            {
                City = branch.City,
                CountryId = branch.CountryId,
                Id = branch.Id,
                Name = branch.Name,
                Phone = branch.Phone,
                Phone2 = branch.Phone2,
                StateProvinceId = branch.StateProvinceId,
                StreetAddress = branch.StreetAddress,
                StreetAddress2 = branch.StreetAddress2,
                UserId = branch.UserId,
                ZipPostalCode = branch.ZipPostalCode,
                WeekOpenHour = branch.WeekOpenTime.Hours,
                WeekCloseHour = branch.WeekCloseTime.Hours,
                WeekOpenMinute = branch.WeekOpenTime.Minutes,
                WeekCloseMinute = branch.WeekCloseTime.Minutes,
                WorksOnSaturday = branch.WorksOnSaturday,
                WorksOnSunday = branch.WorksOnSunday,
                SelectedUsersIds = _branchWorkerService.GetUsersIdsByBranchId(id).ToList()
            };

            if (branch.SaturdayOpenTime != null && branch.SaturdayCloseTime != null)
            {
                model.SaturdayOpenHour = branch.SaturdayOpenTime.Value.Hours;
                model.SaturdayOpenMinute = branch.SaturdayOpenTime.Value.Minutes;
                model.SaturdayCloseHour = branch.SaturdayCloseTime.Value.Hours;
                model.SaturdayCloseMinute = branch.SaturdayCloseTime.Value.Minutes;
            }

            if (branch.SundayOpenTime != null && branch.SundayCloseTime != null)
            {
                model.SundayOpenHour = branch.SundayOpenTime.Value.Hours;
                model.SundayOpenMinute = branch.SundayOpenTime.Value.Minutes;
                model.SundayCloseHour = branch.SundayCloseTime.Value.Hours;
                model.SundayCloseMinute = branch.SundayCloseTime.Value.Minutes;
            }

            PrepareBranchModel(model);
            ViewData["AllCustomers"] = SelectListHelper.GetAllCustomers(_customerService);
            return View("~/Plugins/Teed.Plugin.Medical/Views/Branch/Edit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BranchModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedBranches))
                return AccessDeniedView();

            if (!ModelState.IsValid) return BadRequest();

            if (!CreateScheduleIsValid(model))
            {
                ViewData["AllCustomers"] = SelectListHelper.GetAllCustomers(_customerService);
                ModelState.AddModelError("", "Debes llenar todos los campos habilitados en los horarios.");
                PrepareBranchModel(model);
                return View("~/Plugins/Teed.Plugin.Medical/Views/Branch/Create.cshtml", model);
            }

            if (model.UserId == 0)
            {
                ViewData["AllCustomers"] = SelectListHelper.GetAllCustomers(_customerService);
                ModelState.AddModelError("", "Debes seleccionar al encargado de esta sucursal.");
                PrepareBranchModel(model);
                return View("~/Plugins/Teed.Plugin.Medical/Views/Branch/Create.cshtml", model);
            }

            Branch branch = new Branch()
            {
                City = model.City,
                UserId = model.UserId,
                StreetAddress = model.StreetAddress,
                StreetAddress2 = model.StreetAddress2,
                CountryId = model.CountryId,
                Name = model.Name,
                Phone = model.Phone,
                Phone2 = model.Phone2,
                StateProvinceId = model.StateProvinceId,
                WorksOnSaturday = model.WorksOnSaturday,
                WorksOnSunday = model.WorksOnSunday,
                ZipPostalCode = model.ZipPostalCode,
                WeekCloseTime = new TimeSpan((int)model.WeekCloseHour, (int)model.WeekCloseMinute, 0),
                WeekOpenTime = new TimeSpan((int)model.WeekOpenHour, (int)model.WeekOpenMinute, 0)
            };

            if (model.WorksOnSaturday) branch.SaturdayOpenTime = new TimeSpan((int)model.SaturdayOpenHour, (int)model.SaturdayOpenMinute, 0);
            else branch.SaturdayOpenTime = null;

            if (model.WorksOnSaturday) branch.SaturdayCloseTime = new TimeSpan((int)model.SaturdayCloseHour, (int)model.SaturdayCloseMinute, 0);
            else branch.SaturdayCloseTime = null;

            if (model.WorksOnSunday) branch.SundayOpenTime = new TimeSpan((int)model.SundayOpenHour, (int)model.SundayOpenMinute, 0);
            else branch.SundayOpenTime = null;

            if (model.WorksOnSunday) branch.SundayCloseTime = new TimeSpan((int)model.SundayCloseHour, (int)model.SundayCloseMinute, 0);
            else branch.SundayCloseTime = null;

            _branchService.Insert(branch);

            if (model.SelectedUsersIds != null)
            {
                foreach (var item in model.SelectedUsersIds)
                {
                    _branchWorkerService.Insert(new BranchWorker()
                    {
                        CustomerId = item,
                        BranchId = branch.Id
                    });
                }
            }

            return RedirectToAction("List");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BranchModel model)
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedBranches))
                return AccessDeniedView();

            if (!ModelState.IsValid) return BadRequest();

            Branch branch = _branchService.GetById(model.Id);
            if (branch == null) return NotFound();

            if (!CreateScheduleIsValid(model))
            {
                ModelState.AddModelError("", "Debes llenar todos los campos habilitados en los horarios.");
                PrepareBranchModel(model);
                return View("~/Plugins/Teed.Plugin.Medical/Views/Branch/Edit.cshtml", model);
            }

            if (model.UserId == 0)
            {
                ModelState.AddModelError("", "Debes seleccionar al encargado de esta sucursal.");
                PrepareBranchModel(model);
                return View("~/Plugins/Teed.Plugin.Medical/Views/Branch/Edit.cshtml", model);
            }

            branch.City = model.City;
            branch.UserId = model.UserId;
            branch.StreetAddress = model.StreetAddress;
            branch.StreetAddress2 = model.StreetAddress2;
            branch.CountryId = model.CountryId;
            branch.Name = model.Name;
            branch.Phone = model.Phone;
            branch.Phone2 = model.Phone2;
            branch.StateProvinceId = model.StateProvinceId;
            branch.WorksOnSaturday = model.WorksOnSaturday;
            branch.WorksOnSunday = model.WorksOnSunday;
            branch.ZipPostalCode = model.ZipPostalCode;
            branch.WeekCloseTime = new TimeSpan((int)model.WeekCloseHour, (int)model.WeekCloseMinute, 0);
            branch.WeekOpenTime = new TimeSpan((int)model.WeekOpenHour, (int)model.WeekOpenMinute, 0);

            if (model.WorksOnSaturday) branch.SaturdayOpenTime = new TimeSpan((int)model.SaturdayOpenHour, (int)model.SaturdayOpenMinute, 0);
            else branch.SaturdayOpenTime = null;

            if (model.WorksOnSaturday) branch.SaturdayCloseTime = new TimeSpan((int)model.SaturdayCloseHour, (int)model.SaturdayCloseMinute, 0);
            else branch.SaturdayCloseTime = null;

            if (model.WorksOnSunday) branch.SundayOpenTime = new TimeSpan((int)model.SundayOpenHour, (int)model.SundayOpenMinute, 0);
            else branch.SundayOpenTime = null;

            if (model.WorksOnSunday) branch.SundayCloseTime = new TimeSpan((int)model.SundayCloseHour, (int)model.SundayCloseMinute, 0);
            else branch.SundayCloseTime = null;

            _branchService.Update(branch);

            var existingUsers = _branchWorkerService.GetByBranchId(branch.Id).ToList();
            foreach (var user in existingUsers)
            {
                if (!model.SelectedUsersIds.Contains(user.CustomerId))
                {
                    _branchWorkerService.Delete(user);
                }
            }
            foreach (var customerId in model.SelectedUsersIds)
            {
                if (_branchWorkerService.GetByCustomerIdAndBranchId(customerId, branch.Id).FirstOrDefault() == null)
                {
                    _branchWorkerService.Insert(new BranchWorker()
                    {
                        CustomerId = customerId,
                        BranchId = branch.Id
                    });
                }
            }

            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult ListData()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedBranches))
                return AccessDeniedView();

            var branches = _branchService.ListAsNoTracking();
            var gridModel = new DataSourceResult
            {
                Data = branches,
                Total = branches.TotalCount
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult UsersListData()
        {
            if (!_permissionService.Authorize(TeedMedicalPermissionProvider.TeedBranches))
                return AccessDeniedView();

            List<int> roleIds = new List<int>();
            foreach (var role in _customerService.GetAllCustomerRoles())
            {
                if (role.SystemName == "Administrators" ||
                    role.SystemName == "Manager")
                {
                    roleIds.Add(role.Id);
                }
            }
            var customers = _customerService.GetAllCustomers(customerRoleIds: roleIds.ToArray()).ToList();
            var elements = customers.Select(x => new CustomerListModel
            {
                Id = x.Id,
                Customer = $"{x.GetFullName()} ({x.Email})"
            }).ToList();

            return Json(elements);
        }

        private bool CreateScheduleIsValid(BranchModel model)
        {
            if (model.WeekOpenHour == null || model.WeekCloseHour == null || model.WeekOpenMinute == null || model.WeekCloseMinute == null)
                return false;
            else if (model.WorksOnSaturday && (model.SaturdayOpenHour == null || model.SaturdayCloseHour == null || model.SaturdayOpenMinute == null || model.SaturdayCloseMinute == null))
                return false;
            else if (model.WorksOnSunday && (model.SundayOpenHour == null || model.SundayCloseHour == null || model.SundayOpenMinute == null || model.SundayCloseMinute == null))
                return false;
            else
                return true;
        }

        private void PrepareBranchModel(BranchModel model)
        {
            // GET COUNTRIES
            model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries(showHidden: true))
            {
                model.AvailableCountries.Add(new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(),
                    Selected = c.Id == model.CountryId
                });
            }
            model.AvailableCountries.Add(new SelectListItem { Text = "Otro", Value = "999" });

            // GET STATES
            var states = _stateProvinceService.GetStateProvincesByCountryId(Int32.Parse(model.AvailableCountries[1].Value)).ToList();
            if (states.Any())
            {
                model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectState"), Value = "0" });
                foreach (var s in states)
                {
                    model.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
                }
                model.AvailableStates.Add(new SelectListItem { Text = "Otro", Value = "999" });
            }
            else
            {
                var anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);
                model.AvailableStates.Add(new SelectListItem
                {
                    Text = _localizationService.GetResource(anyCountrySelected ? "Admin.Address.OtherNonUS" : "Admin.Address.SelectState"),
                    Value = "0"
                });
            }
        }
    }
}
