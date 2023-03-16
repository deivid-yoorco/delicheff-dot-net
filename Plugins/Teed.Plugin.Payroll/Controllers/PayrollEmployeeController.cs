using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Teed.Plugin.Payroll.Security;
using Teed.Plugin.Payroll.Services;
using Teed.Plugin.Payroll.Models.PayrollEmployee;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;
using Nop.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Teed.Plugin.Payroll.Domain.Benefits;
using Teed.Plugin.Payroll.Domain.JobCatalogs;
using Nop.Services.Helpers;
using Nop.Services.Media;
using Nop.Core.Domain.Customers;
using Ionic.Zip;
using System.Globalization;
using OfficeOpenXml;
using Teed.Plugin.Payroll.Helpers;
using Teed.Plugin.Payroll.Models.Assistance;
using OfficeOpenXml.Style;
using System.Drawing;
using Teed.Plugin.Payroll.Domain.Assistances;
using Microsoft.AspNetCore.Authorization;
using Teed.Plugin.Payroll.Domain.PayrollEmployeeJobs;
using System.Net.Http;
using Newtonsoft.Json;

namespace Teed.Plugin.Payroll.Controllers
{
    [Area(AreaNames.Admin)]
    public class PayrollEmployeeController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly PayrollEmployeeService _payrollEmployeeService;
        private readonly PayrollEmployeeFileService _payrollEmployeeFileService;
        private readonly PayrollSalaryService _payrollSalaryService;
        private readonly JobCatalogService _jobCatalogService;
        private readonly SubordinateService _subordinateService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly BiweeklyPaymentService _biweeklyPaymentService;
        private readonly BenefitService _benefitService;
        private readonly IPictureService _pictureService;
        private readonly IStoreContext _storeContext;
        private readonly AssistanceService _assistanceService;
        private readonly TakenVacationDayService _takenVacationDayService;
        private readonly PayrollEmployeeJobService _payrollEmployeeJobService;
        private readonly AssistanceOverrideService _assistanceOverrideService;

        public PayrollEmployeeController(IPermissionService permissionService, PayrollEmployeeService payrollEmployeeService,
            PayrollEmployeeFileService payrollEmployeeFileService, PayrollSalaryService payrollSalaryService,
            JobCatalogService jobCatalogService, SubordinateService subordinateService, ICustomerService customerService,
            IWorkContext workContext, BiweeklyPaymentService biweeklyPaymentService, BenefitService benefitService,
            IPictureService pictureService, IStoreContext storeContext, AssistanceService assistanceService,
            TakenVacationDayService takenVacationDayService, PayrollEmployeeJobService payrollEmployeeJobService,
            AssistanceOverrideService assistanceOverrideService)
        {
            _permissionService = permissionService;
            _payrollEmployeeService = payrollEmployeeService;
            _payrollEmployeeFileService = payrollEmployeeFileService;
            _payrollSalaryService = payrollSalaryService;
            _jobCatalogService = jobCatalogService;
            _subordinateService = subordinateService;
            _customerService = customerService;
            _workContext = workContext;
            _biweeklyPaymentService = biweeklyPaymentService;
            _benefitService = benefitService;
            _pictureService = pictureService;
            _storeContext = storeContext;
            _assistanceService = assistanceService;
            _takenVacationDayService = takenVacationDayService;
            _payrollEmployeeJobService = payrollEmployeeJobService;
            _assistanceOverrideService = assistanceOverrideService;
        }

        public CreateOrUpdateModel PrepareModel(CreateOrUpdateModel model)
        {
            var roles = _customerService.GetAllCustomerRoles(true)
                .Where(x => x.SystemName?.ToLower() == "employee" || x.SystemName?.ToLower() == "exemployee")
                .Select(x => x.Id).ToArray();
            var customers = _customerService.GetAllCustomers(customerRoleIds: roles);
            var customersForSelect = customers.Select(x => new
            {
                x.Id,
                Name = x.GetFullName() + $" ({x.Email})"
            }).ToList();

            model.Customers = new SelectList(customersForSelect, "Id", "Name");

            var types = Enum.GetValues(typeof(FileType)).Cast<FileType>().ToList();
            var list = types.Where(x => (int)x != 0).Select(x => new
            {
                Id = (int)x,
                Name = x.GetDisplayName()
            });
            model.FileTypes = new SelectList(list, "Id", "Name");

            var banks = Enum.GetValues(typeof(BankType)).Cast<BankType>().ToList();
            var bankList = banks.Select(x => new
            {
                Id = (int)x,
                Name = x.GetDisplayName()
            });
            model.BankTypes = new SelectList(bankList, "Id", "Name");

            if (model.Id > 0)
            {
                var employee = _payrollEmployeeService.GetById(model.Id);
                if (employee.PayrollSalaries != null)
                {
                    var salaries = employee.PayrollSalaries.Select(x => new
                    {
                        x.Id,
                        Name = x.ApplyDate
                    });
                    model.Salaries = new SelectList(salaries, "Id", "Name");
                }
            }

            var benefits = Enum.GetValues(typeof(BenefitType)).Cast<BenefitType>().ToList();
            var benefiteTypes = benefits.Select(x => new
            {
                Id = x.GetDisplayName(),
                Name = x.GetDisplayName()
            });
            model.BenefitTypes = new SelectList(benefiteTypes, "Id", "Name");

            using (HttpClient client = new HttpClient())
            {
                var url =
                    _storeContext.CurrentStore.SecureUrl
                    //"https://localhost:44387/"
                    ;
                var result = client.GetAsync(url + "/Admin/Franchise/GetFranchisesFromExternal").Result;
                if (result.IsSuccessStatusCode)
                {
                    var resultJson = result.Content.ReadAsStringAsync().Result;
                    List<FranchisePayrollInfo> results = JsonConvert.DeserializeObject<List<FranchisePayrollInfo>>(resultJson);
                    model.Franchises = new SelectList(results, "Id", "Name");
                }
            }

            if (model.Id > 0)
            {
                model.SubordinateIds = _subordinateService.GetAllByBossId(model.Id).Select(x => x.PayrollSubordinateId).ToList();

                if (model.DateOfAdmission != null && model.DateOfAdmission != DateTime.MinValue)
                {
                    var today = DateTime.Now.Date;
                    model.YearsCompleted = (today.Year - model.DateOfAdmission.Value.Year - 1) +
                        (((today.Month > model.DateOfAdmission.Value.Month) ||
                        ((today.Month == model.DateOfAdmission.Value.Month) && (today.Day >= model.DateOfAdmission.Value.Day))) ? 1 : 0);
                    for (int i = 0; i < model.YearsCompleted; i++)
                    {
                        model.LiberatedVacationDaysAmount += 6 + (2 * i);
                    }
                    model.PendingVacationDaysAmount = model.LiberatedVacationDaysAmount - _assistanceOverrideService.GetAllByEmployeeId(model.Id)
                        .Where(x => x.Type == (int)AssistanceType.Vacation).Count();
                }
            }

            model.Log = model.Log?.Replace("\n", "<br>");

            return model;
        }

        protected bool CustomerIsParter(PayrollEmployee payrollEmployee)
        {
            var partnerRole = _customerService.GetCustomerRoleBySystemName("Partner");
            var customer = _customerService.GetCustomerById(payrollEmployee.CustomerId);
            if (customer != null)
                return customer.GetCustomerRoleIds().Contains(partnerRole.Id);
            else
                return false;
        }

        public void UploadOrGetProfilePicture(CreateOrUpdateModel model, bool isUpload)
        {
            model.DefaultPicture = _storeContext.CurrentStore.SecureUrl + "/images/default-avatar.jpg";
            if (isUpload)
            {
                if (model.ProfilePictureId != null && model.ProfilePictureId > 0)
                {
                    var customer = _customerService.GetCustomerById(model.CustomerId);
                    if (customer != null)
                    {
                        var payroll = _payrollEmployeeService.GetByCustomerId(model.CustomerId);
                        if (payroll != null)
                        {
                            if (customer.ProfilePictureId != model.ProfilePictureId)
                            {
                                payroll.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                                    $"la imagen del perfil del expediente de {customer.ProfilePictureId} a {model.ProfilePictureId}.\n";

                                customer.ProfilePictureId = model.ProfilePictureId;
                                _customerService.UpdateCustomer(customer);
                            }
                        }
                    }
                }
                if (model.PayrollProfilePictureId != null && model.PayrollProfilePictureId > 0)
                {
                    var payroll = _payrollEmployeeService.GetByCustomerId(model.CustomerId);
                    if (payroll != null)
                    {
                        if (payroll.PayrollProfilePictureId != model.PayrollProfilePictureId)
                        {
                            payroll.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                                $"la imagen del perfil del expediente de {payroll.PayrollProfilePictureId} a {model.PayrollProfilePictureId}.\n";

                            payroll.PayrollProfilePictureId = model.PayrollProfilePictureId;
                            _payrollEmployeeService.Update(payroll);
                        }
                    }
                }
            }
            else
            {
                var customer = _customerService.GetCustomerById(model.CustomerId);
                if (customer != null)
                {
                    model.ProfilePictureId = customer.ProfilePictureId;
                    if (model.ProfilePictureId.HasValue)
                        model.ProfilePicture = _pictureService.GetPictureUrl(model.ProfilePictureId.Value);
                }
                var payroll = _payrollEmployeeService.GetByCustomerId(model.CustomerId);
                if (payroll != null)
                {
                    model.PayrollProfilePictureId = payroll.PayrollProfilePictureId;
                    if (model.PayrollProfilePictureId.HasValue)
                        model.PayrollProfilePicture = _pictureService.GetPictureUrl(model.PayrollProfilePictureId.Value);
                }
            }
        }

