using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Manager.Domain.ExpensesCategories;
using Teed.Plugin.Manager.Domain.PartnerLiabilities;
using Teed.Plugin.Manager.Helpers;
using Teed.Plugin.Manager.Models.PartnerLiabilities;
using Teed.Plugin.Manager.Services;
using Teed.Plugin.Medical.Security;

namespace Teed.Plugin.Manager.Controllers
{
    [Area(AreaNames.Admin)]
    public class PartnerLiabilitiesController : BasePluginController
    {
        private readonly PartnerLiabilityService _partnerLiabilityService;
        private readonly ExpensesCategoriesService _expensesCategoriesService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;

        public PartnerLiabilitiesController(PartnerLiabilityService partnerLiabilityService, IPermissionService permissionService, IWorkContext workContext,
            ICustomerService customerService, ExpensesCategoriesService expensesCategoriesService, ExpenseFileService expenseFileService)
        {
            _partnerLiabilityService = partnerLiabilityService;
            _permissionService = permissionService;
            _workContext = workContext;
            _customerService = customerService;
            _expensesCategoriesService = expensesCategoriesService;
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedPartnerLiabilities))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Manager/Views/PartnerLiabilities/List.cshtml");
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedPartnerLiabilities))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Manager/Views/PartnerLiabilities/Create.cshtml");
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedPartnerLiabilities))
                return AccessDeniedView();

            PartnerLiability partnerLiability = _partnerLiabilityService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (partnerLiability == null) return NotFound();

            EditViewModel model = new EditViewModel()
            {
                Amount = partnerLiability.Amount,
                CategoryId = partnerLiability.CategoryId,
                Comments = partnerLiability.Comments,
                PartnerId = partnerLiability.PartnerId,
                SelectedDate = partnerLiability.SelectedDate.ToString("dd-MM-yyyy")
            };

            return View("~/Plugins/Teed.Plugin.Manager/Views/PartnerLiabilities/Edit.cshtml", model);
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command, string selectedDate = null, int selectedUserId = 0)
        {
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedPartnerLiabilities))
                return AccessDeniedView();

            var query = _partnerLiabilityService.GetAll();
            if (selectedUserId > 0)
            {
                query = query.Where(x => x.PartnerId == selectedUserId);
            }

            List<IGrouping<int, ListViewModel>> resultList = query
                .Select(x => new { x.Amount, x.SelectedDate, x.PartnerId, x.Id })
                .OrderBy(x => x.SelectedDate)
                .ToList()
                .Select(x => new ListViewModel()
                {
                    AmountFormatted = x.Amount.ToString("C", CultureInfo.GetCultureInfo("es-MX")),
                    Amount = x.Amount,
                    DateObject = x.SelectedDate,
                    DateString = x.SelectedDate.ToString("dd-MM-yyyy"),
                    PartnerName = _customerService.GetCustomerById(x.PartnerId).GetFullName(),
                    Id = x.Id,
                    PartnerId = x.PartnerId
                })
                .GroupBy(x => x.PartnerId)
                .ToList();

            GetBalance(resultList);

            var processedList = resultList.SelectMany(x => x).Reverse();
            if (!string.IsNullOrEmpty(selectedDate))
            {
                DateTime dateParsed = DateTime.ParseExact(selectedDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;
                processedList = processedList.Where(x => x.DateObject.Date == dateParsed);
            }

            var gridModel = new DataSourceResult
            {
                Data = processedList.ToList(),
                Total = processedList.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult CategoryListData()
        {
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedPartnerLiabilities))
                return AccessDeniedView();

            var categories = _expensesCategoriesService.GetAll().ToList();
            var lastChildList = new List<ExpensesCategories>();
            foreach (var category in categories)
            {
                var hasChild = categories.Where(x => x.ParentId == category.ExpenseCategoryId).Any();
                if (!hasChild)
                {
                    lastChildList.Add(category);
                }
            }

            var elements = lastChildList.Select(x => new
            {
                Id = x.ExpenseCategoryId,
                Category = ExpenseCategoryHelper.GetFormattedBreadCrumb(x, _expensesCategoriesService)
            }).ToList();

            return Json(elements);
        }

        [HttpPost]
        public IActionResult UserListData()
        {
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedPartnerLiabilities))
                return AccessDeniedView();

            int? roleId = _customerService.GetCustomerRoleBySystemName("Partner")?.Id;
            var users = _customerService.GetAllCustomers(customerRoleIds: new int[] { roleId ?? 0 }).ToList();
            var elements = users.Select(x => new
            {
                x.Id,
                User = x.GetFullName()
            }).ToList();

            return Json(elements);
        }

        [HttpPost]
        public IActionResult Create(CreateViewModel model)
        {
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedPartnerLiabilities))
                return AccessDeniedView();

            DateTime dateParsed = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(model.SelectedDate))
            {
                dateParsed = DateTime.ParseExact(model.SelectedDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            }

            var partnerLiability = new PartnerLiability()
            {
                Amount = model.Amount,
                CategoryId = model.CategoryId,
                Comments = model.Comments,
                CreatedByUserId = _workContext.CurrentCustomer.Id,
                PartnerId = model.PartnerId,
                SelectedDate = dateParsed,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id})" + " creó un pasivo con socio."
            };
            _partnerLiabilityService.Insert(partnerLiability);

            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult Edit(EditViewModel model)
        {
            if (!_permissionService.Authorize(TeedManagerPermissionProvider.TeedPartnerLiabilities))
                return AccessDeniedView();

            PartnerLiability partnerLiability = _partnerLiabilityService.GetAll().Where(x => x.Id == model.Id).FirstOrDefault();
            if (partnerLiability == null) return NotFound();

            model.SelectedDateParsed = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(model.SelectedDate))
            {
                model.SelectedDateParsed = DateTime.ParseExact(model.SelectedDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            }

            UpdateLog(partnerLiability, model);

            partnerLiability.PartnerId = model.PartnerId;
            partnerLiability.SelectedDate = model.SelectedDateParsed;
            partnerLiability.Comments = model.Comments;
            partnerLiability.CategoryId = model.CategoryId;
            partnerLiability.Amount = model.Amount;

            _partnerLiabilityService.Update(partnerLiability);

            return RedirectToAction("List");
        }

        [HttpGet]
        public decimal GetUserBalance(int id)
        {
            return _partnerLiabilityService.GetAll().Where(x => x.PartnerId == id).Select(x => x.Amount).DefaultIfEmpty(0).Sum(x => x);
        }

        private void UpdateLog(PartnerLiability prevData, EditViewModel newData)
        {
            var log = string.Empty;
            if (prevData.Amount != newData.Amount)
            {
                log += $". Cambió el monto de {prevData.Amount} a {newData.Amount}";
            }

            if (prevData.PartnerId != newData.PartnerId)
            {
                log += $". Cambió el socio de ID {prevData.PartnerId} a ID {newData.PartnerId}";
            }

            if (prevData.CategoryId != newData.CategoryId)
            {
                var cat1 = _expensesCategoriesService.GetAll().Where(x => x.Id == prevData.CategoryId).FirstOrDefault();
                var cat2 = _expensesCategoriesService.GetAll().Where(x => x.Id == newData.CategoryId).FirstOrDefault();
                log += $". Cambió la categoría de {cat1.Value} ({prevData.CategoryId}) a {cat2.Value} ({newData.CategoryId})";
            }

            if (prevData.Comments != newData.Comments)
            {
                log += $". Cambió el comentario de {prevData.Comments} a {newData.Comments}";
            }

            if (prevData.SelectedDate.Date != newData.SelectedDateParsed.Date)
            {
                log += $". Cambió la fecha de {prevData.SelectedDate.ToString("dd-MM-yyyy")} a {newData.SelectedDateParsed.ToString("dd-MM-yyyy")}";
            }

            prevData.Log += "/n" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + $"{_workContext.CurrentCustomer.GetFullName()} ({_workContext.CurrentCustomer.Id}) editó el registro de pasivo con socios" + log + ".";
        }

        private void GetBalance(List<IGrouping<int, ListViewModel>> resultList)
        {
            foreach (var user in resultList)
            {
                decimal currentBalance = 0;
                foreach (var item in user)
                {
                    currentBalance += item.Amount;
                    item.Balance = currentBalance.ToString("C", CultureInfo.GetCultureInfo("es-MX"));
                }
            }
        }
    }
}