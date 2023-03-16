using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using System.Linq;
using Teed.Plugin.Payroll.Security;
using Teed.Plugin.Payroll.Services;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;
using Nop.Core;
using Teed.Plugin.Payroll.Domain.Bonuses;
using System.Collections.Generic;
using Teed.Plugin.Payroll.Domain.JobCatalogs;
using Teed.Plugin.Payroll.Models.Bonus;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.Filters;
using Newtonsoft.Json;
using Teed.Plugin.Payroll.Domain.Incidents;
using Teed.Plugin.Payroll.Helpers;
using System.Globalization;

namespace Teed.Plugin.Payroll.Controllers
{
    [Area(AreaNames.Admin)]
    public class BonusController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly PayrollEmployeeService _payrollEmployeeService;
        private readonly AssistanceService _assistanceService;
        private readonly BonusService _bonusService;
        private readonly BonusAmountService _bonusAmountService;
        private readonly BonusApplicationService _bonusApplicationService;
        private readonly JobCatalogService _jobCatalogService;
        private readonly IncidentService _incidentService;
        private readonly IWorkContext _workContext;

        public BonusController(IPermissionService permissionService, PayrollEmployeeService payrollEmployeeService,
            AssistanceService assistanceService, BonusService bonusService, BonusAmountService bonusAmountService,
            BonusApplicationService bonusApplicationService, JobCatalogService jobCatalogService, IncidentService incidentService,
            IWorkContext workContext)
        {
            _permissionService = permissionService;
            _payrollEmployeeService = payrollEmployeeService;
            _bonusService = bonusService;
            _bonusAmountService = bonusAmountService;
            _bonusApplicationService = bonusApplicationService;
            _jobCatalogService = jobCatalogService;
            _workContext = workContext;
            _incidentService = incidentService;
            _assistanceService = assistanceService;
        }