        public string GetProfilePicture(int? pictureId)
        {
            if (pictureId.HasValue && pictureId > 0)
            {
                return _pictureService.GetPictureUrl(pictureId.Value);
            }
            else
            {
                return _storeContext.CurrentStore.SecureUrl + "/images/default-avatar.jpg";
            }
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();

            var model = new PayrollEmployeesAdditionalModel();
            var employeeRole = _customerService.GetCustomerRoleBySystemName("employee");
            if (employeeRole != null)
            {
                var customerEmployees = _customerService.GetAllCustomers(customerRoleIds: new int[] { employeeRole.Id })
                    .ToList();
                var payrollEmployees = _payrollEmployeeService.GetAll(includeExEmployees: true)
                    .ToList();
                var payrollEmployeesQuery = _payrollEmployeeService.GetAll()
                    .Where(x => !x.Deleted);
                var payrollEmployeeIds = payrollEmployees.Select(x => x.CustomerId).ToList();
                var employeesWithoutPayroll = customerEmployees.Where(x => !payrollEmployeeIds.Contains(x.Id)).ToList();

                model.MissingPayrolls = employeesWithoutPayroll.Select(x => new MissingPayrollEmployee
                {
                    Id = x.Id,
                    Email = x.Email,
                    Name = x.GetFullName()
                }).OrderBy(x => x.Name).ToList();

                var employeesGrouping = payrollEmployeesQuery.GroupBy(x => x.CustomerId).Where(x => x.Count() > 1).ToList();
                var duplicatedCustomerIds = employeesGrouping.Select(x => x.Key).ToList();
                var duplicatedCustomers = _customerService.GetAllCustomersQuery()
                    .Where(x => duplicatedCustomerIds.Contains(x.Id)).ToList();
                var groupingToList = employeesGrouping.SelectMany(x => x.ToList())
                    .OrderBy(x => x.CustomerId).ThenBy(x => x.FirstNames).ToList();
                model.DuplicatedCustomerPayrollEmployees = groupingToList.Select(x => new DuplicatedCustomerPayrollEmployee
                {
                    CustomerId = x.CustomerId,
                    CustomerName = duplicatedCustomers.Where(y => x.CustomerId == y.Id).FirstOrDefault() != null ?
                        duplicatedCustomers.Where(y => x.CustomerId == y.Id).FirstOrDefault().GetFullName() +
                        " (" + duplicatedCustomers.Where(y => x.CustomerId == y.Id).FirstOrDefault().Email + ")"
                        : "---",
                    EmployeeId = x.Id,
                    EmployeeName = x.GetFullName()
                }).OrderBy(x => x.CustomerName).ToList();

                var today = DateTime.Now.Date;
                var pendingTrialEmployees = payrollEmployees.Where(x => (x.EmployeeStatusId == (int)EmployeeStatus.Active ||
                x.EmployeeStatusId == (int)EmployeeStatus.Candidate) &&
                x.TrialPeriodEndDate > today).ToList();
                model.InTrialPeriodEmployees = pendingTrialEmployees.Select(x => new InTrialPeriodEmployee
                {
                    EmployeeId = x.Id,
                    EmployeeName = x.GetFullName(),
                    TrialPeriodEndDate = x.TrialPeriodEndDate.Value,
                    DaysLeft = (int)(x.TrialPeriodEndDate.Value - today).TotalDays
                }).ToList();
            }

            return View("~/Plugins/Teed.Plugin.Payroll/Views/PayrollEmployee/List.cshtml", model);
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command, int exceptUserId = 0,
            bool bossOfSubordinates = false, bool? discharged = null, int franchiseId = 0)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();

            var jobCatalogData = _jobCatalogService.GetAll().ToList();