        public void PrepareModel(BonusModel model)
        {
            model.Jobs = _jobCatalogService.GetAll().Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            }).OrderBy(x => x.Text)
            .ToList();
            var employees = _payrollEmployeeService.GetAll()
                .Where(x => x.EmployeeStatusId == (int)EmployeeStatus.Active &&
                x.CustomerId > 0).ToList();
            foreach (var employee in employees)
            {
                var item = new SelectListItem
                {
                    Value = employee.Id.ToString(),
                    Text = GetFullName(employee)
                };
                model.AuthorizationEmployees.Add(item);
            }
            model.AuthorizationEmployees = model.AuthorizationEmployees.OrderBy(x => x.Text).ToList();
            model.ConditionTypes = new SelectList(Enum.GetValues(typeof(ConditionType)).Cast<ConditionType>().Select(x => new
            {
                Id = (int)x,
                Name = x.GetDisplayName()
            }), "Id", "Name");
            model.BonusTypes = new SelectList(Enum.GetValues(typeof(BonusType)).Cast<BonusType>().Select(x => new
            {
                Id = (int)x,
                Name = x.GetDisplayName()
            }), "Id", "Name");
            model.FrequencyTypes = new SelectList(Enum.GetValues(typeof(FrequencyType)).Cast<FrequencyType>().Select(x => new
            {
                Id = (int)x,
                Name = x.GetDisplayName()
            }), "Id", "Name");
        }

        public IActionResult Index()
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Bonuses)
                && !_permissionService.Authorize(TeedPayrollPermissionProvider.ManageBonuses))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Payroll/Views/Bonuses/Index.cshtml",
                _permissionService.Authorize(TeedPayrollPermissionProvider.ManageBonuses));
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Bonuses)
                && !_permissionService.Authorize(TeedPayrollPermissionProvider.ManageBonuses))
                return AccessDeniedView();

            var bonus = _bonusService.GetAll().ToList();
            var bonusAmounts = _bonusAmountService.GetAll().ToList();
            var bonusAapplications = _bonusApplicationService.GetAll().ToList();
            var jobCatalogs = _jobCatalogService.GetAll().ToList();
            var employees = _payrollEmployeeService.GetAll().ToList();
            var pagedList = new PagedList<Bonus>(bonus, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    x.Name,
                    AuthorizationEmployee = GetAuthorizationEmployee(x.AuthorizationEmployeeId, employees),
                    Jobs = GetJobsOfBonus(x, jobCatalogs),
                    LastAmount = GetLastAmount(x.Id, bonusAmounts),
                    ConditionType = ((ConditionType)x.ConditionTypeId).GetDisplayName(),
                    BonusType = ((BonusType)x.BonusTypeId).GetDisplayName(),
                    FrequencyType = ((FrequencyType)x.FrequencyTypeId).GetDisplayName(),
                    x.IsActive
                }).ToList(),
                Total = bonus.Count()
            };

            return Json(gridModel);
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.ManageBonuses))
                return AccessDeniedView();

            var model = new BonusModel();
            PrepareModel(model);

            return View("~/Plugins/Teed.Plugin.Payroll/Views/Bonuses/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(BonusModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.ManageBonuses))
                return AccessDeniedView();

            if (model.AuthorizationEmployeeId < 1 || !model.JobIds.Any())
            {
                ErrorNotification("No se guardaron los cambios del bono, favor de asegurarse de que la información solicitada esta completa.");
                PrepareModel(model);
                return View("~/Plugins/Teed.Plugin.Payroll/Views/Bonuses/Create.cshtml", model);
            }

            var bonus = new Bonus
            {
                Name = model.Name,
                JobCatalogIdsFormat = string.Join("|", model.JobIds),
                ConditionTypeId = model.ConditionTypeId,
                BonusTypeId = model.BonusTypeId,
                FrequencyTypeId = model.FrequencyTypeId,
                AuthorizationEmployeeId = model.AuthorizationEmployeeId,
                IsActive = true,
                Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) agregó un nuevo bono \"{model.Name}\"\n"
            };

            _bonusService.Insert(bonus);

            if (continueEditing)
            {
                SuccessNotification("Se guardaron los cambios del bono de forma correcta.");
                PrepareModel(model);
                return RedirectToAction("Edit", new { bonus.Id });
            }
            return RedirectToAction("Index");
        }

        //public IActionResult Edit(int Id)
        //{
        //    if (!_permissionService.Authorize(TeedPayrollPermissionProvider.ManageBonuses))
        //        return AccessDeniedView();

        //    var bonus = _bonusService.GetById(Id);
        //    if (bonus == null)
        //        return RedirectToAction("Index");

        //    var model = new BonusModel
        //    {
        //        Id = bonus.Id,
        //        Name = bonus.Name,
        //        JobIds = bonus.GetJobCatalogIds(),
        //        ConditionTypeId = bonus.ConditionTypeId,
        //        BonusTypeId = bonus.BonusTypeId,
        //        FrequencyTypeId = bonus.FrequencyTypeId,
        //        AuthorizationEmployeeId = bonus.AuthorizationEmployeeId,
        //        IsActive = bonus.IsActive,
        //        Log = bonus.Log
        //    };
        //    PrepareModel(model);

        //    return View("~/Plugins/Teed.Plugin.Payroll/Views/Bonuses/Edit.cshtml", model);
        //}

        //[HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        //public IActionResult Edit(BonusModel model, bool continueEditing)
        //{
        //    if (!_permissionService.Authorize(TeedPayrollPermissionProvider.ManageBonuses))
        //        return AccessDeniedView();

        //    var bonus = _bonusService.GetById(model.Id);
        //    if (bonus == null)
        //        return RedirectToAction("Index");

        //    if (model.AuthorizationEmployeeId < 1 || !model.JobIds.Any())
        //    {
        //        ErrorNotification("No se guardaron los cambios del bono, favor de asegurarse de que la información solicitada esta completa.");
        //        PrepareModel(model);
        //        return View("~/Plugins/Teed.Plugin.Payroll/Views/Bonuses/Create.cshtml", model);
        //    }

        //    if (bonus.Name != model.Name)
        //    {
        //        bonus.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el nombre de \"{bonus.Name}\" a \"{model.Name}\"\n";
        //        bonus.Name = model.Name;
        //    }
        //    var newJobIds = model.JobIds.Except(bonus.JobCatalogIdsFormat.Split('|').Select(x => int.Parse(x))).ToList();
        //    if (newJobIds.Any())
        //    {
        //        bonus.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió los empleos del bono de \"{GetJobsByIds(bonus.GetJobCatalogIds())}\" a \"{GetJobsByIds(model.JobIds.ToList())}\"\n";
        //        bonus.JobCatalogIdsFormat = string.Join("|", model.JobIds);
        //    }
        //    if (bonus.ConditionTypeId != model.ConditionTypeId)
        //    {
        //        bonus.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el tipo de condición de {((ConditionType)bonus.ConditionTypeId).GetDisplayName()} a {((ConditionType)model.ConditionTypeId).GetDisplayName()}\n";
        //        bonus.ConditionTypeId = model.ConditionTypeId;
        //    }
        //    if (bonus.BonusTypeId != model.BonusTypeId)
        //    {
        //        bonus.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el tipo de bono de {((BonusType)bonus.BonusTypeId).GetDisplayName()} a {((BonusType)model.BonusTypeId).GetDisplayName()}\n";
        //        bonus.BonusTypeId = model.BonusTypeId;
        //    }
        //    if (bonus.FrequencyTypeId != model.FrequencyTypeId)
        //    {
        //        bonus.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el tipo de frecuencia de {((FrequencyType)bonus.FrequencyTypeId).GetDisplayName()} a {((FrequencyType)model.FrequencyTypeId).GetDisplayName()}\n";
        //        bonus.FrequencyTypeId = model.FrequencyTypeId;
        //    }
        //    if (bonus.AuthorizationEmployeeId != model.AuthorizationEmployeeId)
        //    {
        //        bonus.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el empleado autorizado de {_payrollEmployeeService.GetById(bonus.AuthorizationEmployeeId).GetFullName()} a {_payrollEmployeeService.GetById(model.AuthorizationEmployeeId).GetFullName()}\n";
        //        bonus.AuthorizationEmployeeId = model.AuthorizationEmployeeId;
        //    }
        //    if (bonus.IsActive != model.IsActive)
        //    {
        //        bonus.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió si está activo el bono de {(bonus.IsActive ? "\"Si\"" : "\"No\"")} a {(model.IsActive ? "\"Si\"" : "\"No\"")}\n";
        //        bonus.IsActive = model.IsActive;
        //    }

        //    _bonusService.Update(bonus);

        //    if (continueEditing)
        //    {
        //        SuccessNotification("Se guardaron los cambios del bono de forma correcta.");
        //        PrepareModel(model);
        //        return RedirectToAction("Edit", new { bonus.Id });
        //    }
        //    return RedirectToAction("Index");
        //}

        public IActionResult Authorize(int Id)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Bonuses)
                && !_permissionService.Authorize(TeedPayrollPermissionProvider.ManageBonuses))
                return AccessDeniedView();

            var bonus = _bonusService.GetById(Id);
            if (bonus == null)
                return RedirectToAction("Index");

            var model = new AuthorizeModel
            {
                IsAuth =
                    _permissionService.Authorize(TeedPayrollPermissionProvider.ManageBonuses)
                    && bonus.AuthorizationEmployeeId == _payrollEmployeeService.GetByCustomerId(_workContext.CurrentCustomer.Id)?.Id
                    ,
                Id = bonus.Id,
                Name = bonus.Name,
                JobIds = bonus.GetJobCatalogIds(),
                ConditionTypeId = bonus.ConditionTypeId,
                BonusTypeId = bonus.BonusTypeId,
                FrequencyTypeId = bonus.FrequencyTypeId,
                AuthorizationEmployeeId = bonus.AuthorizationEmployeeId,
                IsActive = bonus.IsActive,
                Log = bonus.Log,

                ApplicationsLog = GetCurrentEntitiesLog(bonus.Id),
                JobsOfBonus = GetJobsData(bonus.GetJobCatalogIds()),
                EmployeesOfBonus = GetEmployeesData(bonus.GetJobCatalogIds()),
                Entities = GetCurrentEntitiesData(bonus.Id)
            };

            return View("~/Plugins/Teed.Plugin.Payroll/Views/Bonuses/Authorize.cshtml", model);
        }


        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Authorize(AuthorizeModel model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Bonuses)
                && !_permissionService.Authorize(TeedPayrollPermissionProvider.ManageBonuses))
                return AccessDeniedView();

            var today = DateTime.Now.Date;
            var bonus = _bonusService.GetById(model.Id);
            if (bonus == null)
                return RedirectToAction("Index");
            try
            {
                var entities = JsonConvert.DeserializeObject<List<SelectedEntity>>(model.SelectedEntities);
                var entityTypeForBonus = bonus.BonusTypeId == (int)BonusType.Individual ? (int)EntityType.Employee : (int)EntityType.Job;
                foreach (var entity in entities)
                {
                    BonusApplication existingEntity = null;
                    if (entity.OldDate != null)
                        existingEntity = _bonusApplicationService.GetExact(entityTypeForBonus, entity.Id, bonus.Id, entity.OldDate.Value.Date);
                    else
                        existingEntity = _bonusApplicationService.GetExact(entityTypeForBonus, entity.Id, bonus.Id, entity.Date.Date);
                    var nameOfEntity = entityTypeForBonus == (int)EntityType.Employee ? _payrollEmployeeService.GetById(entity.Id).GetFullName() : _jobCatalogService.GetById(entity.Id).Name;
                    if (!entity.IsDelete)
                    {
                        if (existingEntity != null)
                        {
                            if (existingEntity.WillApply != entity.Apply)
                            {
                                existingEntity.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó una autorización para {(entityTypeForBonus == (int)EntityType.Employee ? "el empleado" : "el empleo")} \"{nameOfEntity}\" de \"{(existingEntity.WillApply ? "Aplica" : "No aplica")}\" a \"{(entity.Apply ? "Aplica" : "No aplica")}\".\n";
                                existingEntity.WillApply = entity.Apply;
                            }
                            if (existingEntity.Date.Date != entity.Date.Date)
                            {
                                existingEntity.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) editó la fecha de una autorización para {(entityTypeForBonus == (int)EntityType.Employee ? "el empleado" : "el empleo")} \"{nameOfEntity}\" de {existingEntity.Date:dd/MM/yyyy} a {entity.Date:dd/MM/yyyy}.\n";
                                existingEntity.Date = entity.Date.Date;
                            }
                            _bonusApplicationService.Update(existingEntity);
                        }
                        else
                        {
                            var currentAmount = bonus.GetAppliableAmount(entity.Date.Date);
                            if (currentAmount != null)
                            {
                                var application = new BonusApplication
                                {
                                    BonusId = bonus.Id,
                                    Amount = currentAmount.Amount,
                                    WillApply = entity.Apply,
                                    EntityTypeId = entityTypeForBonus,
                                    EntityId = entity.Id,
                                    Date = entity.Date.Date,
                                    Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) agregó una autorización para {(entityTypeForBonus == (int)EntityType.Employee ? "el empleado" : "el empleo")} \"{nameOfEntity}\" con la última cantidad registrada/aplicada del bono ({currentAmount.Amount:C}).\n"
                                };
                                _bonusApplicationService.Insert(application);
                            }
                        }
                    }
                    else if (existingEntity != null)
                        _bonusApplicationService.Delete(existingEntity);
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }
            if (model.SaveAndContinue)
                return RedirectToAction("Authorize", new { bonus.Id });
            else
                return RedirectToAction("Index");
        }

        public IActionResult AddAuthorize()
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.ManageBonuses))
                return AccessDeniedView();

            var model = new AuthorizeModel();

            return View("~/Plugins/Teed.Plugin.Payroll/Views/Bonuses/Authorize.cshtml", model);
        }

        public List<JobData> GetJobsData(List<int> ids)
        {
            var final = new List<JobData>();
            if (ids.Any())
            {
                var jobs = _jobCatalogService.GetAll()
                    .Where(x => ids.Contains(x.Id))
                    .Select(x => new JobData
                    {
                        Id = x.Id,
                        Name = x.Name
                    })
                    .ToList();
                if (jobs.Any())
                    final = jobs.OrderBy(x => x.Name).ToList();
            }
            return final;
        }

        public List<EmployeeData> GetEmployeesData(List<int> ids)
        {
            var final = new List<EmployeeData>();
            if (ids.Any())
            {
                var employees = _payrollEmployeeService.GetAll()
                    .Where(x => x.DateOfDeparture == null)
                    .ToList()
                    .Where(x => ids.Contains(x.GetCurrentJob()?.Id ?? 0))
                    .ToList();
                if (employees.Any())
                {
                    foreach (var employee in employees)
                    {
                        final.Add(new EmployeeData
                        {
                            Id = employee.Id,
                            Name = employee.GetFullName()
                        });
                    }
                    final = final.OrderBy(x => x.Name).ToList();
                }
            }
            return final;
        }

        public List<ExistingEntity> GetCurrentEntitiesData(int id)
        {
            var final = new List<ExistingEntity>();
            var festiveDates = _assistanceService.GetFestiveDatesFromGroceries()
                .Where(x => !x.DontApplyToPayroll).ToList();
            if (id > 0)
            {
                var applications = _bonusApplicationService.GetAll()
                    .Where(x => x.BonusId == id)
                    .ToList()
                    .Select(x => new ExistingEntity
                    {
                        Id = x.EntityId,
                        Name = GetNameOfEntity(x),
                        RegularBiweekDate = x.Date.ToString("dd/MM/yyyy"),
                        Date = x.Bonus.FrequencyTypeId == (int)FrequencyType.ByBiweek ?
                            BiweeklyDatesHelper.GetBiweeklyDates(x.Date, festiveDates).StartOfBiweekly.ToString("dd/MM/yyyy") : x.Date.ToString("dd/MM/yyyy"),
                        EndDate = x.Bonus.FrequencyTypeId == (int)FrequencyType.ByBiweek ?
                            BiweeklyDatesHelper.GetBiweeklyDates(x.Date, festiveDates).EndOfBiweekly.ToString("dd/MM/yyyy") : string.Empty,
                        Apply = x.WillApply,
                    })
                    .ToList();
                if (applications.Any())
                    final = applications.OrderBy(x => x.Id).ToList();
            }
            return final;
        }

        public string GetNameOfEntity(BonusApplication application)
        {
            var final = string.Empty;
            if (application.EntityTypeId == (int)EntityType.Employee)
            {
                var emplyee = _payrollEmployeeService.GetById(application.EntityId);
                final = emplyee.GetFullName();
            }
            else
            {
                var job = _jobCatalogService.GetById(application.EntityId);
                final = job.Name;
            }
            return final;
        }

        public string GetCurrentEntitiesLog(int id)
        {
            var final = string.Empty;
            if (id > 0)
            {
                var logs = _bonusApplicationService.GetAll()
                    .Where(x => x.BonusId == id)
                    .OrderBy(x => x.Date)
                    .Select(x => x.Log)
                    .ToList();
                if (logs.Any())
                    final = string.Join("\n", logs);
            }
            return final;
        }

        [HttpPost]
        public IActionResult AmountsList(DataSourceRequest command, int Id)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Bonuses)
                && !_permissionService.Authorize(TeedPayrollPermissionProvider.ManageBonuses))
                return AccessDeniedView();

            var bonusAmounts = _bonusAmountService.GetAllAmountsByBonusId(Id)
                .OrderByDescending(x => x.ApplyDate)
                .ThenByDescending(x => x.CreatedOnUtc)
                .ToList();
            var pagedList = new PagedList<BonusAmount>(bonusAmounts, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    ApplyDate = x.ApplyDate.ToString("dd-MM-yyyy"),
                    Created = x.CreatedOnUtc.ToLocalTime().ToString("dd-MM-yyyy hh:mm:ss tt"),
                    Amount = x.Amount.ToString("C")
                })
                .ToList(),
                Total = bonusAmounts.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult AddBonusAmount(AmountModel model, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.ManageBonuses))
                return AccessDeniedView();

            if (model.BonusId < 1 || model.Amount < 1 || model.ApplyDate == DateTime.MinValue)
                return BadRequest();

            var bonus = _bonusService.GetById(model.BonusId);
            if (bonus == null)
                return BadRequest();

            var amount = new BonusAmount
            {
                BonusId = bonus.Id,
                Amount = model.Amount,
                ApplyDate = model.ApplyDate,
                Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) agregó un nuevo monto para el bono \"{bonus.Name}\"\n"
            };
            _bonusAmountService.Insert(amount);

            bonus.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) agregó un nuevo monto de {amount.Amount:C} para el bono \"{bonus.Name}\"\n";
            _bonusService.Update(bonus);

            return Ok();
        }

        [HttpGet]
        public IActionResult BonusAutomaticApplicationProcess(DateTime start, DateTime end)
        {
            return Ok();
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.Bonuses)
                && !_permissionService.Authorize(TeedPayrollPermissionProvider.ManageBonuses))
                return AccessDeniedView();
            var bonus = _bonusService.GetById(id);
            if (bonus == null)
                return Ok();

            bonus.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) eliminó el bono \"{bonus.Name}\"\n";
            _bonusService.Delete(bonus);
            return Ok();
        }

        [HttpGet]
        public IActionResult ApplyNewBonuses(string startFormatted, string endFormatted)
        {
            var start = DateTime.ParseExact(startFormatted, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var end = DateTime.ParseExact(endFormatted, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var count = BonusApplicationHelper.InsertNewBonusApplications(start, end,
                _bonusService, _bonusApplicationService, _jobCatalogService,
                _payrollEmployeeService, _incidentService, _assistanceService);
            return Ok(count);
        }

        public string GetLastAmount(int bonusId, List<BonusAmount> bonusAmounts)
        {
            var final = (decimal)0;
            if (bonusId > 0)
            {
                bonusAmounts = bonusAmounts.Where(x => x.BonusId == bonusId).ToList();
                if (bonusAmounts.Any())
                    final = bonusAmounts.OrderByDescending(x => x.ApplyDate).FirstOrDefault().Amount;
            }
            return final.ToString("C");
        }

        public string GetJobsOfBonus(Bonus bonus, List<JobCatalog> jobCatalogs)
        {
            var final = string.Empty;
            var jobIds = bonus.GetJobCatalogIds();
            if (jobIds.Any())
            {
                var jobsOfBonus = jobCatalogs.Where(x => jobIds.Contains(x.Id)).ToList();
                if (jobsOfBonus.Any())
                    final = string.Join(", ", jobsOfBonus.Select(x => x.Name));
            }
            return final;
        }

        public string GetJobsByIds(List<int> ids)
        {
            var final = string.Empty;
            var jobs = _jobCatalogService.GetAll().Where(x => ids.Contains(x.Id)).ToList();
            if (jobs.Any())
                final = string.Join(", ", jobs.Select(x => x.Name));
            return final;
        }

        public string GetAuthorizationEmployee(int Id, List<PayrollEmployee> payrollEmployees)
        {
            var final = string.Empty;
            if (Id > 0)
            {
                var employee = payrollEmployees.Where(x => x.Id == Id).FirstOrDefault();
                if (employee != null)
                    final = employee.GetFullName();
            }
            return final;
        }

        public string GetFullName(PayrollEmployee payrollEmployees)
        {
            var final = string.Empty;
            if (payrollEmployees != null)
                final = payrollEmployees.GetFullName();
            return final;
        }
    }
}