            var payrolls = _payrollEmployeeService.GetAll(includeExEmployees: true).ToList();
            if (exceptUserId > 0 || _permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                payrolls = payrolls.Where(x => x.Id != exceptUserId).ToList();
            else if (bossOfSubordinates)
            {
                var currentUserId = _workContext.CurrentCustomer.Id;
                var payrollOfBoss = _payrollEmployeeService.GetByCustomerId(currentUserId);
                if (payrollOfBoss == null)
                    return Ok();
                var subJobsIds = _jobCatalogService.GetAllByParentId(payrollOfBoss.GetCurrentJob()?.Id ?? 0)
                    .Select(x => x.Id).ToList();
                var subordinatesOfBossIds = _payrollEmployeeService.GetAll(includeExEmployees: true).ToList()
                    .Where(x => subJobsIds.Contains(x.GetCurrentJob()?.Id ?? 0))
                    .Select(x => x.Id).ToList();
                payrolls = payrolls.Where(x => subordinatesOfBossIds.Contains(x.Id)).ToList();
            }
            if (discharged != null)
            {
                var onlyShowDischarged = discharged ?? false;
                if (!onlyShowDischarged)
                    payrolls = payrolls.Where(x => x.EmployeeStatusId != (int)EmployeeStatus.Discharged).ToList();
                else
                    payrolls = payrolls.Where(x => x.EmployeeStatusId == (int)EmployeeStatus.Discharged).ToList();
            }
            if (franchiseId > 0)
            {
                payrolls = payrolls.Where(x => x.FranchiseId == franchiseId).ToList();
            }
            payrolls = payrolls.OrderBy(y => y.GetFullName()).ToList();
            var customerIds = payrolls.Select(y => y.CustomerId).ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => customerIds.Contains(x.Id))
                .ToList()
                .Where(x => x.IsInCustomerRole("exemployee") || x.IsInCustomerRole("employee"))
                .ToList();
            var pagedList = new PagedList<PayrollEmployee>(payrolls, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    FullName = x.GetFullName(),
                    LinkedUser = GetLinkedUser(x),
                    JobCatalog = franchiseId > 0 ? x.GetCurrentJob()?.Name : GetJobCatalogTree(x.GetCurrentJob(), jobCatalogData),
                    EmployeeStatus = EnumHelper.GetDisplayName((EmployeeStatus)x.EmployeeStatusId),
                    x.EmployeeStatusId,
                    PendingCount = GetPendingCount(x),
                    Picture = GetProfilePicture(x.PayrollProfilePictureId)
                }).ToList(),
                Total = payrolls.Count()
            };

            return Json(gridModel);
        }

        public LinkedUserModel GetLinkedUser(PayrollEmployee payrollEmployee)
        {
            var model = new LinkedUserModel();
            var customer = _customerService.GetCustomerById(payrollEmployee.CustomerId);
            if (customer != null)
            {
                model.Name = customer.GetFullName();
                model.Id = payrollEmployee.CustomerId;
            }
            else
            {
                model.Name = "Sin especificar";
                model.Id = 0;
            }
            return model;
        }

        public IActionResult GetIncompleteDataCount(int exceptUserId = 0, bool bossOfSubordinates = false)
        {
            var payrolls = _payrollEmployeeService.GetAll()
                .Where(x => x.EmployeeStatusId != (int)EmployeeStatus.Discharged).ToList();
            if (exceptUserId > 0)
                payrolls = payrolls.Where(x => x.Id != exceptUserId).ToList();

            else if (bossOfSubordinates)
            {
                var currentUserId = _workContext.CurrentCustomer.Id;
                var payrollOfBoss = _payrollEmployeeService.GetByCustomerId(currentUserId);
                if (payrollOfBoss == null)
                    return Ok();
                var subJobsIds = _jobCatalogService.GetAllByParentId(payrollOfBoss.GetCurrentJob()?.Id ?? 0)
                    .Select(x => x.Id).ToList();
                var subordinatesOfBossIds = _payrollEmployeeService.GetAll()
                    .ToList()
                    .Where(x => subJobsIds.Contains(x.GetCurrentJob()?.Id ?? 0))
                    .Select(x => x.Id).ToList();
                payrolls = payrolls.Where(x => subordinatesOfBossIds.Contains(x.Id)).ToList();
            }

            var info = GetPendingEmployeesData(payrolls);
            return Ok(new
            {
                incomplete = info.Item1,
                complete = info.Item2,
                porcentage = info.Item3
            });
        }

        [HttpPost]
        public IActionResult ListPaymentsData(int id)
        {

            var payments = _biweeklyPaymentService.GetAllByPayrollEmployeeId(id);
            var gridModel = new DataSourceResult
            {
                Data = payments.Select(x => new
                {
                    Id = x.PayrollEmployeeId,
                    Date = x.Payday.ToString("dd/MM/yyyy")
                }).ToList(),
                Total = payments.Count()
            };

            return Json(gridModel);
        }

        private string GetJobCatalogTree(JobCatalog userJob, List<JobCatalog> jobCatalogs)
        {
            if (userJob != null)
            {
                string value = userJob.Name;
                var currentJob = userJob;
                while (currentJob != null && currentJob.ParentJobId.HasValue && currentJob.ParentJobId > 0)
                {
                    currentJob = jobCatalogs.Where(x => x.Id == currentJob.ParentJobId).FirstOrDefault();
                    if (currentJob != null)
                    {
                        value = currentJob.Name + " >> " + value;
                    }
                }
                return value;
            }
            else
                return "Sin especificar";
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            var model = new CreateOrUpdateModel();
            PrepareModel(model);
            UploadOrGetProfilePicture(model, false);

            return View("~/Plugins/Teed.Plugin.Payroll/Views/PayrollEmployee/Create.cshtml", model);
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            var payrollEmployee = _payrollEmployeeService.GetById(id);
            if (payrollEmployee == null)
                return RedirectToAction("List");

            var model = new CreateOrUpdateModel
            {
                Id = payrollEmployee.Id,
                Address = payrollEmployee.Address,
                Latitude = payrollEmployee.Latitude,
                Longitude = payrollEmployee.Longitude,
                BankType = payrollEmployee.BankTypeId,
                Clabe = payrollEmployee.Clabe,
                AccountNumber = payrollEmployee.AccountNumber,
                Cellphone = payrollEmployee.Cellphone,
                CURP = payrollEmployee.CURP,
                DateOfAdmission = payrollEmployee.DateOfAdmission,
                DateOfBirth = payrollEmployee.DateOfBirth,
                TrialPeriodEndDate = payrollEmployee.TrialPeriodEndDate,
                DateOfDeparture = payrollEmployee.DateOfDeparture,
                SatisfactoryDepartureProcess = payrollEmployee.SatisfactoryDepartureProcess ?? false,
                EmployeeNumber = payrollEmployee.EmployeeNumber,
                FirstNames = payrollEmployee.FirstNames,
                MiddleName = payrollEmployee.MiddleName,
                LastName = payrollEmployee.LastName,
                IMSS = payrollEmployee.IMSS,
                Landline = payrollEmployee.Landline,
                JobCatalogId = 0,
                CustomerId = payrollEmployee.CustomerId,
                ReferenceOneName = payrollEmployee.ReferenceOneName,
                ReferenceOneContact = payrollEmployee.ReferenceOneContact,
                ReferenceTwoName = payrollEmployee.ReferenceTwoName,
                ReferenceTwoContact = payrollEmployee.ReferenceTwoContact,
                ReferenceThreeName = payrollEmployee.ReferenceThreeName,
                ReferenceThreeContact = payrollEmployee.ReferenceThreeContact,
                RFC = payrollEmployee.RFC,
                EmployeeStatus = (EmployeeStatus)payrollEmployee.EmployeeStatusId,
                EmployeeStatusId = payrollEmployee.EmployeeStatusId,
                DepartureReasonType = (DepartureReasonType)(payrollEmployee.DepartureReasonTypeId ?? 0),
                DepartureReasonTypeId = payrollEmployee.DepartureReasonTypeId ?? 0,
                DepartureComment = payrollEmployee.DepartureComment,
                PayrollContractType = (PayrollContractType)(payrollEmployee.PayrollContractTypeId ?? 0),
                PayrollContractTypeId = payrollEmployee.PayrollContractTypeId ?? 0,
                IsParnter = CustomerIsParter(payrollEmployee),
                FranchiseId = payrollEmployee.FranchiseId,
                Log = payrollEmployee.Log
            };
            UploadOrGetProfilePicture(model, false);
            PrepareModel(model);
            model.PendingDataCount = GetPendingCount(payrollEmployee);

            return View("~/Plugins/Teed.Plugin.Payroll/Views/PayrollEmployee/Edit.cshtml", model);
        }

        private Tuple<int, int, decimal> GetPendingEmployeesData(List<PayrollEmployee> payrollEmployees)
        {
            int count = 0;
            int complete = 0;
            int incompleteInfo = 0;
            int completeInfo = 0;
            int totalInfo = 0;
            foreach (var item in payrollEmployees)
            {
                var result = GetPendingCount(item);
                if (result.Files > 0 || result.Info > 0 || result.Salary > 0)
                {
                    count++;
                    incompleteInfo += result.Total;
                }
                else
                {
                    complete++;
                    if (totalInfo < 1)
                        totalInfo = result.TotalCompleted;
                }
                completeInfo += result.TotalCompleted;
            }
            var allInfoCount = payrollEmployees.Count * totalInfo;
            decimal porcentage = 0;
            if (allInfoCount > 0)
                porcentage = Math.Round((decimal)(completeInfo * 100) / (decimal)allInfoCount, 2);
            return new Tuple<int, int, decimal>(count, complete, porcentage);
        }

        private PendingModel GetPendingCount(PayrollEmployee payrollEmployee)
        {
            var pendingFilesCount = 0;
            var pendingInfoCount = 0;
            var pendingSalaryCount = 0;
            var pendingJobCatalogCount = 0;
            var totalCompleted = 0;

            // Check if Partner
            var isPartner = CustomerIsParter(payrollEmployee);

            // Files
            var currentFiles = _payrollEmployeeFileService.GetAll().Where(x => x.PayrollEmployeeId == payrollEmployee.Id).Select(x => x.FileTypeId).Distinct().ToList();
            if (isPartner)
                if (!currentFiles.Contains((int)FileType.SignedContract))
                    currentFiles.Add((int)FileType.SignedContract);

            var files = Enum.GetValues(typeof(FileType)).Cast<FileType>()
                .Where(x => !FileTypes.OptionalFileTypes.Contains(x));
            if (payrollEmployee.PayrollContractTypeId == (int)PayrollContractType.OperatingShareholder ||
                payrollEmployee.PayrollContractTypeId == (int)PayrollContractType.ServiceProvider ||
                payrollEmployee.PayrollContractTypeId == (int)PayrollContractType.Sporadic)
                files = files.Where(x => !FileTypes.ExtraOptionalFileTypes.Contains(x));
            pendingFilesCount = files.Where(x => !currentFiles.Contains((int)x)).Count();
            totalCompleted = files.Where(x => currentFiles.Contains((int)x)).Count();

            if (currentFiles.Where(x => x == (int)FileType.EmploymentTerminationDocument).FirstOrDefault() == 0 &&
                (payrollEmployee.EmployeeStatusId == (int)EmployeeStatus.Discharged ||
                payrollEmployee.DateOfDeparture.HasValue))
            {
                pendingFilesCount++;
            }
            else
                totalCompleted++;

            // Salary
            pendingSalaryCount = _payrollSalaryService.GetAll().Where(x => x.PayrollEmployeeId == payrollEmployee.Id).Count() > 0 ? 0 : 1;
            if (isPartner)
                pendingSalaryCount = 0;
            if (pendingSalaryCount < 1)
                totalCompleted++;

            // Job Catalog
            pendingJobCatalogCount = payrollEmployee.GetCurrentJob() == null ? 1 : 0;

            // Info
            if (string.IsNullOrEmpty(payrollEmployee.FirstNames))
                pendingInfoCount++;
            else
                totalCompleted++;

            if (string.IsNullOrEmpty(payrollEmployee.LastName))
                pendingInfoCount++;
            else
                totalCompleted++;

            if (string.IsNullOrEmpty(payrollEmployee.MiddleName))
                pendingInfoCount++;
            else
                totalCompleted++;

            if (!payrollEmployee.DateOfBirth.HasValue)
                pendingInfoCount++;
            else
                totalCompleted++;

            if (!payrollEmployee.DateOfAdmission.HasValue)
                pendingInfoCount++;
            else
                totalCompleted++;

            if (!payrollEmployee.DateOfDeparture.HasValue && payrollEmployee.EmployeeStatusId == (int)EmployeeStatus.Discharged)
                pendingInfoCount++;
            else
                totalCompleted++;

            if (!(payrollEmployee.SatisfactoryDepartureProcess ?? false) && payrollEmployee.EmployeeStatusId == (int)EmployeeStatus.Discharged)
                pendingInfoCount++;
            else
                totalCompleted++;

            if (string.IsNullOrEmpty(payrollEmployee.Address))
                pendingInfoCount++;
            else
                totalCompleted++;

            if (string.IsNullOrEmpty(payrollEmployee.Cellphone))
                pendingInfoCount++;
            else
                totalCompleted++;

            if (string.IsNullOrEmpty(payrollEmployee.Landline))
                pendingInfoCount++;
            else
                totalCompleted++;

            if (string.IsNullOrEmpty(payrollEmployee.ReferenceOneName))
                pendingInfoCount++;
            else
                totalCompleted++;

            if (string.IsNullOrEmpty(payrollEmployee.ReferenceOneContact))
                pendingInfoCount++;
            else
                totalCompleted++;

            if (string.IsNullOrEmpty(payrollEmployee.ReferenceTwoName))
                pendingInfoCount++;
            else
                totalCompleted++;

            if (string.IsNullOrEmpty(payrollEmployee.ReferenceTwoContact))
                pendingInfoCount++;
            else
                totalCompleted++;

            if (string.IsNullOrEmpty(payrollEmployee.ReferenceThreeName))
                pendingInfoCount++;
            else
                totalCompleted++;

            if (string.IsNullOrEmpty(payrollEmployee.ReferenceThreeContact))
                pendingInfoCount++;
            else
                totalCompleted++;

            if (string.IsNullOrEmpty(payrollEmployee.IMSS))
                pendingInfoCount++;
            else
                totalCompleted++;

            if (string.IsNullOrEmpty(payrollEmployee.RFC))
                pendingInfoCount++;
            else
                totalCompleted++;

            if (string.IsNullOrEmpty(payrollEmployee.CURP))
                pendingInfoCount++;
            else
                totalCompleted++;

            if (string.IsNullOrEmpty(payrollEmployee.Clabe))
                pendingInfoCount++;
            else
                totalCompleted++;

            if (string.IsNullOrEmpty(payrollEmployee.AccountNumber))
                pendingInfoCount++;
            else
                totalCompleted++;

            if (payrollEmployee.EmployeeNumber == 0)
                pendingInfoCount++;
            else
                totalCompleted++;

            if (payrollEmployee.CustomerId == 0)
                pendingInfoCount++;
            else
                totalCompleted++;

            var result = new PendingModel()
            {
                Files = pendingFilesCount,
                Info = pendingInfoCount,
                Salary = pendingSalaryCount,
                JobCatalog = pendingJobCatalogCount,
                Total = pendingFilesCount + pendingInfoCount + pendingSalaryCount + pendingJobCatalogCount,
                TotalCompleted = totalCompleted
            };

            return result;
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(CreateOrUpdateModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();

            if (model.CustomerId > 0)
            {
                var payrollEmployee = new PayrollEmployee
                {
                    Address = model.Address,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    BankTypeId = model.BankType,
                    Clabe = model.Clabe,
                    AccountNumber = model.AccountNumber,
                    Cellphone = model.Cellphone,
                    CURP = model.CURP,
                    DateOfAdmission = model.DateOfAdmission,
                    DateOfBirth = model.DateOfBirth,
                    TrialPeriodEndDate = model.TrialPeriodEndDate,
                    DateOfDeparture = model.DateOfDeparture,
                    SatisfactoryDepartureProcess = model.SatisfactoryDepartureProcess,
                    EmployeeNumber = model.EmployeeNumber,
                    FirstNames = model.FirstNames,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
                    IMSS = model.IMSS,
                    Landline = model.Landline,
                    CustomerId = model.CustomerId,
                    ReferenceOneName = model.ReferenceOneName,
                    ReferenceOneContact = model.ReferenceOneContact,
                    ReferenceTwoName = model.ReferenceTwoName,
                    ReferenceTwoContact = model.ReferenceTwoContact,
                    ReferenceThreeName = model.ReferenceThreeName,
                    ReferenceThreeContact = model.ReferenceThreeContact,
                    PayrollContractTypeId = (int)model.PayrollContractType,
                    RFC = model.RFC,
                    EmployeeStatusId = (int)model.EmployeeStatus,
                    JobCatalogId = _jobCatalogService.GetAll().Where(x => !x.Deleted).OrderByDescending(x => x.Id).FirstOrDefault()?.Id ?? 0,
                    FranchiseId = model.FranchiseId != 0 ? model.FranchiseId : null,
                    Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) creó el expediente.\n"
                };

                //if (model.JobCatalogId > 0)
                //    payrollEmployee.JobCatalogId = model.JobCatalogId;
                //else
                //    payrollEmployee.JobCatalogId = null;

                _payrollEmployeeService.Insert(payrollEmployee);
                UploadOrGetProfilePicture(model, true);

                if (continueEditing)
                {
                    return RedirectToAction("Edit", new { id = payrollEmployee.Id });
                }
            }
            else
                ModelState.AddModelError("", "Necesitas colocar un usuario ligado al expediente para poder guardarlo.");

            if (ModelState.ErrorCount > 0)
            {
                PrepareModel(model);
                UploadOrGetProfilePicture(model, false);
                return View("~/Plugins/Teed.Plugin.Payroll/Views/PayrollEmployee/Create.cshtml", model);
            }

            return RedirectToAction("List");
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(CreateOrUpdateModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            var payrollEmployee = _payrollEmployeeService.GetById(model.Id);
            if (payrollEmployee == null)
                return RedirectToAction("List");

            if (payrollEmployee.Address != model.Address)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"la dirección de \"{payrollEmployee.Address ?? ""}\" a \"{model.Address ?? null}\".\n";
                payrollEmployee.Address = model.Address;
            }

            payrollEmployee.Latitude = model.Latitude;
            payrollEmployee.Longitude = model.Longitude;

            if (payrollEmployee.BankTypeId != model.BankType)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el tipo de banco de {((BankType)payrollEmployee.BankTypeId).GetDisplayName()} a {((BankType)model.BankType).GetDisplayName()}.\n";
                payrollEmployee.BankTypeId = model.BankType;
            }

            if (payrollEmployee.Clabe != model.Clabe)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"la CLABE de \"{payrollEmployee.Clabe ?? ""}\" a \"{model.Clabe ?? ""}\".\n";
                payrollEmployee.Clabe = model.Clabe;
            }

            if (payrollEmployee.AccountNumber != model.AccountNumber)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el número de cuenta de \"{payrollEmployee.AccountNumber ?? ""}\" a \"{model.AccountNumber ?? ""}\".\n";
                payrollEmployee.AccountNumber = model.AccountNumber;
            }

            if (payrollEmployee.Cellphone != model.Cellphone)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el número de celular de \"{payrollEmployee.Cellphone ?? ""}\" a \"{model.Cellphone ?? ""}\".\n";
                payrollEmployee.Cellphone = model.Cellphone;
            }

            if (payrollEmployee.CURP != model.CURP)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"la CURP de \"{payrollEmployee.CURP ?? ""}\" a \"{model.CURP ?? ""}\".\n";
                payrollEmployee.CURP = model.CURP;
            }

            if (payrollEmployee.DateOfAdmission != model.DateOfAdmission)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"la fecha de ingreso de {(payrollEmployee.DateOfAdmission != null ? payrollEmployee.DateOfAdmission.Value.ToString("dd/MM/yyyy)") : "vacío")} a {(model.DateOfAdmission != null ? model.DateOfAdmission.Value.ToString("dd/MM/yyyy)") : "vacío")}.\n";
                payrollEmployee.DateOfAdmission = model.DateOfAdmission;
            }

            if (payrollEmployee.DateOfBirth != model.DateOfBirth)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"la fecha de nacimiento de {(payrollEmployee.DateOfBirth != null ? payrollEmployee.DateOfBirth.Value.ToString("dd/MM/yyyy)") : "vacío")} a {(model.DateOfBirth != null ? model.DateOfBirth.Value.ToString("dd/MM/yyyy)") : "vacío")}.\n";
                payrollEmployee.DateOfBirth = model.DateOfBirth;
            }

            if (payrollEmployee.DateOfDeparture != model.DateOfDeparture)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"la fecha de salida de {(payrollEmployee.DateOfDeparture != null ? payrollEmployee.DateOfDeparture.Value.ToString("dd/MM/yyyy)") : "vacío")} a {(model.DateOfDeparture != null ? model.DateOfDeparture.Value.ToString("dd/MM/yyyy)") : "vacío")}.\n";
                payrollEmployee.DateOfDeparture = model.DateOfDeparture;
            }

            if (payrollEmployee.TrialPeriodEndDate != model.TrialPeriodEndDate)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"la fecha de fin de periodo de prueba de {(payrollEmployee.TrialPeriodEndDate != null ? payrollEmployee.TrialPeriodEndDate.Value.ToString("dd/MM/yyyy)") : "vacío")} a {(model.TrialPeriodEndDate != null ? model.TrialPeriodEndDate.Value.ToString("dd/MM/yyyy)") : "vacío")}.\n";
                payrollEmployee.TrialPeriodEndDate = model.TrialPeriodEndDate;
            }

            if (payrollEmployee.EmployeeNumber != model.EmployeeNumber)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el número de empleado de \"{payrollEmployee.EmployeeNumber}\" a \"{model.EmployeeNumber}\".\n";
                payrollEmployee.EmployeeNumber = model.EmployeeNumber;
            }

            if (payrollEmployee.FirstNames != model.FirstNames)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"los nombres de \"{payrollEmployee.FirstNames ?? ""}\" a \"{model.FirstNames ?? ""}\".\n";
                payrollEmployee.FirstNames = model.FirstNames;
            }

            if (payrollEmployee.MiddleName != model.MiddleName)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el apellido paterno de \"{payrollEmployee.MiddleName ?? ""}\" a \"{model.MiddleName ?? ""}\".\n";
                payrollEmployee.MiddleName = model.MiddleName;
            }

            if (payrollEmployee.LastName != model.LastName)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el apellido materno de \"{payrollEmployee.LastName ?? ""}\" a \"{model.LastName ?? ""}\".\n";
                payrollEmployee.LastName = model.LastName;
            }

            if (payrollEmployee.IMSS != model.IMSS)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el IMSS de \"{payrollEmployee.IMSS ?? ""}\" a \"{model.IMSS ?? ""}\".\n";
                payrollEmployee.IMSS = model.IMSS;
            }

            if (payrollEmployee.Landline != model.Landline)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el teléfono fijo de \"{payrollEmployee.Landline ?? ""}\" a \"{model.Landline ?? ""}\".\n";
                payrollEmployee.Landline = model.Landline;
            }

            if (payrollEmployee.SatisfactoryDepartureProcess != model.SatisfactoryDepartureProcess)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el proceso de baja satisfactorio de \"{((payrollEmployee.SatisfactoryDepartureProcess ?? false) ? "Si" : "No")}\" a \"{(model.SatisfactoryDepartureProcess ? "Si" : "No")}\".\n";
                payrollEmployee.SatisfactoryDepartureProcess = model.SatisfactoryDepartureProcess;
            }

            //if (payrollEmployee.JobCatalogId != model.JobCatalogId)
            //{
            //    var pastJob = _jobCatalogService.GetById(payrollEmployee.JobCatalogId ?? 0);
            //    var newJob = _jobCatalogService.GetById(model.JobCatalogId);
            //    payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
            //        $"el empleo de \"{pastJob?.Name ?? "ninguno"}\" a \"{newJob?.Name ?? "ninguno"}\".\n";
            //    if (model.JobCatalogId > 0)
            //        payrollEmployee.JobCatalogId = model.JobCatalogId;
            //    else
            //        payrollEmployee.JobCatalogId = null;
            //}

            if (payrollEmployee.ReferenceOneName != model.ReferenceOneName)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el nómbre de la referencia uno de \"{payrollEmployee.ReferenceOneName ?? ""}\" a \"{model.ReferenceOneName ?? ""}\".\n";
                payrollEmployee.ReferenceOneName = model.ReferenceOneName;
            }

            if (payrollEmployee.ReferenceOneContact != model.ReferenceOneContact)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"la forma de contacto de la referencia uno de \"{payrollEmployee.ReferenceOneContact ?? ""}\" a \"{model.ReferenceOneContact ?? ""}\".\n";
                payrollEmployee.ReferenceOneContact = model.ReferenceOneContact;
            }

            if (payrollEmployee.ReferenceTwoName != model.ReferenceTwoName)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el nómbre de la referencia dos de \"{payrollEmployee.ReferenceTwoName ?? ""}\" a \"{model.ReferenceTwoName ?? ""}\".\n";
                payrollEmployee.ReferenceTwoName = model.ReferenceTwoName;
            }

            if (payrollEmployee.ReferenceTwoContact != model.ReferenceTwoContact)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"la forma de contacto de la referencia dos de \"{payrollEmployee.ReferenceTwoContact ?? ""}\" a \"{model.ReferenceTwoContact ?? ""}\".\n";
                payrollEmployee.ReferenceTwoContact = model.ReferenceTwoContact;
            }

            if (payrollEmployee.ReferenceThreeName != model.ReferenceThreeName)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el nómbre de la referencia tres de \"{payrollEmployee.ReferenceThreeName ?? ""}\" a \"{model.ReferenceThreeName ?? ""}\".\n";
                payrollEmployee.ReferenceThreeName = model.ReferenceThreeName;
            }

            if (payrollEmployee.ReferenceThreeContact != model.ReferenceThreeContact)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"la forma de contacto de la referencia tres de \"{payrollEmployee.ReferenceThreeContact ?? ""}\" a \"{model.ReferenceThreeContact ?? ""}\".\n";
                payrollEmployee.ReferenceThreeContact = model.ReferenceThreeContact;
            }

            if (payrollEmployee.RFC != model.RFC)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el RFC de \"{payrollEmployee.RFC ?? ""}\" a \"{model.RFC ?? ""}\".\n";
                payrollEmployee.RFC = model.RFC;
            }

            if (payrollEmployee.CustomerId != model.CustomerId)
            {
                var pastCustomer = _customerService.GetCustomerById(payrollEmployee.CustomerId);
                var newCustomer = _customerService.GetCustomerById(model.CustomerId);
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el usuario ligado al expediente de \"{(string.IsNullOrEmpty(pastCustomer?.GetFullName()) ? "ninguno" : pastCustomer?.GetFullName())} {(pastCustomer != null ? $"({pastCustomer.Email})" : "")}\" a \"{(string.IsNullOrEmpty(newCustomer?.GetFullName()) ? "ninguno" : newCustomer?.GetFullName())} {(newCustomer != null ? $"({newCustomer.Email})" : "")}\".\n";
                payrollEmployee.CustomerId = model.CustomerId;
            }

            if (payrollEmployee.EmployeeStatusId != (int)model.EmployeeStatus)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el estatus del empleado de {((EmployeeStatus)payrollEmployee.EmployeeStatusId).GetDisplayName()} a {model.EmployeeStatus.GetDisplayName()}.\n";
                payrollEmployee.EmployeeStatusId = (int)model.EmployeeStatus;
            }

            if (payrollEmployee.DepartureReasonTypeId != (int)model.DepartureReasonType)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el motivo de baja de {(payrollEmployee.DepartureReasonTypeId != null ? ((DepartureReasonType)payrollEmployee.DepartureReasonTypeId).GetDisplayName() : "Ninguno")} a {model.DepartureReasonType.GetDisplayName()}.\n";
                payrollEmployee.DepartureReasonTypeId = (int)model.DepartureReasonType;
            }

            if (payrollEmployee.DepartureComment != model.DepartureComment)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el comentarios de la baja de \"{payrollEmployee.DepartureComment ?? ""}\" a \"{model.DepartureComment ?? ""}\".\n";
                payrollEmployee.DepartureComment = model.DepartureComment;
            }

            if (payrollEmployee.PayrollContractTypeId != (int)model.PayrollContractType)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                    $"el tipo de contrato de {(payrollEmployee.PayrollContractTypeId != null ? ((PayrollContractType)payrollEmployee.PayrollContractTypeId).GetDisplayName() : "Ninguno")} a {model.PayrollContractType.GetDisplayName()}.\n";
                payrollEmployee.PayrollContractTypeId = (int)model.PayrollContractType;
            }

            model.FranchiseId = model.FranchiseId != 0 ? model.FranchiseId : null;
            if (payrollEmployee.FranchiseId != model.FranchiseId)
            {
                using (HttpClient client = new HttpClient())
                {
                    var url =
                        //_storeContext.CurrentStore.SecureUrl
                        "https://localhost:44387/"
                        ;
                    var result = client.GetAsync(url + "/Admin/Franchise/GetFranchisesFromExternal").Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var resultJson = result.Content.ReadAsStringAsync().Result;
                        List<FranchisePayrollInfo> results = JsonConvert.DeserializeObject<List<FranchisePayrollInfo>>(resultJson);
                        model.Franchises = new SelectList(results, "Id", "Name");
                    }
                }

                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió " +
                     $"la franquicia a la que pertenece el empleado de " +
                     $"\"{(payrollEmployee.FranchiseId != null ? model.Franchises.Where(x => x.Value == payrollEmployee.FranchiseId?.ToString()).FirstOrDefault()?.Text : "No aplica/No es empleado de franquicia")}\" a " +
                     $"\"{(model.FranchiseId != null ? model.Franchises.Where(x => x.Value == model.FranchiseId?.ToString()).FirstOrDefault()?.Text : "No aplica/No es empleado de franquicia")}\".\n";
                payrollEmployee.FranchiseId = model.FranchiseId;
            }

            _payrollEmployeeService.Update(payrollEmployee);
            UploadOrGetProfilePicture(model, true);

            if (continueEditing)
            {
                return RedirectToAction("Edit", new { id = payrollEmployee.Id });
            }
            return RedirectToAction("List");
        }

        public List<PayrollEmployeeFile> FilesLoad(int payrollId, bool emptyBytes = false)
        {
            var payrollEmployee = _payrollEmployeeService.GetById(payrollId);
            var files = new List<PayrollEmployeeFile>();
            var currentFiles = new List<int>();
            if (!emptyBytes)
            {
                files = _payrollEmployeeFileService.GetByPayrollEmployeeId(payrollId);
                currentFiles = files.Select(x => x.FileTypeId).Distinct().ToList();
            }
            else
            {
                var filesQuery = _payrollEmployeeFileService.GetAll()
                    .Where(x => x.PayrollEmployeeId == payrollId && !x.Deleted);
                files = filesQuery.Select(x => new
                {
                    x.CreatedOnUtc,
                    x.Deleted,
                    x.Description,
                    x.Extension,
                    x.FileMimeType,
                    x.FileTypeId,
                    x.Id,
                    x.PayrollEmployeeId,
                    x.Size,
                    x.Title,
                    x.UpdatedOnUtc
                }).ToList()
                .Select(x => new PayrollEmployeeFile
                {
                    CreatedOnUtc = x.CreatedOnUtc,
                    Deleted = x.Deleted,
                    Description = x.Description,
                    Extension = x.Extension,
                    FileMimeType = x.FileMimeType,
                    FileTypeId = x.FileTypeId,
                    Id = x.Id,
                    PayrollEmployeeId = x.PayrollEmployeeId,
                    Size = x.Size,
                    Title = x.Title,
                    UpdatedOnUtc = x.UpdatedOnUtc
                }).ToList();
            }

            currentFiles = files.Select(x => x.FileTypeId).Distinct().ToList();
            var pendingFiles = Enum.GetValues(typeof(FileType)).Cast<FileType>()
                .Where(x => !FileTypes.OptionalFileTypes.Contains(x)
                && !currentFiles.Contains((int)x)).ToList();

            if (payrollEmployee.PayrollContractTypeId == (int)PayrollContractType.OperatingShareholder ||
                    payrollEmployee.PayrollContractTypeId == (int)PayrollContractType.ServiceProvider ||
                    payrollEmployee.PayrollContractTypeId == (int)PayrollContractType.Sporadic)
            {
                pendingFiles = pendingFiles.Where(x => !FileTypes.ExtraOptionalFileTypes.Contains(x)).ToList();
            }

            foreach (var file in pendingFiles)
            {
                files.Insert(0, new PayrollEmployeeFile
                {
                    Id = 0,
                    FileTypeId = (int)file
                });
            }

            if (currentFiles.Where(x => x == (int)FileType.EmploymentTerminationDocument).FirstOrDefault() == 0 &&
                (payrollEmployee.EmployeeStatusId == (int)EmployeeStatus.Discharged ||
                payrollEmployee.DateOfDeparture.HasValue))
            {
                files.Insert(0, new PayrollEmployeeFile
                {
                    Id = 0,
                    FileTypeId = (int)FileType.EmploymentTerminationDocument
                });
            }

            if (CustomerIsParter(payrollEmployee))
                files = files.Where(x => x.FileTypeId != (int)FileType.SignedContract).ToList();

            return files;
        }

        [HttpPost]
        public IActionResult EmployeeFileList(int payrollId)
        {
            //PayrollEmployee payrollEmployee = _payrollEmployeeService.GetById(payrollId);

            //var files = _payrollEmployeeFileService.GetByPayrollEmployeeId(payrollId);
            //var currentFiles = files.Select(x => x.FileTypeId).Distinct().ToList();
            //var pendingFiles = Enum.GetValues(typeof(FileType)).Cast<FileType>()
            //    .Where(x => x != FileType.EmploymentTerminationDocument && x != FileType.ProfilePicture && x != FileType.SocioeconomicStudy
            //    && !currentFiles.Contains((int)x)).ToList();

            //foreach (var file in pendingFiles)
            //{
            //    files.Insert(0, new PayrollEmployeeFile
            //    {
            //        Id = 0,
            //        FileTypeId = (int)file
            //    });
            //}

            //if (currentFiles.Where(x => x == (int)FileType.EmploymentTerminationDocument).FirstOrDefault() == 0 &&
            //    (payrollEmployee.EmployeeStatusId == (int)EmployeeStatus.Discharged ||
            //    payrollEmployee.DateOfDeparture.HasValue))
            //{
            //    files.Insert(0, new PayrollEmployeeFile
            //    {
            //        Id = 0,
            //        FileTypeId = (int)FileType.EmploymentTerminationDocument
            //    });
            //}

            //if (CustomerIsParter(payrollEmployee))
            //    files = files.Where(x => x.FileTypeId != (int)FileType.SignedContract).ToList();

            var files = FilesLoad(payrollId, true);

            var gridModel = new DataSourceResult
            {
                Data = files.Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Description,
                    FileType = x.FileTypeId == 0 ? "0" : ((FileType)x.FileTypeId).GetDisplayName(),
                    //Thumbnail = x.File
                }).ToList(),
                Total = files.Count()
            };

            return Json(gridModel);
        }

        [HttpGet]
        public IActionResult EmployeeFileGet(int id)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee) ||
                id == 0)
                return BadRequest();

            var file = _payrollEmployeeFileService.GetById(id);
            if (file != null)
                return Ok(file.File);
            else
                return BadRequest();
        }

        [HttpPost]
        public IActionResult EmployeeFileAdd(PayrollEmployeeFileModel model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            try
            {
                byte[] bytes = new byte[0];
                using (var ms = new MemoryStream())
                {
                    model.File.CopyTo(ms);
                    bytes = ms.ToArray();
                }
                var file = new PayrollEmployeeFile
                {
                    File = bytes,
                    Title = model.Title,
                    Description = model.Description,
                    Extension = model.File.FileName.Substring(model.File.FileName.LastIndexOf(".") + 1),
                    FileMimeType = model.File.ContentType,
                    FileTypeId = (int)model.FileType,
                    Size = (int)model.File.Length,
                    PayrollEmployeeId = model.PayrollEmployeeId,
                };
                _payrollEmployeeFileService.Insert(file);
                var payrollEmployee = _payrollEmployeeService.GetById(model.PayrollEmployeeId);
                if (payrollEmployee != null)
                {
                    payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) subío un archivo de tipo {model.FileType.GetDisplayName()} al expediente.\n";
                    _payrollEmployeeService.Update(payrollEmployee);
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        public IActionResult EmployeeFileUpdate(PayrollEmployeeFileModel model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            var file = _payrollEmployeeFileService.GetById(model.Id);
            if (file == null)
                return Ok();

            file.Title = model.Title;
            file.Description = model.Description;
            _payrollEmployeeFileService.Update(file);
            var payrollEmployee = _payrollEmployeeService.GetById(model.PayrollEmployeeId);
            if (payrollEmployee != null)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) actualizó un archivo de tipo {model.FileType.GetDisplayName()} ({file.Id}) del expediente.\n";
                _payrollEmployeeService.Update(payrollEmployee);
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult EmployeeFileDelete(PayrollEmployeeFileModel model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            var file = _payrollEmployeeFileService.GetById(model.Id);
            if (file == null)
                return Ok();

            file.Deleted = true;
            _payrollEmployeeFileService.Update(file);
            var payrollEmployee = _payrollEmployeeService.GetById(model.PayrollEmployeeId);
            if (payrollEmployee != null)
            {
                payrollEmployee.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) eliminó un archivo de tipo {model.FileType.GetDisplayName()} ({file.Id}) del expediente.\n";
                _payrollEmployeeService.Update(payrollEmployee);
            }

            return Ok();
        }

        [HttpGet]
        public IActionResult EmployeeFileDownload(int id)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            var file = _payrollEmployeeFileService.GetById(id);
            if (file == null)
                return Ok();
            var model = new PayrollEmployeeFileModel
            {
                Title = ((FileType)file.FileTypeId).GetDisplayName(),
                Extension = file.Extension,
                FileArray = file.File

            };
            return Json(model);
        }

        [HttpPost]
        public IActionResult EmployeeSalaryList(int payrollId)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            var salaries = _payrollSalaryService.GetByPayrollEmployeeId(payrollId)
                .OrderByDescending(x => x.ApplyDate).ToList();

            var employee = _payrollEmployeeService.GetById(payrollId);
            var currentSalary = employee.GetCurrentSalary();
            var gridModel = new DataSourceResult
            {
                Data = salaries.Select(x => new
                {
                    x.Id,
                    ApplyDate = x.ApplyDate?.ToString("dd/MM/yyyy"),
                    x.Bonds,
                    x.NetIncome,
                    x.SocialSecurityContributions,
                    x.WithheldISR,
                    IntegratedDailyWage = x.NetIncome > 0 ? decimal.Round(x.NetIncome / (decimal)30.42, 2) : 0,
                    Benefits = GetBenefitsTotal(x),
                    GrossSalary = GetGrossSalary(x),
                    IsCurrent = (currentSalary?.Id ?? 0) == x.Id
                }).ToList(),
                Total = salaries.Count()
            };

            return Json(gridModel);
        }

        public decimal GetBenefitsTotal(PayrollSalary salary)
        {
            var benefits = _benefitService.GetAllBySalaryOrEmployeeId(salary.Id, true);
            return benefits.Select(x => x.Amount).DefaultIfEmpty().Sum();
        }

        public string GetGrossSalary(PayrollSalary salary)
        {
            return Math.Round(
                salary.NetIncome + salary.WithheldISR + salary.SocialSecurityContributions +
                salary.Bonds + salary.IntegratedDailyWage
                , 2).ToString("C");
        }

        [HttpPost]
        public IActionResult EmployeeSalaryAdd(PayrollSalaryModel model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            try
            {
                var salary = new PayrollSalary
                {
                    IntegratedDailyWage = model.IntegratedDailyWage,
                    ApplyDate = model.ApplyDate,
                    //Bonds = model.Bonds,
                    NetIncome = model.NetIncome,
                    //SocialSecurityContributions = model.SocialSecurityContributions,
                    //WithheldISR = model.WithheldISR,
                    PayrollEmployeeId = model.PayrollEmployeeId,
                };
                _payrollSalaryService.Insert(salary);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        public IActionResult EmployeeSalaryDelete(PayrollSalaryModel model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            var salary = _payrollSalaryService.GetById(model.Id);
            if (salary == null)
                return Ok();

            salary.Deleted = true;
            _payrollSalaryService.Update(salary);
            return Ok();
        }

        [HttpPost]
        public IActionResult EmployeeSalaryUpdate(PayrollSalaryModel model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            var salary = _payrollSalaryService.GetById(model.Id);
            if (salary == null)
                return Ok();
            salary.NetIncome = model.NetIncome;
            //salary.Bonds = model.Bonds;
            //salary.SocialSecurityContributions = model.SocialSecurityContributions;
            //salary.WithheldISR = model.WithheldISR;
            if (!string.IsNullOrEmpty(model.ApplyDateString))
            {
                var dateInfo = model.ApplyDateString.Split(' ').ToList();
                var parsedDate = DateTime.ParseExact($"{dateInfo[2]}/{dateInfo[1]}/{dateInfo[3]}", "dd/MMM/yyyy", CultureInfo.InvariantCulture);
                salary.ApplyDate = parsedDate;
            }
            _payrollSalaryService.Update(salary);
            return Ok();
        }

        [HttpPost]
        public IActionResult BenefitList(int Id, bool ForSalary)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            var benefits = _benefitService.GetAllBySalaryOrEmployeeId(Id, ForSalary);
            var gridModel = new DataSourceResult
            {
                Data = benefits.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description,
                    x.Amount
                }).ToList(),
                Total = benefits.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult BenefitAdd(AddBenefit model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            try
            {
                var benefit = new Benefit
                {
                    Name = model.Name,
                    Description = model.Description,
                    Amount = model.Amount,
                    IsForSalary = model.isForSalary,
                    SalaryOrEmployeeId = model.LinkId,
                };
                _benefitService.Insert(benefit);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        public IActionResult BenefitDelete(AddBenefit model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            var benefit = _benefitService.GetById(model.Id);
            if (benefit == null)
                return Ok();

            benefit.Deleted = true;
            _benefitService.Update(benefit);
            return Ok();
        }

        [HttpPost]
        public IActionResult EmployeeVacationList(int payrollId)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            var vacations = _assistanceOverrideService.GetAllByEmployeeId(payrollId)
                .Where(x => x.Type == (int)AssistanceType.Vacation).ToList();
            var gridModel = new DataSourceResult
            {
                Data = vacations.Select(x => new
                {
                    x.Id,
                    VacationDate = x.OverriddenDate.ToString("dd/MM/yyyy"),
                    x.Comment,
                }).ToList(),
                Total = vacations.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult EmployeeVacationAdd(AddEmployeeVacation model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            try
            {
                var payroll = _payrollEmployeeService.GetById(model.PayrollEmployeeId);
                if (payroll != null)
                {
                    payroll.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) agregó " +
                    $"la fecha de vacación tomada con fecha {model.VacationDate.Value:dd/MM/yyyy} y comentario \"{model.Comment ?? ""}\".\n";
                    var vacation = new TakenVacationDay
                    {
                        PayrollId = model.PayrollEmployeeId,
                        VacationDate = model.VacationDate.Value,
                        Comment = model.Comment
                    };
                    _takenVacationDayService.Insert(vacation);
                    _payrollEmployeeService.Update(payroll);
                }
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        public IActionResult EmployeeVacationDelete(AddEmployeeVacation model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();

            var vacation = _takenVacationDayService.GetById(model.Id);
            if (vacation == null)
                return Ok();
            vacation.Deleted = true;
            _takenVacationDayService.Update(vacation);

            var payroll = _payrollEmployeeService.GetById(vacation.PayrollId);
            if (payroll != null)
            {
                payroll.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) eliminó " +
                $"la fecha de vacación tomada con fecha {model.VacationDate.Value:dd/MM/yyyy} y comentario \"{model.Comment ?? ""}\".\n";

                _payrollEmployeeService.Update(payroll);
            }
            return Ok();
        }

        //[HttpPost]
        //public IActionResult SaveSubordinates(SaveSubordinatesData model)
        //{
        //    var currentSubordinates = _subordinateService.GetAllByBossId(model.BossId);
        //    if (currentSubordinates.Any())
        //    {
        //        var currentIds = currentSubordinates.Select(x => x.PayrollSubordinateId).ToList();
        //        if (model.SubordinateIds == null)
        //            model.SubordinateIds = new int[0];
        //        var toDelete = currentIds.Except(model.SubordinateIds).ToList();
        //        foreach (var subordinateId in toDelete)
        //        {
        //            var current = currentSubordinates.Where(x => x.PayrollSubordinateId == subordinateId).FirstOrDefault();
        //            _subordinateService.SetAsDeleted(current);
        //        }
        //        var toInsert = model.SubordinateIds.Except(currentIds).Where(x => x > 0).ToList();
        //        foreach (var subordinateId in toInsert)
        //        {
        //            _subordinateService.Insert(new Subordinate
        //            {
        //                BossId = model.BossId,
        //                PayrollSubordinateId = subordinateId
        //            });
        //        }
        //    }
        //    else
        //    {
        //        foreach (var subordinateId in model.SubordinateIds)
        //        {
        //            _subordinateService.Insert(new Subordinate
        //            {
        //                BossId = model.BossId,
        //                PayrollSubordinateId = subordinateId
        //            });
        //        }
        //    }

        //    return Ok();
        //}

        [HttpGet]
        public FileResult DownloadZip(int payrollId)
        {
            var files = FilesLoad(payrollId);
            using (ZipFile zip = new ZipFile())
            {
                foreach (var filezip in files.Where(x => x.Id > 0))
                {
                    zip.AddEntry((filezip.FileTypeId == 0 ? "0" : ((FileType)filezip.FileTypeId).GetDisplayName()) + "_" + filezip.UpdatedOnUtc.ToLocalTime().ToString("dd-MM-yyyy-hh-mm-ss") + '.' + filezip.Extension, filezip.File);
                }
                var fileNameZip = "Documentos_" + files.Where(x => x.PayrollEmployee != null).FirstOrDefault().PayrollEmployee.GetFullName() + "_" + DateTime.Now.ToString("dd-MM-yyyy");
                using (MemoryStream output = new MemoryStream())
                {
                    zip.Save(output);
                    return File(output.ToArray(), "application/zip", fileNameZip + ".zip");
                }
            }

        }

        [HttpGet]
        public IActionResult PayrollReport(string from, string to)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();

            if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
            {
                var fromDate = DateTime.ParseExact(from, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;
                var toDate = DateTime.ParseExact(to, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;
                var payrolls = _payrollEmployeeService.GetAll(includeExEmployees: true)
                    .Where(x => !x.Deleted)
                    .ToList()
                    .Where(x => fromDate <= x.CreatedOnUtc.ToLocalTime().Date &&
                    x.CreatedOnUtc.ToLocalTime().Date <= toDate)
                    .ToList();
                var customerIds = payrolls.Select(x => x.CustomerId).ToList();
                var customers = _customerService.GetAllCustomersQuery()
                    .Where(x => customerIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.Email })
                    .ToList();

                using (var stream = new MemoryStream())
                {
                    using (var xlPackage = new ExcelPackage(stream))
                    {
                        var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                        int row = 1;

                        worksheet.Cells[row, 1].Value = "Nombre completo del trabajador";
                        worksheet.Cells[row, 2].Value = "Fehca de alta";
                        worksheet.Cells[row, 3].Value = "Fehca de baja";
                        worksheet.Cells[row, 4].Value = "Numero de seguridad social";
                        worksheet.Cells[row, 5].Value = "CURP";
                        worksheet.Cells[row, 6].Value = "RFC";
                        worksheet.Cells[row, 7].Value = "Correo electrónico";
                        row++;

                        foreach (var payroll in payrolls)
                        {
                            var customer = customers.Where(x => x.Id == payroll.CustomerId).FirstOrDefault();
                            worksheet.Cells[row, 1].Value = payroll.GetFullName();
                            worksheet.Cells[row, 2].Value = payroll.DateOfAdmission == null ? DateTime.MinValue : payroll.DateOfAdmission.Value;
                            worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[row, 3].Value = payroll.DateOfDeparture == null ? DateTime.MinValue : payroll.DateOfDeparture.Value;
                            worksheet.Cells[row, 3].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells[row, 4].Value = payroll.IMSS;
                            worksheet.Cells[row, 5].Value = payroll.CURP;
                            worksheet.Cells[row, 6].Value = payroll.RFC;
                            worksheet.Cells[row, 7].Value = customer?.Email ?? "Usuario vinculado no encontrado";
                            row++;
                        }

                        for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                        {
                            worksheet.Column(i).AutoFit();
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }

                        xlPackage.Save();
                    }

                    return File(stream.ToArray(), MimeTypes.TextXlsx, $"Reporte expedientes nomina del " + fromDate.ToString("dd-MM-yyyy") + " al " + toDate.ToString("dd-MM-yyyy") + ".xlsx");
                }
            }
            return Ok();
        }

        [HttpGet]
        [Route("Admin/Customer/[action]/{CustomerId}/{employeeNumber}")]
        public IActionResult UpdateEmployeeNumber(int CustomerId, int employeeNumber)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return BadRequest();

            if (CustomerId > 0 && employeeNumber > 0)
            {
                var existingPayroll = _payrollEmployeeService.GetAll(onlyEmployees: false)
                    .Where(x => x.CustomerId == CustomerId)
                    .OrderByDescending(x => x.CreatedOnUtc)
                    .FirstOrDefault();
                if (existingPayroll == null)
                {
                    var deliveryJob = _jobCatalogService.GetAll()
                        .Where(x => x.Name.Trim().ToLower() == "staff de reparto")
                        .FirstOrDefault();
                    if (deliveryJob == null)
                        return BadRequest();
                    var payrollEmployee = new PayrollEmployee
                    {
                        CustomerId = CustomerId,
                        EmployeeNumber = employeeNumber,
                        JobCatalogId = deliveryJob.Id,
                        EmployeeStatusId = (int)EmployeeStatus.Candidate,
                        BankTypeId = (int)BankType.Other,
                        Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) creó el expediente fantasma.\n"
                    };
                    _payrollEmployeeService.Insert(payrollEmployee);
                    _payrollEmployeeJobService.Insert(new PayrollEmployeeJob
                    {
                        JobCatalogId = deliveryJob.Id,
                        PayrollEmployeeId = payrollEmployee.Id,
                        ApplyDate = new DateTime(1990, 1, 1).Date,
                        Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - Se insertó de forma automática la conexión empleado-empleo por el usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) para el expediente fantasma.\n"
                    });
                }
                else if (existingPayroll.EmployeeNumber != employeeNumber)
                {
                    existingPayroll.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el número de empleado de {existingPayroll.EmployeeNumber} a {employeeNumber}.\n";
                    existingPayroll.EmployeeNumber = employeeNumber;
                    _payrollEmployeeService.Update(existingPayroll);
                }
            }

            return Ok();
        }

        [HttpGet]
        public IActionResult ActivePayrollEmployees(string forBiweek = null)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return BadRequest();

            var today = DateTime.UtcNow;
            var noneColor = Color.FromArgb(255, 255, 255);
            var departureColor = Color.FromArgb(0, 185, 72);
            var delayColor = Color.FromArgb(173, 0, 238);
            var abscenseColor = Color.FromArgb(246, 248, 245);
            var onTimeColor = Color.FromArgb(253, 89, 0);
            var payedDisabilityColor = Color.FromArgb(100, 149, 237);
            var subsidizedDisabilityColor = Color.FromArgb(0, 0, 205);
            var borderColor = Color.FromArgb(213, 213, 213);
            var vacationColor = Color.FromArgb(160, 0, 160);

            var existingPayroll = new List<PayrollEmployee>();
            if (string.IsNullOrEmpty(forBiweek))
                existingPayroll = _payrollEmployeeService.GetAll()
                    .Where(x => x.EmployeeStatusId == (int)EmployeeStatus.Active)
                    .ToList()
                    .OrderBy(x => x.GetFullName())
                    .ToList();
            else
                existingPayroll = _payrollEmployeeService.GetAll(includeExEmployees: true)
                    .ToList()
                    .OrderBy(x => x.GetFullName())
                    .ToList();

            //// extra work
            //var startDate = new DateTime(2022, 8, 1).Date;
            //var endDate = new DateTime(2022, 8, 31).Date;
            //existingPayroll = _payrollEmployeeService.GetAll(includeExEmployees: true)
            //        .ToList()
            //        .OrderBy(x => x.GetFullName())
            //        .ToList();
            //existingPayroll = existingPayroll.Where(x => (startDate <= x.DateOfAdmission && x.DateOfAdmission <= endDate) ||
            //    (startDate <= x.DateOfDeparture && x.DateOfDeparture <= endDate)).ToList();
            ////

            var payrollIds = existingPayroll.Select(x => x.Id).ToList();
            var customerIds = existingPayroll.Select(x => x.CustomerId).ToList();
            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => customerIds.Contains(x.Id)).ToList();

            var salaries = _payrollSalaryService.GetAll()
                .Where(x => payrollIds.Contains(x.PayrollEmployeeId) && !x.Deleted)
                .ToList();

            var vacations = _assistanceOverrideService.GetAll()
                .Where(x => x.Type == (int)AssistanceType.Vacation &&
                    payrollIds.Contains(x.PayrollEmployeeId))
                .ToList();

            var datesBetween = new List<DateTime>();
            var naturalWorkDays = 0;
            if (!string.IsNullOrEmpty(forBiweek))
            {
                var parsedDate = DateTime.ParseExact(forBiweek, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var festiveDates = _assistanceService.GetFestiveDatesFromGroceries()
                    .Where(x => !x.DontApplyToPayroll).ToList();
                var biweekly = BiweeklyDatesHelper.GetBiweeklyDates(parsedDate, festiveDates);
                naturalWorkDays = (biweekly.EndOfBiweekly - biweekly.StartOfBiweekly).Days + 1;

                datesBetween = Enumerable.Range(0, 1 + biweekly.EndOfBiweekly.Subtract(biweekly.StartOfBiweekly).Days)
                  .Select(offset => biweekly.StartOfBiweekly.AddDays(offset))
                  .OrderBy(x => x)
                  .ToList();

                existingPayroll = existingPayroll.Where(x => x.DateOfDeparture >= biweekly.StartOfBiweekly || x.DateOfDeparture == null)
                    .ToList();
            }

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    var count = 1;
                    worksheet.Cells[row, count].Value = "Número de empleado";
                    count++;
                    worksheet.Cells[row, count].Value = "Nombre completo";
                    count++;
                    worksheet.Cells[row, count].Value = "Correo del empleado";
                    count++;
                    worksheet.Cells[row, count].Value = "Fecha de nacimiento";
                    count++;
                    worksheet.Cells[row, count].Value = "Fecha de ingreso";
                    count++;
                    worksheet.Cells[row, count].Value = "Teléfono celular";
                    count++;
                    worksheet.Cells[row, count].Value = "Puesto";
                    count++;
                    worksheet.Cells[row, count].Value = "Tipo de contrato";
                    count++;
                    worksheet.Cells[row, count].Value = "IMSS";
                    count++;
                    worksheet.Cells[row, count].Value = "RFC";
                    count++;
                    worksheet.Cells[row, count].Value = "CURP";
                    count++;
                    worksheet.Cells[row, count].Value = "Dirección";
                    count++;
                    worksheet.Cells[row, count].Value = "Banco";
                    count++;
                    worksheet.Cells[row, count].Value = "CLABE";
                    count++;
                    worksheet.Cells[row, count].Value = "Cuenta";
                    count++;
                    worksheet.Cells[row, count].Value = "Sueldo neto mensual";
                    count++;

                    if (datesBetween.Any())
                    {
                        count++;
                        foreach (var date in datesBetween)
                        {
                            worksheet.Cells[row, count].Value = date;
                            worksheet.Cells[row, count].Style.Numberformat.Format = "dd-mm-yyyy";
                            count++;
                        }
                        worksheet.Cells[row, count].Value = "Retardos";
                        count++;
                        worksheet.Cells[row, count].Value = "Faltas por retardos";
                        count++;
                        worksheet.Cells[row, count].Value = "Inasistencias";
                        count++;
                        worksheet.Cells[row, count].Value = "Incapacidad pagadas";
                        count++;
                        worksheet.Cells[row, count].Value = "Incapacidad subsidiadas por IMSS";
                        count++;
                        worksheet.Cells[row, count].Value = "Total de faltas por retardos, inasistencias e incapacidad subsidiadas por IMSS";
                        count++;
                        count++;

                        worksheet.Cells[row, count].Value = "Sueldo neto mensual";
                        count++;
                        worksheet.Cells[row, count].Value = "Sueldo neto quincenal";
                        count++;
                        worksheet.Cells[row, count].Value = "Sueldo diario";
                        count++;
                        worksheet.Cells[row, count].Value = "Días naturales de quincena";
                        count++;
                        worksheet.Cells[row, count].Value = "Días laborables";
                        count++;
                        worksheet.Cells[row, count].Value = "Días laborados";
                        count++;
                        worksheet.Cells[row, count].Value = "Porcentaje laborado de quincena";
                        count++;
                        worksheet.Cells[row, count].Value = "Pago por días laborados";
                        count++;
                        worksheet.Cells[row, count].Value = "Pago días de descanso proporcional";
                        count++;
                        worksheet.Cells[row, count].Value = "Neto a pagar por sueldo";
                        count++;
                        worksheet.Cells[row, count].Value = "Prima vacacional";
                        count++;
                        worksheet.Cells[row, count].Value = "Neto a pagar";
                        count++;
                    }
                    row++;

                    foreach (var payroll in existingPayroll)
                    {
                        count = 1;
                        worksheet.Cells[row, count].Value = payroll.EmployeeNumber;
                        count++;
                        worksheet.Cells[row, count].Value = payroll.GetFullName();
                        count++;
                        worksheet.Cells[row, count].Value = customers.Where(x => x.Id == payroll.CustomerId).FirstOrDefault()?.Email ?? "---";
                        count++;
                        worksheet.Cells[row, count].Value = payroll.DateOfBirth;
                        worksheet.Cells[row, count].Style.Numberformat.Format = "dd-mm-yyyy";
                        count++;
                        worksheet.Cells[row, count].Value = payroll.DateOfAdmission;
                        worksheet.Cells[row, count].Style.Numberformat.Format = "dd-mm-yyyy";
                        count++;
                        worksheet.Cells[row, count].Value = payroll.Cellphone;
                        count++;
                        worksheet.Cells[row, count].Value = payroll.GetCurrentJob()?.Name;
                        count++;
                        worksheet.Cells[row, count].Value = ((PayrollContractType)(payroll.PayrollContractTypeId ?? 0)).GetDisplayName();
                        count++;
                        worksheet.Cells[row, count].Value = "'" + payroll.IMSS;
                        count++;
                        worksheet.Cells[row, count].Value = payroll.RFC;
                        count++;
                        worksheet.Cells[row, count].Value = payroll.CURP;
                        count++;
                        worksheet.Cells[row, count].Value = payroll.Address;
                        count++;
                        worksheet.Cells[row, count].Value = payroll.BankTypeId != null ? ((BankType)payroll.BankTypeId).GetDisplayName() : "Sin especificar";
                        count++;
                        worksheet.Cells[row, count].Value = "'" + payroll.Clabe;
                        count++;
                        worksheet.Cells[row, count].Value = "'" + payroll.AccountNumber;
                        count++;

                        var salary = salaries.Where(x => x.PayrollEmployeeId == payroll.Id)
                            .Where(x => today >= x.ApplyDate)
                            .OrderByDescending(x => x.CreatedOnUtc).FirstOrDefault();
                        worksheet.Cells[row, count].Value = salary?.NetIncome ?? 0;
                        worksheet.Cells[row, count].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                        count++;

                        var vacationsOfEmployee = vacations.Where(x => x.PayrollEmployeeId == payroll.Id).ToList();
                        var vacationDatesOfEmployee = vacationsOfEmployee.Select(x => x.OverriddenDate.Date).ToList();
                        var subjectToWorkingHours = (payroll.GetCurrentJob()?.SubjectToWorkingHours ?? true);

                        if (datesBetween.Any())
                        {
                            count++;
                            var vacationalAmount = (decimal)0;
                            var salaryAmount = payroll.GetCurrentSalaryValue() * 2;
                            var dailyAmount = (decimal)0;
                            if (salaryAmount > 0)
                                dailyAmount = salaryAmount / (decimal)30.42;
                            var anniversaryDate = payroll.DateOfAdmission != null ?
                                datesBetween.Where(x => x.Date.ToString("dd/MM") == payroll.DateOfAdmission.Value.ToString("dd/MM")).FirstOrDefault() :
                                (DateTime?)null;
                            if (anniversaryDate != null)
                            {
                                var years = (int)decimal.Floor((decimal)(anniversaryDate - payroll.DateOfAdmission.Value)?.TotalDays / 360);
                                if (years > 0)
                                {
                                    var daysForVacational = years == 1 ? 6 : years == 2 ? 8 :
                                        years == 3 ? 10 : years == 4 ? 12 :
                                        years >= 5 && years <= 9 ? 14 :
                                        years >= 10 && years <= 14 ? 16 : -1;
                                    vacationalAmount = (daysForVacational * dailyAmount) * (decimal)0.25;
                                }
                            }

                            if (payroll.Id == 148)
                                _ = 0;
                            var parsedDate = DateTime.ParseExact(forBiweek, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            var festiveDates = _assistanceService.GetFestiveDatesFromGroceries()
                                .Where(x => !x.DontApplyToPayroll).ToList();
                            var biweekly = BiweeklyDatesHelper.GetBiweeklyDates(parsedDate, festiveDates);
                            var model = AssistanceHelper.DoAssistanceModelWork(new List<DateOfEmployee>(), new List<PayrollEmployee> { payroll }, today,
                                _assistanceService, _assistanceOverrideService, _customerService, _jobCatalogService, true);
                            var assistancesOfEmployee = model.Item1
                                .Where(x => biweekly.StartOfBiweekly.Date <= x.Date.Date && x.Date.Date <= biweekly.EndOfBiweekly.Date)
                                .OrderByDescending(x => x.Date).ToList();
                            foreach (var date in datesBetween)
                            {
                                var dateOfEmployee = assistancesOfEmployee.Where(x => date.Date == x.Date).FirstOrDefault();
                                var letter = "";
                                if (dateOfEmployee != null)
                                {
                                    if (!subjectToWorkingHours && (payroll.DateOfDeparture == null || dateOfEmployee.Date < payroll.DateOfDeparture))
                                        letter = "A";
                                    else if (vacationDatesOfEmployee.Contains(date.Date) ||
                                        dateOfEmployee.Assistance == (int)AssistanceType.Vacation && (payroll.DateOfDeparture == null || dateOfEmployee.Date < payroll.DateOfDeparture))
                                        letter = "V";
                                    else if (payroll.DateOfDeparture == date.Date)
                                        letter = "B";
                                    else if (dateOfEmployee.Assistance == (int)AssistanceType.InTime && (payroll.DateOfDeparture == null || dateOfEmployee.Date < payroll.DateOfDeparture))
                                        letter = "A";
                                    else if (dateOfEmployee.Assistance == (int)AssistanceType.Absence && (payroll.DateOfDeparture == null || dateOfEmployee.Date < payroll.DateOfDeparture))
                                        letter = "F";
                                    else if (dateOfEmployee.Assistance == (int)AssistanceType.Delay && (payroll.DateOfDeparture == null || dateOfEmployee.Date < payroll.DateOfDeparture))
                                        letter = "R";
                                    else if (dateOfEmployee.Assistance == (int)AssistanceType.PayedDisability && (payroll.DateOfDeparture == null || dateOfEmployee.Date < payroll.DateOfDeparture))
                                        letter = "IP";
                                    else if (dateOfEmployee.Assistance == (int)AssistanceType.SubsidizedDisability && (payroll.DateOfDeparture == null || dateOfEmployee.Date < payroll.DateOfDeparture))
                                        letter = "IS";
                                } else if (payroll.DateOfDeparture == date.Date)
                                    letter = "B";

                                worksheet.Cells[row, count].Value = letter;
                                worksheet.Cells[row, count].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[row, count].Style.Fill.BackgroundColor.SetColor(
                                    letter == "A" ? onTimeColor : letter == "B" ? departureColor :
                                    letter == "F" ? abscenseColor : letter == "V" ? vacationColor :
                                    letter == "R" ? delayColor : letter == "IP" ? payedDisabilityColor :
                                    letter == "IS" ? subsidizedDisabilityColor :
                                    noneColor
                                    );
                                worksheet.Cells[row, count].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                worksheet.Cells[row, count].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                worksheet.Cells[row, count].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                worksheet.Cells[row, count].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                worksheet.Cells[row, count].Style.Border.Top.Color.SetColor(borderColor);
                                worksheet.Cells[row, count].Style.Border.Left.Color.SetColor(borderColor);
                                worksheet.Cells[row, count].Style.Border.Right.Color.SetColor(borderColor);
                                worksheet.Cells[row, count].Style.Border.Bottom.Color.SetColor(borderColor);
                                worksheet.Cells[row, count].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                count++;
                            }

                            var delays = !subjectToWorkingHours ? assistancesOfEmployee.Where(x => !vacationDatesOfEmployee.Contains(x.Date) &&
                                x.Assistance == (int)AssistanceType.Delay && (payroll.DateOfDeparture != null && x.Date >= payroll.DateOfDeparture)).Count()

                                : assistancesOfEmployee.Where(x => !vacationDatesOfEmployee.Contains(x.Date) &&
                                x.Assistance == (int)AssistanceType.Delay && (payroll.DateOfDeparture == null || x.Date < payroll.DateOfDeparture)).Count();

                            var abscensesByDelays = delays > 0 ? (int)(decimal.Floor(delays / 3)) : 0;
                            var abscenses = !subjectToWorkingHours ? assistancesOfEmployee.Where(x => !vacationDatesOfEmployee.Contains(x.Date) &&
                                x.Assistance == (int)AssistanceType.Absence && (payroll.DateOfDeparture != null && x.Date >= payroll.DateOfDeparture)).Count()

                                : assistancesOfEmployee.Where(x => !vacationDatesOfEmployee.Contains(x.Date) &&
                                x.Assistance == (int)AssistanceType.Absence && (payroll.DateOfDeparture == null || x.Date < payroll.DateOfDeparture)).Count();

                            var payedDisabilities = !subjectToWorkingHours ? assistancesOfEmployee.Where(x => !vacationDatesOfEmployee.Contains(x.Date) &&
                                x.Assistance == (int)AssistanceType.PayedDisability && (payroll.DateOfDeparture == null && x.Date >= payroll.DateOfDeparture)).Count()
                                
                                : assistancesOfEmployee.Where(x => !vacationDatesOfEmployee.Contains(x.Date) &&
                                x.Assistance == (int)AssistanceType.PayedDisability && (payroll.DateOfDeparture == null || x.Date < payroll.DateOfDeparture)).Count();

                            var subsidizedDisabilities = !subjectToWorkingHours ? 0 : assistancesOfEmployee.Where(x => !vacationDatesOfEmployee.Contains(x.Date) &&
                                x.Assistance == (int)AssistanceType.SubsidizedDisability && (payroll.DateOfDeparture == null || x.Date < payroll.DateOfDeparture)).Count();

                            var totalAbscences = abscensesByDelays + abscenses + subsidizedDisabilities;
                            worksheet.Cells[row, count].Value = delays;
                            worksheet.Cells[row, count].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            count++;

                            worksheet.Cells[row, count].Value = abscensesByDelays;
                            worksheet.Cells[row, count].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            count++;

                            worksheet.Cells[row, count].Value = abscenses;
                            worksheet.Cells[row, count].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            count++;

                            worksheet.Cells[row, count].Value = payedDisabilities;
                            worksheet.Cells[row, count].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            count++;

                            worksheet.Cells[row, count].Value = subsidizedDisabilities;
                            worksheet.Cells[row, count].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            count++;

                            worksheet.Cells[row, count].Value = totalAbscences;
                            worksheet.Cells[row, count].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            count++;
                            count++;

                            worksheet.Cells[row, count].Value = salaryAmount;
                            worksheet.Cells[row, count].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            count++;
                            worksheet.Cells[row, count].Value = salaryAmount / 2;
                            worksheet.Cells[row, count].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            count++;
                            worksheet.Cells[row, count].Value = dailyAmount;
                            worksheet.Cells[row, count].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            count++;

                            // amounts info
                            var workingDaysOfWeek = _assistanceService.GetScheduleOfEmployee(payroll.GetCurrentJob()?.WorkSchedule);
                            var workingDays = workingDaysOfWeek.Select(x => x.DayOfWeek).Distinct().ToList();
                            var workDates = datesBetween.Where(x => workingDays.Contains(x.DayOfWeek)).ToList();

                            var datesBeforAdmission = new List<DateTime>();
                            if (payroll.DateOfAdmission != null)
                                if (workDates.FirstOrDefault() <= payroll.DateOfAdmission && payroll.DateOfAdmission <= workDates.LastOrDefault())
                                    datesBeforAdmission = workDates.Where(x => payroll.DateOfAdmission > x).ToList();
                            var workedDaysCount = workDates.Count() - datesBeforAdmission.Count();
                            var workingDatesAmount = workDates.Count();
                            var workedDatesAmount = workedDaysCount - totalAbscences;
                            var workedPorcentage = decimal.Round(workedDatesAmount > 0 ? (workedDatesAmount * 100) / workingDatesAmount : 0, 2);
                            var discountByAbscenses = dailyAmount * workedDatesAmount;
                            var biweeklyAmount = (salaryAmount / 2);
                            var proportionalFreePayDays = ((decimal)naturalWorkDays - (decimal)workingDatesAmount) * ((decimal)workedDatesAmount / (decimal)workingDatesAmount) * (decimal)dailyAmount;
                            var netWorthBiweekly = discountByAbscenses + proportionalFreePayDays;

                            worksheet.Cells[row, count].Value = naturalWorkDays;
                            worksheet.Cells[row, count].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            count++;
                            worksheet.Cells[row, count].Value = workingDatesAmount;
                            worksheet.Cells[row, count].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            count++;
                            worksheet.Cells[row, count].Value = workedDatesAmount;
                            worksheet.Cells[row, count].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            count++;
                            worksheet.Cells[row, count].Value = workedPorcentage + "%";
                            //worksheet.Cells[row, count].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            count++;
                            worksheet.Cells[row, count].Value = decimal.Round(discountByAbscenses, 2); 
                            worksheet.Cells[row, count].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            count++;

                            worksheet.Cells[row, count].Value = decimal.Round(proportionalFreePayDays, 2); 
                            worksheet.Cells[row, count].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            count++;

                            worksheet.Cells[row, count].Value = decimal.Round(netWorthBiweekly, 2); 
                            worksheet.Cells[row, count].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            count++;

                            worksheet.Cells[row, count].Value = vacationalAmount;
                            worksheet.Cells[row, count].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            count++;

                            worksheet.Cells[row, count].Value = decimal.Round(netWorthBiweekly + vacationalAmount, 2);
                            worksheet.Cells[row, count].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
                            count++;
                        }

                        row++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                if (string.IsNullOrEmpty(forBiweek))
                    return File(stream.ToArray(), MimeTypes.TextXlsx, $"CEL - Detalles de empleados activos " + DateTime.Now.ToString("dd-MM-yyyy") + ".xlsx");
                else
                    return File(stream.ToArray(), MimeTypes.TextXlsx, $"CEL - Detalles de empleados de quincena " + forBiweek.Replace("/", "-") + ".xlsx");
            }
        }

        [HttpGet]
        public IActionResult PayrollInfo()
        {
            var employees = _payrollEmployeeService.GetAll(includeExEmployees: true)
                .ToList()
                .Select(x => new {
                    Name = x.GetFullName(),
                    JobName = x.GetCurrentJob()?.Name,
                    x.BankTypeId,
                    x.AccountNumber,
                    x.Clabe
                })
                .OrderBy(x => x.Name)
                .ToList();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Nombre completo del trabajador";
                    worksheet.Cells[row, 2].Value = "Nombre del empleo";
                    worksheet.Cells[row, 3].Value = "Banco";
                    worksheet.Cells[row, 4].Value = "Numero de cuenta";
                    worksheet.Cells[row, 5].Value = "CLABE";
                    row++;

                    foreach (var employee in employees)
                    {
                        worksheet.Cells[row, 1].Value = employee.Name;
                        worksheet.Cells[row, 2].Value = employee.JobName;
                        worksheet.Cells[row, 3].Value = ((BankType)employee.BankTypeId).GetDisplayName();
                        worksheet.Cells[row, 4].Value = "'" + employee.AccountNumber;
                        worksheet.Cells[row, 5].Value = "'" + employee.Clabe;
                        row++;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"Empleados info bancaria.xlsx");
            }
        }

        //    [HttpGet]
        //    public IActionResult InfoTest()
        //    {
        //        if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
        //            return BadRequest();

        //        var employees = _payrollEmployeeService.GetAll(includeExEmployees: true)
        //            .Where(x => !x.Deleted &&
        //            (x.EmployeeStatusId == (int)EmployeeStatus.Active || x.EmployeeStatusId == (int)EmployeeStatus.Discharged))
        //            .ToList()
        //            .OrderBy(x => x.GetFullName())
        //            .ToList();

        //        using (var stream = new MemoryStream())
        //        {
        //            using (var xlPackage = new ExcelPackage(stream))
        //            {
        //                var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
        //                int row = 1;

        //                worksheet.Cells[row, 1].Value = "Nombre completo";
        //                worksheet.Cells[row, 2].Value = "Estatus";
        //                row++;

        //                foreach (var employee in employees)
        //                {
        //                    worksheet.Cells[row, 1].Value = employee.GetFullName();
        //                    worksheet.Cells[row, 2].Value = ((EmployeeStatus)employee.EmployeeStatusId).GetDisplayName();

        //                    row++;
        //                }

        //                for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
        //                {
        //                    worksheet.Column(i).AutoFit();
        //                    worksheet.Cells[1, i].Style.Font.Bold = true;
        //                }

        //                xlPackage.Save();
        //            }

        //            return File(stream.ToArray(), MimeTypes.TextXlsx, $"CEL - Detalles de empleados activos o no activos " + DateTime.Now.ToString("dd-MM-yyyy") + ".xlsx");
        //        }
        //    }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult GetEmployeesByIdsExternal([FromBody] List<int> employeeIds)
        {
            try
            {
                if (employeeIds == null)
                    employeeIds = new List<int>();
                var query = _payrollEmployeeService.GetAll();
                if (employeeIds.Any())
                    query = query.Where(x => employeeIds.Contains(x.Id));
                var customerIds = query.Select(x => x.CustomerId).Distinct().ToList();
                var customers = _customerService.GetAllCustomersQuery()
                    .Where(x => !x.Deleted && customerIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.Email })
                    .ToList();
                var employees = query
                    .ToList()
                    .Select(x => new
                    {
                        x.Id,
                        x.CustomerId,
                        FullName = x.GetFullName() + (customers.Where(y => y.Id == x.CustomerId).FirstOrDefault() != null ?
                            $" ({customers.Where(y => y.Id == x.CustomerId).FirstOrDefault().Email})" : ""),
                        Rfc = x.RFC,
                        Curp = x.CURP,
                        Phone = x.Cellphone,
                        Job = x.GetCurrentJob()?.Name ?? "Sin especificar",
                    })
                    .ToList();
                return Json(employees);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        public IActionResult Delete(CreateOrUpdateModel model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.PayrollEmployee))
                return AccessDeniedView();
            var employe = _payrollEmployeeService.GetById(model.Id);
            if (employe == null)
                return RedirectToAction("List");

            employe.Deleted = true;
            _payrollEmployeeService.Update(employe);
            return RedirectToAction("List");
        }
    }

    public class SaveSubordinatesData
    {
        public int BossId { get; set; }
        public int[] SubordinateIds { get; set; }
    }

    public class PendingModel
    {
        public int Info { get; set; }
        public int Files { get; set; }
        public int Salary { get; set; }
        public int JobCatalog { get; set; }
        public int Total { get; set; }
        public int TotalCompleted { get; set; }
    }

    public class LinkedUserModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class FranchisePayrollInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}